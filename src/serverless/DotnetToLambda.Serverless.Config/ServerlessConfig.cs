using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using DotnetToLambda.Core.Infrastructure;
using DotnetToLambda.Core.Models;
using DotnetToLambda.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetToLambda.Serverless.Config
{
    public static class ServerlessConfig
    {
        public static IServiceProvider Services { get; private set; }

        public static void ConfigureServices()
        {
            var client = new AmazonSecretsManagerClient();
            
            var serviceCollection = new ServiceCollection();

            var connectionDetails = LoadDatabaseSecret(client);

            serviceCollection.AddDbContext<BookingContext>(options =>
                options.UseMySQL(connectionDetails.ToString()));
            
            serviceCollection.AddHttpClient<ICustomerService, CustomerService>();
            serviceCollection.AddTransient<IBookingRepository, BookingRepository>();
            serviceCollection.AddSingleton<DatabaseConnection>(connectionDetails);
            serviceCollection.AddSingleton<IConfiguration>(LoadAppConfiguration());

            serviceCollection.AddLogging(logging =>
            {
                logging.AddLambdaLogger();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            Services = serviceCollection.BuildServiceProvider();
        }

        private static DatabaseConnection LoadDatabaseSecret(AmazonSecretsManagerClient client)
        {
            var databaseConnectionSecret = client.GetSecretValueAsync(new GetSecretValueRequest()
            {
                SecretId = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_SECRET_ID"),
            }).Result;

            return JsonSerializer
                .Deserialize<DatabaseConnection>(databaseConnectionSecret.SecretString);
        }

        private static IConfiguration LoadAppConfiguration()
        {
            var client = new AmazonSimpleSystemsManagementClient();
            var param = client.GetParameterAsync(new GetParameterRequest()
            {
                Name = "dotnet-to-lambda-dev"
            }).Result;
            
            return new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(param.Parameter.Value)))
                .Build();
        }
    }
}
