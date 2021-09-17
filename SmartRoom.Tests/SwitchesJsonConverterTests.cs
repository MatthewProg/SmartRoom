
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.UITest;

namespace SmartRoom.Tests
{
    [TestFixture(Platform.Android)]
    public class SwitchesJsonConverterTests
    {
        IApp app;
        Platform platform;

        public SwitchesJsonConverterTests(Platform platform)
        {
            this.platform = platform;
        }

        [Test]
        public void SerializeMultiple()
        {
            var converter = new Converters.SwitchesJsonConverter();
            var list = new ObservableCollection<Models.SwitchModel>()
            {
                new Models.ToggleSwitchModel()
                {
                    Title = "Hey",
                    Pin = "D10"
                },
                new Models.SliderSwitchModel()
                {
                    Title = "OK",
                    Pin = "20"
                },
                new Models.ColorSwitchModel()
                {
                    Title = "Col",
                    RedPin = "1",
                    GreenPin = "2",
                    BluePin = "3"
                }
            };
            var expected = "[{\"st\":0,\"o\":{\"P\":\"D10\",\"T\":\"Hey\"}},{\"st\":1,\"o\":{\"P\":\"20\",\"T\":\"OK\"}},{\"st\":2,\"o\":{\"R\":\"1\",\"G\":\"2\",\"B\":\"3\",\"T\":\"Col\"}}]";

            var result = JsonConvert.SerializeObject(list, converter);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SerializeSingle()
        {
            var converter = new Converters.SwitchesJsonConverter();
            var list = new ObservableCollection<Models.SwitchModel>()
            {
                new Models.ColorSwitchModel()
                {
                    Title = "Col",
                    RedPin = "1",
                    GreenPin = "2",
                    BluePin = "3"
                }
            };
            var expected = "[{\"st\":2,\"o\":{\"R\":\"1\",\"G\":\"2\",\"B\":\"3\",\"T\":\"Col\"}}]";

            var result = JsonConvert.SerializeObject(list, converter);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SerializeEmpty()
        {
            var converter = new Converters.SwitchesJsonConverter();
            var list = new ObservableCollection<Models.SwitchModel>();
            var expected = "[]";

            var result = JsonConvert.SerializeObject(list, converter);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DeserializeMultiple()
        {
            var converter = new Converters.SwitchesJsonConverter();
            var json = "[{\"st\":0,\"o\":{\"P\":\"D10\",\"T\":\"Hey\"}},{\"st\":1,\"o\":{\"P\":\"20\",\"T\":\"OK\"}},{\"st\":2,\"o\":{\"R\":\"1\",\"G\":\"2\",\"B\":\"3\",\"T\":\"Col\"}}]";
            var expected = new ObservableCollection<Models.SwitchModel>()
            {
                new Models.ToggleSwitchModel()
                {
                    Title = "Hey",
                    Pin = "D10"
                },
                new Models.SliderSwitchModel()
                {
                    Title = "OK",
                    Pin = "20"
                },
                new Models.ColorSwitchModel()
                {
                    Title = "Col",
                    RedPin = "1",
                    GreenPin = "2",
                    BluePin = "3"
                }
            };

            var result = JsonConvert.DeserializeObject<ObservableCollection<Models.SwitchModel>>(json, converter);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DeserializeSingle()
        {
            var converter = new Converters.SwitchesJsonConverter();
            var json = "[{\"st\":2,\"o\":{\"R\":\"1\",\"G\":\"2\",\"B\":\"3\",\"T\":\"Col\"}}]";
            var expected = new ObservableCollection<Models.SwitchModel>()
            {
                new Models.ColorSwitchModel()
                {
                    Title = "Col",
                    RedPin = "1",
                    GreenPin = "2",
                    BluePin = "3"
                }
            };

            var result = JsonConvert.DeserializeObject<ObservableCollection<Models.SwitchModel>>(json, converter);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DeserializeEmpty()
        {
            var converter = new Converters.SwitchesJsonConverter();
            var json = "[]";
            var expected = new ObservableCollection<Models.SwitchModel>();

            var result = JsonConvert.DeserializeObject<ObservableCollection<Models.SwitchModel>>(json, converter);

            Assert.AreEqual(expected, result);
        }
    }
}
