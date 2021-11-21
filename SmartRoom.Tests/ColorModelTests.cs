using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SmartRoom.Models;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using System.Drawing;

namespace SmartRoom.Tests
{
    [TestFixture(Platform.Android)]
    public class ColorModelTests
    {
        IApp app;
        Platform platform;

        public ColorModelTests(Platform platform)
        {
            this.platform = platform;
        }

        //[SetUp]
        //public void BeforeEachTest()
        //{
        //    app = AppInitializer.StartApp(platform);
        //}

        //[Test]
        //public void WelcomeTextIsDisplayed()
        //{
        //    AppResult[] results = app.WaitForElement(c => c.Marked("Welcome to Xamarin.Forms!"));
        //    app.Screenshot("Welcome screen.");

        //    Assert.IsTrue(results.Any());
        //}

        [TestCase(0F, 0F, 0F, 0, 0, 0)]
        [TestCase(360F, 0F, 0.27F, 69, 69, 69)]
        [TestCase(150F, 0F, 0.78F, 199, 199, 199)]
        [TestCase(240F, 0.8F, 0.12F, 6, 6, 55)]
        [TestCase(360F, 0.6F, 0.39F, 159, 40, 40)]
        [TestCase(109F, 0.7F, 0.63F, 119, 227, 95)]
        [TestCase(109F, 0F, 0F, 0, 0, 0)]
        [TestCase(0F, 0F, 1F, 255, 255, 255)]
        [TestCase(109F, 1F, 0.5F, 47, 255, 0)]
        [TestCase(0F, 0.5F, 0.5F, 191, 64, 64)]

        public void FromHSL(float h, float s, float l, byte r, byte g, byte b)
        {
            var expected = new Models.ColorTypes.RGB(r, g, b);
            ColorModel model = new ColorModel();
            
            model.FromHSL(h, s, l);

            var result = model.GetRGB();
            Assert.AreEqual(expected, result, "E: r={0} g={1} b={2}\n  R: r={3} g={4} b={5}", 
                expected.R, expected.G, expected.B, result.R, result.G, result.B);
        }

        [TestCase(0F, 0F, 0F, 0, 0, 0)]
        [TestCase(80F, 1F, 0.5F, 85, 128, 0)]
        [TestCase(300F, 0F, 0.5F, 128, 128, 128)]
        [TestCase(340F, 1F, 1F, 255, 0, 85)]
        [TestCase(80F, 0.8F, 0.56F, 105, 143, 29)]
        [TestCase(120F, 0.8F, 0.56F, 29, 143, 29)]
        [TestCase(209F, 0F, 1F, 255, 255, 255)]
        [TestCase(290F, 1F, 0F, 0, 0, 0)]
        [TestCase(63F, 0.18F, 0.9F, 227, 230, 188)]
        [TestCase(64F, 0.25F, 0.2F, 50, 51, 38)]
        public void FromHSV(float h, float s, float v, byte r, byte g, byte b)
        {
            var expected = new Models.ColorTypes.RGB(r, g, b);
            ColorModel model = new ColorModel();

            model.FromHSV(h, s, v);

            var result = model.GetRGB();
            Assert.AreEqual(expected, result, "E: r={0} g={1} b={2}\n  R: r={3} g={4} b={5}",
                expected.R, expected.G, expected.B, result.R, result.G, result.B);
        }

        [Test]
        public void FromRGB()
        {
            var expected = new Models.ColorTypes.RGB(178, 79, 56);
            ColorModel model = new ColorModel();

            model.FromRGB(178, 79, 56);

            var result = model.GetRGB();
            Assert.AreEqual(expected, result, "E: r={0} g={1} b={2}\n  R: r={3} g={4} b={5}",
                expected.R, expected.G, expected.B, result.R, result.G, result.B);
        }

        [TestCase(-1644826, 230, 230, 230)]
        [TestCase(-16777216, 0, 0, 0)]
        [TestCase(-1, 255, 255, 255)]
        [TestCase(-65281, 255, 0, 255)]
        [TestCase(-16776961, 0, 0, 255)]
        [TestCase(-1410847, 234, 120, 225)]
        public void FromAndroid(int color, byte r, byte g, byte b)
        {
            var expected = new Models.ColorTypes.RGB(r, g, b);
            ColorModel model = new ColorModel();

            model.FromAndroid(color);

            var result = model.GetRGB();
            Assert.AreEqual(expected, result, "E: r={0} g={1} b={2}\n  R: r={3} g={4} b={5}",
                expected.R, expected.G, expected.B, result.R, result.G, result.B);
        }

