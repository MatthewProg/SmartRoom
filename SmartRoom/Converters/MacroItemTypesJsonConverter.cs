using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartRoom.Extensions;
using System;
using System.Reflection;

namespace SmartRoom.Converters
{
    public class MacroItemTypesJsonConverter : JsonConverter<Extensions.UniqueDictionary<Type, uint>>
    {
        public MacroItemTypesJsonConverter()
        {
            ;
        }

        public override void WriteJson(JsonWriter writer, UniqueDictionary<Type, uint> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach(var item in value.Values)
            {
                JProperty id = new JProperty("I", item);
                Type t;
                if (value.TryGetKey(item, out t) == false)
                    continue;
                JProperty type = new JProperty("T", t.FullName);

                JObject o = new JObject(id, type);
                o.WriteTo(writer);
            }
            writer.WriteEndArray();
        }

        public override UniqueDictionary<Type, uint> ReadJson(JsonReader reader, Type objectType, UniqueDictionary<Type, uint> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var output = new UniqueDictionary<Type, uint>();
            if (reader.TokenType != JsonToken.Null)
            {
                JArray arr = (JArray)JToken.Load(reader);
                foreach (var o in arr)
                {
                    uint id = o.Value<uint>("I");
                    var qn = Assembly.CreateQualifiedName("SmartRoom", o.Value<string>("T"));
                    Type t = Type.GetType(qn);
                    output.Add(t, id);
                }
            }
            return output;
        }
    }
}