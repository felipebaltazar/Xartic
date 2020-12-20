using Newtonsoft.Json;
using System.Collections.Generic;

namespace Xartic.Api.Domain.Models
{
    public class GameCategory
    {
        [JsonProperty("words")]
        public IList<string> Words { get; set; }
    }
}
