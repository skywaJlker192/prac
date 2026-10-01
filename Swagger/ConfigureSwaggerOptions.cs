using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskFlowApi.Swagger;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            var info = new OpenApiInfo
            {
                Title = "TaskFlow API",
                Version = description.ApiVersion.ToString(),
                Description = "REST API для управления задачами"
            };

            if (description.IsDeprecated)
            {
                info.Description += " ВНИМАНИЕ: эта версия API устарела (deprecated), используйте более новую.";
            }

            options.SwaggerDoc(description.GroupName, info);
        }
    }
}
