using System.Web.Http;
using ApiApp.Common;

namespace ApiApp.Controllers
{
    public class BaseController : ApiController
    {
        private AppConfiguration appConfiguration = new AppConfiguration();

        protected AppConfiguration AppConfiguration => this.appConfiguration;

        protected IHttpActionResult Accepted()
        {
            return new AcceptedHttpResult();
        }
    }
}
