using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace VPA.Models
{
    public interface IVPAEnquiryRequetRepository
    {
        Task<IEnumerable<VPAEnquiryRequest>> GetAllVPAEnquiryRequest();
        Task<VPAEnquiryRequest> GetVPAEnquiryRequest(string id);
        Task AddVPAEnquiryRequest(VPAEnquiryRequest item);
        Task AddVPAEnquiryResponse(VPAEnquiryResponse item);
        Task AddVPAEnquiryException(VPAEnquiryException item);
        Task<DeleteResult> RemoveVPAEnquiryRequest(string id);
        Task<UpdateResult> UpdateVPAEnquiryRequest(string id, string snumber);

        // demo interface - full document update
        Task<ReplaceOneResult> UpdateVPAEnquiryRequest(string id, VPAEnquiryRequest item);

        // should be used with high cautious, only in relation with demo setup
        Task<DeleteResult> RemoveAllVPAEnquiryRequest();
    }
}
