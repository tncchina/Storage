using System.Net;
using System.Web.Http;
using ApiApp.Models;
using Swashbuckle.Swagger.Annotations;

namespace ApiApp.Controllers
{
    public class ActionsController : ApiController
    {
        [Route("api/actions/uploadbatch")]
        [HttpPost]
        [SwaggerOperation("Upload")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public UploadBatchResponse UploadImageBatch([FromBody]UploadBatchRequest value)
        {
            return new UploadBatchResponse { BatchTag = value.BatchTag, ContainerUrl = "http://blob.url.sas" };
        }
    }
}
