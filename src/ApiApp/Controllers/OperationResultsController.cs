using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

using ApiApp.Common;
using ApiApp.Models;
using Microsoft.Azure.Documents.Client;
using Swashbuckle.Swagger.Annotations;

namespace ApiApp.Controllers
{
    [Route("api/operationresults")]
    public class OperationResultsController : BaseController
    {
        [HttpGet]
        [SwaggerOperation("Get Operation Result")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IModelResult> Get(string id)
        {
            OperationResult result;
            var response = await this.AppConfiguration.CosmosDBClient.ReadDocumentAsync(
                UriFactory.CreateDocumentUri(this.AppConfiguration.DatabaseId, this.AppConfiguration.OperationResultCollectionId, id),
                new RequestOptions() { });

            result = (OperationResult)(dynamic)response.Resource;

            if (result.Status.Code == OperationStage.InProgress)
            {
                this.ActionContext.Response.Headers.Location = this.Request.RequestUri;
                this.ActionContext.Response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(Constants.RetryIntervalInSeconds));
                this.ActionContext.Response.StatusCode = HttpStatusCode.Accepted;

                return null;
            }
            else if (result.Status.Code == OperationStage.Fail)
            {
                return new OperationStatus { Code = ErrorCode.LongOperationFailed, Message = result.Status.Message };
            }
            else
            {
                return null;
            }
        }
    }
}
