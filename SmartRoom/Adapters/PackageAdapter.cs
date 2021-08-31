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

namespace SmartRoom.Adapters
{
    public static class PackageAdapter
    {
        public static UInt32 AnalogPinsOffset { get; set; } = 14U;

        public static byte[] CreateSetPackage(string pin, bool fade, byte value)
        {
            byte header = (byte)(fade ? 0b10100000 : 0b10000000);

            if (string.IsNullOrWhiteSpace(pin)) 
                throw new ArgumentNullException("pin", "Pin cannot be null or whitespace");
            pin = pin.Trim().ToUpper();

            //Set destination
            byte pinByte = 0;
            if(pin[0] == 'A' || pin[0] == 'D')
            {
                if (pin.Length == 1) 
                    throw new ArgumentException("Pin number is incorrect", "pin");
                if (byte.TryParse(pin.Substring(1), out pinByte) == false) 
                    throw new ArgumentException("Pin number is incorrect or too big", "pin");

                if (pin[0] == 'A') pinByte += (byte)AnalogPinsOffset;
            }
            else
            {
                if (byte.TryParse(pin, out pinByte) == false) 
                    throw new ArgumentException("Pin number is incorrect or too big", "pin");
                header |= 0b01000000;
            }

            //Set pin
            if (pinByte > 0b00011111)
                throw new OverflowException("Pin number is too big");
            header += pinByte;

            return new byte[2] { header, value };
        }

        public static byte CreateGetPackage(string pinId, bool isId = false)
        {
            byte header = (byte)(isId ? 0b01000000 : 0b00000000);

            if (string.IsNullOrWhiteSpace(pinId))
                throw new ArgumentNullException("pinId", "Pin/ID cannot be null or whitespace");
            pinId = pinId.Trim().ToUpper();

            //Set destination
            byte pinByte = 0;
            if (isId == false)
            {
                if (pinId[0] == 'A' || pinId[0] == 'D')
                {
                    if (pinId.Length == 1)
                        throw new ArgumentException("Pin/ID number is incorrect", "pinId");
                    if (byte.TryParse(pinId.Substring(1), out pinByte) == false)
                        throw new ArgumentException("Pin/ID number is incorrect or too big", "pinId");

                    if (pinId[0] == 'A') pinByte += (byte)AnalogPinsOffset;
                }
                else
                {
                    if (byte.TryParse(pinId, out pinByte) == false)
                        throw new ArgumentException("Pin/ID number is incorrect or too big", "pinId");
                    header |= 0b00100000;
                }
            }
            else
            {
                if (byte.TryParse(pinId, out pinByte) == false)
                    throw new ArgumentException("Pin/ID number is incorrect or too big", "pinId");
            }

            //Set pin
            if (pinByte > 0b00011111)
                throw new OverflowException("Pin number is too big");
            header += pinByte;

            return header;
        }

        public static List<Models.PackageModel> DeserializePackages(byte[] data)
        {
            var output = new List<Models.PackageModel>();

            for (int index = 0; index < data.Length; index++)
            {
                //Identify Pin/ID
                string pinId = null;
                bool isId = false;

                byte source = (byte)((data[index] & 0b01100000) >> 5);
                byte pin = (byte)(data[index] & 0b00011111);
                if (source == 0) //Arduino Pin
                {
                    if (pin >= AnalogPinsOffset) pinId = "A" + (pin - AnalogPinsOffset).ToString();
                    else pinId = "D" + pin.ToString();
                    isId = false;
                }
                else //TLC or ID
                {
                    pinId = pin.ToString();
                    isId = (pin == 3);
                }

                //Get value & add to list
                if((data[index] & 0b10000000) >> 7  == 0) //Value
                {
                    index++;
                    output.Add(new Models.PackageValueModel(pinId, isId, data[index]));
                }
                else //Text
                {
                    index++;
                    string text = "";
                    while(data[index] != 0b00000011)
                    {
                        text += (char)data[index];
                        index++;
                    }
                    output.Add(new Models.PackageTextModel(pinId, isId, text));
                }
            }

            return output;
        }
    }
}