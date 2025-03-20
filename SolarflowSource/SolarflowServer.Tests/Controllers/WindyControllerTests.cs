﻿using Microsoft.AspNetCore.Mvc;
using Moq;
using SolarflowServer.Services;
using Newtonsoft.Json.Linq;

namespace SolarflowServer.Tests.Controllers
{
    public class WindyControllerTests
    {
        private readonly Mock<IWindyService> _mockWindyService;
        private readonly WindyController _controller;

        public WindyControllerTests()
        {
            _mockWindyService = new Mock<IWindyService>();
            _controller = new WindyController(_mockWindyService.Object);
        }

        [Fact]
        public async Task GetWeatherForecast_ShouldReturnOk_WhenValidCoordinatesAreProvided()
        {
            
            var fakeWeatherData = JObject.Parse("{\"wind\": {\"speed\": 15.0}}");
            _mockWindyService
                .Setup(s => s.GetWeatherDataAsync(It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(fakeWeatherData);

         
            var result = await _controller.GetWeatherForecast(38.7169, -9.1399);

           
            var okResult = Assert.IsType<OkObjectResult>(result);
            var jsonResponse = Assert.IsType<JObject>(okResult.Value);
            Assert.Equal(15.0, (double)jsonResponse["wind"]["speed"]);
        }

        [Fact]
        public async Task GetWeatherForecast_ShouldReturnBadRequest_WhenInvalidCoordinatesAreProvided()
        {
            var result = await _controller.GetWeatherForecast(0, 0);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetWeatherForecast_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            _mockWindyService
                .Setup(s => s.GetWeatherDataAsync(It.IsAny<double>(), It.IsAny<double>()))
                .ThrowsAsync(new System.Exception("Erro na API"));

            var result = await _controller.GetWeatherForecast(38.7169, -9.1399);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
