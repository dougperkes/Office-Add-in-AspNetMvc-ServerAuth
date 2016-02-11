﻿using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using OfficeAddInServerAuth.Helpers;
using OfficeAddInServerAuth.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OfficeAddInServerAuth.Controllers
{
    public class HomeController : Controller
    {
        // The URL that auth should redirect to after a successful login.
        Uri loginRedirectUri => new Uri(Url.Action(nameof(Authorize), "Home", null, Request.Url.Scheme));

        // The URL to redirect to after a logout.
        Uri logoutRedirectUri => new Uri(Url.Action(nameof(Index), "Home", null, Request.Url.Scheme));

        public ActionResult Index()
        {
            var userAuthStateId = Settings.GetUserAuthStateId(ControllerContext.HttpContext);
            ViewBag.StateKey = userAuthStateId;
            ViewBag.IsAuthenticated = (Data.GetUserSessionToken(userAuthStateId) != null);
            return View();
        }

        public ActionResult Logout()
        {
            var userAuthStateId = Settings.GetUserAuthStateId(ControllerContext.HttpContext);
            Data.DeleteUserSessionToken(userAuthStateId);
            Response.Cookies.Clear();

            return Redirect(Settings.LogoutAuthority + logoutRedirectUri.ToString());
        }

        public ActionResult Login(string authState)
        {
            if (string.IsNullOrEmpty(Settings.ClientId) || string.IsNullOrEmpty(Settings.ClientSecret))
            {
                ViewBag.Message = "Please set your client ID and client secret in the Web.config file";
                return View();
            }


            var authContext = new AuthenticationContext(Settings.AzureADAuthority);

            // Generate the parameterized URL for Azure login.
            Uri authUri = authContext.GetAuthorizationRequestURL(
                Settings.GraphApiResource,
                Settings.ClientId,
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
                    new ClientCredential(Settings.ClientId, Settings.ClientSecret), // use the client ID and secret to establish app identity.
                    Settings.GraphApiResource);                                     // provide the identifier of the resource we want to access

                await SaveAuthToken(authState, authResult);

                authState.authStatus = "success";

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                authState.authStatus = "failure";
            }

            return RedirectToAction(nameof(AuthorizeComplete), new { authState = JsonConvert.SerializeObject(authState) });
        }

        private static async Task SaveAuthToken(AuthState authState, AuthenticationResult authResult)
        {
            var idToken = SessionToken.ParseJwtToken(authResult.IdToken);
            string username = null;
            var userNameClaim = idToken.Claims.FirstOrDefault(x => x.Type == "preferred_username");
            if (userNameClaim != null)
                username = userNameClaim.Value;

            using (var db = new AddInContext())
            {
                var token = new SessionToken()
                {
                    Id = authState.stateKey,
                    CreatedOn = DateTime.Now,
                    AccessToken = authResult.AccessToken,
                    Provider = Settings.GraphApiResource,
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