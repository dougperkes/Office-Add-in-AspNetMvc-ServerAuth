using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MimeKit;
using Newtonsoft.Json;
using OfficeAddInServerAuth.Models;

namespace OfficeAddInServerAuth.Helpers
{
    public class GoogleApiHelper
    {
        public static async Task<SendMessageResponse> SendMessageAsync(string accessToken,
            SendMessageRequest sendMessageRequest, string username)
        {
            var message = new MimeMessage();
            //message.From.Add(new MailboxAddress(sendMessageRequest.Message.));
            foreach (var to in sendMessageRequest.Message.ToRecipients)
            {
                message.To.Add(new MailboxAddress(to.EmailAddress.Name, to.EmailAddress.Address));
            }
            message.Subject = sendMessageRequest.Message.Subject;

            var builder = new BodyBuilder();

            // Set the plain-text version of the message text
            //builder.TextBody = @"";

            // Set the html version of the message text
            builder.HtmlBody = sendMessageRequest.Message.Body.Content;

            // Now we just need to set the message body and we're done
            message.Body = builder.ToMessageBody();
            var encodedEmail = Base64UrlEncode(message.ToString());
            var url = $"https://www.googleapis.com/upload/gmail/v1/users/{username}/messages/send?uploadType=media";
            var sendMessageResponse = new SendMessageResponse { Status = SendMessageStatusEnum.NotSent };
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    request.Content = new StringContent(encodedEmail, Encoding.UTF8, "message/rfc822");
                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            sendMessageResponse.Status = SendMessageStatusEnum.Sent;
                            sendMessageResponse.StatusMessage = null;
                        }
                        else
                        {
                            sendMessageResponse.Status = SendMessageStatusEnum.Fail;
                            sendMessageResponse.StatusMessage = response.ReasonPhrase;
                        }
                    }
                }
            }
            return sendMessageResponse;
        }

        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }
    }
}