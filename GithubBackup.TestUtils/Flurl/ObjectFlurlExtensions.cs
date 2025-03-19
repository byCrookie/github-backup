using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using GithubBackup.Core.Flurl;

namespace GithubBackup.TestUtils.Flurl;

public static class ObjectFlurlExtensions
{
    public static IFlurlResponse ToFlurlJsonResponse(this object obj)
    {
        return new FlurlResponse(
            new FlurlCall
            {
                HttpResponseMessage = new HttpResponseMessage
                {
                    Content = new CapturedJsonContent(JsonSerializer.Serialize(obj)),
                    RequestMessage = new HttpRequestMessage(),
                },
                Request = new FlurlRequest
                {
                    Settings =
                    {
                        JsonSerializer = new DefaultJsonSerializer(
                            new JsonSerializerOptions
                            {
                                Converters = { new JsonStringEnumConverter() },
                            }
                        ),
                    },
                },
            }
        );
    }
}
