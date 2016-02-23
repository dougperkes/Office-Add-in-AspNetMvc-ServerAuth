using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OfficeAddInServerAuth.Models;

namespace OfficeAddInServerAuth.Helpers
{
    public static class Data
    {
        public static SessionToken GetUserSessionToken(string userAuthSessionId, string provider)
        {
            SessionToken st = null;
            using (var db = new AddInContext())
            {
                st = db.SessionTokens.FirstOrDefault(t => t.Id == userAuthSessionId && t.Provider == provider);
            }
            return st;
        }

        public static SessionToken GetUserSessionTokenAny(string userAuthSessionId)
        {
            SessionToken st = null;
            using (var db = new AddInContext())
            {
                st = db.SessionTokens.FirstOrDefault(t => t.Id == userAuthSessionId);
            }
            return st;
        }

        public static void DeleteUserSessionToken(string userAuthSessionId, string provider)
        {
            using (var db = new AddInContext())
            {
                var st = db.SessionTokens.Where(t => t.Id == userAuthSessionId && t.Provider == provider);
                if (st.Any())
                {
                    db.SessionTokens.RemoveRange(st);
                    db.SaveChanges();
                }
            }
        }
    }
}