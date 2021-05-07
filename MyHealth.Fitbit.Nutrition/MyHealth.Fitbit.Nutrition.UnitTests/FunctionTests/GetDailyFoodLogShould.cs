using AutoMapper;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.Common;
using MyHealth.Fitbit.Nutrition.Functions;
using MyHealth.Fitbit.Nutrition.Models;
using MyHealth.Fitbit.Nutrition.Services;
using System;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.Fitbit.Nutrition.UnitTests.FunctionTests
{
    public class GetDailyFoodLogShould
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IFitbitApiService> _mockFitbitApiService;
        private Mock<IMapper> _mockMapper;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<ILogger> _mockLogger;
        private TimerInfo _testTimerInfo;

        private GetDailyFoodLog _func;

        public GetDailyFoodLogShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockFitbitApiService = new Mock<IFitbitApiService>();
            _mockMapper = new Mock<IMapper>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockLogger = new Mock<ILogger>();
            _testTimerInfo = default(TimerInfo);

            _func = new GetDailyFoodLog(
                _mockConfiguration.Object,
                _mockFitbitApiService.Object,
                _mockMapper.Object,
                _mockServiceBusHelpers.Object);
        }

        [Fact]
        public async Task RetrieveFoodLogResponseAndSendMappedObjectToNutritionTopic()
        {
            // Arrange
            _mockFitbitApiService.Setup(x => x.GetFoodLogs(It.IsAny<string>())).ReturnsAsync(It.IsAny<FoodResponseObject>());
            _mockMapper.Setup(x => x.Map(It.IsAny<FoodResponseObject>(), It.IsAny<mdl.Nutrition>())).Verifiable();
            _mockServiceBusHelpers.Setup(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>())).Returns(Task.CompletedTask);

            // Act
            Func<Task> getDailyFoodLogAction = async () => await _func.Run(_testTimerInfo, _mockLogger.Object);

            // Assert
            await getDailyFoodLogAction.Should().NotThrowAsync<Exception>();
            _mockServiceBusHelpers.Verify(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>()), Times.Once);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ThrowAndCatchExceptionWhenFitApiServiceThrowsException()
        {
            // Arrange
            _mockFitbitApiService.Setup(x => x.GetFoodLogs(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> getDailySleepAction = async () => await _func.Run(_testTimerInfo, _mockLogger.Object);

            // Assert
            await getDailySleepAction.Should().ThrowAsync<Exception>();
            _mockServiceBusHelpers.Verify(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>()), Times.Never);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task ThrowAndCatchExceptionWhenMapperThrowsException()
        {
            // Arrange
            _mockFitbitApiService.Setup(x => x.GetFoodLogs(It.IsAny<string>())).ReturnsAsync(It.IsAny<FoodResponseObject>());
            _mockMapper.Setup(x => x.Map(It.IsAny<FoodResponseObject>(), It.IsAny<mdl.Nutrition>())).Throws(new Exception());

            // Act
            Func<Task> getDailySleepAction = async () => await _func.Run(_testTimerInfo, _mockLogger.Object);

            // Assert
            await getDailySleepAction.Should().ThrowAsync<Exception>();
            _mockServiceBusHelpers.Verify(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>()), Times.Never);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task ThrowAndCatchExceptionWhenSendMessageToTopicThrowsException()
        {
            // Arrange
            _mockFitbitApiService.Setup(x => x.GetFoodLogs(It.IsAny<string>())).ReturnsAsync(It.IsAny<FoodResponseObject>());
            _mockMapper.Setup(x => x.Map(It.IsAny<FoodResponseObject>(), It.IsAny<mdl.Nutrition>())).Verifiable();
            _mockServiceBusHelpers.Setup(x => x.SendMessageToTopic(It.IsAny<string>(), It.IsAny<mdl.Nutrition>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> getDailySleepAction = async () => await _func.Run(_testTimerInfo, _mockLogger.Object);

            // Assert
            await getDailySleepAction.Should().ThrowAsync<Exception>();
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
