#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using Voat.Common;
using Voat.Common.Components;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Http.Filters;
using Voat.Http.Middleware;
using Voat.UI.Areas.Admin;
using Voat.UI.Runtime;
using Voat.UI.Utils;
using Voat.Utilities;

namespace Voat
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }
            
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            
            //Configure Voat Runtime 
            Configuration.ConfigureVoat();

        }

        public static IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.RequireHeaderSymmetry = false;
                //Trust forwarding headers only from local/loopback proxies and RFC1918
                //(docker/Caddy/nginx sidecar networks) — mirrors HttpExtensions.RemoteAddress.
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(System.Net.IPAddress.Parse("10.0.0.0"), 8));
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(System.Net.IPAddress.Parse("172.16.0.0"), 12));
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(System.Net.IPAddress.Parse("192.168.0.0"), 16));
            });

            services.AddDbContext<IdentityDataContext>();

            services.AddIdentity<VoatIdentityUser, IdentityRole>(x => {
                x.Password = VoatSettings.Instance.PasswordOptions;
            }).AddEntityFrameworkStores<IdentityDataContext>()
            .AddUserManager<VoatUserManager>()
            .AddDefaultTokenProviders();
            
           
            services.ConfigureApplicationCookie(options => {
                if (!String.IsNullOrEmpty(VoatSettings.Instance.CookieDomain))
                {
                    options.Cookie.Domain = VoatSettings.Instance.CookieDomain;
                }
                if (!String.IsNullOrEmpty(VoatSettings.Instance.CookieName))
                {
                    options.Cookie.Name = VoatSettings.Instance.CookieName;
                }
                //Explicit cookie policy (was implicit framework defaults).
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.AccessDeniedPath = new PathString("/error/default");
            });

            var mvcBuilder = services.AddMvc(options =>
            {
                // Preserve the legacy IRouteBuilder route table (RouteConfig) on ASP.NET Core 10:
                // UseMvc requires endpoint routing to be disabled.
                options.EnableEndpointRouting = false;
            });

            services.Configure<RazorViewEngineOptions>(options => {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            mvcBuilder.AddMvcOptions(o => {
                o.Filters.Add(typeof(GlobalExceptionFilter));
                o.Filters.Add(typeof(RuntimeStateFilter));
                o.Filters.Add(typeof(RouteLoggerFilter));
            });

            services.AddAntiforgery(options => {
                options.HeaderName = Utilities.CONSTANTS.REQUEST_VERIFICATION_HEADER_NAME;
                options.FormFieldName = Utilities.CONSTANTS.REQUEST_VERIFICATION_HEADER_NAME;
            });
            
            // Add application services.
            services.AddScoped<IViewRenderService, ViewRenderService>();
            //services.AddScoped<VoatUserManager, VoatUserManager>();

            //services.AddTransient<IEmailSender, AuthMessageSender>();
            //services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddLogging(loggingBuilder =>
            {
                //Can not figure out how to filter out native log statements (i.e. Microsoft.AspNetCore.Mvc...) from logging
                //using the default format json. So have to implement a custom filter.
                //loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                var filter = new LoggingFilter();
                loggingBuilder.AddFilter(filter.Filter);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Console and debug providers moved to ConfigureServices(AddLogging); the ILoggerFactory
            // AddConsole/AddDebug overloads are obsolete on modern ASP.NET Core.
            loggerFactory.AddProvider(new VoatLoggerProvider());

            //Honor X-Forwarded-* from trusted proxies before anything reads scheme or client ip.
            //(The options were configured in ConfigureServices but the middleware was never added.)
            app.UseForwardedHeaders();

            //Security headers (a strict CSP is intentionally not set: 250 legacy views rely on
            //inline scripts/handlers; enforcing one without a rewrite would break features).
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                await next();
            });

            if (!env.IsDevelopment() && VoatSettings.Instance.ForceHTTPS)
            {
                app.UseHsts();
            }
            
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage(); // removed in .NET 5+ (EF migrations diagnostics); not needed since Voat runs on hand-written SQL scripts
                //app.UseBrowserLink();
                VoatSettings.Instance.IsDevelopment = true;
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/error/status/{0}");
            }
            
            ////Configure Voat Middleware
            app.UseVoatGlobalExceptionLogger();

            app.UseStaticFiles();
            // UseIdentity() became UseAuthentication() + UseAuthorization() in ASP.NET Core 3.0+.
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseVoatRequestDurationLogger();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
            app.UseVoatRuntimeState();

            app.UseMvc(routes =>
            {
                RouteConfig.RegisterRoutes(routes);
            });

            //Construct Statics
            FilePather.Instance = new FilePather(env.WebRootPath);
        }
    }
}
