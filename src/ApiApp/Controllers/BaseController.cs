using System.Net;
using System.Web.Http;
using ApiApp.Models;
using Swashbuckle.Swagger.Annotations;

namespace ApiApp.Controllers
{
    public class BaseController : ApiController
    {
        private AppConfiguration appConfiguration = new AppConfiguration();

        protected AppConfiguration AppConfiguration => this.appConfiguration;
    }
}
