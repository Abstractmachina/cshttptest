using System.Drawing;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System;
using System.Net.Http;

namespace MyApp
{
    public class ImagePrompter
    {
        private static async Task<string> Interrupt(string baseAddress, HttpClient client)
        {
            string uri = baseAddress + "/sdapi/v1/interrupt";

            var response = await client.PostAsync("/sdapi/v1/skip", null);
            // var requestMsg = new HttpRequestMessage(HttpMethod.Post, uri);
            // var json = JsonConvert.SerializeObject(payload);
            // requestMsg.Content = new StringContent(json);

            // requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // await client.SendAsync(requestMsg);
            var responseMsg = await response.Content.ReadAsStringAsync();
            return responseMsg;
        }


        public static async Task GenerateAndPollImage(string baseAddress, Payload payload)
        {
            using (HttpClient client = new())
            {
                try
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    // IApiHandler handler = new Auto1111Handler(client);

                    var interruptStatus = await Interrupt(baseAddress, client);
                    Console.WriteLine(interruptStatus);
                    string uri = baseAddress + "/sdapi/v1/txt2img";

                    var requestMsg = new HttpRequestMessage();
                    var json = JsonConvert.SerializeObject(payload);

                    // requestMsg.Content = new StringContent(json);

                    // requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    Console.WriteLine("test");
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") ;

                    var response = client.PostAsync("/sdapi/v1/txt2img", content);

                    while (!response.IsCompleted)
                    {
                        Console.WriteLine("still in progress ...");

                        string progressUri = baseAddress + "/sdapi/v1/progress";

                        var progressResponse = await client.GetAsync(progressUri);
                        progressResponse.EnsureSuccessStatusCode();

                        string responseContent = await progressResponse.Content.ReadAsStringAsync();
                        // Assuming the API returns a JSON response with a status field, parse it
                        // and check if the status is "completed" or "error."
                        var status = JsonConvert.DeserializeObject<ProgressStatus>(responseContent);
                        // string status = ParseStatusFromApiResponse(responseContent);

                        Console.WriteLine("Progress: " + status.Progress);
                        Console.WriteLine("estimated eta: " + status.Eta);
                        Console.WriteLine("Job: " + status.stateObject.Job);
                        // if (status == "completed")
                        // {
                        //     // Image generation completed successfully
                        //     string imageUrl = ParseImageUrlFromApiResponse(responseContent);
                        //     Console.WriteLine($"Image generated successfully! Image URL: {imageUrl}");
                        //     break;
                        // }
                        // else if (status == "error")
                        // {
                        //     // Image generation encountered an error
                        //     string errorMessage = ParseErrorMessageFromApiResponse(responseContent);
                        //     Console.WriteLine($"Error occurred during image generation: {errorMessage}");
                        //     break;
                        // }

                        await Task.Delay(2000);
                    }

                    Console.WriteLine("Image generation finished ...");

                    var result = await response.Result.Content.ReadAsStringAsync();

                    Console.WriteLine(result);

                    var resultObject = JsonConvert.DeserializeObject<ResponseObject>(result);

                    // string jobId = await InitiateImageGenerationAsync();
                    // Console.WriteLine($"Image generation started. Job ID: {jobId}");

                    // await PollImageGenerationStatusAsync(jobId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

            }
        }

        public static async Task<string> Auto1111_T2I(string address, Payload payload)
        {
            using (HttpClient client = new())
            {
                Console.WriteLine("... Attempting to send POST request /sdapi/v1/txt2img");
                string endpoint = address + "/sdapi/v1/txt2img";

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

                var json = JsonConvert.SerializeObject(payload);
                request.Content = new StringContent(json);

                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(request);
                Console.WriteLine(response.StatusCode);
                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("... results coming in ....");


                return result;
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
                    // var responseObject = JsonConvert.DeserializeObject<ResponseArtefact>(result);
                    // Console.WriteLine(responseObject.finishReason);
                    // Console.WriteLine(responseObject.seed);
                    // if (responseObject == null) throw new Exception("API request failed. ResponseObject is null");
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

    // public class ResponseArtefact
    // {
    //     public List<ResponseObject>? artifacts { get; set; }
    // }

    // public class ResponseObject
    // {
    //     [JsonProperty("base64")]
    //     public string? Base64 { get; set; }
    //     [JsonProperty("finishReason")]
    //     public string? FinishReason { get; set; }
    //     [JsonProperty("seed")]
    //     public Int64? Seed { get; set; }
    // }

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
