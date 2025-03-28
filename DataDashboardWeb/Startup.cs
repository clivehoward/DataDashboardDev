using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Rewrite;

namespace DataDashboardWeb
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
            services.AddMvc();

            var appSettings = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettings);

            // CONFIGURE FOR USE OF SESSION STATE /////////////////////////
            // We are using Redis Cache which is configured here (Microsoft.Extensions.Caching.Redis.Core)
            // Adds a default in-memory implementation of IDistributedCache.
            //services.AddDistributedMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = appSettings.GetValue<string>("RedisInstanceName");
                options.Configuration = appSettings.GetValue<string>("RedisConfiguration");
            });

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                //options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
            });
            // CONFIGURE FOR USE OF SESSION STATE /////////////////////////
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            // CONFIGURE FOR USE OF SESSION STATE /////////////////////////
            app.UseSession();
            // CONFIGURE FOR USE OF SESSION STATE /////////////////////////

            // This should force HTTPS for all requests by redirecting accordingly
            var options = new RewriteOptions();
            options.Rules.Add(new NonWwwRule());
            options.AddRedirectToHttpsPermanent();
            app.UseRewriter(options);

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
