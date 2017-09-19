using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace VPA.Models
{
    public class VPAEnquiryRequestRepository : IVPAEnquiryRequetRepository
    {
        private readonly vpaContext _context = null;

        public VPAEnquiryRequestRepository(IOptions<Settings> settings)
        {
            _context = new vpaContext(settings);
        }

        public async Task<IEnumerable<VPAEnquiryRequest>> GetAllVPAEnquiryRequest()
        {
            return await _context.VPAEnquiryRequest.Find(_ => true).ToListAsync();
        }

        public async Task<VPAEnquiryRequest> GetVPAEnquiryRequest(string id)
        {
            var filter = Builders<VPAEnquiryRequest>.Filter.Eq("_id", id);
            return await _context.VPAEnquiryRequest
                                 .Find(filter)
                                 .FirstOrDefaultAsync();
        }

        public async Task AddVPAEnquiryRequest(VPAEnquiryRequest item)
        {
            await _context.VPAEnquiryRequest.InsertOneAsync(item);
        }

        public async Task AddVPAEnquiryResponse(VPAEnquiryResponse item)
        {
            await _context.VPAEnquiryResponse.InsertOneAsync(item);
        }

        public async Task AddVPAEnquiryException(VPAEnquiryException item)
        {
            await _context.VPAEnquiryException.InsertOneAsync(item);
        }

        public async Task<DeleteResult> RemoveVPAEnquiryRequest(string id)
        {
            return await _context.VPAEnquiryRequest.DeleteOneAsync(
                         Builders<VPAEnquiryRequest>.Filter.Eq("Id", id));
        }

        public async Task<UpdateResult> UpdateVPAEnquiryRequest(string id, string vpa)
        {
            var filter = Builders<VPAEnquiryRequest>.Filter.Eq(s => s.requestId.ToString(), id);
            var update = Builders<VPAEnquiryRequest>.Update
                                .Set(s => s.targetVPA, vpa)
                                .CurrentDate(s => s.updatedOn);
            return await _context.VPAEnquiryRequest.UpdateOneAsync(filter, update);
        }

        public async Task<ReplaceOneResult> UpdateVPAEnquiryRequest(string id, VPAEnquiryRequest item)
        {
            return await _context.VPAEnquiryRequest
                                 .ReplaceOneAsync(n => n.requestId.Equals(id)
                                                     , item
                                                     , new UpdateOptions { IsUpsert = true });
        }

        public async Task<DeleteResult> RemoveAllVPAEnquiryRequest()
        {
            return await _context.VPAEnquiryRequest.DeleteManyAsync(new BsonDocument());
        }
    }
}

