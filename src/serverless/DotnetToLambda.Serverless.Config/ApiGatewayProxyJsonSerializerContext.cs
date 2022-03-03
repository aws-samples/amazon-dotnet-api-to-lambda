using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace DotnetToLambda.Serverless.Config
{
    [JsonSerializable(typeof(APIGatewayProxyRequest))]
    [JsonSerializable(typeof(APIGatewayProxyResponse))]
    public partial class ApiGatewayProxyJsonSerializerContext : JsonSerializerContext
    {
    }
}