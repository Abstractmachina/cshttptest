using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MyApp
{

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

//     public System.Net.Mime.MediaTypeNames.Image Base64ToImage(string base64String)
//  {
//     // Convert base 64 string to byte[]
//     byte[] imageBytes = Convert.FromBase64String(base64String);
//     // Convert byte[] to Image
//     using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
//     {
//         Image image = Image.FromStream(ms, true);
//         return image;
//     }
//  }
}