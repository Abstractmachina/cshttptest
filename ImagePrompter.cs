using System.Drawing;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System;



namespace MyApp
{
    public class ImagePrompter
    {
        // private static Image FromBase64String(string input)
        // {
        //     //data:image/gif;base64,
        //     //this image is a single pixel (black)
        //     byte[] bytes = Convert.FromBase64String(input);

        //     Image image;
        //     using (MemoryStream ms = new MemoryStream(bytes))
        //     {
        //         image = Image.FromStream(ms);
        //     }

        //     return image;
        // }

        // public static void ResultFromText(string path)
        // {
        //     string contents = File.ReadAllText(path);
        //     // Console.WriteLine(contents);
        //     var converted = JsonConvert.DeserializeObject<ResponseArtefact>(contents);
        //     Console.WriteLine(converted.artifacts[0].Seed);

        //     var image = ImagePrompter.FromBase64String(converted.artifacts[0].Base64);
        //     image.Save(@"./output/test1.jpg");

        // }





        public static async Task<string> Auto1111_T2I(string address, string username, string password, Payload payload) {
            using (HttpClient client = new())
            {
                try
                {
                    // var settings = new JsonSerializerSettings
                    // {
                    //     Converters = { new T2IJsonConverter() }
                    // };
                    string endpoint = address + "/sdapi/v1/txt2img";
                    Console.WriteLine(endpoint);

                    var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                    // request.Headers.Add("Authorization", apiKey);

                    var json = JsonConvert.SerializeObject(payload);
                    Console.WriteLine(json);
                    request.Content = new StringContent(json);
                    // request.Content = new StringContent(JsonConvert.SerializeObject(newReqBody, settings));
                    
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await client.SendAsync(request);
                    Console.WriteLine(response.StatusCode);
                    var result = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("results coming in ....");
                    Console.WriteLine(result);

                    string path = @"./output/test1.txt";
                    // Util.SaveStringToFile(result, path);

                    return result;


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return "";
                }
            }
        }
        


        
        /// <summary>
        /// api call for stability ai's official api service. 
        /// </summary>
        /// <returns></returns>
        public static async Task DS_T2I_prompt()
        {
            using (HttpClient client = new())
            {
                try
                {
                    DotNetEnv.Env.Load();
                    string apiKey = Environment.GetEnvironmentVariable("DREAMSTUDIO_KEY");
                    // client.DefaultRequestHeaders.Add("Authorizations", apiKey);                
                    if (apiKey == null) throw new Exception("Missing Stability API key.");

                    var newReqBody = new ReqBody(
                        new List<TextPrompt>{
                            new TextPrompt("star-shaped, red apple", 0.5f)
                        },
                        512, 512
                    );

                    var settings = new JsonSerializerSettings
                    {
                        Converters = { new T2IJsonConverter() }
                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stability.ai/v1/generation/stable-diffusion-v1-5/text-to-image");
                    request.Headers.Add("Authorization", apiKey);
                    request.Content = new StringContent(JsonConvert.SerializeObject(newReqBody, settings));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await client.SendAsync(request);
                    var result = await response.Content.ReadAsStringAsync();

                    string path = @"./output/test1.txt";
                    // Util.SaveStringToFile(result, path);

                    return;
                    var responseObject = JsonConvert.DeserializeObject<ResponseArtefact>(result);
                    // Console.WriteLine(responseObject.finishReason);
                    // Console.WriteLine(responseObject.seed);
                    if (responseObject == null) throw new Exception("API request failed. ResponseObject is null");
                    // var rr = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseObject.artifacts[0]);
                    // foreach(KeyValuePair<string, object>pair in rr) {
                    //     Console.WriteLine(pair.Key);
                    //     Console.WriteLine(pair.Value);
                    // }
                    // Console.WriteLine(responseObject.artifacts.Count);
                    // foreach (var v in responseObject.artifacts)
                    // Console.WriteLine(v);
                    // foreach(KeyValuePair<string, object>item in responseObject) Console.WriteLine(item.Key.GetType());
                    return;


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }


        /// <summary>
        /// OBSOLETE api call to stable diffusion api - third party provider, didnt know that beforehand. 
        /// </summary>
        /// <returns></returns>
        public static async Task T2I_prompt()
        {
            using (HttpClient client = new())
            {
                try
                {
                    DotNetEnv.Env.Load();
                    string apiKey = Environment.GetEnvironmentVariable("API_KEY");


                    var newPrompt = new TextToImagePrompt(apiKey);

                    var settings = new JsonSerializerSettings
                    {
                        Converters = { new T2IJsonConverter() }
                    };

                    StringContent body = new(JsonConvert.SerializeObject(newPrompt, settings));
                    body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    // StringContent body = new(JsonConvert.SerializeObject(newPrompt));

                    Console.WriteLine(await body.ReadAsStringAsync());

                    HttpResponseMessage response = await client.PostAsync("https://stablediffusionapi.com/api/v3/text2img", body);
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        throw new Exception(content);
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);

                    var json = JsonConvert.DeserializeObject<SDResponseObject>(result);

                    Console.WriteLine(json.output[0]);

                    //Console.WriteLine(json[""])


                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
            }
        }
    }

    public class ResponseArtefact
    {
        public List<ResponseObject>? artifacts { get; set; }
    }

    public class ResponseObject
    {
        [JsonProperty("base64")]
        public string? Base64 { get; set; }
        [JsonProperty("finishReason")]
        public string? FinishReason { get; set; }
        [JsonProperty("seed")]
        public Int64? Seed { get; set; }
    }

    public class ReqBody
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public List<TextPrompt> Text_prompts { get; set; }
        // public float Cfg_scale {get;set;}
        // public string Clip_guidance_preset {get;set;}
        // public string Sampler {get;set;}
        // public int Samples {get;set;}
        // public int Seed {get;set;}
        // public int Steps {get;set;}
        // public string Style_presets {get;set;}

        public ReqBody(List<TextPrompt> textPrompts, int height, int width)
        {
            Text_prompts = textPrompts;
            Height = height;
            Width = width;
        }

        public override string ToString()
        {
            string output = "ReqBody Object:\n";
            output += $"\tHeight: {Height}\n";
            output += $"\tWidth: {Width}\n";
            output += "\tText Prompts:\n";
            foreach (var tp in Text_prompts)
            {
                output += $"\t\tText: '{tp.Text}', Weight: {tp.Weight}\n";
            }
            return output;
        }
    }

    public class SDResponseObject
    {
        public string status { get; set; }
        public string generationTime { get; set; }
        public List<string> output { get; set; }

    }
}
