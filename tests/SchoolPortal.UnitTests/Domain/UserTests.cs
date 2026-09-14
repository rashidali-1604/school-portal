using System;
using FluentAssertions;
using SchoolPortal.Domain.Common;
using SchoolPortal.Domain.Users;
using Xunit;

namespace SchoolPortal.UnitTests.Domain;

public class UserTests
{
    private static readonly DateTimeOffset Now = new(2026, 09, 14, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_creates_active_user_with_normalised_email()
    {
        var user = User.Register("Amelia", "Nguyen", "  Amelia.Nguyen@Example.com  ", UserRole.Parent, Now, 20m);

        user.Status.Should().Be(UserStatus.Active);
        user.Email.Should().Be("amelia.nguyen@example.com");
        user.WalletBalance.Should().Be(20m);
        user.Adjustments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Nguyen", "a@b.com")]
    [InlineData("Amelia", "", "a@b.com")]
    [InlineData("Amelia", "Nguyen", "not-an-email")]
    public void Register_rejects_invalid_input(string first, string last, string email)
    {
        Action act = () => User.Register(first, last, email, UserRole.Parent, Now);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Register_rejects_negative_opening_balance()
    {
        Action act = () => User.Register("A", "B", "a@b.com", UserRole.Parent, Now, -5m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AdjustWallet_positive_increases_balance_and_records_audit_row()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, Now, 10m);

        var adjustment = user.AdjustWallet(15m, AdjustmentReason.Topup, "reload", "req-1", "admin", Now);

        user.WalletBalance.Should().Be(25m);
        user.Adjustments.Should().ContainSingle();
        adjustment.Amount.Should().Be(15m);
        adjustment.BalanceAfter.Should().Be(25m);
        adjustment.RequestId.Should().Be("req-1");
    }

    [Fact]
    public void AdjustWallet_negative_decreases_balance_when_sufficient_funds()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, Now, 50m);

        user.AdjustWallet(-20m, AdjustmentReason.Purchase, "lunch", "req-2", "admin", Now);

        user.WalletBalance.Should().Be(30m);
    }

    [Fact]
    public void AdjustWallet_rejects_negative_going_below_zero()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, Now, 10m);

        Action act = () => user.AdjustWallet(-15m, AdjustmentReason.Purchase, null, "req-3", "admin", Now);

        act.Should().Throw<InsufficientFundsException>();
        user.WalletBalance.Should().Be(10m);
        user.Adjustments.Should().BeEmpty();
    }

    [Fact]
    public void AdjustWallet_rejects_zero_amount()
    {
        var user = User.Register("A", "B", "a@b.com", UserRole.Parent, Now, 10m);

        Action act = () => user.AdjustWallet(0m, AdjustmentReason.Correction, null, "req-4", "admin", Now);

        act.Should().Throw<DomainException>();
    }
}
