using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VPA.Models
{
    public class VPATranslateResponse
    {
        public int httpStatusCode { get; set; }
        public string error { get; set; }
        public string errorDescription { get; set; }
        //public string Id { get; set; }
        [BsonId]
        public string vpaId { get; set; }
        public string pspId { get; set; }
        public string vpaType { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string accountCurrency { get; set; }
        public IndividualInformation individualInformation { get; set; }
        public MerchantInformation merchantInformation { get; set; }
        public ContactInformation contactInformation { get; set; }
        public bool hasAssociatedVpas { get; set; }
        public List<AssociatedVpas> associatedVpas { get; set; }
    }
    public class IndividualInformation
    {
        public string bvn { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
    }
    public class MerchantInformation
    {
        public string rcNumber { get; set; }
        public string tin { get; set; }
        public string companyName { get; set; }
    }
    public class ContactInformation
    {
        public string email { get; set; }
        public string phone { get; set; }
    }
    public class AssociatedVpas
    {
        public string vpaId { get; set; }
        public Limit limit { get; set; }
    }
    public class Limit
    {
        public bool hasLimit { get; set; }
        public double amount { get; set; }
    }
}
