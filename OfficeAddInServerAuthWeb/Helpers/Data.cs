using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OfficeAddInServerAuth.Models;

namespace OfficeAddInServerAuth.Helpers
{
    public static class Data
    {
        public static SessionToken GetUserSessionToken(string userAuthSessionId)
        {
            SessionToken st = null;
            using (var db = new AddInContext())
            {
                st = db.SessionTokens.FirstOrDefault(t => t.Id == userAuthSessionId);
            }
            return st;
        }

        public static void DeleteUserSessionToken(string userAuthSessionId)
        {
            using (var db = new AddInContext())
            {
                var st = db.SessionTokens.Where(t => t.Id == userAuthSessionId);
                if (st.Any())
                {
                    db.SessionTokens.RemoveRange(st);
                    db.SaveChanges();
                }
            }
        }
    }
}