using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MecanicaOS.UnitTests.API.Controllers
{
    public class HealthControllerTests
    {
        [Fact]
        public void GetHealth_DeveRetornarOkComStatusOk()
        {
            // Arrange
            var controller = new HealthController();

            // Act
            var result = controller.GetHealth();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            Assert.NotNull(value);

            var statusProperty = value.GetType().GetProperty("status");
            Assert.NotNull(statusProperty);

            var statusValue = statusProperty.GetValue(value);
            Assert.Equal("ok", statusValue);
        }
    }
}
