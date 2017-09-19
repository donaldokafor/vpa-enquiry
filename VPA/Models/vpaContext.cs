using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace VPA.Models
{
    public class vpaContext
    {
        private readonly IMongoDatabase _database = null;

        public vpaContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<VPAEnquiryRequest> VPAEnquiryRequest
        {
            get
            {
                return _database.GetCollection<VPAEnquiryRequest>("requestlogs");
            }
        }
        public IMongoCollection<VPAEnquiryResponse> VPAEnquiryResponse
        {
            get
            {
                return _database.GetCollection<VPAEnquiryResponse>("responselogs");
            }
        }
        public IMongoCollection<VPAEnquiryException> VPAEnquiryException
        {
            get
            {
                return _database.GetCollection<VPAEnquiryException>("exceptionlogs");
            }
        }
    }
}
