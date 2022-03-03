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
    if (!apigProxyEvent.PathParameters.ContainsKey("bookingId"))
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = 400,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
            
    var bookingId = apigProxyEvent.PathParameters["bookingId"];
            
    context.Logger.LogInformation($"Received request for booking {bookingId}");

    var booking = await _bookingRepository.Retrieve(bookingId);

    if (booking == null)
    {
        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(new
            {
                bookingId = bookingId,
                message = "Booking not found"
            }),
            StatusCode = 404,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
            
    return new APIGatewayProxyResponse
    {
        Body = JsonSerializer.Serialize(booking),
        StatusCode = 200,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
    };  
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();
