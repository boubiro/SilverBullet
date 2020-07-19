using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace OpenBullet
{
    public static class CheckUpdate
    {
        public static SBUpdate Run(string url)
        {
            var data = string.Empty;
            using (var wc = new WebClient())
            {
                data = wc.DownloadString(url);
            }
            return JsonConvert.DeserializeObject<SBUpdate>(data);
        }
    }
    public class SBUpdate
    {
        public bool Update { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public List<Notes> Notes { get; set; }
    }
    public class Notes
    {
        public string Note { get; set; }
    }
}
