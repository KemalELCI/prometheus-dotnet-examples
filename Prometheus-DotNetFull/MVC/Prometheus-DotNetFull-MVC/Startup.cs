using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Prometheus_DotNetFull_MVC.Startup))]
namespace Prometheus_DotNetFull_MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            PrometheusConfig.ConfigurePrometheus(app);
        }
    }
}
