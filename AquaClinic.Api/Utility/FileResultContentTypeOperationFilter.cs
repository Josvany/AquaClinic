using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace AquaClinic.Api.Utility
{
    public class FileResultContentTypeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var requestAttribute = context.MethodInfo.GetCustomAttributes(typeof(FileResultContentTypeAttribute), false)
                .Cast<FileResultContentTypeAttribute>()
                .FirstOrDefault();

            if (requestAttribute == null) return;

            operation.Responses.Clear();
            operation.Responses.Add("200", new OpenApiResponse
            {
                Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    requestAttribute.ContentType, new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        }
                    }
                }
            }
            });
        }
    }
}
