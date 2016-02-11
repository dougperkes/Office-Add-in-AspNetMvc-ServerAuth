using System;
using System.Configuration;
using System.Web;

namespace OfficeAddInServerAuth.Helpers
{
    public static class Settings
    {
        public static string ClientId => ConfigurationManager.AppSettings["ClientID"];
        public static string ClientSecret => ConfigurationManager.AppSettings["ClientSecret"];

        public static string AzureADAuthority = @"https://login.microsoftonline.com/common";
        public static string LogoutAuthority = @"https://login.microsoftonline.com/common/oauth2/logout?post_logout_redirect_uri=";
        public static string GraphApiResource = @"https://graph.microsoft.com/";

        public static string SendMessageUrl = @"https://graph.microsoft.com/v1.0/me/microsoft.graph.sendmail";
        public static string GetMeUrl = @"https://graph.microsoft.com/v1.0/me";
        public static string MessageBody => ConfigurationManager.AppSettings["MessageBody"];
        public static string MessageSubject => ConfigurationManager.AppSettings["MessageSubject"];

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