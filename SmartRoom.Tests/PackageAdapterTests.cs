using NUnit.Framework;
using SmartRoom.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.UITest;

namespace SmartRoom.Tests
{
    [TestFixture(Platform.Android)]
    public class PackageAdapterTests
    {
        IApp app;
        Platform platform;

        public PackageAdapterTests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void Init()
        {
            PackageAdapter.AnalogPinsOffset = 14U;
        }

        private byte[] CreatePkg(byte header, string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            var output = new byte[bytes.Length + 2];
            output[0] = header;
            bytes.CopyTo(output, 1);
            output[output.Length - 1] = 0b00000011; //End of text char
            return output;
        }

        [TestCase("0", false, 128, new byte[2] { 0b11000000, 128 })]
        [TestCase("0", true, 98, new byte[2] { 0b11100000, 98 })]
        [TestCase("6", false, 56, new byte[2] { 0b11000110, 56 })]
        [TestCase("6", true, 12, new byte[2] { 0b11100110, 12 })]
        [TestCase("006", false, 56, new byte[2] { 0b11000110, 56 })]
        [TestCase("006", true, 12, new byte[2] { 0b11100110, 12 })]
        [TestCase("31", false, 0, new byte[2] { 0b11011111, 0 })]
        [TestCase("31", true, 255, new byte[2] { 0b11111111, 255 })]
        [TestCase("A0", false, 9, new byte[2] { 0b10001110, 9 })]
        [TestCase("a0", true, 78, new byte[2] { 0b10101110, 78 })]
        [TestCase("A5", false, 90, new byte[2] { 0b10010011, 90 })]
        [TestCase("a5", true, 66, new byte[2] { 0b10110011, 66 })]
        [TestCase("D10", false, 1, new byte[2] { 0b10001010, 1 })]
        [TestCase("d10", true, 32, new byte[2] { 0b10101010, 32 })]
        [TestCase("D000010", false, 1, new byte[2] { 0b10001010, 1 })]
        [TestCase("d000010", true, 32, new byte[2] { 0b10101010, 32 })]
        public void CreateSetPackageCorrect(string pin, bool fade, byte value, byte[] expected)
        {
            var result = PackageAdapter.CreateSetPackage(pin, fade, value);

            Assert.AreEqual(expected, result);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void CreateSetPackageNullPin(string pin)
        {
            Assert.Throws<ArgumentNullException>(() => PackageAdapter.CreateSetPackage(pin, false, 255));
        }

        [TestCase("c1")]
        [TestCase("-9")]
        [TestCase("D-1")]
        [TestCase("0.5")]
        [TestCase("0,5")]
        [TestCase("D")]
        [TestCase("a")]
        public void CreateSetPackageWrongPin(string pin)
        {
            Assert.Throws<ArgumentException>(() => PackageAdapter.CreateSetPackage(pin, false, 255));
        }

        [TestCase("32")]
        [TestCase("D40")]
        [TestCase("a33")]
        public void CreateSetPackageOverflowPin(string pin)
        {
            Assert.Throws<OverflowException>(() => PackageAdapter.CreateSetPackage(pin, false, 255));
        }

        [TestCase("0", false, 0b00100000)]
        [TestCase("0", true, 0b01000000)]
        [TestCase("d0", false, 0b00000000)]
        [TestCase("A0", false, 0b00001110)]
        [TestCase("10", false, 0b00101010)]
        [TestCase("010", false, 0b00101010)]
        [TestCase("10", true, 0b01001010)]
        [TestCase("0010", true, 0b01001010)]
        [TestCase("d10", false, 0b00001010)]
        [TestCase("d00010", false, 0b00001010)]
        [TestCase("A5", false, 0b00010011)]
        [TestCase("A05", false, 0b00010011)]
        public void CreateGetPackageCorrect(string pinId, bool isId, byte expected)
        {
            var result = PackageAdapter.CreateGetPackage(pinId, isId);

            Assert.AreEqual(expected, result);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void CreateGetPackageNullPin(string pin)
        {
            Assert.Throws<ArgumentNullException>(() => PackageAdapter.CreateGetPackage(pin));
        }

        [TestCase("c1", false)]
        [TestCase("-9", false)]
        [TestCase("D-1", false)]
        [TestCase("0.5", false)]
        [TestCase("0,5", false)]
        [TestCase("D", false)]
        [TestCase("a", false)]
        [TestCase("a1", true)]
        [TestCase("D10", true)]
        public void CreateGetPackageWrongPin(string pin, bool isId)
        {
            Assert.Throws<ArgumentException>(() => PackageAdapter.CreateGetPackage(pin, isId));
        }

        [TestCase("32", false)]
        [TestCase("37", true)]
        [TestCase("D40", false)]
        [TestCase("a33", false)]
        public void CreateGetPackageOverflowPin(string pin, bool isId)
        {
            Assert.Throws<OverflowException>(() => PackageAdapter.CreateGetPackage(pin, isId));
        }

        [TestCase(0b00010000, 0b10111010, "A2", false, 186)]
        [TestCase(0b00000010, 0b11101001, "D2", false, 233)]
        [TestCase(0b00101001, 0b00000000, "9", false, 0)]
        [TestCase(0b01000011, 0b11111111, "3", true, 255)]
        public void DeserializePackagesSingleValue(byte pkg, byte value, string expectedPin, bool expectedIsId, byte expectedValue)
        {
            var data = new byte[2] { pkg, value };
            var expected = new Models.PackageValueModel()
            {
                IsID = expectedIsId,
                PinId = expectedPin,
                Value = expectedValue
            };

            var result = PackageAdapter.DeserializePackages(data);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0] is Models.PackageValueModel);
            Assert.AreEqual(expected, result[0] as Models.PackageValueModel);
        }

