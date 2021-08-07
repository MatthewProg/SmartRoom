using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{
    public class ColorModel : INotifyPropertyChanged
    {
        private ColorTypes.RGB _rgb;

        public ColorModel() : this(0, 0, 0)
        {
            ;
        }

        public ColorModel(byte r, byte g, byte b)
        {
            _rgb = new ColorTypes.RGB(r, g, b);
        }
        public string GetHex() => string.Format("#{0:X2}{1:X2}{2:X2}", _rgb.R, _rgb.G, _rgb.B);
        public ColorTypes.RGB GetRGB() => _rgb;
        public ColorTypes.HSL GetHSL()
        {
            var col = System.Drawing.Color.FromArgb(_rgb.R, _rgb.G, _rgb.B);
            var output = new ColorTypes.HSL();
            output.H = (float)Math.Round(col.GetHue());
            output.S = (float)Math.Round(col.GetSaturation(), 2);
            output.L = (float)Math.Round(col.GetBrightness(), 2);
            return output;
        }
        public ColorTypes.HSV GetHSV()
        {
            var col = System.Drawing.Color.FromArgb(_rgb.R, _rgb.G, _rgb.B);
            var s = col.GetSaturation();
            var l = col.GetBrightness();

            var output = new ColorTypes.HSV();
            output.H = (float)Math.Round(col.GetHue());
            output.V = (float)Math.Round(l + (s * Math.Min(l, 1 - l)), 2);
            output.S = (float)Math.Round((output.V == 0F) ? 0F : (2F - 2F * l / output.V), 2);
            return output;
        }

        public void FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) throw new ArgumentNullException("hex");
            if (hex[0] != '#') hex = "#" + hex;
            if (hex.Length != 4 && hex.Length != 5 && hex.Length != 7 && hex.Length != 9) throw new ArgumentException("Wrong lenght of arg", "hex");
            var col = System.Drawing.ColorTranslator.FromHtml(hex);
            _rgb.R = col.R;
            _rgb.G = col.G;
            _rgb.B = col.B;
            OnPropertyChanged("");
        }
        public void FromRGB(byte r, byte g, byte b) => FromRGB(new ColorTypes.RGB(r, g, b));
        public void FromRGB(ColorTypes.RGB rgb)
        {
            _rgb.R = rgb.R;
            _rgb.G = rgb.G;
            _rgb.B = rgb.B;
            OnPropertyChanged("");
        }
        public void FromHSL(float h, float s, float l) => FromHSL(new ColorTypes.HSL(h, s, l));
        public void FromHSL(ColorTypes.HSL hsl)
        {
            if (hsl.S == 0)
            {
                _rgb.R = _rgb.G = _rgb.B = (byte)Math.Round(hsl.L * 255);
            }
            else
            {
                float C = (1F - Math.Abs(2F * hsl.L - 1F)) * hsl.S;
                float X = C * (1F - Math.Abs(hsl.H / 60F % 2F - 1F));
                float m = hsl.L - C / 2F;

                float r, g, b;
                if (hsl.H >= 0F && hsl.H < 60F) { r = C; g = X; b = 0F; }
                else if (hsl.H >= 60F && hsl.H < 120F) { r = X; g = C; b = 0F; }
                else if (hsl.H >= 120F && hsl.H < 180F) { r = 0F; g = C; b = X; }
                else if (hsl.H >= 180F && hsl.H < 240F) { r = 0F; g = X; b = C; }
                else if (hsl.H >= 240F && hsl.H < 300F) { r = X; g = 0F; b = C; }
                else { r = C; g = 0F; b = X; }

                _rgb.R = (byte)Math.Round((r + m) * 255);
                _rgb.G = (byte)Math.Round((g + m) * 255);
                _rgb.B = (byte)Math.Round((b + m) * 255);
            }
            OnPropertyChanged("");
        }
        public void FromHSV(float h, float s, float v) => FromHSV(new ColorTypes.HSV(h, s, v));
        public void FromHSV(ColorTypes.HSV hsv)
        {
            float C = hsv.S * hsv.V;
            float X = C * (1F - Math.Abs((hsv.H / 60F % 2F) - 1F));
            float m = hsv.V - C;
            float r, g, b;
            if (hsv.H >= 0F && hsv.H < 60F) { r = C; g = X; b = 0F; }
            else if (hsv.H >= 60F && hsv.H < 120F) { r = X; g = C; b = 0F; }
            else if (hsv.H >= 120F && hsv.H < 180F) { r = 0F; g = C; b = X; }
            else if (hsv.H >= 180F && hsv.H < 240F) { r = 0F; g = X; b = C; }
            else if (hsv.H >= 240F && hsv.H < 300F) { r = X; g = 0F; b = C; }
            else { r = C; g = 0F; b = X; }

            _rgb.R = (byte)Math.Round((r + m) * 255);
            _rgb.G = (byte)Math.Round((g + m) * 255);
            _rgb.B = (byte)Math.Round((b + m) * 255);
            OnPropertyChanged("");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}