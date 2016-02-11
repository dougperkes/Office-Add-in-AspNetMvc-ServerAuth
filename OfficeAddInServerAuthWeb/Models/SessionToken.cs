using System;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace OfficeAddInServerAuth.Models
{
    public class SessionToken
    {
        /// <summary>
        /// This is the user SessionID
        /// </summary>
        [MaxLength(36)]
        public string Id { get; set; }
        public string AccessToken { get; set; }
        [MaxLength(150)]
        public string Provider { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(100)]
        public string Username { get; set; }

        //TODO: Validate the token so we can extract the user name and user id properties from the id_token
        public static JwtSecurityToken ParseJwtToken(string jwtToken)
        {
            JwtSecurityToken jst = new JwtSecurityToken(jwtToken);
            return jst;
        }
    }
}