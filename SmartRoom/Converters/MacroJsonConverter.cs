using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartRoom.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Converters
{
    public class MacroJsonConverter : JsonConverter<IList<Models.MacroModel>>
    {
        public MacroJsonConverter()
        {
            ;
        }

        public override void WriteJson(JsonWriter writer, IList<Models.MacroModel> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("M");
            writer.WriteStartArray();
            var conv = new Converters.MacroItemsJsonConverter();
            foreach (var item in value)
            {
                var o = JObject.FromObject(item);
                var arr = JArray.Parse(JsonConvert.SerializeObject(item.Items, conv));
                o.Add("I", arr);
                o.WriteTo(writer);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("T");
            JArray.Parse(JsonConvert.SerializeObject(conv.ItemTypes, new Converters.MacroItemTypesJsonConverter())).WriteTo(writer);
            writer.WriteEndObject();
        }

        public override IList<Models.MacroModel> ReadJson(JsonReader reader, Type objectType, IList<Models.MacroModel> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var output = Activator.CreateInstance(objectType) as IList<Models.MacroModel>;
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);

                var types = new Extensions.UniqueDictionary<Type, uint>();
                types = JsonConvert.DeserializeObject<Extensions.UniqueDictionary<Type, uint>>(token["T"].ToString(), new Converters.MacroItemTypesJsonConverter());
                var conv = new Converters.MacroItemsJsonConverter(types);

                JArray arr = (JArray)token["M"];

                foreach (var obj in arr)
                {
                    var items = obj.SelectToken("I") as JArray;
                    JObject o = (JObject)obj;
                    o.Remove("I");
                    var macro = JsonConvert.DeserializeObject<Models.MacroModel>(o.ToString());
                    macro.Items = JsonConvert.DeserializeObject<ObservableCollection<Interfaces.IMacroItemModel>>(items.ToString(), conv);
                    output.Add(macro);
                }
            }
            return output;
        }
    }
}