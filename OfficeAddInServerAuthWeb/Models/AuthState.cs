using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OfficeAddInServerAuth.Models
{
    public class AuthState
    {
        public string stateKey { get; set; }
        public string signalRHubId { get; set; }
        public string authStatus { get; set; }
    }
}