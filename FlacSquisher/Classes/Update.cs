using Newtonsoft.Json;
using Serilog;
using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace FlacSquisher
{
    public class Update
    {
        public const string GitHubAPILink = "https://api.github.com/repos/spreedated/flacsquisher/releases/latest";
        public GitHubResponse GitHubResponse = null;
        public event EventHandler<EventArgsResponse> GotResponse;

        public async void OnlineCheck()
        {
            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();
            try
            {
                string json = null;
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); //Set the User Agent to "request"

                    using HttpResponseMessage response = client.GetAsync(GitHubAPILink).Result;
                    response.EnsureSuccessStatusCode();
                    json = await response.Content.ReadAsStringAsync();
                }
                GitHubResponse = JsonConvert.DeserializeObject<GitHubResponse>(json);
                Log.Information("[Update][Check] Update response success (200)");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Update][Check] Error: ");
            }
            sWatch.Stop();
            Log.Information("[Update][Check] Process duration \"" + sWatch.Elapsed.ToString(@"hh\:mm\:ss\:fffffff") + "\"");
        }
        public async void StartupCheck()
        {
            Task t = new Task(()=> {
                Thread.Sleep(5000);
                OnlineCheck();
            });
            t.Start();
            await t;
            if (this.GitHubResponse == null)
            {
                GotResponse(this, new EventArgsResponse() { Response="Error in version lookup" });
                return;
            }

            if (Assembly.GetExecutingAssembly().GetName().Version < this.GitHubResponse.Version)
            {
                GotResponse(this, new EventArgsResponse() { Response = "New version available! - " + this.GitHubResponse.Version.ToString()});
            }
            else if (Assembly.GetExecutingAssembly().GetName().Version > this.GitHubResponse.Version)
            {
                GotResponse(this, new EventArgsResponse() { Response = "You have a newer version than online!" });
            }
            else
            {
                GotResponse(this, new EventArgsResponse() { Response = "You are on the latest version" });
            }
        }
        public async void DownloadZIP()
        {
            if (this.GitHubResponse.Files[0].DownloadURL == null)
            {
                return;
            }
            string fileName = Path.Combine(Environment.CurrentDirectory, this.GitHubResponse.Files[0].DownloadURL.Substring(this.GitHubResponse.Files[0].DownloadURL.LastIndexOf('/') + 1));
            string downloadFilepath = Path.Combine(Environment.CurrentDirectory, fileName);
            Task t = new Task(() => {
                using WebClient wc = new WebClient();
                wc.DownloadFile(this.GitHubResponse.Files[0].DownloadURL, downloadFilepath);
            });
            t.Start();
            await t;
            if (File.Exists(downloadFilepath))
            {
                ProcessStartInfo pInfo = new ProcessStartInfo() { FileName = downloadFilepath, WorkingDirectory = Path.GetDirectoryName(downloadFilepath), UseShellExecute = true };
                Process.Start(pInfo);
            }
            else
            {
                throw new FileNotFoundException("Error in Download, no file found on disk");
            }
        }
    }
    public class GitHubResponse
    {
        [JsonProperty("tag_name")]
        public Version Version { get; set; }
        [JsonProperty("target_commitish")]
        public string Branch { get; set; }
        [JsonProperty("published_at")]
        public DateTime PublishDate { get; set; }
        [JsonProperty("assets")]
        public List<Assets> Files { get; set; }
        public class Assets
        {
            [JsonProperty("browser_download_url")]
            public string DownloadURL { get; set; }
            [JsonProperty("size")]
            public long Bytes { get; set; }
        }
        [JsonProperty("body")]
        public string Description { get; set; }
    }
    public class EventArgsResponse : EventArgs
    { 
        public string Response { get; set; }
    }
}
