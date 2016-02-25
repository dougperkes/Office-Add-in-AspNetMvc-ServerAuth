using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using OfficeAddInServerAuth.Models;

namespace OfficeAddInServerAuth.Helpers
{
    public class DropBoxApiHelper
    {
        public static async Task<DropBoxSpaceUsage> GetDropBoxSpaceUsage(SessionToken token)
        {
            var url = "https://api.dropboxapi.com/2/users/get_space_usage";
            DropBoxSpaceUsage usage = null;
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            usage =
                                JsonConvert.DeserializeObject<DropBoxSpaceUsage>(
                                    await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            }
            return usage;
        }
    }
}