using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xartic.Api.Abstractions;
using Xartic.Api.Hub;
using Xartic.Api.Infrastructure;
using Xartic.Api.Infrastructure.Helpers;

namespace Xartic.Api
{
    public class Startup
    {
        private const string AUTENTICATION = "BasicAuthentication";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IConnectionMonitor<XarticHub>, XarticHubConnectionMonitor>();

            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials();
            }));

            services.AddSwaggerGen(c => SwaggerFactory.CreateSwaggerOptions(c));

            //Adicionando schema de autenticação
            //services.AddAuthentication(AUTENTICATION)
            //        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AUTENTICATION, null);

            //Adicionando o SignalR self-hosted
            var signalService = services
                .AddSignalR(configuration => configuration.EnableDetailedErrors = Debugger.IsAttached)
                .AddJsonProtocol()
                .AddMessagePackProtocol(options =>
                {
                    options.FormatterResolvers = new List<MessagePack.IFormatterResolver>()
                    {
                        MessagePack.Resolvers.StandardResolver.Instance
                    };
                });

            signalService.AddHubOptions<XarticHub>(options => options.MaximumReceiveMessageSize = 102400000);

            // Para usar o host do Azure
            //signalService.AddAzureSignalR(options => {
            //    options.ConnectionString = signalConnectionString;
            //});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if(!Debugger.IsAttached)
                app.UseHttpsRedirection();

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseCors("CorsPolicy");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            //Para utilizar o host do Azure
            //app.UseAzureSignalR(routes => routes.MapHub<XarticHub>("/Xartic"));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<XarticHub>("/Xartic");
                endpoints.MapControllers();
            });

            SwaggerFactory.ConfigureSwagger(app);
        }
    }
}
