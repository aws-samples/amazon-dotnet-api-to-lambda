using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using DotnetToLambda.Core.Exceptions;
using DotnetToLambda.Core.ViewModels;
using DotnetToLambda.Core.Models;
using DotnetToLambda.Core.Services;
using DotnetToLambda.Serverless.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<ApiGatewayProxyJsonSerializerContext>))]

ServerlessConfig.ConfigureServices();

var _bookingRepository = ServerlessConfig.Services.GetRequiredService<IBookingRepository>();
var _customerService = ServerlessConfig.Services.GetRequiredService<ICustomerService>();

var handler = async (APIGatewayProxyRequest apigProxyEvent, ILambdaContext context) =>
{
    var request = JsonSerializer.Deserialize<ReserveBookingDTO>(apigProxyEvent.Body);
            
    context.Logger.LogInformation("Received request to reserve a new booking:");

    context.Logger.LogInformation(JsonSerializer.Serialize(request));
            
    if (!await _customerService.CustomerExists(request.CustomerId))
    {
        context.Logger.LogWarning($"Customer {request.CustomerId} does not exist in the customer service");
                
        return new APIGatewayProxyResponse
        {
            Body = "{\"message\": \"Customer not found\"}",
            StatusCode = 400,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    var booking = Booking.Create(Guid.NewGuid().ToString(), request.CustomerId, request.OutboundFlightId, request.ChargeId);

    context.Logger.LogInformation($"Booking created with Id ${booking.BookingId}");

    try
    {
        await _bookingRepository.Add(booking);
    }
    catch (DuplicateRequestException ex)
    {
        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(ex.DuplicateBooking),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    context.Logger.LogInformation("Booking added");
            
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