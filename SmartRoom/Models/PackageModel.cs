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
    public abstract class PackageModel
    {
        public PackageModel() : this(null, false) { ; }
        public PackageModel(string pinId, bool isID)
        {
            PinId = pinId;
            IsID = isID;
        }

        public string PinId { get; set; }
        public bool IsID { get; set; }
    }

    public class PackageValueModel : PackageModel, IEquatable<PackageValueModel>
    {
        public PackageValueModel() : base() { ; }
        public PackageValueModel(string pinId, bool isID, byte value) : base(pinId, isID)
        {
            Value = value;
        }

        public byte Value { get; set; }

        public bool Equals(PackageValueModel other)
        {
            return (this.IsID == other.IsID &&
                    this.PinId == other.PinId &&
                    this.Value == other.Value);
        }
    }

    public class PackageTextModel : PackageModel, IEquatable<PackageTextModel>
    {
        public PackageTextModel() : base() { ; }
        public PackageTextModel(string pinId, bool isID, string text) : base(pinId, isID)
        {
            Text = text;
        }

        public string Text { get; set; }

        public bool Equals(PackageTextModel other)
        {
            return (this.IsID == other.IsID &&
               this.PinId == other.PinId &&
               this.Text == other.Text);
        }
    }
}