using System.Web.Http;

namespace ApiApp
{
#pragma warning disable SA1649 // File name must match first type name
    public class WebApiApplication : System.Web.HttpApplication
#pragma warning restore SA1649 // File name must match first type name
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
