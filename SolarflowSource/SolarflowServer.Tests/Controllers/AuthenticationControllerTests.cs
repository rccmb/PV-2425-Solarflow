using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SolarflowServer.Controllers;
using SolarflowServer.DTOs.Authentication;
using Microsoft.AspNetCore.Http;

namespace SolarflowServer.Tests.Controllers
{
    public class AuthenticationControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null, null, null, null, null, null, null, null);

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null, null, null, null);

        _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

        _controller = new AuthenticationController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsOk()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Email = "test@example.com",
            Password = "Password123!",
            Fullname = "Test User"
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Email = "test@example.com",
            Password = "Password123!",
            Fullname = "Test User"
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid user" }));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginDto = new LoginDTO { Email = "test@example.com", Password = "Password123!" };
        var user = new ApplicationUser { Id = 1, Email = "test@example.com" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(user, It.IsAny<string>(), false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _configurationMock.Setup(x => x["Jwt:Secret"]).Returns("SECRET-KEYSECRET-KEYSECRET-KEYSECRET-KEYSECRET-KEYSECRET-KEY");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDTO { Email = "test@example.com", Password = "wrongpassword" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        // Arrange
        _signInManagerMock.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }
}
}