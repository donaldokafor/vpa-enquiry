using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace VPA.Models
{
    public class VPAEnquiryRequestPostgreSql : IVPAEnquiryRequestPostgreRepo
    {
        private readonly vpaPostgreSqlContext _context;
        private readonly ILogger _logger;

        public VPAEnquiryRequestPostgreSql(vpaPostgreSqlContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("VPAEnquiryRequestPostgreSql");
        }

        public void AddVPAEnquiryRequest(VPAEnquiryRequest vpaenquiryrequest)
        {
            if (vpaenquiryrequest != null)
            {
                _context.VPAEnquiryRequest.Add(vpaenquiryrequest);
            }
            else
            {
                var vpaRequest = _context.VPAEnquiryRequest.Find(vpaenquiryrequest.requestId);
                vpaRequest.targetVPA = vpaenquiryrequest.targetVPA;
                _context.VPAEnquiryRequest.Add(vpaenquiryrequest);
            }
            _context.SaveChanges();
        }

        public void UpdateVPAEnquiryRequest(string requestId, VPAEnquiryRequest vpaenquiryrequest)
        {
            _context.VPAEnquiryRequest.Update(vpaenquiryrequest);
            _context.SaveChanges();
        }
        //public void UpdateVPAEnquiryRequest(string requestId, string targetvpa)
        //{
        //    var vpaRequest = _context.VPAEnquiryRequest.Find(requestId);
        //    vpaRequest.targetVPA = targetvpa;
        //    _context.VPAEnquiryRequest.Add(vpaRequest);

        //    _context.SaveChanges();
        //}
        public void DeleteVPAEnquiryRequest(string requestId)
        {
            var entity = _context.VPAEnquiryRequest.First(t => t.requestId == requestId);
            _context.VPAEnquiryRequest.Remove(entity);
            _context.SaveChanges();
        }
        //public void RemoveAllVPAEnquiryRequest(string requestId)
        //{
        //    var entity = _context.VPAEnquiryRequest.First(t => t.requestId == requestId);
        //    _context.VPAEnquiryRequest.Remove(entity);
        //    _context.SaveChanges();
        //}
        public VPAEnquiryRequest GetVPAEnquiryRequest(string requestId)
        {
            return _context.VPAEnquiryRequest.First(t => t.requestId == requestId);
        }

        public List<VPAEnquiryRequest> GetVPAEnquiryRequest()
        {
            // Using the shadow property EF.Property<DateTime>(dataEventRecord)
            return _context.VPAEnquiryRequest.OrderByDescending(request => EF.Property<DateTime>(request, "UpdatedTimestamp")).ToList();
        }

    }
}

