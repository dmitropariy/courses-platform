using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace courses_platform
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var desc in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                {
                    Title = "Versioned API Demo",
                    Version = desc.ApiVersion.ToString(),
                    Description = $"API Version {desc.ApiVersion}"
                });
            }

            //options.DocInclusionPredicate((docName, apiDesc) =>
            //{
            //    var model = apiDesc.ActionDescriptor?.GetApiVersionModel();
            //    if (model == null) return false;
            //    return model.DeclaredApiVersions.Any(v => $"v{v}" == docName) ||
            //           model.ImplementedApiVersions.Any(v => $"v{v}" == docName);
            //});
        }
    }
}
