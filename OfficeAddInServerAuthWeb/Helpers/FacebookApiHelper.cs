using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeAddInServerAuth.Models;

namespace OfficeAddInServerAuth.Helpers
{
    public class FacebookApiHelper
    {
        public static async Task<SendMessageResponse> PostMessageAsync(string accessToken, string userId, string message)
        {
            var url = $"https://graph.facebook.com/v2.5/{userId}/feed";
            var postbody = $"access_token={accessToken}&" +
                              "format=json&" +
                              $"message={Uri.EscapeDataString(message)}";
            var sendMessageResponse = new SendMessageResponse { Status = SendMessageStatusEnum.NotSent };

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {

                    request.Content = new StringContent(postbody, Encoding.UTF8, "application/x-www-form-urlencoded");
                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            sendMessageResponse.Status = SendMessageStatusEnum.Sent;
                        }
                        else
                        {
                            JToken error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"];
                            var fbError = JsonConvert.DeserializeObject<FacebookError>(error.ToString());
                            sendMessageResponse.Status = SendMessageStatusEnum.Fail;
                            sendMessageResponse.StatusMessage = fbError.error_user_msg;
                        }
                    }
                }
            }

            return sendMessageResponse;
        }
    }

    public class FacebookError
    {
        /*
        {
            "error": {
                "message": "Duplicate status message",
                "type": "OAuthException",
                "code": 506,
                "error_subcode": 1455006,
                "is_transient": false,
                "error_user_title": "Duplicate Status Update",
                "error_user_msg": "This status update is identical to the last one you posted. Try posting something different, or delete your previous update.",
                "fbtrace_id": "F4Iffln\\/NwA"
            }
        }
        */
        public string message { get; set; }
        public string type { get; set; }
        public int code { get; set; }
        public int error_subcode { get; set; }
        public bool is_transient { get; set; }
        public string error_user_title { get; set; }
        public string error_user_msg { get; set; }
    }
}