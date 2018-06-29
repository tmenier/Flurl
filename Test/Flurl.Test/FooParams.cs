using Newtonsoft.Json;

namespace Flurl.Test
{
    public class FooParams
    {
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("per_page")]
        public int PerPage { get; set; }
        [JsonProperty("term")]
        public string SearchTerm { get; set; }

        public static FooParams Instance()
            => new FooParams
            {
                Page = 1,
                PerPage = 10,
                SearchTerm = "SpaceGhost"
            };
    }
}
