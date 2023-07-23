using Newtonsoft.Json;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try{
                string address = "https://ebaf54c2744fdd214f.gradio.live";

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

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            // ImagePrompter.ResultFromText(@"./output/test1.txt");
        }
    }
}

    