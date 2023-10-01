using Flurl.Http;

namespace GithubBackup.Core.Flurl;

public static class FlurlExtensions
{
    public static Task<List<TItem>> GetGithubPagedJsonAsync<TReponse, TItem>(
        this IFlurlRequest request,
        int perPage,
        Func<TReponse, List<TItem>> getItems,
        CancellationToken ct)
    {
        return request
            .SetQueryParam("per_page", perPage)
            .GetPagedJsonAsync(
                getItems,
                (_, _, items) => items.Count == perPage,
                (rq, index) => rq.SetQueryParam("page", index + 1),
                ct
            );
    }

    public static async Task<List<TItem>> GetPagedJsonAsync<TReponse, TItem>(
        this IFlurlRequest request,
        Func<TReponse, List<TItem>> getItems,
        Func<TReponse, int, List<TItem>, bool> hasNextPage,
        Action<IFlurlRequest, int> getNextPageRequest,
        CancellationToken ct)
    {
        var allItems = new List<TItem>();
        var index = 0;
        while (true)
        {
            getNextPageRequest(request, index);
            var response = await request.GetJsonAsync<TReponse>(ct);
            var items = getItems(response);
            allItems.AddRange(items);

            if (!hasNextPage(response, index, items))
            {
                return allItems;
            }

            index++;
        }
    }
}