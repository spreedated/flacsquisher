using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;
using System.ComponentModel;
using Serilog;

namespace FlacSquisher
{
    public class FSConfigJObject
    {
        [JsonProperty("linputDirec")]
        public string LastInputDirectory { get; set; }
        [JsonProperty("loutputDirec")]
        public string LastOutputDirectory { get; set; }
        [JsonProperty("lencoder")]
        public Encode.AudioEncoders LastEncoder { get; set; }
        [JsonProperty("mp3settings")]
        public MP3 MP3Settings { get; set; } = new MP3();
        [JsonProperty("configCreated")]
        public DateTime ConfigCreated { get; set; }
        [JsonProperty("configModified")]
        public DateTime ConfigModified { get; set; }
        public class MP3
        {
            [JsonProperty("lMP3Bitrate")]
            public Encode.MP3.Bitrates LastMP3Bitrate { get; set; }
        }
    }

    public static class FSConfig
    {
        public static FSConfigJObject Config { get; set; } = null;
    }

    public partial class FConfig
    {
        private static readonly string jsonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "flacConfig.json");
        public static bool readOnlyFileSystem = false;
        public FConfig()
        {
            if (File.Exists(jsonPath))
            {
                Log.Information("[FConfig][Entry] Config file found, trying to load...");
                FConfig.Load();
            }
            else
            {
                Log.Warning("[FConfig][Entry] Config not file found, trying to create blank...");
                FConfig.Save(true); //Save blank/null values
            }
        }
        public static void Load()
        {
            string json = null;
            using (FileStream stream = File.Open(jsonPath, FileMode.Open))
            {
                using StreamReader reader = new StreamReader(stream);
                json += reader.ReadToEnd();
            }
            try
            {
                FSConfig.Config = JsonConvert.DeserializeObject<FSConfigJObject>(json);
                Log.Information("[FConfig][Load] Config loaded successfully!");
            }
            catch (Exception ex)
            {
                File.Delete(jsonPath);
                Log.Error(ex, "[FConfig][Load] Error: ");
                Load();
            }
        }
        public static void Reload()
        {
            Log.Information("[FConfig][Reload] Reloading...");
            FSConfig.Config = new FSConfigJObject();
            FConfig.Load();
            Log.Information("[FConfig][Reload] Reload complete.");
        }
        public static void Save(bool fresh = false)
        {
            if (readOnlyFileSystem)
            {
                Log.Information("[FConfig][Save] Filesystem marked readonly, no saves.");
                return;
            }
            Log.Information("[FConfig][Save] Saving...");
            if (fresh)
            {
                Log.Information("[FConfig][Save] Brand new config to save");
                FSConfig.Config = new FSConfigJObject
                {
                    ConfigCreated = DateTime.Now
                };
            }
            FSConfig.Config.ConfigModified = DateTime.Now;
            string jsonOut = JsonConvert.SerializeObject(FSConfig.Config, Formatting.Indented);
            try
            {
                using FileStream stream = File.Open(jsonPath, FileMode.OpenOrCreate);
                using StreamWriter writer = new StreamWriter(stream);
                writer.Write(jsonOut);
                Log.Information("[FConfig][Save] Save successfully!");
            }
            catch (Exception ex)
            {
                readOnlyFileSystem = true;
                Log.Error(ex, "[FConfig][Save] Error: ");
            }
        }
    }
}
