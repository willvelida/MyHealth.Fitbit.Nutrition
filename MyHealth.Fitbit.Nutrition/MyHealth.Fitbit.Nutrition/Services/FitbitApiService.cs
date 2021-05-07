using Microsoft.Extensions.Configuration;
using MyHealth.Common;
using MyHealth.Fitbit.Nutrition.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MyHealth.Fitbit.Nutrition.Services
{
    public class FitbitApiService : IFitbitApiService
    {
        private readonly IConfiguration _configuration;
        private readonly IKeyVaultHelper _keyVaultHelper;
        private HttpClient _httpClient;

        public FitbitApiService(
            IConfiguration configuration,
            IKeyVaultHelper keyVaultHelper,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _keyVaultHelper = keyVaultHelper;
            _httpClient = httpClient;
        }

        public async Task<FoodResponseObject> GetFoodLogs(string date)
        {
            try
            {
                var fitbitAccessToken = await _keyVaultHelper.RetrieveSecretFromKeyVaultAsync(_configuration["AccessTokenName"]);
                _httpClient.DefaultRequestHeaders.Clear();
                Uri getDailyFoodLogUri = new Uri($"https://api.fitbit.com/1/user/-/foods/log/date/{date}.json");
                var request = new HttpRequestMessage(HttpMethod.Get, getDailyFoodLogUri);
                request.Content = new StringContent("");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", fitbitAccessToken.Value);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var foodResponse = JsonConvert.DeserializeObject<FoodResponseObject>(responseString);

                return foodResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
