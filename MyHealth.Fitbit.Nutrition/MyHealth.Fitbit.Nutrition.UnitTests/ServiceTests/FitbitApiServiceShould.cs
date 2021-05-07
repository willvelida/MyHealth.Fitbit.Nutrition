using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.Common;
using MyHealth.Fitbit.Nutrition.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MyHealth.Fitbit.Nutrition.UnitTests.ServiceTests
{
    public class FitbitApiServiceShould
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IKeyVaultHelper> _mockKeyVaultHelper;
        private Mock<HttpClient> _mockHttpClient;

        private FitbitApiService _sut;

        public FitbitApiServiceShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockKeyVaultHelper = new Mock<IKeyVaultHelper>();
            _mockHttpClient = new Mock<HttpClient>();

            _sut = new FitbitApiService(
                _mockConfiguration.Object,
                _mockKeyVaultHelper.Object,
                _mockHttpClient.Object);
        }

        [Fact]
        public async Task CatchAndThrowExceptionWhenRetrieveSecretFromKeyVaultThrowsException()
        {
            // Arrange
            _mockKeyVaultHelper.Setup(x => x.RetrieveSecretFromKeyVaultAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> fitbitApiServiceAction = async () => await _sut.GetFoodLogs("04/05/2021");

            // Assert
            await fitbitApiServiceAction.Should().ThrowAsync<Exception>();
        }
    }
}
