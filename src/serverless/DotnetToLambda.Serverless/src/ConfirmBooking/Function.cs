using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DotnetToLambda.Core.ViewModels;
using DotnetToLambda.Core.Models;
using DotnetToLambda.Serverless.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ConfirmBooking
{
    public class Function
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<Function> _logger;
        
        public Function()
        {
            ServerlessConfig.ConfigureServices();

            this._bookingRepository = ServerlessConfig.Services.GetRequiredService<IBookingRepository>();
            this._logger = ServerlessConfig.Services.GetRequiredService<ILogger<Function>>();
        }
        
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var request = JsonSerializer.Deserialize<ConfirmBookingDTO>(apigProxyEvent.Body);
            
            this._logger.LogInformation("Received request to confirm a booking:");

            this._logger.LogInformation(JsonSerializer.Serialize(request));

            var existingBooking = await this._bookingRepository.Retrieve(request.BookingId);

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
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            this._logger.LogInformation($"Found booking, confirmed");

            existingBooking.Confirm();

            await this._bookingRepository.Update(existingBooking);

            this._logger.LogInformation("Booking updated");
            
            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(existingBooking),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
