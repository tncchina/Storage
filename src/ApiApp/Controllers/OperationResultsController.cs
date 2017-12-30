using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ApiApp.Common;
using ApiApp.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Swashbuckle.Swagger.Annotations;

namespace ApiApp.Controllers
{
    public class OperationResultsController : BaseController
    {
        [HttpGet]
        [SwaggerOperation("Get Operation Result")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IModelResult> Get(string id)
        {
            OperationResult result;
            ResourceResponse<Document> response = null;

            try
            {
                try
                {
                    response = await this.AppConfiguration.CosmosDBClient.ReadDocumentAsync(
                        UriFactory.CreateDocumentUri(this.AppConfiguration.DatabaseId, this.AppConfiguration.OperationResultCollectionId, id),
                        new RequestOptions() { PartitionKey = new PartitionKey(id) });
                }
                catch (DocumentClientException ex)
                {
                    if (ex.Error?.Code == "NotFound")
                    {
                        return new OperationStatus { Code = ErrorCode.OperationNotFound, Message = $"Operation with id='{id}' not found in database" };
                    }
                }

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
                    var queryParameters = HttpUtility.ParseQueryString(this.ActionContext.Request.RequestUri.Query);
                    string documentDBSkipToken = queryParameters.AllKeys.Contains(Constants.SkipToken) ? Encoding.UTF8.GetString(Convert.FromBase64String(queryParameters[Constants.SkipToken])) : null;

                    FeedResponse<AnimalImage> feedResponse;
                    using (IDocumentQuery<AnimalImage> animalImageQuery = this.AppConfiguration.CosmosDBClient.CreateDocumentQuery<AnimalImage>(
                        this.AppConfiguration.AnimalImageCollectionId,
                        new FeedOptions { MaxItemCount = this.AppConfiguration.MaxCosmosDBItemCount, RequestContinuation = documentDBSkipToken })
                        .Where(a => a.UploadBatchId == id).AsDocumentQuery())
                    {
                        feedResponse = await animalImageQuery.ExecuteNextAsync<AnimalImage>();
                    }

                    List<AnimalImage> animalImages = feedResponse.ToList();

                    CloudBlobClient blobClient = this.AppConfiguration.BlobStorageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference(this.AppConfiguration.AnimalImageContainer);

                    animalImages.ForEach(a => Utils.GenerateWriteSasUrl(container.GetBlockBlobReference(a.ImageBlob), this.AppConfiguration.BatchUploadSasExpirationHours));

                    return new UploadBatchResponse
                    {
                        Animals = animalImages.ToArray(),
                        NextLink = CreateNextLink(this.ActionContext.Request.RequestUri, feedResponse.ResponseContinuation)
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationStatus { Code = "UnexpectedException", Message = ex.ToString() };
            }
        }

        private static string CreateNextLink(Uri requestUri, string responseContinuation)
        {
            if (string.IsNullOrEmpty(responseContinuation))
            {
                return null;
            }

            NameValueCollection queryParameters = HttpUtility.ParseQueryString(requestUri.Query);
            queryParameters[Constants.SkipToken] = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseContinuation));

            UriBuilder uriBuilder = new UriBuilder(requestUri.Scheme, requestUri.Host, requestUri.Port, requestUri.LocalPath)
            {
                Query = queryParameters.ToString()
            };

            return uriBuilder.ToString();
        }
    }
}
