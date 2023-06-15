using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http.Headers;



namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await ImagePrompter.T2I_prompt();
        }
    }






    public class ImagePrompter
    {

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


    class CustomContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }

    }

    class T2IJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var prompt = (TextToImagePrompt)value;
            var jsonObject = new JObject();
            var contractResolver = serializer.ContractResolver as DefaultContractResolver;

            // Serialize properties
            foreach (var property in value.GetType().GetProperties())
            {
                var propertyName = contractResolver != null ? contractResolver.GetResolvedPropertyName(property.Name) : property.Name;
                var propertyValue = property.GetValue(value);
                if (propertyValue == null)
                {
                    jsonObject.Add(propertyName.ToLower(), JValue.CreateNull());
                }
                else if (propertyValue is bool)
                {
                    jsonObject.Add(propertyName.ToLower(),
                    ((bool)propertyValue == true) ? "yes" : "no");

                }
                else
                {
                    jsonObject.Add(propertyName.ToLower(), JToken.FromObject(propertyValue));
                }
            }
            jsonObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TextToImagePrompt);
        }
    }
}