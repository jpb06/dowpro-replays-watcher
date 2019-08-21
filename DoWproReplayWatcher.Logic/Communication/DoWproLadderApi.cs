using DoWproReplayWatcher.Logic.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Communication
{
    public class DoWproLadderApi
    {
        public static string ApiUrl = string.Empty;

        public static async Task<string> SendResult(byte[] archiveData)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage authResponse = await client.PostAsync($"{ApiUrl}/login", new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("login", LogicSettings.Default.ApiUserName),
                        new KeyValuePair<string, string>("password", LogicSettings.Default.ApiPassword)
                    }));
                    string authResponseContent = await authResponse.Content.ReadAsStringAsync();

                    AuthResult authResult = JsonConvert.DeserializeObject<AuthResult>(authResponseContent);

                    if (authResult.ExpirationDate == null && authResult.Token == null)
                        return "Unable to login";

                    MultipartFormDataContent content = new MultipartFormDataContent();
                    StreamContent archiveContent = new StreamContent(new MemoryStream(archiveData));
                    archiveContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
                    content.Add(archiveContent, "arc", "arc.zip");
                    HttpResponseMessage uploadResponse = await client.PostAsync($"{ApiUrl}/sendResult", content);
                    string uploadResponseContent = await uploadResponse.Content.ReadAsStringAsync();

                    return uploadResponseContent;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