        [Test]
        public void DeserializePackagesMultipleValue()
        {
            var data = new byte[8] { 
                0b00010000, 0b10111010, //A2 186
                0b00000010, 0b11101001, //D2 233
                0b00101001, 0b00000000, //9 0
                0b01000011, 0b11111111 //ID3 255
            };
            var expected = new List<Models.PackageValueModel>()
            {
                new Models.PackageValueModel("A2", false, 186),
                new Models.PackageValueModel("D2", false, 233),
                new Models.PackageValueModel("9", false, 0),
                new Models.PackageValueModel("3", true, 255)
            };

            var result = PackageAdapter.DeserializePackages(data);

            Assert.IsTrue(result.Count == expected.Count);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.IsTrue(result[i] is Models.PackageValueModel);
                Assert.AreEqual(expected[i], result[i] as Models.PackageValueModel);
            }
        }

        [TestCase(0b10010000, "Short", "A2", false)]
        [TestCase(0b10000010, "Longer text", "D2", false)]
        [TestCase(0b10101001, "\n", "9", false)]
        [TestCase(0b11000011, "Test", "3", true)]
        public void DeserializePackagesSingleText(byte pkg, string expectedText, string expectedPin, bool expectedIsId)
        {
            var data = CreatePkg(pkg, expectedText);
            var expected = new Models.PackageTextModel()
            {
                IsID = expectedIsId,
                PinId = expectedPin,
                Text = expectedText
            };

            var result = PackageAdapter.DeserializePackages(data);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0] is Models.PackageTextModel);
            Assert.AreEqual(expected, result[0] as Models.PackageTextModel);
        }

        [Test]
        public void DeserializePackagesMultipleText()
        {
            var pkg1 = CreatePkg(0b10010000, "Short");
            var pkg2 = CreatePkg(0b10000010, "Longer text");
            var pkg3 = CreatePkg(0b10101001, "\n");
            var pkg4 = CreatePkg(0b11000011, "Test");
            var data = new byte[pkg1.Length + pkg2.Length + pkg3.Length + pkg4.Length];
            pkg1.CopyTo(data, 0);
            pkg2.CopyTo(data, pkg1.Length);
            pkg3.CopyTo(data, pkg1.Length + pkg2.Length);
            pkg4.CopyTo(data, pkg1.Length + pkg2.Length + pkg3.Length);
            var expected = new List<Models.PackageTextModel>()
            {
                new Models.PackageTextModel("A2", false, "Short"),
                new Models.PackageTextModel("D2", false, "Longer text"),
                new Models.PackageTextModel("9", false, "\n"),
                new Models.PackageTextModel("3", true, "Test")
            };

            var result = PackageAdapter.DeserializePackages(data);

            Assert.IsTrue(result.Count == expected.Count);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.IsTrue(result[i] is Models.PackageTextModel);
                Assert.AreEqual(expected[i], result[i] as Models.PackageTextModel);
            }
        }

        [Test]
        public void DeserializePackagesMixed()
        {
            var pkg1 = CreatePkg(0b10010000, "Short");
            var pkg2 = new byte[2] { 0b00000010, 0b11101001 };
            var pkg3 = CreatePkg(0b10101001, "\n");
            var pkg4 = new byte[2] { 0b01000011, 0b11111111 };
            var data = new byte[pkg1.Length + pkg2.Length + pkg3.Length + pkg4.Length];
            pkg1.CopyTo(data, 0);
            pkg2.CopyTo(data, pkg1.Length);
            pkg3.CopyTo(data, pkg1.Length + pkg2.Length);
            pkg4.CopyTo(data, pkg1.Length + pkg2.Length + pkg3.Length);
            var expected = new List<Models.PackageModel>()
            {
                new Models.PackageTextModel("A2", false, "Short"),
                new Models.PackageValueModel("D2", false, 233),
                new Models.PackageTextModel("9", false, "\n"),
                new Models.PackageValueModel("3", true, 255)
            };

            var result = PackageAdapter.DeserializePackages(data);

            Assert.IsTrue(result.Count == expected.Count);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }
    }
}
