using System;
using System.Configuration;
using System.Web;

namespace OfficeAddInServerAuth.Helpers
{
    public static class Settings
    {
        public static string AzureADClientId => ConfigurationManager.AppSettings["AAD:ClientID"];
        public static string AzureADClientSecret => ConfigurationManager.AppSettings["AAD:ClientSecret"];
        public static string AzureAD2ClientId => ConfigurationManager.AppSettings["AAD2:ClientID"];
        public static string AzureAD2ClientSecret => ConfigurationManager.AppSettings["AAD2:ClientSecret"];

        public static string AzureADAuthority = @"https://login.microsoftonline.com/common";
        public static string AzureADLogoutAuthority = @"https://login.microsoftonline.com/common/oauth2/logout?post_logout_redirect_uri=";
        public static string GraphApiResource = @"https://graph.microsoft.com/";
        public static string AzureAD2Authority = @"https://login.microsoftonline.com/common/oauth2/v2.0/";

        public static string SendMessageUrl = @"https://graph.microsoft.com/v1.0/me/microsoft.graph.sendmail";
        public static string GetMeUrl = @"https://graph.microsoft.com/v1.0/me";
        public static string MessageBody => ConfigurationManager.AppSettings["MessageBody"];
        public static string MessageSubject => ConfigurationManager.AppSettings["MessageSubject"];
        public static string GoogleClientId => ConfigurationManager.AppSettings["Google:ClientID"];
        public static string GoogleClientSecret => ConfigurationManager.AppSettings["Google:ClientSecret"];

        public static string FacebookClientId => ConfigurationManager.AppSettings["Facebook:ClientID"];
        public static string FacebookClientSecret => ConfigurationManager.AppSettings["Facebook:ClientSecret"];

        public static string DropBoxClientId => ConfigurationManager.AppSettings["DropBox:ClientID"];
        public static string DropBoxClientSecret => ConfigurationManager.AppSettings["DropBox:ClientSecret"];

        public static string GoogleAuthority = @"https://accounts.google.com";
        public static string FacebookAuthority = "https://www.facebook.com/";
        public static string DropBoxAuthority = "https://www.dropbox.com/";

        public static string GetUserAuthStateId(HttpContextBase ctx)
        {
            string id;
            if (ctx.Request.Cookies[SessionKeys.Login.UserAuthStateId] == null)
            {
                id = Guid.NewGuid().ToString("N");
                ctx.Response.Cookies.Add(new HttpCookie(SessionKeys.Login.UserAuthStateId)
                {
                    Expires = DateTime.Now.AddMinutes(20),
                    Value = id
                });
            }
            else
            {
                id = ctx.Request.Cookies[SessionKeys.Login.UserAuthStateId].Value;
            }

            return id;
        }
    }
}