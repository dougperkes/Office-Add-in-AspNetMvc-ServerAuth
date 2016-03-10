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

            var scope = "public_profile,email,publish_actions";
            var authUrl = "https://www.facebook.com/dialog/oauth?" +
                            $"client_id={Settings.FacebookClientId}" +
                            $"&redirect_uri={loginRedirectUri}/" + 
                            $"&state={Uri.EscapeDataString(authState)}" +
                            "&response_type=code" +
                            "&display=popup" + 
                            $"&scope={scope}";

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
                var authCode = Request.Params["code"];
                var url = "https://graph.facebook.com/v2.3/oauth/access_token?" +
                          $"client_id={Settings.FacebookClientId}" +
                          $"&redirect_uri={loginRedirectUri}/" +
                          $"&client_secret={Settings.FacebookClientSecret}" +
                          $"&code={authCode}";

                authState.authStatus = "failure";

                using (var client = new HttpClient())
                {
                    OAuthResult oauthResult = null;
                    //Facebook uses a GET rather than a POST
                    using (var response = await client.GetAsync(url))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            oauthResult = JsonConvert.DeserializeObject<OAuthResult>(result);
                            authState.authStatus = "success";
                        }
                    }

                    if (oauthResult != null && authState.authStatus == "success")
                    {
                        url = $"https://graph.facebook.com/v2.5/me?access_token={oauthResult.access_token}&fields=name%2Cid%2Cemail%2Cfirst_name%2Clast_name&format=json";
                        using (var response = await client.GetAsync(url))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var userData = JsonConvert.DeserializeObject<FacebookUserProfile>(result);
                                await SaveAuthToken(authState, oauthResult, userData);

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

            //instead of doing a server-side redirect, we have to do a client-side redirect to get around
            //some issues with the display dialog API not getting properly wired up after a server-side redirect
            var redirectUrl = Url.Action(nameof(AuthorizeComplete), new { authState = JsonConvert.SerializeObject(authState) });
            ViewBag.redirectUrl = redirectUrl;
            return View();
            //return RedirectToAction(nameof(AuthorizeComplete), new { authState = JsonConvert.SerializeObject(authState) });
        }

        private static async Task SaveAuthToken(AuthState authState, OAuthResult authResult, FacebookUserProfile userProfile)
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

                var token = new SessionToken()
                {
                    Id = authState.stateKey,
                    CreatedOn = DateTime.Now,
                    AccessToken = authResult.access_token,
                    Provider = Settings.FacebookAuthority,
                    Username = userProfile.id,
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

        private class FacebookUserProfile
        {
            public string name { get; set; }
            public string id { get; set; }
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }   
        }
    }
}