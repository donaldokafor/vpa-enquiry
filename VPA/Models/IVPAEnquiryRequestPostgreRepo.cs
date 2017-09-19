using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VPA.Models
{
    public interface IVPAEnquiryRequestPostgreRepo
    {
        void AddVPAEnquiryRequest(VPAEnquiryRequest vpaenquiryrequest);
        void UpdateVPAEnquiryRequest(string requestId, VPAEnquiryRequest vpaenquiryrequest);
        void DeleteVPAEnquiryRequest(string requestId);
        VPAEnquiryRequest GetVPAEnquiryRequest(string requestId);
        List<VPAEnquiryRequest> GetVPAEnquiryRequest();
    }
}
