using System.Text.Json;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using GithubBackup.Core.Flurl;

namespace GithubBackup.Core.Tests.Utils;

public static class ObjectFlurlExtensions
{
    public static IFlurlResponse ToFlurlResponse(this object obj)
    {
        var requestMessage = new HttpRequestMessage();
        requestMessage.Options.TryAdd("FlurlHttpCall", new FlurlCall
        {
            Request = new FlurlRequest
            {
                Settings = new TestFlurlHttpSettings
                {
                    JsonSerializer = new TextJsonSerializer()
                }
            }
        });
            
        return new FlurlResponse(new HttpResponseMessage
        {
            Content = new CapturedJsonContent(JsonSerializer.Serialize(obj)),
            RequestMessage = requestMessage
        });
    }
}