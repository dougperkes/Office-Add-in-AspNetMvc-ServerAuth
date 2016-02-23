using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using OfficeAddInServerAuth.Helpers;
using OfficeAddInServerAuth.Models;

namespace OfficeAddInServerAuth.Controllers
{
    public class FacebookAuthController : Controller
    {
        // The URL that auth should redirect to after a successful login.
        Uri loginRedirectUri => new Uri(Url.Action(nameof(Authorize), "FacebookAuth", null, Request.Url.Scheme));

        // The URL to redirect to after a logout.
        Uri logoutRedirectUri => new Uri(Url.Action(nameof(HomeController.Index), "Home", null, Request.Url.Scheme));


        public ActionResult Logout()
        {
            var userAuthStateId = Settings.GetUserAuthStateId(ControllerContext.HttpContext);
            Data.DeleteUserSessionToken(userAuthStateId, Settings.FacebookAuthority);
            Response.Cookies.Clear();

            return Redirect(logoutRedirectUri.ToString());
        }

        public ActionResult Login(string authState)
        {
            if (string.IsNullOrEmpty(Settings.FacebookClientId) || string.IsNullOrEmpty(Settings.FacebookClientSecret))
            {
                ViewBag.Message = "Please set your client ID and client secret in the Web.config file";
                return View();
            }

            var scope = "email profile https://www.Facebookapis.com/auth/gmail.send";
            var authUrl =  "https://accounts.Facebook.com/o/oauth2/v2/auth?" +
                          $"scope={Uri.EscapeDataString(scope)}&" +
                          $"state={Uri.EscapeDataString(authState)}&" +
                          $"redirect_uri={loginRedirectUri}&" +
                           "response_type=code&" +
                          $"client_id={Settings.FacebookClientId}";

            // Redirect the browser to the login page, then come back to the Authorize method below.
            return Redirect(authUrl);
        }

        public async Task<ActionResult> Authorize()
        {
            var authStateString = Request.QueryString["state"];
            var authState = JsonConvert.DeserializeObject<AuthState>(authStateString);
            try
            {
                // Get the token.
                const string url = "https://www.Facebookapis.com/oauth2/v4/token";
                var authCode = Request.Params["code"];
                var postbody = $"code={authCode}&" +
                               "grant_type=authorization_code&" +
                               $"client_id={Settings.FacebookClientId}&" +
                               $"client_secret={Settings.FacebookClientSecret}&" +
                               $"redirect_uri={loginRedirectUri}";

                authState.authStatus = "failure";

                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        
                        request.Content = new StringContent(postbody, Encoding.UTF8, "application/x-www-form-urlencoded");
                        using (var response = await client.SendAsync(request))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var oauthResult = JsonConvert.DeserializeObject<GoogleOAuthResult>(result);
                                await SaveAuthToken(authState, oauthResult);
                                authState.authStatus = "success";
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                authState.authStatus = "failure";
            }

            return RedirectToAction(nameof(AuthorizeComplete), new { authState = JsonConvert.SerializeObject(authState) });
        }

        private static async Task SaveAuthToken(AuthState authState, GoogleOAuthResult authResult)
        {
            using (var db = new AddInContext())
            {
                var existingToken =
                    await
                        db.SessionTokens.FirstOrDefaultAsync(
                            t => t.Provider == Settings.FacebookAuthority && t.Id == authState.stateKey);
                if (existingToken != null)
                {
                    db.SessionTokens.Remove(existingToken);
                }
                string username = null;
                var jwt = SessionToken.ParseJwtToken(authResult.id_token);
                var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == "email");
                if (emailClaim != null)
                    username = emailClaim.Value;

                var token = new SessionToken()
                {
                    Id = authState.stateKey,
                    CreatedOn = DateTime.Now,
                    AccessToken = authResult.access_token,
                    Provider = Settings.FacebookAuthority,
                    Username = username,
                };
                db.SessionTokens.Add(token);
                await db.SaveChangesAsync();
            }
        }

        public ActionResult AuthorizeComplete(string authState)
        {
            ViewBag.AuthState = authState;
            return View();
        }
    }
}