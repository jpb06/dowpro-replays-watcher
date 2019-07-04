using DoWproReplayWatcher.Logic.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Communication
{
    public class DoWproLadderApi
    {
        public static async Task<string> SendResult(byte[] archiveData)
        {
            const string url = 
                "http://localhost:3001/api";
            //"https://dowpro.cf/api";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new MultipartFormDataContent();
                    var archiveContent = new StreamContent(new MemoryStream(archiveData));
                    archiveContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");

                    var c = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("login", "dowpro-replays-watcher-api-usr"),
                        new KeyValuePair<string, string>("password", "6XHB9JXcr1511oV")
                    });
                    var res = await client.PostAsync($"{url}/login", c);
                    var co = await res.Content.ReadAsStringAsync();

                    AuthResult r = JsonConvert.DeserializeObject<AuthResult>(co);

                    if (r.ExpirationDate == null && r.Token == null)
                        return "Unable to login";

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", r.Token);
                    content.Add(archiveContent, "arc", "arc.zip");
                    var response = await client.PostAsync($"{url}/sendResult", content);
                    var co2 = await response.Content.ReadAsStringAsync();

                    return co2;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
