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
    var request = JsonSerializer.Deserialize<ConfirmBookingDTO>(apigProxyEvent.Body);

    context.Logger.LogInformation("Received request to confirm a booking:");

    context.Logger.LogInformation(JsonSerializer.Serialize(request));

    var existingBooking = await _bookingRepository.Retrieve(request.BookingId);

    if (existingBooking == null)
    {
        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(new
            {
                bookingId = request.BookingId,
                message = "Booking not found"
            }),
            StatusCode = 404,
            Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
        };
    }

    context.Logger.LogInformation($"Found booking, confirmed");

    existingBooking.Confirm();

    await _bookingRepository.Update(existingBooking);

    context.Logger.LogInformation("Booking updated");

    return new APIGatewayProxyResponse
    {
        Body = JsonSerializer.Serialize(existingBooking),
        StatusCode = 200,
        Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
    };
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();
