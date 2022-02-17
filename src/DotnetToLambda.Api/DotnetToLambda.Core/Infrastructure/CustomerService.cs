using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetToLambda.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotnetToLambda.Core.Infrastructure
{
    public class CustomerService : ICustomerService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CustomerService> _logger;
        private readonly Random _random;

        public CustomerService(IConfiguration configuration, HttpClient httpClient, ILogger<CustomerService> logger)
        {
            this._configuration = configuration;
            this._httpClient = httpClient;
            this._logger = logger;
            this._random = new Random(DateTime.Now.Second);
        }

        public async Task<bool> CustomerExists(string customerIdentifier)
        {
            this._logger.LogInformation($"Checking if customer {customerIdentifier} exists");
            
            var randomNumber = this._random.Next(0, 15);
            
            this._logger.LogInformation($"Using random user identifier {randomNumber}");
            
            // Using JSON placeholder API's to simulate an external API call. The user endpoint returns data with 
            // an integer between 1 and 10 and no results between 11 and 15. The random number generator is
            // to simulate external failures.
            var checkCustomerExists =
                await this._httpClient.GetAsync($"{this._configuration["CustomerApiEndpoint"]}/{randomNumber}");

            if (!checkCustomerExists.IsSuccessStatusCode)
            {
                this._logger.LogError($"Failure calling customer API error");
                
                return false;
            }
            
            var responseBody = await checkCustomerExists.Content.ReadAsStringAsync();
            
            this._logger.LogInformation($"Customer API response is {responseBody}");

            if (responseBody == "{}")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}