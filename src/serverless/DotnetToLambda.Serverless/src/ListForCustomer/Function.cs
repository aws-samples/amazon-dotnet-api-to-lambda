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

namespace ListForCustomer
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
            if (!apigProxyEvent.PathParameters.ContainsKey("customerId"))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var customerId = apigProxyEvent.PathParameters["customerId"];
            
            this._logger.LogInformation($"Received request to list bookings for: {customerId}");

            var customerBookings = await this._bookingRepository.ListForCustomer(customerId);
            
            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(customerBookings),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
