using Newtonsoft.Json;

namespace Martic.Infrastructure.Helpers.Settings
{
    [JsonObject("api")]
    public sealed class ApiSettings
    {
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }
    }
}
