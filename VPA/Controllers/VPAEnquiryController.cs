using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VPA.Models;
using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;
using VPA.Filters;
using FluentValidation.Results;
using System.Collections.Generic;
using LiteDB;
using System.Linq;

namespace VPA.Controllers
{
    [Produces("application/json")]
    [Route("api/VPAEnquiry")]
    public class VPAEnquiryController : Controller
    {
        //private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IVPAEnquiryRequetRepository _requestRepository;
        private readonly IVPAEnquiryRequestPostgreRepo _requestPostgreRepo;
        private readonly IDistributedCache _distributedCache;
        IOptions<AppSettings> _settings;
        VPAEnquiryResponse _response;
        VPATranslateResponse _vpaTranslateResponse;
        VPAInformation _vInfo;
        PersonalInformation _pInfo;
        AccountInformation _aInfo;
        MerchantInformation _mInfo;
        List<AssociatedVpas> _aVpas;
        VPAEnquiryRequest _req;

        //public VPAEnquiryController(IVPAEnquiryRequetRepository requestRepository, IVPAEnquiryRequestPostgreRepo requestPostgreRepo
        //    , IDistributedCache distributedCache, IOptions<AppSettings> settings)
        //{
        //    _requestRepository = requestRepository;
        //    _requestPostgreRepo = requestPostgreRepo;
        //    _distributedCache = distributedCache;
        //    _settings = settings;
        //}

