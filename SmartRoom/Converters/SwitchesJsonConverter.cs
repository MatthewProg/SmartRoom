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
using System.Linq;
using System.Text;

namespace SmartRoom.Converters
{
    public class SwitchesJsonConverter : JsonConverter<IList<Models.SwitchModel>>
    {
        private enum SwitchType
        {
            TOGGLE, SLIDER, COLOR
        };

        public SwitchesJsonConverter()
        {
            ;
        }

        public override void WriteJson(JsonWriter writer, IList<SwitchModel> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                JToken token = null;
                SwitchType switchType = SwitchType.TOGGLE;

                if (item is Models.ToggleSwitchModel)
                {
                    token = JToken.FromObject(item as Models.ToggleSwitchModel);
                    switchType = SwitchType.TOGGLE;
                }
                else if (item is Models.SliderSwitchModel)
                {
                    token = JToken.FromObject(item as Models.SliderSwitchModel);
                    switchType = SwitchType.SLIDER;
                }
                else if (item is Models.ColorSwitchModel)
                {
                    token = JToken.FromObject(item as Models.ColorSwitchModel);
                    switchType = SwitchType.COLOR;
                }
                else
                    continue;

                var obj = JObject.FromObject(new { st = switchType, o = token });
                obj.WriteTo(writer);
            }
            writer.WriteEndArray();
        }

        public override IList<SwitchModel> ReadJson(JsonReader reader, Type objectType, IList<SwitchModel> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var output = Activator.CreateInstance(objectType) as IList<SwitchModel>;
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);
                JArray arr = (JArray)token;

                foreach(var obj in arr)
                {
                    var st = (SwitchType)obj["st"].Value<int>();
                    SwitchModel sm = null;

                    if (st == SwitchType.TOGGLE)
                        sm = obj["o"].ToObject<ToggleSwitchModel>();
                    else if (st == SwitchType.SLIDER)
                        sm = obj["o"].ToObject<SliderSwitchModel>();
                    else if (st == SwitchType.COLOR)
                        sm = obj["o"].ToObject<ColorSwitchModel>();

                    output.Add(sm);
                }
            }
            return output;
        }
    }
}