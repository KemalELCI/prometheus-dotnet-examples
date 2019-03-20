using Owin;
using Prometheus.Client;
using Prometheus.Client.Owin;
using System.Diagnostics;

namespace Prometheus_DotNetFull_WebForms
{
    public static class PrometheusConfig
    {
        private static readonly Counter RequestCount = Metrics
            .CreateCounter("Requests_Count", "Number of Requests", labelNames: new[] { "method", "endpoint" });

        private static readonly Histogram RequestTimerHistogram = Metrics
            .CreateHistogram("Requests_Time_Histogram", "Duration of Requests", labelNames: new[] { "method", "endpoint" });

        private static readonly Gauge RequestTimerGauge = Metrics
           .CreateGauge("Requests_Time_Gauge", "Duration of Requests", labelNames: new[] { "method", "endpoint" });

        public static void ConfigurePrometheus(IAppBuilder app)
        {
            app.UsePrometheusServer();

            app.Use(async (context, next) =>
            {
                var watch = Stopwatch.StartNew();
                await next();
                watch.Stop();
                RequestTimerHistogram.Labels(context.Request.Method, context.Request.Uri.AbsoluteUri).Observe(watch.Elapsed.TotalSeconds);
                RequestTimerGauge.Labels(context.Request.Method, context.Request.Uri.AbsoluteUri).Set(watch.Elapsed.TotalSeconds);
            });

            app.Use(async (context, next) =>
            {
                RequestCount.Labels(context.Request.Method, context.Request.Uri.AbsoluteUri).Inc();
                await next();
            });
        }
    }
}

