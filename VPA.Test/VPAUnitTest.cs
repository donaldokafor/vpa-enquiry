using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using VPA.Models;
using VPA.Controllers;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace VPA.Test
{
    public class VPAUnitTest
    {
        [Fact]
        public async Task TestVPAEnquiry()
        {
            VPAEnquiryResponse response = new VPAEnquiryResponse();
            VPATranslateResponse vtr = new VPATranslateResponse();
            VPAInformation vInfo = new VPAInformation();
            PersonalInformation pInfo = new PersonalInformation();
            AccountInformation aInfo = new AccountInformation();
            MerchantInformation mInfo = new MerchantInformation();
            List<AssociatedVpas> aVpas = new List<AssociatedVpas>();
            VPAEnquiryRequest req = new VPAEnquiryRequest();
            VPAEnquiryRequest vrequest = new VPAEnquiryRequest()
            {
                channelCode = 7,
                instructedInstitutionCode = "044",
                instructingInstitutionCode = "230",
                requestId = "hdfufsh3652",
                targetVPA = "fuzzytoocool",
                updatedOn = DateTime.Now
            };

            VPATranslateResponse vpaTranslateResponse = new VPATranslateResponse();

            VPAEnquiryController controller = new VPAEnquiryController(getCacheSettings(), getAppSettings(), response,vtr, vInfo,pInfo,aInfo,mInfo,aVpas,req);
            vpaTranslateResponse = await controller.testvpaEnquiry(vrequest);

            //Assert.Equal(v, vpaTranslateResponse);
            Assert.NotNull(vpaTranslateResponse.vpaId);
            Assert.Equal(vrequest.targetVPA, vpaTranslateResponse.vpaId);

        }
        [Fact]
        public async Task TestAccountEnquiry()
        {
            VPAEnquiryResponse response = new VPAEnquiryResponse();
            VPATranslateResponse vtr = new VPATranslateResponse();
            VPAInformation vInfo = new VPAInformation();
            PersonalInformation pInfo = new PersonalInformation();
            AccountInformation aInfo = new AccountInformation();
            MerchantInformation mInfo = new MerchantInformation();
            List<AssociatedVpas> aVpas = new List<AssociatedVpas>();
            VPAEnquiryRequest req = new VPAEnquiryRequest();
            AccountEnquiryRequest arequest = new AccountEnquiryRequest()
            {
                channelCode = 5,
                instructedInstitutionCode = "202",
                instructingInstitutionCode = "11",
                requestId = "jh3r4y75hui",
                targetAccountNumber = "2024878029"
            };

            AccountInformation ainf = new AccountInformation();
            VPAEnquiryController controller = new VPAEnquiryController(getCacheSettings(), getAppSettings(), response, vtr, vInfo, pInfo, aInfo, mInfo, aVpas, req);
            ainf = await controller.testAccountEnquiry(arequest);

            Assert.NotNull(ainf.accountNumber);
            Assert.Equal(arequest.targetAccountNumber, ainf.accountNumber);
        }
        [Fact]
        public async Task TestAddressEnquiry()
        {
            VPAEnquiryResponse response = new VPAEnquiryResponse();
            VPATranslateResponse vtr = new VPATranslateResponse();
            VPAInformation vInfo = new VPAInformation();
            PersonalInformation pInfo = new PersonalInformation();
            AccountInformation aInfo = new AccountInformation();
            MerchantInformation mInfo = new MerchantInformation();
            List<AssociatedVpas> aVpas = new List<AssociatedVpas>();
            VPAEnquiryRequest req = new VPAEnquiryRequest()
            {
                channelCode = 8,
                instructedInstitutionCode = "123",
                instructingInstitutionCode = "402",
                requestId = "i88hgtbh",
                targetVPA = "musaarca",
                updatedOn = DateTime.Now
            };

            VPAEnquiryResponse respose = new VPAEnquiryResponse();
            VPAEnquiryController controller = new VPAEnquiryController(getCacheSettings(), getAppSettings(), response, vtr, vInfo, pInfo, aInfo, mInfo, aVpas, req);
            respose = await controller.AddressEnquiry(req);

            Assert.NotNull(respose.httpStatusCode);
            Assert.Equal((int)HttpStatusCode.OK, respose.httpStatusCode);
        }
        public IOptions<AppSettings> getAppSettings()
        {
            AppSettings app = new AppSettings()
            {
                accountEnquiryUri = "/accountenquiry",
                isTest = true,
                translateUri = "/translateuri"
            };
            var settings = new Mock<IOptions<AppSettings>>();
            settings.Setup(x => x.Value).Returns(app);
            return settings.Object;
        }
        public IDistributedCache getCacheSettings()
        {
            var settings = new Mock<IDistributedCache>();
            return settings.Object;
        }
    }
}
