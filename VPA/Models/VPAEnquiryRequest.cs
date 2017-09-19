using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VPA.Models
{
    public class VPAEnquiryRequest //: IValidatableObject
    {
        //[Required]
        //[BsonId]
        [BsonId]

        public string requestId { get; set; }
        //[Required]
        public string instructingInstitutionCode { get; set; }
        //[Required]
        public string instructedInstitutionCode { get; set; }
        //[Required]
        public int channelCode { get; set; }
        //[Required]
        public string targetVPA { get; set; }
        //[Required]
        //[DataType(DataType.DateTime)]
        public DateTime updatedOn { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    var _channelCode = new[] { "channelCode" };
        //    if (channelCode <= 0)
        //    {
        //        yield return new ValidationResult("Code cannot be less than or equal zero", _channelCode);
        //    }
        //    //throw new NotImplementedException();
        //}
    }

    public class AccountEnquiryRequest
    {
        public string requestId { get; set; }
        public string instructingInstitutionCode { get; set; }
        public string instructedInstitutionCode { get; set; }
        public int channelCode { get; set; }
        public string targetAccountNumber { get; set; }
    }
}
