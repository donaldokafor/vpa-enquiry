using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VPA.Models
{
    public class VPAInformation
    {
        public string virtualPaymentAddress { get; set; }
        public List<AssociatedVpas> associatedVpas { get; set; }
        public bool hasAssociatedVPA { get; set; }
    }
}
