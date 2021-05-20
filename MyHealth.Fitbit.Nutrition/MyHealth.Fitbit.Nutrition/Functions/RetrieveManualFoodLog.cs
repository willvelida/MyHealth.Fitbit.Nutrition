using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.Fitbit.Nutrition.Services;
using MyHealth.Fitbit.Nutrition.Validators;
using System;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.Fitbit.Nutrition.Functions
{
    public class RetrieveManualFoodLog
    {
        private readonly IConfiguration _configuration;
        private readonly IFitbitApiService _fitbitApiService;
        private readonly IMapper _mapper;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IDateValidator _dateValidator;

        public RetrieveManualFoodLog(
            IConfiguration configuration,
            IFitbitApiService fitbitApiService,
            IMapper mapper,
            IServiceBusHelpers serviceBusHelpers,
            IDateValidator dateValidator)
        {
            _configuration = configuration;
            _fitbitApiService = fitbitApiService;
            _mapper = mapper;
            _serviceBusHelpers = serviceBusHelpers;
            _dateValidator = dateValidator;
        }

        [FunctionName(nameof(RetrieveManualFoodLog))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "FoodLog/{date}")] HttpRequest req,
            ILogger log,
            string date)
        {
            IActionResult result;

            try
            {
                bool isDateValid = _dateValidator.IsNutritionDateValid(date);
                if (isDateValid == false)
                {
                    result = new BadRequestResult();
                    return result;
                }

                log.LogInformation($"Attempting to manually retrieve Food Log for {date}");
                var foodResponse = await _fitbitApiService.GetFoodLogs(date);
                if (foodResponse == null)
                {
                    result = new NotFoundResult();
                    return result;
                }

                log.LogInformation("Mapping API Response to Nutrition object");
                var nutrition = new mdl.Nutrition();
                nutrition.NutritionDate = date;
                _mapper.Map(foodResponse, nutrition);

                log.LogInformation("Sending mapped Nutrition Log to Service Bus");
                await _serviceBusHelpers.SendMessageToTopic(_configuration["NutritionTopic"], nutrition);

                result = new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Exception thrown in {nameof(RetrieveManualFoodLog)}: {ex.Message}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
