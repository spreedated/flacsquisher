using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FlacSquisher
{
    public class Update
    {
        public const string GitHubAPILink = "https://api.github.com/repos/spreedated/flacsquisher/releases/latest";
        public GitHubResponse GitHubResponse = null;

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

}
