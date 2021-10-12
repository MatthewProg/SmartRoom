using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartRoom.Extensions;
using SmartRoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Converters
{
    public class MacroItemsJsonConverter : JsonConverter<IList<Interfaces.IMacroItemModel>>
    {
        public UniqueDictionary<Type, uint> ItemTypes { get; private set; }

        public MacroItemsJsonConverter() : this(new UniqueDictionary<Type, uint>())
        { ; }

        public MacroItemsJsonConverter(UniqueDictionary<Type, uint> itemTypes)
        {
            ItemTypes = itemTypes;
        }

        public override void WriteJson(JsonWriter writer, IList<Interfaces.IMacroItemModel> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                uint itemTypeId = 0;
                if (ItemTypes.TryGetValue(item.GetType(), out itemTypeId) == false)
                {
                    itemTypeId = (uint)ItemTypes.Count;
                    ItemTypes.Add(item.GetType(), itemTypeId);
                }

                JToken token = JToken.Parse(item.MacroSerialize());
                var obj = JObject.FromObject(new { mt = itemTypeId, o = token });
                obj.WriteTo(writer);
            }
            writer.WriteEndArray();
        }

        public override IList<Interfaces.IMacroItemModel> ReadJson(JsonReader reader, Type objectType, IList<Interfaces.IMacroItemModel> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var output = Activator.CreateInstance(objectType) as IList<Interfaces.IMacroItemModel>;
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);
                JArray arr = (JArray)token;

                foreach (var obj in arr)
                {
                    var mt = obj["mt"].Value<uint>();
                    Interfaces.IMacroItemModel model = null;

                    Type t;
                    if (ItemTypes.TryGetKey(mt, out t) == false)
                        continue;

                    model = (Interfaces.IMacroItemModel)Activator.CreateInstance(t);
                    model.MacroDeserialize(obj["o"].ToString());

                    if (model == null) 
                        continue;

                    output.Add(model);
                }
            }
            return output;
        }
    }
}