using HeyRed.Mime;
using NAudio.Flac;
using NAudio.Lame;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FlacSquisher
{
    public class Encode
    {
        public enum AudioEncoders
        {
            [Description("MP3 (LAME Encoder)")]
            MP3 = 0,
            [Description("WAVE")]
            WAVE,
            [Description("OGG (OggEn2)")]
            OGG,
            [Description("OPUS")]
            OPUS,
        }
        public class MP3
        {
            Bitrates Bitrate { get; set; }
            string InputPath { get; set; }
            string OutputPath { get; set; }
            LameConfig LConfig { get; set; }
            public enum Bitrates
            {
                [Description("320")]
                _320 = 0,
                [Description("256")]
                _256,
                [Description("224")]
                _224,
                [Description("192")]
                _192,
                [Description("160")]
                _160,
                [Description("128")]
                _128,
                [Description("96")]
                _96,
                [Description("64")]
                _64,
                [Description("32")]
                _32
            }

            public MP3(string inputPath, string outputPath, Bitrates bitrate = Bitrates._320, MPEGMode mPEGMode = MPEGMode.JointStereo)
            {
                this.Bitrate = bitrate;
                this.InputPath = inputPath;
                this.OutputPath = outputPath;

                LConfig = new LameConfig()
                {
                    BitRate = Convert.ToInt32(this.Bitrate.GetEnumDescription()),
                    Analysis = true,
                    Mode = mPEGMode
                };
            }
            public Task Process()
            {
                Task t = new Task(() =>
                {
                    int fCount = Directory.GetFiles(this.InputPath, "*.flac").Length;

                    //Conversion
                    List<Task> tasks = new List<Task>();
                    foreach (string flacFile in Directory.GetFiles(this.InputPath, "*.flac"))
                    {
                        Task t = new Task(() =>
                        {
                            Stopwatch sWatch = new Stopwatch();
                            Log.Information("[Encode][MP3][Process] Processing \"" + Path.GetFileNameWithoutExtension(flacFile) + "\"");
                            sWatch.Start();
                            byte[] flacBuffer;
                            using (FlacReader flacReader = new FlacReader(flacFile))
                            {
                                using (StreamReader streamReader = new StreamReader(flacReader))
                                {
                                    flacBuffer = new byte[streamReader.BaseStream.Length];
                                    streamReader.BaseStream.Read(flacBuffer, 0, flacBuffer.Length);
                                }
                            }
                            
                            LibFlacSharp.FlacFile r = new LibFlacSharp.FlacFile(flacFile);
                            LibFlacSharp.Metadata.VorbisComment s = r.VorbisComment;

                            LConfig.ID3 = new ID3TagData() {
                                Artist = s.CommentList.Where(x => x.Key.ToLower().StartsWith("artist")).FirstOrDefault().Value,
                                AlbumArtist = s.CommentList.Where(x => x.Key.ToLower().Contains("album") && x.Key.ToLower().Contains("artist")).FirstOrDefault().Value,
                                Album = s.CommentList.Where(x => x.Key.ToLower().StartsWith("album") && x.Key.ToLower().EndsWith("album")).FirstOrDefault().Value,
                                Title = s.CommentList.Where(x => x.Key.ToLower().StartsWith("title")).FirstOrDefault().Value,
                                Year = s.CommentList.Where(x => x.Key.ToLower().StartsWith("date")).FirstOrDefault().Value,
                                Track = s.CommentList.Where(x => x.Key.ToLower().StartsWith("tracknumber")).FirstOrDefault().Value,
                                Genre = s.CommentList.Where(x => x.Key.ToLower().StartsWith("genre")).FirstOrDefault().Value,
                                Comment = s.CommentList.Where(x => x.Key.ToLower().Contains("comment")).FirstOrDefault().Value,
                                AlbumArt = r.Pictures.Count() > 0 ? r.Pictures.Values.ElementAt(0).PictureData : null
                            };
                            
                            //TODO: User defined Tags like, mood, original composer, etc.

                            //TODO: Create a folder.jpg when not found - out of a files embedded picture
                            //using (FileStream fStream = File.Create(Path.Combine(this.OutputPath, Path.GetFileNameWithoutExtension(flacFile) + "." + MimeTypesMap.GetExtension(r.Pictures.Values.ElementAt(0).MIMEType)), r.Pictures.Values.ElementAt(0).PictureData.Length, FileOptions.SequentialScan))
                            //{
                            //    fStream.Write(r.Pictures.Values.ElementAt(0).PictureData, 0, r.Pictures.Values.ElementAt(0).PictureData.Length);
                            //}

                            using (LameMP3FileWriter lameMP3FileWriter = new LameMP3FileWriter(Path.Combine(this.OutputPath, Path.GetFileNameWithoutExtension(flacFile) + ".mp3"), new NAudio.Wave.WaveFormat(), LConfig))
                            {
                                using StreamWriter streamWriter = new StreamWriter(lameMP3FileWriter);
                                lameMP3FileWriter.Write(flacBuffer, 0, flacBuffer.Length);
                            }
                            flacBuffer = null;

                            sWatch.Stop();
                            Log.Information("[Encode][MP3][Process] \"" + Path.GetFileNameWithoutExtension(flacFile) + "\" Processing finished - Duration: " + sWatch.Elapsed.ToString(@"hh\:mm\:ss\:fffffff"));
                        });
                        tasks.Add(t);
                    }
                    tasks.All((x) => { x.Start(); return true; });
                    Task.WaitAll(tasks.ToArray());
                    //# ### #

                    //Copy Non Audio Files
                    CopyNonAudioFiles(this.InputPath, this.OutputPath);
                    //# ### #
                });
                t.Start();
                return t;
            }
        }
        public class WAVE
        {
            string InputPath { get; set; }
            string OutputPath { get; set; }
            public WAVE(string inputPath, string outputPath)
            {
                this.InputPath = inputPath;
                this.OutputPath = outputPath;
            }
            public Task Process()
            {
                Task t = new Task(() =>
                {
                    int fCount = Directory.GetFiles(this.InputPath, "*.flac").Length;

                    //Conversion
                    List<Task> tasks = new List<Task>();
                    foreach (string flacFile in Directory.GetFiles(this.InputPath, "*.flac"))
                    {
                        Task t = new Task(() =>
                        {
                            Stopwatch sWatch = new Stopwatch();
                            Log.Information("[Encode][WAVE][Process] Processing \"" + Path.GetFileNameWithoutExtension(flacFile) + "\"");
                            sWatch.Start();

                            //Main Conversion Process
                            using NAudio.Flac.FlacReader flacReader = new FlacReader(flacFile);
                            byte[] flacBuffer;
                            using (StreamReader streamReader = new StreamReader(flacReader))
                            {
                                flacBuffer = new byte[streamReader.BaseStream.Length];
                                streamReader.BaseStream.Read(flacBuffer, 0, flacBuffer.Length);
                            }
                            using (NAudio.Wave.WaveFileWriter waveFileWriter = new NAudio.Wave.WaveFileWriter(Path.Combine(this.OutputPath, Path.GetFileNameWithoutExtension(flacFile) + ".wav"), new NAudio.Wave.WaveFormat()))
                            {
                                using StreamWriter streamWriter = new StreamWriter(waveFileWriter);
                                waveFileWriter.Write(flacBuffer, 0, flacBuffer.Length);
                            }
                            flacBuffer = null;
                            //# ### #

                            sWatch.Stop();
                            Log.Information("[Encode][WAVE][Process] \"" + Path.GetFileNameWithoutExtension(flacFile) + "\" Processing finished - Duration: " + sWatch.Elapsed.ToString(@"hh\:mm\:ss\:fffffff"));
                        });
                        tasks.Add(t);
                    }
                    tasks.All((x) => { x.Start(); return true; });
                    Task.WaitAll(tasks.ToArray());
                    //# ### #

                    //Copy Non Audio Files
                    CopyNonAudioFiles(this.InputPath, this.OutputPath);
                    //# ### #
                });
                t.Start();
                return t;
            }
        }

        public static void CopyNonAudioFiles(string inputFolder, string outputFolder)
        {
            foreach (string f in Directory.GetFiles(inputFolder).Where(x => { return !x.EndsWith(".flac"); }))
            {
                Stopwatch sWatch = new Stopwatch();
                Log.Information("[Encode][CopyNonAudioFiles] Copying \"" + Path.GetFileName(f) + "\"");
                sWatch.Start();
                File.Copy(f, Path.Combine(outputFolder, Path.GetFileName(f)), true);
                sWatch.Stop();
                Log.Information("[Encode][CopyNonAudioFiles] Copying finished \"" + Path.GetFileName(f) + "\" Duration: " + sWatch.Elapsed.ToString(@"hh\:mm\:ss\:fffffff"));
            }
        }
    }
}

