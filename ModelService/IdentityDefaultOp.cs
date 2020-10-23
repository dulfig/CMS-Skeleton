using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService
{
    public class IdentityDefaultOp
    {
        //----------------Password Requirements---------------------
        public bool PwdReqDigit { get; set; }
        public int PwdReqLength { get; set; }
        public bool PwdReqNonAlphanumeric { get; set; }
        public bool PwdReqUpperCase { get; set; }
        public bool PwdReqLowerCase { get; set; }
        public int PwdReqUniqueChars { get; set; }
        //----------------Lockout Properties---------------------
        public double LockoutDefaultLockoutMinutes { get; set; }
        public int LockoutMaxFailedAttempts { get; set; }
        public bool LockoutAllowedForNewUser { get; set; }
        //----------------User Requirements---------------------
        public bool UserReqUniqueEmail { get; set; }
        public bool SignInReqConfirmEmail { get; set; }
        public string FailPath { get; set; }

    }
}
