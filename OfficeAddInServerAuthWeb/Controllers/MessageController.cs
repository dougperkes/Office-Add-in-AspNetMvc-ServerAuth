using OfficeAddInServerAuth.Helpers;
using OfficeAddInServerAuth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OfficeAddInServerAuth.Controllers
{
    public class MessageController : Controller
    {
        public ActionResult Index(SendMessageResponse sendMessageResponse, UserInfo userInfo)
        {
            EnsureUser(ref userInfo);

            ViewBag.UserInfo = userInfo;
            ViewBag.MessageResponse = sendMessageResponse;

            return View();
        }


        public async Task<ActionResult> SendMessageSubmit(UserInfo userInfo)
        {
            // After Index method renders the View, user clicks Send Mail, which comes in here.
            EnsureUser(ref userInfo);
            SendMessageResponse sendMessageResult = new SendMessageResponse();
            // Send email using the Microsoft Graph API.
            var token = Data.GetUserSessionTokenAny(Settings.GetUserAuthStateId(ControllerContext.HttpContext));

            if (token.Provider == Settings.AzureADAuthority)
            {
                sendMessageResult = await GraphApiHelper.SendMessageAsync(
                    token.AccessToken,
                    GenerateEmail(userInfo));
            }
            else if (token.Provider == Settings.GoogleAuthority)
            {
                sendMessageResult = await GoogleApiHelper.SendMessageAsync(token.AccessToken, GenerateEmail(userInfo), token.Username);
            }
            // Reuse the Index view for messages (sent, not sent, fail) .
            // Redirect to tell the browser to call the app back via the Index method.
            return RedirectToAction(nameof(Index), new RouteValueDictionary(new Dictionary<string, object>{
                { "Status", sendMessageResult.Status },
                { "StatusMessage", sendMessageResult.StatusMessage },
                { "Address", userInfo.Address },
            }));
        }



        // Use the login user name or recipient email address if no user name.
        void EnsureUser(ref UserInfo userInfo)
        {
            var token = Data.GetUserSessionTokenAny(Settings.GetUserAuthStateId(ControllerContext.HttpContext));
            var currentUser = new UserInfo() {Name = token.Username, Address = token.Username};


            if (string.IsNullOrEmpty(userInfo?.Address))
            {
                userInfo = currentUser;
            }
            else if (userInfo.Address.Equals(currentUser.Address, StringComparison.OrdinalIgnoreCase))
            {
                userInfo = currentUser;
            }
            else
            {
                userInfo.Name = userInfo.Address;
            }
        }

        // Create email with predefine body and subject.
        SendMessageRequest GenerateEmail(UserInfo to)
        {
            return CreateEmailObject(
                to: to,
                subject: Settings.MessageSubject,
                body: string.Format(Settings.MessageBody, to.Name)
            );
        }

        // Create email object in the required request format/data contract.
        private SendMessageRequest CreateEmailObject(UserInfo to, string subject, string body)
        {
            return new SendMessageRequest
            {
                Message = new Message
                {
                    Subject = subject,
                    Body = new MessageBody
                    {
                        ContentType = "Html",
                        Content = body
                    },
                    ToRecipients = new List<Recipient>
                    {
                        new Recipient
                        {
                            EmailAddress = new UserInfo
                            {
                                 Name =  to.Name,
                                 Address = to.Address
                            }
                        }
                    }
                },
                SaveToSentItems = true
            };
        }

    }
}