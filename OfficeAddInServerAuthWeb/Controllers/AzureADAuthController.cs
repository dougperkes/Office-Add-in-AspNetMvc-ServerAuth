using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using OfficeAddInServerAuth.Helpers;
using OfficeAddInServerAuth.Models;

namespace OfficeAddInServerAuth.Controllers
{
    public class AzureADAuthController : Controller
    {
        // The URL that auth should redirect to after a successful login.
        Uri loginRedirectUri => new Uri(Url.Action(nameof(Authorize), "AzureADAuth", null, Request.Url.Scheme));

        // The URL to redirect to after a logout.
        Uri logoutRedirectUri => new Uri(Url.Action(nameof(HomeController.Index), "Home", null, Request.Url.Scheme));


        public ActionResult Logout()
        {
            var userAuthStateId = Settings.GetUserAuthStateId(ControllerContext.HttpContext);
            Data.DeleteUserSessionToken(userAuthStateId, Settings.AzureADAuthority);
            Response.Cookies.Clear();

            return Redirect(Settings.AzureADLogoutAuthority + logoutRedirectUri.ToString());
        }

        public ActionResult Login(string authState)
        {
            if (string.IsNullOrEmpty(Settings.AzureADClientId) || string.IsNullOrEmpty(Settings.AzureADClientSecret))
            {
                ViewBag.Message = "Please set your client ID and client secret in the Web.config file";
                return View();
            }


            var authContext = new AuthenticationContext(Settings.AzureADAuthority);

            // Generate the parameterized URL for Azure login.
            Uri authUri = authContext.GetAuthorizationRequestURL(
                Settings.GraphApiResource,
                Settings.AzureADClientId,
                loginRedirectUri,
                UserIdentifier.AnyUser,
                "state=" + authState);

            // Redirect the browser to the login page, then come back to the Authorize method below.
            return Redirect(authUri.ToString());
        }

        public async Task<ActionResult> Authorize()
        {
            var authContext = new AuthenticationContext(Settings.AzureADAuthority);
            var authStateString = Request.QueryString["state"];
            var authState = JsonConvert.DeserializeObject<AuthState>(authStateString);
            try
            {
                // Get the token.
                var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
                    Request.Params["code"],                                         // the auth 'code' parameter from the Azure redirect.
                    loginRedirectUri,                                               // same redirectUri as used before in Login method.
                    new ClientCredential(Settings.AzureADClientId, Settings.AzureADClientSecret), // use the client ID and secret to establish app identity.
                    Settings.GraphApiResource);                                     // provide the identifier of the resource we want to access

                await SaveAuthToken(authState, authResult);

                authState.authStatus = "success";

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

        private static async Task SaveAuthToken(AuthState authState, AuthenticationResult authResult)
        {
            var idToken = SessionToken.ParseJwtToken(authResult.IdToken);
            string username = null;
            var userNameClaim = idToken.Claims.FirstOrDefault(x => x.Type == "upn");
            if (userNameClaim != null)
                username = userNameClaim.Value;

            using (var db = new AddInContext())
            {
                var existingToken =
                                await
                                    db.SessionTokens.FirstOrDefaultAsync(
                                        t => t.Provider == Settings.AzureADAuthority && t.Id == authState.stateKey);
                if (existingToken != null)
                {
                    db.SessionTokens.Remove(existingToken);
                }
                var token = new SessionToken()
                {
                    Id = authState.stateKey,
                    CreatedOn = DateTime.Now,
                    AccessToken = authResult.AccessToken,
                    Provider = Settings.AzureADAuthority,
                    Username = username
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