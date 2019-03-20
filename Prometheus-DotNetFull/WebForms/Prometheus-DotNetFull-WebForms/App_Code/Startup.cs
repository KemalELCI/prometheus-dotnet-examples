using Microsoft.Owin;
using Owin;
using Prometheus.Client;
using Prometheus.Client.Owin;

[assembly: OwinStartupAttribute(typeof(Prometheus_DotNetFull_WebForms.Startup))]
namespace Prometheus_DotNetFull_WebForms
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
            PrometheusConfig.ConfigurePrometheus(app);
        }
    }
}
