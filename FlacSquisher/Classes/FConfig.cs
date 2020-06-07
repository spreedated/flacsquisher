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
using System.Windows.Forms;
using NAudio.Lame;

namespace FlacSquisher
{
    public class FSConfigJObject
    {
        [JsonProperty("linputDirec")]
        public string LastInputDirectory { get; set; }
        [JsonProperty("loutputDirec")]
        public string LastOutputDirectory { get; set; }
        [JsonProperty("lencoder")]
        public Encode.AudioEncoders LastEncoder { get; set; } = Encode.AudioEncoders.MP3;
        [JsonProperty("mp3settings")]
        public MP3 MP3Settings { get; set; } = new MP3();
        [JsonProperty("configCreated")]
        public DateTime ConfigCreated { get; set; }
        [JsonProperty("configModified")]
        public DateTime ConfigModified { get; set; }
        [JsonProperty("appVersion")]
        public Version Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version;
        [JsonProperty("configVersion")]
        public Version ConfigVersion { get; set; } = FSConfig.ConfigVersion;
        public class MP3
        {
            [JsonProperty("lMP3Bitrate")]
            public Encode.MP3.Bitrates LastMP3Bitrate { get; set; } = Encode.MP3.Bitrates._320;
            [JsonProperty("lMP3Mode")]
            public MPEGMode LastMP3Mode { get; set; } = MPEGMode.JointStereo;
        }
    }

    public static class FSConfig
    {
        public static Version ConfigVersion = new Version(1, 0, 0, 1);
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
                if (FSConfig.Config.Version != Assembly.GetExecutingAssembly().GetName().Version)
                {
                    throw new VersionMismatchException("Config created with older version.");
                }
                if (FSConfig.Config.ConfigVersion != FSConfig.ConfigVersion)
                {
                    throw new VersionMismatchException("Config version mismatch");
                }
            }
            catch (Exception ex)
            {
                File.Delete(jsonPath);
                Log.Error(ex, "[FConfig][Load] Error: ");
                Save(true);
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

public class VersionMismatchException : Exception
{
    public override string Message { get; }
    public VersionMismatchException(string message) { this.Message = message; }
}