        public VPAEnquiryController(IDistributedCache distributedCache, IOptions<AppSettings> settings, VPAEnquiryResponse response, VPATranslateResponse vpaTranslateResponse
            , VPAInformation vInfo, PersonalInformation pInfo, AccountInformation aInfo, MerchantInformation mInfo, List<AssociatedVpas> aVpas, VPAEnquiryRequest req)
        {
            _distributedCache = distributedCache;
            _settings = settings;
            _req = req;
            _response = response;
            _vpaTranslateResponse = vpaTranslateResponse;
            _aVpas = aVpas;
            _vInfo = vInfo;
            _pInfo = pInfo;
            _aInfo = aInfo;
            _mInfo = mInfo;
        }      
        [HttpPost("AddressEnquiry")]
        public async Task<VPAEnquiryResponse> AddressEnquiry([FromBody] VPAEnquiryRequest request)
        {
            //VPAEnquiryResponse response = new VPAEnquiryResponse();
            //VPATranslateResponse vpaTranslateResponse = new VPATranslateResponse();
            //VPAInformation vInfo = new VPAInformation();
            //PersonalInformation pInfo = new PersonalInformation();
            //AccountInformation aInfo = new AccountInformation();
            //MerchantInformation mInfo = new MerchantInformation();
            //List<AssociatedVpas> aVpas = new List<AssociatedVpas>();

            //log request on mongo 
            try
            {
                //VPAEnquiryRequest req = new VPAEnquiryRequest()
                //{
                //    channelCode = request.channelCode,
                //    instructedInstitutionCode = request.instructedInstitutionCode,
                //    instructingInstitutionCode = request.instructingInstitutionCode,
                //    requestId = request.requestId,
                //    targetVPA = request.targetVPA,
                //    updatedOn = DateTime.Now
                //};

                _req.channelCode = request.channelCode;
                _req.instructedInstitutionCode = request.instructedInstitutionCode;
                _req.instructingInstitutionCode = request.instructingInstitutionCode;
                _req.requestId = request.requestId;
                _req.targetVPA = request.targetVPA;
                _req.updatedOn = DateTime.Now;
               

                VPAEnquiryRequestValidator validator = new VPAEnquiryRequestValidator();
                ValidationResult results = validator.Validate(_req);

                bool validationSucceeded = results.IsValid;
                IList<ValidationFailure> failures = results.Errors;

                if (!validationSucceeded)
                {
                    string desc = string.Empty;
                    foreach (var f in failures)
                    {
                        desc = desc + f.ErrorMessage;
                    }
                    _response.httpStatusCode = (int)HttpStatusCode.BadRequest;
                    _response.error = "Bad Request";
                    _response.errorDescription = desc;
                    return _response;
                }            
                //if (!_settings.Value.isTest)
                if (_settings.Value.isTest)
                {
                    string bs = testLogRequest(_req);
                }
                else
                {
                    await _requestRepository.AddVPAEnquiryRequest(_req);
                }
            }
            catch (Exception ex)
            {
                await logException(new VPAEnquiryException()
                {
                    action = "Error Logging Incoming Request: " + request.targetVPA,
                    exception = ex.ToString(),
                    requestId = request.requestId,
                    logTime = DateTime.Now
                });
            }

            //check cache for details
            try
            {
                _response = await vpaEnquiry(request.targetVPA);

                if (_response != null && _response.httpStatusCode != 0)
                    return _response;

            }
            catch (Exception ex)
            {
                await logException(new VPAEnquiryException()
                {
                    action = "Error Fetching Request From Cache: " + request.targetVPA,
                    exception = ex.ToString(),
                    requestId = request.requestId,
                    logTime = DateTime.Now
                });
            }

            //assume OK
            _response.httpStatusCode = (int)HttpStatusCode.OK;

            ///vpa translate call
            try
            {
                if (_settings.Value.isTest)
                {
                    //testLogtranslate(request.targetVPA);
                    _vpaTranslateResponse = await testvpaEnquiry(request);
                }
                else
                {
                    _vpaTranslateResponse = await vpaEnquiry(request);
                }

                bool OK = _vpaTranslateResponse.httpStatusCode == (int)HttpStatusCode.OK ||
                    _vpaTranslateResponse.httpStatusCode == (int)HttpStatusCode.Created || _vpaTranslateResponse.httpStatusCode == (int)HttpStatusCode.Accepted;

                if (_vpaTranslateResponse == null || !OK)
                {
                    _response.httpStatusCode = _vpaTranslateResponse.httpStatusCode;
                    _response.error = _vpaTranslateResponse.error;
                    _response.errorDescription = _vpaTranslateResponse.errorDescription;
                    return _response;
                }
                _aVpas = new List<AssociatedVpas>();
                foreach (var avpa in _vpaTranslateResponse.associatedVpas)
                {
                    AssociatedVpas aVpa = new AssociatedVpas()
                    {
                        limit = avpa.limit,
                        vpaId = avpa.vpaId
                    };
                    _aVpas.Add(aVpa);
                }

                _vInfo = new VPAInformation()
                {
                    associatedVpas = _aVpas,
                    hasAssociatedVPA = _vpaTranslateResponse.hasAssociatedVpas,
                    virtualPaymentAddress = _vpaTranslateResponse.vpaId
                };
                _response.vpaInformation = _vInfo;
                _pInfo = new PersonalInformation()
                {
                    email = _vpaTranslateResponse.contactInformation.email,
                    mobilePhoneNumber = _vpaTranslateResponse.contactInformation.phone
                };
                _response.personalInformation = _pInfo;
                _mInfo = new MerchantInformation()
                {
                    companyName = _vpaTranslateResponse.merchantInformation.companyName,
                    rcNumber = _vpaTranslateResponse.merchantInformation.rcNumber,
                    tin = _vpaTranslateResponse.merchantInformation.tin

                };
                _response.merchantInformation = _mInfo;
            }
            catch (Exception ex)
            {
                await logException(new VPAEnquiryException()
                {
                    action = "Error Translating VPA: " + request.targetVPA,
                    exception = ex.ToString(),
                    requestId = request.requestId,
                    logTime = DateTime.Now
                });
            }

            //account enquiry call
            try
            {
                if (_vpaTranslateResponse != null && !string.IsNullOrEmpty(_vpaTranslateResponse.accountNumber))
                {

                    AccountEnquiryRequest arequest = new AccountEnquiryRequest()
                    {
                        channelCode = request.channelCode,
                        instructedInstitutionCode = request.instructedInstitutionCode,
                        instructingInstitutionCode = request.instructingInstitutionCode,
                        requestId = request.requestId,
                        targetAccountNumber = _vpaTranslateResponse.accountNumber
                    };

                    if (_settings.Value.isTest)
                    {
                        _aInfo = await testAccountEnquiry(arequest);
                    }
                    else
                    {
                        _aInfo = await accountEnquiry(arequest);
                    }

                    bool OK = _aInfo.http_status_code == (int)HttpStatusCode.OK ||
                   _aInfo.http_status_code == (int)HttpStatusCode.Created || _aInfo.http_status_code == (int)HttpStatusCode.Accepted;

                    if (_aInfo == null || !OK)
                    {
                        _response.httpStatusCode = _aInfo.http_status_code;
                        _response.error = _aInfo.error;
                        _response.errorDescription = _aInfo.error_description;
                        return _response;
                    }

                    _pInfo = new PersonalInformation()
                    {
                        email = _aInfo.personalinformation.email,
                        mobilePhoneNumber = _aInfo.personalinformation.mobilePhoneNumber
                    };

                    //AccountInformation ai = new AccountInformation()
                    //{
                    //    accountCategory = aInfo.accountCategory,
                    //    accountCurrency = aInfo.accountCurrency,
                    //    accountEntityName = aInfo.accountEntityName,
                    //    accountFirstName = aInfo.accountFirstName,
                    //    accountLastName = aInfo.accountLastName,
                    //    accountMiddleName = aInfo.accountMiddleName,
                    //    accountNumber = aInfo.accountNumber,
                    //    accountStatus = aInfo.accountStatus,
                    //    accountType = aInfo.accountType,
                    //    authorizationCredentialsAllowed = aInfo.authorizationCredentialsAllowed,
                    //    authorizationCredentialsLength = aInfo.authorizationCredentialsLength,
                    //    authorizationCredentialsType = aInfo.authorizationCredentialsType,
                    //    personalinformation = pInfo,
                    //    verificationNumber = aInfo.verificationNumber,
                    //    verificationNumberType = aInfo.verificationNumberType
                    //};

                    _response.personalInformation = _pInfo;

                    _response.accountInformation = _aInfo;
                }
            }
            catch (Exception ex)
            {
                await logException(new VPAEnquiryException()
                {
                    action = "Error on Account Enquiry: " + request.targetVPA,
                    exception = ex.ToString(),
                    requestId = request.requestId,
                    logTime = DateTime.Now
                });
            }

            //add to cache
            try
            {
                string cacheKey = _vpaTranslateResponse.vpaId;
                string cacheValue = JsonHelper.toJson(_response);
                await _distributedCache.SetStringAsync(cacheKey, cacheValue);

            }
            catch (Exception ex)
            {
                await logException(new VPAEnquiryException()
                {
                    action = "Error Writing Response to Cache: " + request.targetVPA,
                    exception = ex.ToString(),
                    requestId = request.requestId,
                    logTime = DateTime.Now
                });
            }

            return _response;
        }

