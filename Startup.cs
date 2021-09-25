using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Depo.Components;
using Depo.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;

namespace Depo
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
            services.AddTransient<DownloadStorage>();
            services.AddTransient<Cache>();
            services.AddControllers();
            services.AddSingleton<DriveApiService>();
            services.AddMemoryCache();
            services.AddSentry();
            services.AddCors(options =>
            {
                
                options.AddDefaultPolicy(
                    builder => builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders();
            app.UseSentryTracing();
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();
            app.UseMiddleware<Firewall>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
            });
            
            //DriveAPI.StartDriveAPI();
        }
    }
}
