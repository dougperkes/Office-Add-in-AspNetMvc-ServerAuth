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
    public class AzureAD2AuthController : Controller
    {
        private const string scope = "openid email profile https://graph.microsoft.com/Mail.Send";
        // The URL that auth should redirect to after a successful login.
        Uri loginRedirectUri => new Uri(Url.Action(nameof(Authorize), "AzureAD2Auth", null, Request.Url.Scheme));

        // The URL to redirect to after a logout.
        Uri logoutRedirectUri => new Uri(Url.Action(nameof(HomeController.Index), "Home", null, Request.Url.Scheme));


        public ActionResult Logout()
        {
            var userAuthStateId = Settings.GetUserAuthStateId(ControllerContext.HttpContext);
            Data.DeleteUserSessionToken(userAuthStateId, Settings.AzureAD2Authority);
            Response.Cookies.Clear();

            return Redirect(Settings.AzureADLogoutAuthority + logoutRedirectUri.ToString());
        }

        public ActionResult Login(string authState)
        {
            if (string.IsNullOrEmpty(Settings.AzureAD2ClientId) || string.IsNullOrEmpty(Settings.AzureAD2ClientSecret))
            {
                ViewBag.Message = "Please set your client ID and client secret in the Web.config file";
                return View();
            }


            var authContext = new AuthenticationContext(Settings.AzureAD2Authority + "authorize");
            // Generate the parameterized URL for Azure login.
            var url = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?" +
                      $"client_id={Settings.AzureAD2ClientId}" +
                      $"&scope={Uri.EscapeDataString(scope)}" +
                      $"&state={Uri.EscapeDataString(authState)}" +
                      $"&redirect_uri={loginRedirectUri}" +
                      "&response_type=code";

            // Redirect the browser to the login page, then come back to the Authorize method below.
            return Redirect(url);
        }

        public async Task<ActionResult> Authorize()
        {
            var authStateString = Request.QueryString["state"];
            var authState = JsonConvert.DeserializeObject<AuthState>(authStateString);
            try
            {
                // Get the token.
                const string url = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
                var authCode = Request.Params["code"];
                var postbody = $"code={authCode}&" +
                              $"&scope={Uri.EscapeDataString(scope)}" +
                               "&grant_type=authorization_code&" +
                               $"client_id={Settings.AzureAD2ClientId}&" +
                               $"client_secret={Settings.AzureAD2ClientSecret}&" +
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
                                var oauthResult = JsonConvert.DeserializeObject<OAuthResult>(result);
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


        private static async Task SaveAuthToken(AuthState authState, OAuthResult authResult)
        {
            var idToken = SessionToken.ParseJwtToken(authResult.id_token);
            string username = null;
            var userNameClaim = idToken.Claims.FirstOrDefault(x => x.Type == "upn" || x.Type == "preferred_username");
            if (userNameClaim != null)
                username = userNameClaim.Value;

            using (var db = new AddInContext())
            {
                var existingToken =
                    await
                        db.SessionTokens.FirstOrDefaultAsync(
                            t => t.Provider == Settings.AzureAD2Authority && t.Id == authState.stateKey);
                if (existingToken != null)
                {
                    db.SessionTokens.Remove(existingToken);
                }

                var token = new SessionToken()
                {
                    Id = authState.stateKey,
                    CreatedOn = DateTime.Now,
                    AccessToken = authResult.access_token,
                    Provider = Settings.AzureAD2Authority,
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