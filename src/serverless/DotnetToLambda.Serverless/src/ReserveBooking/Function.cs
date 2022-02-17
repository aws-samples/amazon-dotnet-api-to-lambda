using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DotnetToLambda.Core.Exceptions;
using DotnetToLambda.Core.ViewModels;
using DotnetToLambda.Core.Models;
using DotnetToLambda.Core.Services;
using DotnetToLambda.Serverless.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ReserveBooking
{
    public class Function
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<Function> _logger;
        private readonly ICustomerService _customerService;
        
        public Function()
        {
            ServerlessConfig.ConfigureServices();

            this._bookingRepository = ServerlessConfig.Services.GetRequiredService<IBookingRepository>();
            this._logger = ServerlessConfig.Services.GetRequiredService<ILogger<Function>>();
            this._customerService = ServerlessConfig.Services.GetRequiredService<ICustomerService>();
        }
        
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var request = JsonSerializer.Deserialize<ReserveBookingDTO>(apigProxyEvent.Body);
            
            this._logger.LogInformation("Received request to reserve a new booking:");

            this._logger.LogInformation(JsonSerializer.Serialize(request));
            
            if (!await this._customerService.CustomerExists(request.CustomerId))
            {
                this._logger.LogWarning($"Customer {request.CustomerId} does not exist in the customer service");
                
                return new APIGatewayProxyResponse
                {
                    Body = "{\"message\": \"Customer not found\"}",
                    StatusCode = 400,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var booking = Booking.Create(Guid.NewGuid().ToString(), request.CustomerId, request.OutboundFlightId, request.ChargeId);

            this._logger.LogInformation($"Booking created with Id ${booking.BookingId}");

            try
            {
                await this._bookingRepository.Add(booking);
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

            this._logger.LogInformation("Booking added");
            
            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(booking),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