        //Actual  Methods
        [HttpGet]
        public async Task<string> Get()
        {
            var cacheKey = "vparequest";
            var existingTime = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(existingTime))
            {
                return "Fetched from cache : " + existingTime;
            }
            else
            {
                existingTime = DateTime.UtcNow.ToString();
                _distributedCache.SetString(cacheKey, existingTime);
                return "Added to cache : " + existingTime;
            }
        }
        public async Task<VPAEnquiryResponse> vpaEnquiry(string cacheKey)
        {
            string cachedValue = string.Empty;
            VPAEnquiryResponse response = new VPAEnquiryResponse();
            try
            {
                cachedValue = await _distributedCache.GetStringAsync(cacheKey);
            }
            catch (Exception ex)
            {

            }
            if (!string.IsNullOrEmpty(cachedValue))
            {
                try
                {
                    //response = (VPAEnquiryResponse)JsonConvert.DeserializeObject(cachedValue);
                    response = JsonHelper.fromJson<VPAEnquiryResponse>(cachedValue);
                }
                catch (Exception ex)
                {
                    await logException(new VPAEnquiryException()
                    {
                        action = "Error deserializing cache value: " + cachedValue,
                        exception = ex.ToString(),
                        requestId = cacheKey,
                        logTime = DateTime.Now
                    });
                }
            }
            return response;
        }
        public async Task<VPATranslateResponse> vpaEnquiry(VPAEnquiryRequest request)
        {
            VPATranslateResponse vpaTranslateResponse = new VPATranslateResponse();
            try
            {
                string uri = _settings.Value.translateUri;
                uri = uri + request.targetVPA;
                var httpClient = new HttpClient();
                var content = await httpClient.GetStringAsync(uri);
                try
                {
                    vpaTranslateResponse = JsonHelper.fromJson<VPATranslateResponse>(content);

                }
                catch (Exception ec)
                {
                    await logException(new VPAEnquiryException()
                    {
                        action = "Error deserializing translate response: " + content,
                        exception = ec.ToString(),
                        requestId = request.targetVPA,
                        logTime = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {

            }
            return vpaTranslateResponse;
        }
        public async Task<AccountInformation> accountEnquiry(AccountEnquiryRequest request)
        {
            AccountInformation ai = new AccountInformation();
            string reqString = string.Empty;
            string resultContent = string.Empty;
            string _ContentType = "application/json";
            string baseUri = _settings.Value.accountEnquiryUri;
            try
            {
                using (var client = new HttpClient())
                {
                    AccountEnquiryRequest areq = new AccountEnquiryRequest()
                    {
                        channelCode = request.channelCode,
                        instructedInstitutionCode = request.instructedInstitutionCode,
                        instructingInstitutionCode = request.instructingInstitutionCode,
                        requestId = request.requestId,
                        targetAccountNumber = request.targetAccountNumber
                    };
                    reqString = JsonHelper.toJson(areq);
                    var content = new StringContent(reqString, Encoding.UTF8, _ContentType);
                    var result = await client.PostAsync(baseUri + "accountenquiry", content);
                    resultContent = await result.Content.ReadAsStringAsync();
                };
                try
                {
                    ai = JsonHelper.fromJson<AccountInformation>(resultContent);
                }
                catch (Exception ec)
                {
                    await logException(new VPAEnquiryException()
                    {
                        action = "Error deserializing account enquiry response: " + resultContent,
                        exception = ec.ToString(),
                        requestId = request.requestId,
                        logTime = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {

            }
            return ai;
        }
        public async Task logException(VPAEnquiryException ex)
        {
            try
            {
                await _requestRepository.AddVPAEnquiryException(new VPAEnquiryException()
                {
                    action = ex.action,
                    exception = ex.exception,
                    requestId = ex.requestId,
                    logTime = DateTime.Now
                });
            }
            catch (Exception)
            {

            }
        }


        //Unit test Methods
        public async Task<AccountInformation> testAccountEnquiry(AccountEnquiryRequest arequest)
        {
            AccountInformation ai = new AccountInformation();
            try
            {
                // Open database (or create if not exits)
                PersonalInformation pi = new PersonalInformation();
                pi.email = "beitbart@yahoo.go";
                pi.mobilePhoneNumber = "2348012345678";

                AccountInformation a = new AccountInformation();
                a.accountCategory = 1;
                a.accountCurrency = "NGN";
                a.accountEntityName = "Eddy Murphy";
                a.accountFirstName = "Eddy";
                a.accountLastName = "Murphy";
                a.accountMiddleName = "Olawale";
                a.accountNumber = arequest.targetAccountNumber;
                a.accountStatus = "Active";
                a.accountType = "Savings";
                a.authorizationCredentialsAllowed = arequest.instructedInstitutionCode;
                a.authorizationCredentialsLength = "4";
                a.authorizationCredentialsType = "PIN";
                a.error = "";
                a.error_description = "";
                a.http_status_code = 200;
                a.personalinformation = pi;
                a.verificationNumber = "12345678909876543210";
                a.verificationNumberType = "BVN";

                string json = JsonHelper.toJson(a);
                try
                {
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    string result = await content.ReadAsStringAsync();
                    ai = JsonHelper.fromJson<AccountInformation>(result);

                }
                catch (Exception ec)
                {
                    testLogException(new VPAEnquiryException()
                    {
                        action = "Error deserializing translate response: " + json,
                        exception = ec.ToString(),
                        requestId = "Test",
                        logTime = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {

            }
            return ai;
        }
        public string testLogtranslate(string vpaid)
        {
            BsonValue bs = new BsonValue();
            try
            {
                // Open database (or create if not exits)
                using (var db = new LiteDatabase(@"MyData.db"))
                {
                    // Get requests collection
                    var translates = db.GetCollection<VPATranslateResponse>("translates");

                    Limit l = new Limit();
                    l.amount = 200;
                    l.hasLimit = true;

                    AssociatedVpas av = new AssociatedVpas();
                    av.vpaId = "merchant2@bank3";
                    av.limit = l;

                    ContactInformation c = new ContactInformation();
                    c.email = "usha@hotmail.com";
                    c.phone = "08012345678";

                    List<AssociatedVpas> avs = new List<AssociatedVpas>();
                    avs.Add(av);

                    IndividualInformation i = new IndividualInformation();
                    i.bvn = "12345678900987654123";
                    i.firstName = "Emeka";
                    i.lastName = "Olu";
                    i.middleName = "Musa";

                    MerchantInformation m = new MerchantInformation();
                    m.companyName = "Jumia";
                    m.rcNumber = "RC1234";
                    m.tin = "";

                    VPATranslateResponse v = new VPATranslateResponse();
                    v.accountCurrency = "NGN";
                    v.accountName = "Emeka Olu";
                    v.accountNumber = "0123456789";
                    v.associatedVpas = avs;
                    v.contactInformation = c;
                    v.error = "";
                    v.errorDescription = "";
                    v.hasAssociatedVpas = true;
                    v.httpStatusCode = 200;
                    v.individualInformation = i;
                    v.merchantInformation = m;
                    v.pspId = "bank1";
                    v.vpaId = vpaid;
                    v.vpaType = "Individual";

                    // Insert new customer document (Id will be auto-incremented)
                    bs = translates.Insert(v);
                }
            }
            catch (Exception ex)
            {

            }
            return bs.ToString();
        }
        public static VPATranslateResponse testvpaEnquiry(string vpaid)
        {
            IEnumerable<VPATranslateResponse> results = new List<VPATranslateResponse>();
            // Open database (or create if not exits)
            using (var db = new LiteDatabase(@"MyData.db"))
            {
                // Get customer collection
                var resp = db.GetCollection<VPATranslateResponse>("translates");

                results = resp.Find(x => x.vpaId == vpaid);
            }
            return results.FirstOrDefault();
        }
        public async Task<VPATranslateResponse> testvpaEnquiry(VPAEnquiryRequest vrequest)
        {
            VPATranslateResponse vpaTranslateResponse = new VPATranslateResponse();
            Limit l = new Limit();
            l.amount = 200;
            l.hasLimit = true;

            AssociatedVpas av = new AssociatedVpas();
            av.vpaId = "merchant2@bank3";
            av.limit = l;

            ContactInformation c = new ContactInformation();
            c.email = "usha@hotmail.com";
            c.phone = "08012345678";

            List<AssociatedVpas> avs = new List<AssociatedVpas>();
            avs.Add(av);

            IndividualInformation i = new IndividualInformation();
            i.bvn = "12345678900987654123";
            i.firstName = vrequest.instructedInstitutionCode;
            i.lastName = "Olu";
            i.middleName = "Musa";

            MerchantInformation m = new MerchantInformation();
            m.companyName = "Jumia";
            m.rcNumber = "RC1234";
            m.tin = "";

            VPATranslateResponse v = new VPATranslateResponse();
            v.accountCurrency = "NGN";
            v.accountName = "Emeka Olu";
            v.accountNumber = "0123456789";
            v.associatedVpas = avs;
            v.contactInformation = c;
            v.error = "";
            v.errorDescription = "";
            v.hasAssociatedVpas = true;
            v.httpStatusCode = 200;
            v.individualInformation = i;
            v.merchantInformation = m;
            v.pspId = "bank1";
            v.vpaId = vrequest.targetVPA;
            v.vpaType = "Individual";

            string json = JsonHelper.toJson(v);
            try
            {
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                string result = await content.ReadAsStringAsync();
                //vpaTranslateResponse = (VPATranslateResponse)JsonConvert.DeserializeObject(content);
                vpaTranslateResponse = JsonHelper.fromJson<VPATranslateResponse>(result);

            }
            catch (Exception ec)
            {
                testLogException(new VPAEnquiryException()
                {
                    action = "Error deserializing translate response: " + json,
                    exception = ec.ToString(),
                    requestId = "Test",
                    logTime = DateTime.Now
                });
            }
            return vpaTranslateResponse;
        }
        public string testLogRequest(VPAEnquiryRequest request)
        {
            BsonValue bs = new BsonValue();
            try
            {
                // Open database (or create if not exits)
                using (var db = new LiteDatabase(@"MyData.db"))
                {
                    // Get requests collection
                    var requests = db.GetCollection<VPAEnquiryRequest>("requests");


                    // Insert new customer document (Id will be auto-incremented)
                    bs = requests.Insert(new VPAEnquiryRequest()
                    {
                        channelCode = request.channelCode,
                        instructedInstitutionCode = request.instructedInstitutionCode,
                        instructingInstitutionCode = request.instructingInstitutionCode,
                        requestId = request.requestId,
                        targetVPA = request.targetVPA,
                        updatedOn = request.updatedOn
                    });
                }
            }
            catch (Exception ex)
            {

            }
            return bs.ToString();
        }
        public static  string testLogException(VPAEnquiryException exception)
        {
            BsonValue bs = new BsonValue();
            try
            {
                // Open database (or create if not exits)
                using (var db = new LiteDatabase(@"MyData.db"))
                {
                    // Get requests collection
                    var exceptions = db.GetCollection<VPAEnquiryException>("exceptions");


                    // Insert new customer document (Id will be auto-incremented)
                    bs = exceptions.Insert(new VPAEnquiryException()
                    {
                        action = exception.action,
                        exception = exception.exception,
                        logTime = exception.logTime,
                        requestId = exception.requestId
                    });
                }
            }
            catch (Exception ex)
            {

            }
            return bs.ToString();
        }
    }
}