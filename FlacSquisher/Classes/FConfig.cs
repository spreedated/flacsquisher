using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;

namespace FlacSquisher
{
    public class FSConfigJObject
    {
        [JsonProperty("linputDirec")]
        public string LastInputDirectory { get; set; }
        [JsonProperty("loutputDirec")]
        public string LastOutputDirectory { get; set; }
        [JsonProperty("lencoder")]
        public Encode.AudioEncoders? LastEncoder { get; set; }
        [JsonProperty("mp3settings")]
        public MP3 MP3Settings { get; set; }
        [JsonProperty("configCreated")]
        public DateTime ConfigCreated { get; set; }
        [JsonProperty("configModified")]
        public DateTime ConfigModified { get; set; }

        public class MP3
        {
            [JsonProperty("lMP3Bitrate")]
            public Encode.MP3.Bitrates? LastMP3Bitrate { get; set; }
        }
    }

    public static class FSConfig
    {
        public static FSConfigJObject Config { get; set; } = null;
    }

    public partial class FConfig
    {
        private static readonly string jsonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "flacConfig.json");
        public FConfig()
        {
            if (File.Exists(jsonPath))
            {
                FConfig.Load();
            }
            else
            {
                FConfig.Save(); //Save blank/null values
            }
        }
        public static void Load()
        {
            string json = null;
            using (FileStream stream = File.Open(jsonPath, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    json += reader.ReadToEnd();
                }
            }
            FSConfig.Config = JsonConvert.DeserializeObject<FSConfigJObject>(json);
        }
        public static void Reload()
        {
            FSConfig.Config = null;
            FConfig.Load();
        }
        public static void Save()
        {
            string jsonOut = JsonConvert.SerializeObject(FSConfig.Config, Formatting.Indented);
            using (FileStream stream = File.Open(jsonPath, FileMode.OpenOrCreate))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(jsonOut);
                }
            }
        }
    }
}
