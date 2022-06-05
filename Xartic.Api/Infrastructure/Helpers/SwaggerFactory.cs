using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Reflection;

namespace Xartic.Api.Infrastructure.Helpers
{
    internal static class SwaggerFactory
    {
        #region Public Methods

        public static SwaggerGenOptions CreateSwaggerOptions(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", BuildApiInfo());
            options.IncludeXmlComments(GetXmlDocPath());
            return options;
        }

        public static IApplicationBuilder ConfigureSwagger(IApplicationBuilder app) =>
            app.UseSwagger().UseSwaggerUI(BuildOptions);

        #endregion

        #region Private Methods

        private static void BuildOptions(SwaggerUIOptions c)
        {
            c.RoutePrefix = Constants.Documentation.ROUTE_PREFIX;
            c.InjectJavascript(Constants.Documentation.CUSTOM_JS);
            c.InjectStylesheet(Constants.Documentation.CUSTOM_CSS);
            c.SwaggerEndpoint(Constants.Documentation.ENDPOINT, Constants.ApiInfo.TITLE);
        }

        private static string GetXmlDocPath()
        {
            var application = PlatformServices.Default.Application;
            var applicationPath = application.ApplicationBasePath;
            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;

            var xmlPath = Path.Combine(applicationPath, $"{applicationName}.xml");
            return xmlPath;
        }

        private static OpenApiInfo BuildApiInfo()
        {
            return new OpenApiInfo
            {
                Title = Constants.ApiInfo.TITLE,
                Version = Constants.ApiInfo.VERSION,
                Description = Constants.ApiInfo.DESCRIPTION,
                Contact = BuildContact()
            };
        }

        private static OpenApiContact BuildContact()
        {
            return new OpenApiContact
            {
                Name = Constants.ApiInfo.Contact.NAME,
                Url = new Uri(Constants.ApiInfo.Contact.URL),
                Email = Constants.ApiInfo.Contact.EMAIL
            };
        }
        #endregion
    }
}
