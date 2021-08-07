using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Models
{ 
    public class ColorModel
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
        public string GetHex() => string.Format("{0:X2}{1:X2}{2:X2}", _rgb.R, _rgb.G, _rgb.B);
        public ColorTypes.HSL GetHSL()
        {
            var col = System.Drawing.Color.FromArgb(_rgb.R, _rgb.G, _rgb.B);
            var output = new ColorTypes.HSL();
            output.H = col.GetHue();
            output.S = col.GetSaturation();
            output.L = col.GetBrightness();
            return output;
        }
        public ColorTypes.HSV GetHSV()
        {
            var col = System.Drawing.Color.FromArgb(_rgb.R, _rgb.G, _rgb.B);
            var s = col.GetSaturation();
            var l = col.GetBrightness();

            var output = new ColorTypes.HSV();
            output.H = col.GetHue();
            output.V = l + (s * Math.Min(l, 1 - l));
            output.S = (output.V == 0F) ? 0F : (2F - 2F * l / output.V);
            return output;
        }

        public void FromHex(string hex)
        {
            var col = System.Drawing.ColorTranslator.FromHtml(hex);
            _rgb.R = col.R;
            _rgb.G = col.G;
            _rgb.B = col.B;
        }
        public void FromRGB(byte r, byte g, byte b) => FromRGB(new ColorTypes.RGB(r, g, b));
        public void FromRGB(ColorTypes.RGB rgb)
        {
            _rgb.R = rgb.R;
            _rgb.G = rgb.G;
            _rgb.B = rgb.B;
        }
        public void FromHSL(float h, float s, float l) => FromHSL(new ColorTypes.HSL(h, s, l));
        public void FromHSL(ColorTypes.HSL hsl)
        {
            if (hsl.S == 0)
            {
                _rgb.R = _rgb.G = _rgb.B = (byte)(hsl.L * 255);
            }
            else
            {
                float hue2rgb(float p, float q, float t)
                {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1 / 6) return p + (q - p) * 6 * t;
                    if (t < 1 / 2) return q;
                    if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
                    return p;
                }

                var q = hsl.L < 0.5 ? hsl.L * (1 + hsl.S) : hsl.L + hsl.S - hsl.L * hsl.S;
                var p = 2 * hsl.L - q;
                _rgb.R = (byte)Math.Round(hue2rgb(p, q, hsl.H + 1 / 3) * 255);
                _rgb.G = (byte)Math.Round(hue2rgb(p, q, hsl.H) * 255);
                _rgb.B = (byte)Math.Round(hue2rgb(p, q, hsl.H - 1 / 3) * 255);
            }
        }
        public void FromHSV(float h, float s, float v) => FromHSV(new ColorTypes.HSV(h, s, v));
        public void FromHSV(ColorTypes.HSV hsv)
        {
            float C = hsv.S * hsv.V;
            float X = C * (1F - Math.Abs(((hsv.H / 60F) % 2F) - 1F));
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
        }
    }
}