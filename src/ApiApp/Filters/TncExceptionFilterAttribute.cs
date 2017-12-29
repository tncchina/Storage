using System.Net.Http;
using System.Web.Http.Filters;

using ApiApp.Common;
using ApiApp.Models;
using Newtonsoft.Json;


namespace ApiApp.Filters
{
    public class TncExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            TncException tncException = context.Exception as TncException;
            if (tncException != null)
            {
                context.Response = new HttpResponseMessage(tncException.StatusCode)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new OperationStatus { Code = tncException.ErrorCode, Message = tncException.ErrorMessage }))
                };
            }
        }
    }
}