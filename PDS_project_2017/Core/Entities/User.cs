using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;
using System.Windows.Media.Imaging;

namespace PDS_project_2017.Core
{
    public class User
    {
        private String _id;
        private String _name;
        private BitmapImage _image;
        private DateTime _lastUpTime;

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

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
