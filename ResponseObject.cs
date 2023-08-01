using Newtonsoft.Json;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class ResponseObject {
        [JsonProperty("images")]
        public List<string> Images {get; set;}
        [JsonProperty("info")]
        public string Info {get; set;}
        public ResponseObject() {
            Images = new List<string>();
            Info = "";
        }
    }
}

    