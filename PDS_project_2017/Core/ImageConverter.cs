using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS_project_2017.Core
{
    // https://stackoverflow.com/questions/44370046/how-do-i-serialize-object-to-json-using-json-net-which-contains-an-image-propert

    class ImageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Image);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var base64 = (string)reader.Value;

            if (base64 == null)
                return null;

            return Image.FromStream(new MemoryStream(Convert.FromBase64String(base64)));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var image = (Image)value;

            var memoryStream = new MemoryStream();

            image.Save(memoryStream, image.RawFormat);

            byte[] imageBytes = memoryStream.ToArray();

            writer.WriteValue(imageBytes);
        }
    }
}
