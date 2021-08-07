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

namespace SmartRoom.Models.ColorTypes
{
    public struct RGB
    {
        public RGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
    }

    public struct HSV
    {
        float _h;
        float _s;
        float _v;

        public HSV(float h, float s, float v) : this()
        {
            H = h;
            S = s;
            V = v;
        }

        public float H { get => _h; set => _h = Math.Max(0F, Math.Min(value, 360F)); }
        public float S { get => _s; set => _s = Math.Max(0F, Math.Min(value, 1F)); }
        public float V { get => _v; set => _v = Math.Max(0F, Math.Min(value, 1F)); }
    }

    public struct HSL
    {
        float _h;
        float _s;
        float _l;

        public HSL(float h, float s, float l) : this()
        {
            H = h;
            S = s;
            L = l;
        }

        public float H { get => _h; set => _h = Math.Max(0F, Math.Min(value, 360F)); }
        public float S { get => _s; set => _s = Math.Max(0F, Math.Min(value, 1F)); }
        public float L { get => _l; set => _l = Math.Max(0F, Math.Min(value, 1F)); }
    }
}