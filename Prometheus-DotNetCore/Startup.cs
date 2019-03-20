using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.Client.AspNetCore;
using Prometheus.Client.HttpRequestDurations;
using Microsoft.AspNetCore.Http.Extensions;
using Prometheus.Client;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Prometheus_DotNetCore
{
    public class Startup
    {


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            // Prometheus
            app.UsePrometheusServer();
            app.UsePrometheusRequestDurations(q =>
            {
                q.IncludePath = true;
                q.IncludeMethod = true;
                q.IncludeStatusCode = true;
                q.IgnoreRoutesConcrete = new[] // Ignore some concrete routes
                {
                    "/favicon.ico",
                    "/robots.txt"
                };
                q.IgnoreRoutesStartWith = new[]
                {
                    "/swagger" // Ignore '/swagger/*'
                };
                q.CustomNormalizePath = new Dictionary<Regex, string>
                {
                    { new Regex(@"\/[0-9]{1,}(?![a-z])"), "/id" } // Replace 'int' in Route
                };
            });


            var reqCounter = Metrics.CreateCounter("Requests_Count", "Number of Requests", labelNames: new[] { "method", "endpoint" });
            app.Use(async (context, next) =>
            {
                reqCounter.Labels(context.Request.Method, context.Request.GetDisplayUrl()).Inc();
                await next();
            });

            var reqTimer = Metrics.CreateGauge("Requests_Time", "Duration of Requests", labelNames: new[] { "method", "endpoint" });
            app.Use(async (context, next) =>
            {
                var watch = Stopwatch.StartNew();
                await next();
                watch.Stop();
                reqTimer.Labels(context.Request.Method, context.Request.GetDisplayUrl()).Set(watch.Elapsed.TotalSeconds);
            });
            //

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