        [TestCase("#fff", 255, 255, 255)]
        [TestCase("#000", 0, 0, 0)]
        [TestCase("#4C9173", 76, 145, 115)]
        [TestCase("#FFE0FE", 255, 224, 254)]
        [TestCase("#5cb31e", 92, 179, 30)]
        [TestCase("fff", 255, 255, 255)]
        [TestCase("050803", 5, 8, 3)]
        [TestCase("#202257", 32, 34, 87)]
        [TestCase("#ffffffff", 255, 255, 255)]
        [TestCase("#8CFFAF", 140, 255, 175)]
        public void FromHexCorrect(string hex, byte r, byte g, byte b)
        {
            var expected = new Models.ColorTypes.RGB(r, g, b);
            ColorModel model = new ColorModel();

            model.FromHex(hex);

            var result = model.GetRGB();
            Assert.AreEqual(expected, result, "E: r={0} g={1} b={2}\n  R: r={3} g={4} b={5}",
                expected.R, expected.G, expected.B, result.R, result.G, result.B);
        }

        [TestCase("ff")]
        [TestCase("#0")]
        [TestCase("fffff")]
        [TestCase("#000000000000000")]
        public void FromHexExceptionLenght(string hex)
        {
            ColorModel model = new ColorModel();

            Assert.Throws<ArgumentException>(() => model.FromHex(hex));
        }

        [Test]
        public void FromHexExceptionNull()
        {
            ColorModel model = new ColorModel();

            Assert.Throws<ArgumentNullException>(() => model.FromHex(""));
        }

        [TestCase(0F, 0F, 0F, 0, 0, 0)]
        [TestCase(0F, 0F, 0.27F, 69, 69, 69)]
        [TestCase(0F, 0F, 0.78F, 199, 199, 199)]
        [TestCase(240F, 0.8F, 0.12F, 6, 6, 55)]
        [TestCase(0F, 0.6F, 0.39F, 159, 40, 40)]
        [TestCase(109F, 0.7F, 0.63F, 119, 227, 95)]
        [TestCase(0F, 0F, 0F, 0, 0, 0)]
        [TestCase(0F, 0F, 1F, 255, 255, 255)]
        [TestCase(109F, 1F, 0.5F, 47, 255, 0)]
        [TestCase(0F, 0.5F, 0.5F, 191, 64, 64)]
        public void GetHSL(float h, float s, float l, byte r, byte g, byte b)
        {
            var expected = new Models.ColorTypes.HSL(h, s, l);
            ColorModel model = new ColorModel();

            model.FromRGB(r, g, b);

            var result = model.GetHSL();
            Assert.AreEqual(expected, result, "E: h={0} s={1} l={2}\n  R: h={3} s={4} l={5}",
                expected.H, expected.S, expected.L, result.H, result.S, result.L);
        }

        [TestCase(0F, 0F, 0F, 0, 0, 0)]
        [TestCase(80F, 1F, 0.5F, 85, 128, 0)]
        [TestCase(0F, 0F, 0.5F, 128, 128, 128)]
        [TestCase(340F, 1F, 1F, 255, 0, 85)]
        [TestCase(80F, 0.8F, 0.56F, 105, 143, 29)]
        [TestCase(120F, 0.8F, 0.56F, 29, 143, 29)]
        [TestCase(0F, 0F, 1F, 255, 255, 255)]
        [TestCase(0F, 0F, 0F, 0, 0, 0)]
        [TestCase(63F, 0.18F, 0.9F, 227, 229, 188)]
        [TestCase(65F, 0.25F, 0.2F, 50, 51, 38)]
        public void GetHSV(float h, float s, float v, byte r, byte g, byte b)
        {
            var expected = new Models.ColorTypes.HSV(h, s, v);
            ColorModel model = new ColorModel();

            model.FromRGB(r, g, b);

            var result = model.GetHSV();
            Assert.AreEqual(expected, result, "E: h={0} s={1} v={2}\n  R: h={3} s={4} v={5}",
                expected.H, expected.S, expected.V, result.H, result.S, result.V);
        }

        [TestCase(-1644826, 230, 230, 230)]
        [TestCase(-16777216, 0, 0, 0)]
        [TestCase(-1, 255, 255, 255)]
        [TestCase(-65281, 255, 0, 255)]
        [TestCase(-16776961, 0, 0, 255)]
        [TestCase(-1410847, 234, 120, 225)]
        public void GetAndroid(int color, byte r, byte g, byte b)
        {
            var expected = color;
            ColorModel model = new ColorModel();

            model.FromRGB(r, g, b);

            var result = model.GetAndroid();
            Assert.AreEqual(expected, result);
        }

        [TestCase("#FFFFFF", 255, 255, 255)]
        [TestCase("#000000", 0, 0, 0)]
        [TestCase("#4C9173", 76, 145, 115)]
        [TestCase("#FFE0FE", 255, 224, 254)]
        [TestCase("#5CB31E", 92, 179, 30)]
        [TestCase("#ECF765", 236, 247, 101)]
        [TestCase("#050803", 5, 8, 3)]
        [TestCase("#636A82", 99, 106, 130)]
        [TestCase("#B8007A", 184, 0, 122)]
        [TestCase("#8CFFAF", 140, 255, 175)]
        public void GetHex(string hex, byte r, byte g, byte b)
        {
            var expected = hex;
            ColorModel model = new ColorModel();

            model.FromRGB(r, g, b);

            var result = model.GetHex();
            Assert.AreEqual(expected, result);
        }
    }
}
