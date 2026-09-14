using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SchoolPortal.Application.Abstractions;
using SchoolPortal.Application.Common;
using SchoolPortal.Application.Users.Commands;
using SchoolPortal.Domain.Users;
using SchoolPortal.Infrastructure.Persistence;
using SchoolPortal.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SchoolPortal.UnitTests.Application;

public class AdjustWalletHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PortalDbContext _context;
    private readonly AdjustWalletHandler _handler;
    private readonly FixedClock _clock;

    public AdjustWalletHandlerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<PortalDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new PortalDbContext(options);
        _context.Database.EnsureCreated();

        var repo = new UserRepository(_context);
        _clock = new FixedClock(new DateTimeOffset(2026, 09, 14, 10, 0, 0, TimeSpan.Zero));
        _handler = new AdjustWalletHandler(repo, _clock, NullLogger<AdjustWalletHandler>.Instance);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Positive_adjustment_increases_balance_and_returns_success()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, _clock.UtcNow, 10m);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _handler.HandleAsync(new AdjustWalletCommand
        {
            UserId = user.Id,
            Amount = 25m,
            Reason = AdjustmentReason.Topup,
            RequestId = "req-1",
            PerformedBy = "admin"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.BalanceAfter.Should().Be(35m);
        result.Value.WasReplayed.Should().BeFalse();
    }

    [Fact]
    public async Task Negative_adjustment_that_overdraws_returns_domain_failure()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, _clock.UtcNow, 5m);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _handler.HandleAsync(new AdjustWalletCommand
        {
            UserId = user.Id,
            Amount = -10m,
            Reason = AdjustmentReason.Purchase,
            RequestId = "req-2",
            PerformedBy = "admin"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("wallet.insufficient_funds");
        result.Error.Type.Should().Be(ErrorType.Domain);

        var stored = await _context.Users.FirstAsync(x => x.Id == user.Id);
        stored.WalletBalance.Should().Be(5m);
    }

    [Fact]
    public async Task Replay_of_same_request_id_returns_original_adjustment_without_second_write()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, _clock.UtcNow, 0m);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var cmd = new AdjustWalletCommand
        {
            UserId = user.Id,
            Amount = 30m,
            Reason = AdjustmentReason.Topup,
            RequestId = "same-key",
            PerformedBy = "admin"
        };

        var first = await _handler.HandleAsync(cmd, CancellationToken.None);
        var second = await _handler.HandleAsync(cmd, CancellationToken.None);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Value.WasReplayed.Should().BeTrue();
        second.Value.AdjustmentId.Should().Be(first.Value.AdjustmentId);

        var count = await _context.WalletAdjustments.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Zero_amount_rejected()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, _clock.UtcNow, 10m);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _handler.HandleAsync(new AdjustWalletCommand
        {
            UserId = user.Id,
            Amount = 0m,
            Reason = AdjustmentReason.Correction,
            RequestId = "req-x",
            PerformedBy = "admin"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("wallet.amount_zero");
    }

    [Fact]
    public async Task Missing_request_id_rejected()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, _clock.UtcNow, 10m);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _handler.HandleAsync(new AdjustWalletCommand
        {
            UserId = user.Id,
            Amount = 5m,
            Reason = AdjustmentReason.Topup,
            RequestId = "",
            PerformedBy = "admin"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("wallet.request_id_required");
    }

    [Fact]
    public async Task Concurrent_adjustments_reject_second_writer_with_concurrency_exception()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, _clock.UtcNow, 100m);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var secondOptions = new DbContextOptionsBuilder<PortalDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var contextA = new PortalDbContext(secondOptions);
        using var contextB = new PortalDbContext(secondOptions);

        var userA = await contextA.Users.FirstAsync(x => x.Id == user.Id);
        var userB = await contextB.Users.FirstAsync(x => x.Id == user.Id);

        userA.AdjustWallet(-10m, AdjustmentReason.Purchase, "a", "req-a", "admin", _clock.UtcNow);
        userB.AdjustWallet(-20m, AdjustmentReason.Purchase, "b", "req-b", "admin", _clock.UtcNow);

        await contextA.SaveChangesAsync();

        Func<Task> act = () => contextB.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task Unknown_user_returns_not_found()
    {
        var result = await _handler.HandleAsync(new AdjustWalletCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 5m,
            Reason = AdjustmentReason.Topup,
            RequestId = "req-nf",
            PerformedBy = "admin"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset now) { UtcNow = now; }
        public DateTimeOffset UtcNow { get; }
    }
}
