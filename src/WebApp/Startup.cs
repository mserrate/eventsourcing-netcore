using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Core;
using Domain.EventStore;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApp.Projections;
using WebApp.ViewModels;

namespace WebApp
{
    public class Startup
    {
        private IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            _environment = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEventStoreConnection>(GetEventStoreConnection());
            services.AddTransient<IAsyncRepository, EventStoreRepository>();
            services.AddTransient(typeof(IModelState<ShoppingCartViewModel>), typeof(ShoppingCartState));
            services.AddSingleton<ProductsCache>();

            SetupProjections();
            // Add framework services.
            services.AddMvc();
            services.AddMemoryCache();
        }

        private IEventStoreConnection GetEventStoreConnection()
        {
            var connection = EventStoreConnection.Create(Configuration.GetConnectionString("EventStore"));
            connection.ConnectAsync().Wait();
            return connection;
        }

        private void SetupProjections()
        {
            ProjectionSetup.Run(_environment.ContentRootPath).Wait();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
