using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace OfficeAddInServerAuth.Models
{
    public class OAuthResult
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; } 
        public string id_token { get; set; }
        public string uid { get; set; }
    }
}