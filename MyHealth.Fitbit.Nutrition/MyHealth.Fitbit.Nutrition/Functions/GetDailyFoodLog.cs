using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.Fitbit.Nutrition.Services;
using System;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.Fitbit.Nutrition.Functions
{
    public class GetDailyFoodLog
    {
        private readonly IConfiguration _configuration;
        private readonly IFitbitApiService _fitbitApiService;
        private readonly IMapper _mapper;
        private readonly IServiceBusHelpers _serviceBusHelpers;

        public GetDailyFoodLog(
            IConfiguration configuration,
            IFitbitApiService fitbitApiService,
            IMapper mapper,
            IServiceBusHelpers serviceBusHelpers)
        {
            _configuration = configuration;
            _fitbitApiService = fitbitApiService;
            _mapper = mapper;
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(GetDailyFoodLog))]
        public async Task Run([TimerTrigger("0 20 5 * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"{nameof(GetDailyFoodLog)} executed at: {DateTime.Now}");
                var dateParameter = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                log.LogInformation($"Attempting to retrieve Food Log for {dateParameter}");
                var foodResponse = await _fitbitApiService.GetFoodLogs(dateParameter);

                log.LogInformation("Mapping API response to Nutrition object");
                var nutrition = new mdl.Nutrition();
                nutrition.NutritionDate = dateParameter;
                _mapper.Map(foodResponse, nutrition);

                log.LogInformation("Sending mapped Nutrition Log to Service Bus");
                await _serviceBusHelpers.SendMessageToTopic(_configuration["NutritionTopic"], nutrition);
            }
            catch (Exception ex)
            {
                log.LogError($"Exception thrown in {nameof(GetDailyFoodLog)}: {ex.Message}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                throw ex;
            }
        }
    }
}
