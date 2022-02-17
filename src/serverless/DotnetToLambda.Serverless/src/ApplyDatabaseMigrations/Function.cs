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
using DotnetToLambda.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ApplyDatabaseMigrations
{
    public class Function
    {
        private readonly BookingContext _bookingContext;
        private readonly ILogger<Function> _logger;
        private readonly IConfiguration _configuration;
        private readonly DatabaseConnection _databaseConnection;
        
        public Function()
        {
            ServerlessConfig.ConfigureServices();

            this._bookingContext= ServerlessConfig.Services.GetRequiredService<BookingContext>();
            this._logger = ServerlessConfig.Services.GetRequiredService<ILogger<Function>>();
            this._databaseConnection = ServerlessConfig.Services.GetRequiredService<DatabaseConnection>();
            this._configuration = ServerlessConfig.Services.GetRequiredService<IConfiguration>();
        }
        
        public void FunctionHandler(string input, ILambdaContext context)
        {
            this._logger.LogInformation("Starting database migration");
            this._logger.LogInformation(this._databaseConnection.ToString());
            this._logger.LogInformation(this._configuration["test"]);

            this._bookingContext.Database.Migrate();

            this._logger.LogInformation("Migration applied");
        }
    }
}
