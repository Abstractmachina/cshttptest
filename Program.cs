using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // await ImagePrompter.DS_T2I_prompt();
            ImagePrompter.ResultFromText(@"./output/test1.txt");
        }
    }



        public class ResponseArtefact {
            public List<ResponseObject>? artifacts{get;set;}
        }

        public class ResponseObject {
            [JsonProperty("base64")]
            public string? Base64 {get;set;}
            [JsonProperty("finishReason")]
            public string? FinishReason {get; set;} 
            [JsonProperty("seed")]
            public Int64? Seed {get;set;}
        }

    public class ImagePrompter
    {

        public static void ResultFromText(string path) {
            string contents = File.ReadAllText(path);
            // Console.WriteLine(contents);
            var converted = JsonConvert.DeserializeObject<ResponseArtefact>(contents);
            Console.WriteLine(converted.artifacts[0].Seed);

        }

        public static async Task DS_T2I_prompt() {
            using (HttpClient client = new()) {
                try {
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
                    if(responseObject == null) throw new Exception("API request failed. ResponseObject is null");
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


                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }
        }


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

    public class ReqBody {
        public int Height {get;set;}
        public int Width {get;set;}
        public List<TextPrompt> Text_prompts {get;set;}
        // public float Cfg_scale {get;set;}
        // public string Clip_guidance_preset {get;set;}
        // public string Sampler {get;set;}
        // public int Samples {get;set;}
        // public int Seed {get;set;}
        // public int Steps {get;set;}
        // public string Style_presets {get;set;}

        public ReqBody(List<TextPrompt> textPrompts, int height, int width) {
            Text_prompts = textPrompts;
            Height = height;
            Width = width;
        }

        public override string ToString()
        {
            string output = "ReqBody Object:\n";
            output += $"\tHeight: {Height}\n";
            output += $"\tWidth: {Width}\n";
            output+= "\tText Prompts:\n";
            foreach (var tp in Text_prompts) {
                output+= $"\t\tText: '{tp.Text}', Weight: {tp.Weight}\n";
            }
            return output;
        }
    }

    public class TextPrompt {
        public string Text {get;set;}
        public float Weight {get;set;}

        public TextPrompt(string text, float weight) {
            Text = text;
            Weight = weight;
        }
    }

    public class SDResponseObject {
        public string status {get;set;}
        public string generationTime {get;set;}
        public List<string> output {get;set;}

    }


    public class TextToImagePrompt
    {
        public string Key { get; set; }
        public string Prompt { get; set; }
        public string? Negative_prompt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Samples { get; set; }
        public int Num_inference_steps { get; set; }
        public bool Safety_checker { get; set; }
        public bool Enhance_prompt { get; set; }
        public int? Seed { get; set; }
        public float Guidance_scale { get; set; }
        public bool Multi_lingual { get; set; }
        public bool Panorama { get; set; }
        public bool Self_attention { get; set; }
        public bool Upscale { get; set; }
        public string Embedding_model { get; set; }
        public string? Webhook { get; set; }
        public string? Track_id { get; set; }

        public TextToImagePrompt(string key) : this(key, "a round, red apple") { }

        public TextToImagePrompt(string key, string prompt) : this(key, prompt, null, 512, 512) { }

        public TextToImagePrompt(string key, string prompt, string? negative_prompt, int width, int height) : this(key, prompt, negative_prompt, width, height, 1, 20) { }

        public TextToImagePrompt(
            string key, string prompt, string negative_prompt, int width, int height, int samples, int num_inference_steps
        ) : this(
            key, prompt, negative_prompt, width, height, samples, num_inference_steps, false, true, null, 7.5f, false, false, false, false, "embeddings_model_id", null, null
        )
        {
        }

        public TextToImagePrompt(string key, string prompt, string negative_prompt, int width, int height, int samples, int num_inference_steps, bool safety_checker, bool enhance_prompt, int? seed, float guidance_scale, bool multi_lingual, bool panorama, bool self_attention, bool upscale, string embeddings_model, string? webhook, string? track_id)
        {
            Key = key;
            Prompt = prompt;
            Negative_prompt = negative_prompt;
            Width = width;
            Height = height;
            Samples = samples;
            Num_inference_steps = num_inference_steps;
            Safety_checker = safety_checker;
            Enhance_prompt = enhance_prompt;
            Seed = seed;
            Guidance_scale = guidance_scale;
            Multi_lingual = multi_lingual;
            Panorama = panorama;
            Self_attention = self_attention;
            Upscale = upscale;
            Embedding_model = embeddings_model;
            Webhook = webhook;
            Track_id = track_id;
        }
    }
}

    