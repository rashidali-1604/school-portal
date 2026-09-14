using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SchoolPortal.Domain.Users;
using SchoolPortal.Infrastructure.Persistence;
using Xunit;

namespace SchoolPortal.IntegrationTests;

public class UsersEndpointsTests : IClassFixture<PortalApiFactory>
{
    private readonly PortalApiFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public UsersEndpointsTests(PortalApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_users_returns_paged_seed_data()
    {
        var response = await _client.GetAsync("/api/users?pageSize=50");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PagedShape>(Json);
        body.Should().NotBeNull();
        body.Items.Should().NotBeEmpty();
        body.TotalCount.Should().Be(body.Items.Count);
    }

    [Fact]
    public async Task GET_users_filters_by_search_term()
    {
        var response = await _client.GetAsync("/api/users?q=nguyen");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PagedShape>(Json);
        body.Items.Should().OnlyContain(x => x.LastName.ToLower().Contains("nguyen")
            || x.FirstName.ToLower().Contains("nguyen")
            || x.Email.ToLower().Contains("nguyen"));
    }

    [Fact]
    public async Task GET_users_by_id_returns_404_for_missing()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_users_by_id_returns_details()
    {
        var first = await FirstSeedUserAsync();
        var response = await _client.GetAsync($"/api/users/{first.Id}");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<UserDetailsShape>(Json);
        body.Id.Should().Be(first.Id);
        body.WalletBalance.Should().Be(first.WalletBalance);
    }

    [Fact]
    public async Task POST_wallet_adjustments_topup_increases_balance()
    {
        var target = await FirstSeedUserAsync();
        var opening = target.WalletBalance;

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{target.Id}/wallet-adjustments")
        {
            Content = JsonContent.Create(new
            {
                amount = 42.50m,
                reason = "Topup",
                note = "test top-up"
            }, options: Json)
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<AdjustmentShape>(Json);
        body.BalanceAfter.Should().Be(opening + 42.50m);
        body.WasReplayed.Should().BeFalse();
    }

    [Fact]
    public async Task POST_wallet_adjustments_rejects_overdraw_with_422()
    {
        var target = await FirstSeedUserAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/users/{target.Id}/wallet-adjustments")
        {
            Content = JsonContent.Create(new
            {
                amount = -(target.WalletBalance + 1000m),
                reason = "Purchase"
            }, options: Json)
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task POST_wallet_adjustments_is_idempotent_by_key()
    {
        var target = await FirstSeedUserAsync();
        var openingBalance = target.WalletBalance;
        var key = Guid.NewGuid().ToString("N");

        HttpRequestMessage Build() => new(HttpMethod.Post, $"/api/users/{target.Id}/wallet-adjustments")
        {
            Content = JsonContent.Create(new { amount = 10m, reason = "Topup" }, options: Json),
            Headers = { { "Idempotency-Key", key } }
        };

        var first = await _client.SendAsync(Build());
        var second = await _client.SendAsync(Build());

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondBody = await second.Content.ReadFromJsonAsync<AdjustmentShape>(Json);
        secondBody.WasReplayed.Should().BeTrue();
        secondBody.BalanceAfter.Should().Be(openingBalance + 10m);

        var detailsResponse = await _client.GetAsync($"/api/users/{target.Id}");
        var details = await detailsResponse.Content.ReadFromJsonAsync<UserDetailsShape>(Json);
        details.WalletBalance.Should().Be(openingBalance + 10m);
    }

    private async Task<UserListItemShape> FirstSeedUserAsync()
    {
        var response = await _client.GetAsync("/api/users?pageSize=50");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedShape>(Json);
        return body.Items[0];
    }

    private sealed class PagedShape
    {
        public List<UserListItemShape> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    private class UserListItemShape
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public decimal WalletBalance { get; set; }
    }

    private sealed class UserDetailsShape : UserListItemShape
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    private sealed class AdjustmentShape
    {
        public Guid AdjustmentId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public bool WasReplayed { get; set; }
    }
}
