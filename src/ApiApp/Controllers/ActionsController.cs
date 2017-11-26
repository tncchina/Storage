using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using ApiApp.Models;

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
