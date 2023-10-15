using AutoBogus;
using FluentAssertions;
using Flurl.Http;
using Flurl.Http.Testing;
using GithubBackup.Core.Flurl;

namespace GithubBackup.Core.Tests.Flurl;

public class FlurlExtensionsTests
{
    [Fact]
    public async Task GetPagedJsonAsync_WhenHasNoPages_ThenReturnEmptyList()
    {
        const string url = "https://example.com/items";
        const string pageParam = "page";
        const int pageSize = 2;

        var itemsBatch = new List<Item>();

        using var httpTest = new HttpTest();
        
        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(new ItemResponse(itemsBatch));

        var items = await new FlurlRequest(url)
            .GetPagedJsonAsync<ItemResponse, Item>(
                r => r.Items,
                (_, _, it) => it.Count == pageSize,
                (r, i) => r.SetQueryParam(pageParam, i + 1),
                (r, c) => r.SendAsync(HttpMethod.Get, null, c),
                CancellationToken.None
            );
        
        items.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetPagedJsonAsync_WhenHasPages_ThenReturnAllItems()
    {
        const string url = "https://example.com/items";
        const string pageParam = "page";
        const int pageSize = 2;

        var itemsBatch1 = new AutoFaker<Item>().Generate(pageSize);
        var itemsBatch2 = new AutoFaker<Item>().Generate(pageSize);
        var itemsBatch3 = new AutoFaker<Item>().Generate(pageSize - 1);

        var expectedItems = new List<Item>();
        expectedItems.AddRange(itemsBatch1);
        expectedItems.AddRange(itemsBatch2);
        expectedItems.AddRange(itemsBatch3);

        using var httpTest = new HttpTest();
        
        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithQueryParam(pageParam, 1)
            .RespondWithJson(new ItemResponse(itemsBatch1));

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithQueryParam(pageParam, 2)
            .RespondWithJson(new ItemResponse(itemsBatch2));

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithQueryParam(pageParam, 3)
            .RespondWithJson(new ItemResponse(itemsBatch3));

        var items = await new FlurlRequest(url)
            .GetPagedJsonAsync<ItemResponse, Item>(
                r => r.Items,
                (_, _, it) => it.Count == pageSize,
                (r, i) => r.SetQueryParam(pageParam, i + 1),
                (r, c) => r.SendAsync(HttpMethod.Get, null, c),
                CancellationToken.None
            );
        
        items.Should().BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
    }
}