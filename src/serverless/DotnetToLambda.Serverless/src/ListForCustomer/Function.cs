using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using DotnetToLambda.Core.ViewModels;
using DotnetToLambda.Core.Models;
using DotnetToLambda.Serverless.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<ApiGatewayProxyJsonSerializerContext>))]

ServerlessConfig.ConfigureServices();

var _bookingRepository = ServerlessConfig.Services.GetRequiredService<IBookingRepository>();

var handler = async (APIGatewayProxyRequest apigProxyEvent, ILambdaContext context) =>
{
    if (!apigProxyEvent.PathParameters.ContainsKey("customerId"))
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = 400,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    var customerId = apigProxyEvent.PathParameters["customerId"];
            
    context.Logger.LogInformation($"Received request to list bookings for: {customerId}");

    var customerBookings = await _bookingRepository.ListForCustomer(customerId);
            
    return new APIGatewayProxyResponse
    {
        Body = JsonSerializer.Serialize(customerBookings),
        StatusCode = 200,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };  
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();
