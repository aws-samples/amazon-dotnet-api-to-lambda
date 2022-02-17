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

namespace Retrieve
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
            if (!apigProxyEvent.PathParameters.ContainsKey("bookingId"))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            
            var bookingId = apigProxyEvent.PathParameters["bookingId"];
            
            this._logger.LogInformation($"Received request for booking {bookingId}");

            var booking = await this._bookingRepository.Retrieve(bookingId);

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
        }
    }
}
