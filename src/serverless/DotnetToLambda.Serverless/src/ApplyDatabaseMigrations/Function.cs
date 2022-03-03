using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using DotnetToLambda.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Bcpg;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<ApiGatewayProxyJsonSerializerContext>))]

ServerlessConfig.ConfigureServices();

var _bookingContext= ServerlessConfig.Services.GetRequiredService<BookingContext>();
var _databaseConnection = ServerlessConfig.Services.GetRequiredService<DatabaseConnection>();
var _configuration = ServerlessConfig.Services.GetRequiredService<IConfiguration>();

var handler = async (string input, ILambdaContext context) =>
{
    context.Logger.LogInformation("Starting database migration");
    context.Logger.LogInformation(_databaseConnection.ToString());
    context.Logger.LogInformation(_configuration["test"]);

    _bookingContext.Database.Migrate();

    context.Logger.LogInformation("Migration applied");
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();