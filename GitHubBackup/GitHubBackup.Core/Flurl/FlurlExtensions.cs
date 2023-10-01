using Flurl.Http;

namespace GithubBackup.Core.Flurl;

public static class FlurlExtensions
{
    public static async Task<List<TItem>> GetPagedJsonAsync<TReponse, TItem>(
        this IFlurlRequest request,
        Func<TReponse, List<TItem>> getItems,
        Func<TReponse, int, List<TItem>, bool> hasNextPage,
        Action<IFlurlRequest, int> getNextPageRequest,
        Func<IFlurlRequest, CancellationToken, Task<IFlurlResponse>> sendRequest,
        CancellationToken ct)
    {
        var allItems = new List<TItem>();
        var index = 0;
        while (true)
        {
            getNextPageRequest(request, index);
            var response = await sendRequest(request, ct).ReceiveJson<TReponse>();
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