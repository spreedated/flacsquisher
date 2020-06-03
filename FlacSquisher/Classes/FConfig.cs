using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics;

namespace FlacSquisher
{
    public partial class FConfig
    {
        public string LastInputDirectory { get; set; }
        public string LastOutputDirectory { get; set; }
        public Encode.AudioEncoders LastEncoder { get; set; }
        public Encode.MP3.Bitrates LastMP3Bitrate { get; set; }
    }

    public partial class FConfig
    {
        private readonly string jsonPath = Path.Combine(Environment.CurrentDirectory, "flacConfig.json");
        public FConfig()
        {
            string json = null;

            if (File.Exists(jsonPath))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                using (FileStream stream = File.Open(jsonPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            json += System.Text.Encoding.UTF8.GetString(new byte[] { Convert.ToByte(reader.Read()) });
                        }
                    }
                }

                stopwatch.Stop();
                Debug.Print("Time 1: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:ffffff"));
                stopwatch.Restart();
                json += File.ReadAllText(jsonPath);
                Debug.Print("Time 2: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:ffffff"));
                stopwatch.Restart();
                using (FileStream stream = File.Open(jsonPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            json += reader.ReadLine();
                        }
                    }
                }
                Debug.Print("Time 3: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:ffffff"));
                stopwatch.Restart();
                using (FileStream stream = File.Open(jsonPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                            json += reader.ReadToEnd();
                    }
                }
                Debug.Print("Time 4: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:ffffff"));
            }
        }
    }
}
