using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class BeaconPacket
    {
        /*
        private ushort senderLength;
        public ushort SenderLength
        {
            get { return senderLength; }
            set { senderLength = value; }
        }*/
        private string sender = "";
        public string Sender
        {
            get { return sender; }
            set { sender = value; }
        }

        private string data;
        public string Data
        {
            get { return data; }
            set { data = value; }
        }
        /*private ushort dataLength;
        public ushort DataLength
        {
            get { return dataLength; }
            set { dataLength = value; }
        }*/

        private byte[] byteData;
        public byte[] ByteData
        {
            get
            {
                if(byteData == null)
                {
                    byteData = ConvertHexStringToByteArray(data);
                }
                return byteData;
            }
            set { byteData = value; }
        }



        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Contains(" "))
                hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }
            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }
    }
}
