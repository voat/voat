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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Voat.Common.Components;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Http.Filters;
using Voat.Http.Middleware;
using Voat.UI.Runtime;

namespace Voat
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
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

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            //var conn = $"127.0.1:6379";
            //var redis = ConnectionMultiplexer.Connect(conn);
            //services.AddDataProtection().PersistKeysToRedis(redis, "DataProtection-Keys");

            //services.AddDistributedRedisCache(option =>
            //{
            //    option.Configuration = conn;
            //    //option.InstanceName = "master";
            //});
            //services.AddSession();

            services.AddDbContext<IdentityDataContext>();

            services.AddIdentity<VoatIdentityUser, IdentityRole>(x => {
                //TODO: This needs to be an abstraction 
                x.Password.RequireDigit = false;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireUppercase = false;
                x.Password.RequireLowercase = false;
            }).AddEntityFrameworkStores<IdentityDataContext>()
            .AddDefaultTokenProviders();


            var mvcBuilder = services.AddMvc();

            mvcBuilder.AddMvcOptions(o => {
                o.Filters.Add(typeof(GlobalExceptionFilter));
                o.Filters.Add(typeof(RuntimeStateFilter));
                //o.Filters.Add(typeof(RouteLoggerFilter));
            });

            services.AddAntiforgery(options => {
                options.HeaderName = "__RequestVerificationToken";
            });
            
            // Add application services.
            //services.AddTransient<IEmailSender, AuthMessageSender>();
            //services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddProvider(new VoatLoggerProvider());

            ////Configure Voat Middleware
            app.UseVoatGlobalExceptionLogger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
                VoatSettings.Instance.IsDevelopment = true;
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseIdentity();

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
