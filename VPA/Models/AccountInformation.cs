using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VPA.Models
{
    public class AccountInformation
    {
        public int http_status_code { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
        public string accountFirstName { get; set; }
        public string accountMiddleName { get; set; }
        public string accountLastName { get; set; }
        public string accountEntityName { get; set; }
        public string accountNumber { get; set; }
        public string accountCurrency { get; set; }
        public string accountType { get; set; }
        public int accountCategory { get; set; }
        public string accountStatus { get; set; }
        public string verificationNumber { get; set; }
        public string verificationNumberType { get; set; }
        public string authorizationCredentialsAllowed { get; set; }
        public string authorizationCredentialsType { get; set; }
        public string authorizationCredentialsLength { get; set; }
        public PersonalInformation personalinformation { get; set; }
    }
}
