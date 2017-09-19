using FluentValidation;
using VPA.Models;

namespace VPA.Filters
{
    public class VPAEnquiryRequestValidator : AbstractValidator<VPAEnquiryRequest>
    {
        public VPAEnquiryRequestValidator()
        {
            RuleFor(req => req.channelCode).GreaterThan(0).WithMessage("Invalid Channel Code");
            RuleFor(req => req.instructedInstitutionCode).NotEmpty().WithMessage("Instructed Institution Code is required");
            RuleFor(req => req.instructingInstitutionCode).NotEmpty().WithMessage("Instructing Institution Code is required");
            RuleFor(req => req.requestId).NotEmpty().WithMessage("Request ID is required");
            RuleFor(req => req.targetVPA).NotEmpty().WithMessage("Virtual payment adsress is required");
        }
    }
}
