using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.Common;
using MyHealth.Fitbit.Nutrition.Functions;
using MyHealth.Fitbit.Nutrition.Models;
using MyHealth.Fitbit.Nutrition.Services;
using MyHealth.Fitbit.Nutrition.Validators;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.Fitbit.Nutrition.UnitTests.FunctionTests
{
    public class RetrieveManualFoodLogShould
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IFitbitApiService> _mockFitbitApiService;
        private Mock<IMapper> _mockMapper;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IDateValidator> _mockDateValidator;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private RetrieveManualFoodLog _func;

        public RetrieveManualFoodLogShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockFitbitApiService = new Mock<IFitbitApiService>();
            _mockMapper = new Mock<IMapper>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockDateValidator = new Mock<IDateValidator>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new RetrieveManualFoodLog(
                _mockConfiguration.Object,
                _mockFitbitApiService.Object,
                _mockMapper.Object,
                _mockServiceBusHelpers.Object,
                _mockDateValidator.Object);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenMappedNutritionLogIsSentToNutritionTopic()
        {
            // Arrange
            var nutrition = new mdl.Nutrition
            {
                NutritionDate = "2019-12-31"
            };
            var foodResponse = new FoodResponseObject
            {
            };
            _mockDateValidator.Setup(x => x.IsNutritionDateValid(It.IsAny<string>())).Returns(true);
            _mockFitbitApiService.Setup(x => x.GetFoodLogs(It.IsAny<string>())).ReturnsAsync(foodResponse);
            _mockMapper.Setup(x => x.Map(It.IsAny<FoodResponseObject>(), It.IsAny<mdl.Nutrition>())).Verifiable();
            _mockServiceBusHelpers.Setup(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>())).Returns(Task.CompletedTask);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, nutrition.NutritionDate);

            // Assert
            Assert.Equal(typeof(OkResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>()), Times.Once);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Theory]
        [InlineData("2020-12-100")]
        [InlineData("2020-111-12")]
        [InlineData("20201-12-11")]
        public async Task ReturnBadRequestResultWhenProvidedDateIsInvalid(string invalidDateInput)
        {
            // Arrange
            var foodResponse = new FoodResponseObject();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(foodResponse));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsNutritionDateValid(invalidDateInput)).Returns(false);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, invalidDateInput);

            // Assert
            Assert.Equal(typeof(BadRequestResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(400, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>()), Times.Never);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ReturnNotFoundResultWhenNoFoodResponseIsFound()
        {
            // Arrange
            var nutrition = new mdl.Nutrition
            {
                NutritionDate = "2019-12-31"
            };
            _mockDateValidator.Setup(x => x.IsNutritionDateValid(It.IsAny<string>())).Returns(true);
            _mockFitbitApiService.Setup(x => x.GetFoodLogs(It.IsAny<string>())).Returns(Task.FromResult<FoodResponseObject>(null));

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, nutrition.NutritionDate);

            // Assert
            Assert.Equal(typeof(NotFoundResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(404, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>()), Times.Never);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task Throw500WhenMappingFoodResponseToNutritionObjectFails()
        {
            // Arrange
            var nutrition = new mdl.Nutrition
            {
                NutritionDate = "2019-12-31"
            };
            var foodResponse = new FoodResponseObject
            {
            };
            _mockDateValidator.Setup(x => x.IsNutritionDateValid(It.IsAny<string>())).Returns(true);
            _mockFitbitApiService.Setup(x => x.GetFoodLogs(It.IsAny<string>())).ReturnsAsync(foodResponse);
            _mockMapper.Setup(x => x.Map(It.IsAny<FoodResponseObject>(), It.IsAny<mdl.Nutrition>())).Throws(new Exception());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, nutrition.NutritionDate);

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task Throw500WhenSendingToNutritionTopicFails()
        {
            // Arrange
            var nutrition = new mdl.Nutrition
            {
                NutritionDate = "2019-12-31"
            };
            var foodResponse = new FoodResponseObject
            {
            };
            _mockDateValidator.Setup(x => x.IsNutritionDateValid(It.IsAny<string>())).Returns(true);
            _mockFitbitApiService.Setup(x => x.GetFoodLogs(It.IsAny<string>())).ReturnsAsync(foodResponse);
            _mockMapper.Setup(x => x.Map(It.IsAny<FoodResponseObject>(), It.IsAny<mdl.Nutrition>())).Verifiable();
            _mockServiceBusHelpers.Setup(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>())).ThrowsAsync(new Exception());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, nutrition.NutritionDate);

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
