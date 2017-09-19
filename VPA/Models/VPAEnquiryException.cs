using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VPA.Models
{
    public class VPAEnquiryException
    {
        [BsonId]
        public string requestId { get; set; }
        public string action { get; set; }
        public string exception { get; set; }
        public DateTime logTime { get; set; }
    }
}
