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
using System.Text.RegularExpressions;

namespace SmartRoom.Models
{
    public class ColorModel : INotifyPropertyChanged, ICloneable
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
        public int GetAndroid()
        {
            UInt32 col = (255U << 24) + (uint)(_rgb.R << 16) + (uint)(_rgb.G << 8) + (uint)_rgb.B;
            return unchecked((int)col);
        }
        public ColorTypes.HSL GetHSL() //src: http://csharphelper.com/blog/2016/08/convert-between-rgb-and-hls-color-models-in-c/
        {
            var output = new ColorTypes.HSL();

            // Convert RGB to a 0.0 to 1.0 range.
            double double_r = _rgb.R / 255.0;
            double double_g = _rgb.G / 255.0;
            double double_b = _rgb.B / 255.0;

            // Get the maximum and minimum RGB components.
            double max = double_r;
            if (max < double_g) max = double_g;
            if (max < double_b) max = double_b;

            double min = double_r;
            if (min > double_g) min = double_g;
            if (min > double_b) min = double_b;

            double diff = max - min;
            double s = 0;
            double h = 0;
            double l = (max + min) / 2;
            if (Math.Abs(diff) < 0.00001)
            {
                s = 0;
                h = 0;  // H is really undefined.
            }
            else
            {
                if (l <= 0.5) s = diff / (max + min);
                else s = diff / (2 - max - min);

                double r_dist = (max - double_r) / diff;
                double g_dist = (max - double_g) / diff;
                double b_dist = (max - double_b) / diff;

                if (double_r == max) h = b_dist - g_dist;
                else if (double_g == max) h = 2 + r_dist - b_dist;
                else h = 4 + g_dist - r_dist;

                h = h * 60;
                if (h < 0) h += 360;
            }

            output.H = (float)Math.Round(h);
            output.S = (float)Math.Round(s, 2);
            output.L = (float)Math.Round(l, 2);

            return output;
        }
        public ColorTypes.HSV GetHSV() //src: https://www.programmingalgorithms.com/algorithm/rgb-to-hsv/
        {
            var output = new ColorTypes.HSV();

            double delta, min;
            double h = 0, s, v;

            min = Math.Min(Math.Min(_rgb.R, _rgb.G), _rgb.B);
            v = Math.Max(Math.Max(_rgb.R, _rgb.G), _rgb.B);
            delta = v - min;

            if (v == 0.0) s = 0;
            else s = delta / v;

            if (s == 0) 
                h = 0.0;
            else
            {
                if (_rgb.R == v)
                    h = (_rgb.G - _rgb.B) / delta;
                else if (_rgb.G == v)
                    h = 2 + (_rgb.B - _rgb.R) / delta;
                else if (_rgb.B == v)
                    h = 4 + (_rgb.R - _rgb.G) / delta;

                h *= 60;

                if (h < 0.0)
                    h = h + 360;
            }

            output.H = (float)Math.Round(h);
            output.V = (float)Math.Round(v / 255D, 2);
            output.S = (float)Math.Round(s, 2);

            return output;
        }

        public void FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) throw new ArgumentNullException("hex");
            if (hex[0] != '#') hex = "#" + hex;
            if (hex.Length != 4 && hex.Length != 5 && hex.Length != 7 && hex.Length != 9) throw new ArgumentException("Wrong lenght of arg", "hex");

            //Unify
            if (hex.Length == 4) hex = "FF" + hex[1] + hex[1] + hex[2] + hex[2] + hex[3] + hex[3];
            else if (hex.Length == 5) hex = "" + hex[1] + hex[1] + hex[2] + hex[2] + hex[3] + hex[3] + hex[4] + hex[4];
            else if (hex.Length == 7) hex = "FF" + hex.Substring(1);
            else if (hex.Length == 9) hex = hex.Substring(1);

            if (Regex.IsMatch(hex, "^([a-fA-F0-9]{8})$") == false)
                return;

            //Convert
            var col = Int32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            _rgb.B = (byte)(col & 255);
            _rgb.G = (byte)((col >> 8) & 255);
            _rgb.R = (byte)((col >> 16) & 255);
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
        public void FromAndroid(int color)
        {
            UInt32 col = unchecked((uint)color);
            _rgb.R = (byte)((col >> 16) & 255);
            _rgb.G = (byte)((col >> 8) & 255);
            _rgb.B = (byte)(col & 255);
            OnPropertyChanged("");
        }
        public void FromHSL(float h, float s, float l) => FromHSL(new ColorTypes.HSL(h, s, l));
        public void FromHSL(ColorTypes.HSL hsl)
        {
            if (hsl.S == 0)
            {
                _rgb.R = _rgb.G = _rgb.B = (byte)Math.Round(0.001 + hsl.L * 255);
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

                _rgb.R = (byte)Math.Round(0.001 + (r + m) * 255);
                _rgb.G = (byte)Math.Round(0.001 + (g + m) * 255);
                _rgb.B = (byte)Math.Round(0.001 + (b + m) * 255);
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

            _rgb.R = (byte)Math.Round(0.001 + (r + m) * 255);
            _rgb.G = (byte)Math.Round(0.001 + (g + m) * 255);
            _rgb.B = (byte)Math.Round(0.001 + (b + m) * 255);
            OnPropertyChanged("");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public object Clone()
        {
            return new ColorModel(_rgb.R, _rgb.G, _rgb.B);
        }
    }
}