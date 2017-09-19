using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VPA.Models
{
    public class VPAEnquiryResponse
    {
        public int httpStatusCode { get; set; }
        public string error { get; set; }
        public string errorDescription { get; set; }
        public AccountInformation accountInformation { get; set; }
        public PersonalInformation personalInformation { get; set; }
        public VPAInformation vpaInformation { get; set; }
        public MerchantInformation merchantInformation { get; set; }
    }
}
