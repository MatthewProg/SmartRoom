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
    public class SensorsJsonConverter : JsonConverter<IList<Models.SensorModel>>
    {
        private enum SensorType
        {
            TEXT, VALUE
        };

        public SensorsJsonConverter()
        {
            ;
        }

        public override void WriteJson(JsonWriter writer, IList<SensorModel> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                JToken token = null;
                SensorType sensorType = SensorType.VALUE;

                if (item is Models.TextSensorModel t)
                {
                    token = JToken.FromObject(t);
                    sensorType = SensorType.TEXT;
                }
                else if (item is Models.ValueSensorModel v)
                {
                    token = JToken.FromObject(v);
                    sensorType = SensorType.VALUE;
                }
                else
                    continue;

                var obj = JObject.FromObject(new { st = sensorType, o = token });
                obj.WriteTo(writer);
            }
            writer.WriteEndArray();
        }

        public override IList<SensorModel> ReadJson(JsonReader reader, Type objectType, IList<SensorModel> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var output = Activator.CreateInstance(objectType) as IList<SensorModel>;
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);
                JArray arr = (JArray)token;

                foreach(var obj in arr)
                {
                    var st = (SensorType)obj["st"].Value<int>();
                    SensorModel sm = null;

                    sm = st switch
                    {
                        SensorType.TEXT => obj["o"].ToObject<TextSensorModel>(),
                        SensorType.VALUE => obj["o"].ToObject<ValueSensorModel>(),
                        _ => null
                    };

                    if (sm == null)
                        continue;

                    output.Add(sm);
                }
            }
            return output;
        }
    }
}