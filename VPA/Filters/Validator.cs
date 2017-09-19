using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using VPA.Models;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace VPA.Filters
{

    //public class Validator: IAuthorizationRequirement
    public class Validator : IValidatableObject
    {
        public IEnumerable<object> Validate(object o, string parent = null)
        {
            Type type = o.GetType();
            PropertyInfo[] properties = type.GetProperties();
            Type attrType = typeof(ValidationAttribute);

            foreach (var propertyInfo in properties)
            {
                var customAttributes = propertyInfo.GetCustomAttributes(attrType, inherit: true);

                foreach (var customAttribute in customAttributes)
                {
                    var validationAttribute = (ValidationAttribute)customAttribute;
                    var isValid = validationAttribute.IsValid(propertyInfo.GetValue(o));

                    if (!isValid)
                    {
                        yield return new
                        {
                            source = parent == null ? propertyInfo.Name : $"{parent}/{propertyInfo.Name}",
                            title = "Invalid Attribute",
                            detail = validationAttribute.ErrorMessage
                        };
                    }
                }
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
        //const string HVal = "HVal";

        //public void OnActionExecuted(ActionExecutedContext context)
        //{
        //    //throw new NotImplementedException();
        //}

        //public void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    string ActionName = filterContext.ActionDescriptor.DisplayName;
        //    IDictionary<string, object> obj = filterContext.ActionArguments;

        //    try
        //    {
        //        if (obj.Count > 0)
        //        {
        //            foreach (var o in obj.Keys)
        //            {
        //                if (filterContext.HttpContext.Request.Headers.Keys.Contains(HVal))
        //                {
        //                    var h_val = filterContext.HttpContext.Request.Headers[HVal];
        //                    //var json = new JavaScriptSerializer().Serialize(obj[o]);
        //                    var json = JsonHelper.toJson(obj[o]);
        //                    string jsonString = json.ToString();

        //                    // Validate Token
        //                    if (!validateToken(jsonString, h_val))
        //                    {
        //                        //var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Invalid Request" };
        //                        filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //                    }
        //                }
        //                else
        //                {
        //                    filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //                    //new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Invalid Request" };
        //                }
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    }
        //    throw new NotImplementedException();
        //}
        //public bool validateToken(string requestjson, string h_val)
        //{
        //    bool isValid = false;
        //    try
        //    {
        //        //string new_h_val = Encrypt(requestjson);
        //        string new_h_val = requestjson;
        //        return new_h_val == h_val;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return isValid;
        //}
    }
}
