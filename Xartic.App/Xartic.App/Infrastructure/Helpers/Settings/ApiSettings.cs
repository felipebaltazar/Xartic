using Newtonsoft.Json;
using Xamarin.Forms.Internals;

namespace Xartic.App.Infrastructure.Helpers.Settings
{
    [Preserve(AllMembers = true)]
    [JsonObject("api")]
    public sealed class ApiSettings
    {
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }
    }
}
