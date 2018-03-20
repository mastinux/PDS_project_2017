using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace PDS_project_2017.Core
{
    class BitmapImageJsonConverter : JsonConverter
    {
        // TODO comment

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Image);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var base64 = (string)reader.Value;

            if (base64 == null)
                return null;

            return Base64StringToBitmap(base64);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bitmapImage = (BitmapImage)value;

            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            writer.WriteValue(data);
        }

        public static BitmapImage Base64StringToBitmap(string base64String)
        {
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(byteBuffer);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
