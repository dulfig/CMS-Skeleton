using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService
{
    public class AppUser : IdentityUser
    {
        public String Notes { get; set; }
        public String DisplayName { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Gender { get; set; }
        public String ProfilePic { get; set; }
        public String Birthday { get; set; }
        public bool IsProfileComplete { get; set; }
        public bool Terms { get; set; }
        public bool IsEmployee { get; set; }
        public String Role { get; set; }
        public DateTime AccCreationDate { get; set; }
        public bool RemeberMe { get; set; }
        public bool IsActive { get; set; }
        public ICollection<AdressModel>UserAdresses { get; set; }

    }
}
