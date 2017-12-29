using System.Collections.Generic;
using System.Web.Http.Filters;
using ApiApp.Common;

namespace ApiApp.Filters
{
    public class CustomHeaderFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            object headersObject;
            if (actionExecutedContext.Request.Properties.TryGetValue(Constants.CustomResponseHeaders, out headersObject))
            {
                Dictionary<string, string> headers = headersObject as Dictionary<string, string>;

                if (headers != null)
                {
                    var responseHeaders = actionExecutedContext.Response.Headers;
                    foreach (var header in headers)
                    {
                        responseHeaders.Add(header.Key, header.Value);
                    }
                }
            }
        }
    }
}