using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OfficeAddInServerAuth.Models
{
    public class AddInContext : DbContext
    {
        public AddInContext() : base("AddInContext")
        {
        }

        public DbSet<SessionToken> SessionTokens { get; set; }
    }
}