using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

using (HttpClient client = new())
{
    DotNetEnv.Env.Load();
    string apiKey = Environment.GetEnvironmentVariable("API_KEY");


    var newPrompt = new TextToImagePrompt(apiKey);

    var settings = new JsonSerializerSettings{
        Converters = { new T2IJsonConverter() }
     };

    StringContent body = new(JsonConvert.SerializeObject(newPrompt, settings));

    // StringContent body = new(JsonConvert.SerializeObject(newPrompt));

    Console.WriteLine(await body.ReadAsStringAsync());
    return;

    HttpResponseMessage response = await client.GetAsync("https://official-joke-api.appspot.com/random_ten");

    if (response.IsSuccessStatusCode)
    {
        string content = await response.Content.ReadAsStringAsync();

        List<Joke>? jokes = JsonConvert.DeserializeObject<List<Joke>>(content);

        if (jokes == null)
            return;

        foreach (Joke joke in jokes)
        {
            Console.WriteLine(joke.Setup);
            Console.WriteLine(joke.Punchline);
            Console.WriteLine();
        }
    }
}

public class Joke
{
    public string Setup { get; set; }
    public string Punchline { get; set; }
}

public class TextToImagePrompt
{
    public string Key { get; set; }
    public string Prompt { get; set; }
    public string Negative_prompt { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
    public string Samples { get; set; }
    public string Num_inference_steps { get; set; }
    public string Safety_checker { get; set; }
    public string Enhance_prompt { get; set; }
    public string Seed { get; set; }
    public string Guidance_scale { get; set; }
    public string Multi_lingual { get; set; }
    public string Panorama { get; set; }
    public string Self_attention { get; set; }
    public string Upscale { get; set; }
    public string Embedding_model { get; set; }
    public string Webhook { get; set; }
    public string Track_id { get; set; }

    public TextToImagePrompt(string key) {
        Key = key;
        Prompt = "a round, red apple";
    }
    
    public TextToImagePrompt(string key, string prompt, string negative_prompt,
    int width, int height, int samples, int num_inference_steps, bool safety_checker, bool enhance_prompt, int seed, float guidance_scale, bool multi_lingual, bool panorama, bool self_attention, bool upscale, string embeddings_model, string webhook, string track_id)
    {

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
            if (propertyValue == null) continue; 
            jsonObject.Add(propertyName.ToLower(), JToken.FromObject(propertyValue));
        }

        // // Modify the "LastName" value
        // var lastNameProperty = jsonObject.Property("lastname");
        // if (lastNameProperty != null)
        // {
        //     lastNameProperty.Value = lastNameProperty.Value.ToString().ToUpper();
        // }
        Console.WriteLine(jsonObject.HasValues);
        foreach (var v in jsonObject) {
            Console.WriteLine(v);
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