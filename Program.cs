using Newtonsoft.Json;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try{
                string address = "https://d8e9eed473eb27af40.gradio.live";

                // load up authentication credentials
                DotNetEnv.Env.Load();
                string? username = Environment.GetEnvironmentVariable("USERNAME");
                string? password = Environment.GetEnvironmentVariable("PASSWORD");

                if (username == null || password == null) throw new Exception("Credentials failed to load");

                // prepare scribble image as base64 string
                byte[] imageArray = System.IO.File.ReadAllBytes(@"./assets/scribbleTest_01.png");
                var scribble = Convert.ToBase64String(imageArray);


                var payload = new Payload("3d printing clay, layer, toolpath", "bad, worse, low quality, strange, ugly", 20, 7, 598, 624, new AlwaysOnScripts(ControlNetSettingsFactory.Create("control_v11p_sd15_scribble [d4ba51ff]", "scribble_hed", scribble)));

                var result = await ImagePrompter.Auto1111_T2I(address, username, password, payload);

                var responseObject = JsonConvert.DeserializeObject<ResponseObject>(result);

                // Console.WriteLine(responseObject.Images.Select(x => Console.WriteLine(x));
                if (responseObject != null) {
                    for (int i = 0; i < responseObject.Images.Count; i++)
                    {
                        var image = Util.FromBase64String(responseObject.Images[i]);
                        var date = DateTime.Now.ToString("yymmdd-hhmmss");
                        string path = String.Format("./output/{0}", date);
                        System.IO.Directory.CreateDirectory(path);
                        image.Save(path + String.Format("/img{0}.png", i), System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            // ImagePrompter.ResultFromText(@"./output/test1.txt");
        }
    }

    internal class ResponseObject {
        [JsonProperty("images")]
        public List<string> Images {get; set;}
        [JsonProperty("info")]
        public string Info {get; set;}
        public ResponseObject() {
        }
    }
}

    