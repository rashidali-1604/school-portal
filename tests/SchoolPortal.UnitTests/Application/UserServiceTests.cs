using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SchoolPortal.Application.Common;
using SchoolPortal.Application.Interfaces;
using SchoolPortal.Application.Services;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;
using SchoolPortal.Infrastructure.Context;
using SchoolPortal.Infrastructure.Repositories;
using Xunit;

namespace SchoolPortal.UnitTests.Application
{
    public class UserServiceTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly UserService _service;
        private readonly FixedClock _clock;

        public UserServiceTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            var repo = new UserRepository(_context);
            _clock = new FixedClock(new DateTimeOffset(2026, 09, 14, 10, 0, 0, TimeSpan.Zero));
            _service = new UserService(repo, _clock, NullLogger<UserService>.Instance);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        [Fact]
        public async Task Positive_adjustment_increases_balance()
        {
            var user = SeedUser(openingBalance: 10m);

            var result = await _service.AdjustWalletAsync(Request(user.Id, 25m, requestId: "req-1"), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.BalanceAfter.Should().Be(35m);
            result.Value.WasReplayed.Should().BeFalse();
        }

        [Fact]
        public async Task Overdraw_is_rejected_and_balance_stays_intact()
        {
            var user = SeedUser(openingBalance: 5m);

            var result = await _service.AdjustWalletAsync(Request(user.Id, -10m, AdjustmentReason.Purchase, "req-2"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("wallet.insufficient_funds");

            var stored = await _context.Users.AsNoTracking().FirstAsync(x => x.Id == user.Id);
            stored.WalletBalance.Should().Be(5m);
        }

        [Fact]
        public async Task Replay_with_same_key_returns_original_adjustment_and_does_not_double_write()
        {
            var user = SeedUser();
            var request = Request(user.Id, 30m, requestId: "same-key");

            var first = await _service.AdjustWalletAsync(request, CancellationToken.None);
            var second = await _service.AdjustWalletAsync(request, CancellationToken.None);

            first.IsSuccess.Should().BeTrue();
            second.IsSuccess.Should().BeTrue();
            second.Value.WasReplayed.Should().BeTrue();
            second.Value.AdjustmentId.Should().Be(first.Value.AdjustmentId);

            (await _context.WalletAdjustments.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Zero_amount_is_rejected_up_front()
        {
            var user = SeedUser(openingBalance: 10m);

            var result = await _service.AdjustWalletAsync(Request(user.Id, 0m, AdjustmentReason.Correction, "req-x"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("wallet.amount_zero");
        }

        [Fact]
        public async Task Missing_idempotency_key_is_rejected()
        {
            var user = SeedUser(openingBalance: 10m);

            var result = await _service.AdjustWalletAsync(Request(user.Id, 5m, requestId: ""), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("wallet.request_id_required");
        }

        [Fact]
        public async Task Unknown_user_returns_not_found()
        {
            var result = await _service.AdjustWalletAsync(Request(Guid.NewGuid(), 5m, requestId: "req-nf"), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Two_writers_racing_the_same_user_produce_a_concurrency_exception()
        {
            var user = SeedUser(openingBalance: 100m);

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

            using var contextA = new AppDbContext(options);
            using var contextB = new AppDbContext(options);

            var userA = await contextA.Users.FirstAsync(x => x.Id == user.Id);
            var userB = await contextB.Users.FirstAsync(x => x.Id == user.Id);

            userA.AdjustWallet(-10m, AdjustmentReason.Purchase, "a", "req-a", "admin", _clock.UtcNow);
            userB.AdjustWallet(-20m, AdjustmentReason.Purchase, "b", "req-b", "admin", _clock.UtcNow);

            await contextA.SaveChangesAsync();

            Func<Task> act = () => contextB.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        }

        [Fact]
        public async Task GetDetails_returns_not_found_for_missing_user()
        {
            var result = await _service.GetDetailsAsync(Guid.NewGuid(), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Search_normalises_page_size_and_returns_seeded_row()
        {
            SeedUser(email: "search@me.com");

            var result = await _service.SearchAsync(new UserSearchCriteria { PageSize = 999 }, CancellationToken.None);

            result.PageSize.Should().Be(20);
            result.Items.Should().ContainSingle(x => x.Email == "search@me.com");
        }

        private User SeedUser(decimal openingBalance = 0m, string email = null)
        {
            var user = User.Register("A", "B", email ?? $"{Guid.NewGuid():N}@t.com", UserRole.Parent, _clock.UtcNow, openingBalance);
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        private static WalletAdjustmentRequest Request(Guid userId, decimal amount, AdjustmentReason reason = AdjustmentReason.Topup, string requestId = "req")
        {
            return new WalletAdjustmentRequest
            {
                UserId = userId,
                Amount = amount,
                Reason = reason,
                RequestId = requestId,
                PerformedBy = "admin"
            };
        }

        private class FixedClock : IClock
        {
            public FixedClock(DateTimeOffset now) { UtcNow = now; }
            public DateTimeOffset UtcNow { get; }
        }
    }
}
