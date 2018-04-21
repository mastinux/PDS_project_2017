using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;
using System.Windows.Media.Imaging;

namespace PDS_project_2017.Core
{
    public class User
    {
        private BitmapImage _image;
        // last time user responded to udp request
        private DateTime _lastUpTime;

        public string IPAddress { get; set; }

        public string Name { get; set; }

        [JsonConverter(typeof(BitmapImageJsonConverter))]
        public BitmapImage Image
        {
            get => _image;
            set => _image = value;
        }

        [ScriptIgnore]
        public DateTime LastUpTime
        {
            get => _lastUpTime;
            set => _lastUpTime = value;
        }
    }
}
