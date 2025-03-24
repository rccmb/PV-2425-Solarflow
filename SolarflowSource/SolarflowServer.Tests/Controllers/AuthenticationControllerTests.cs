using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SolarflowServer.Controllers;
using SolarflowServer.DTOs.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SolarflowServer.Services;
using Microsoft.EntityFrameworkCore;
/*
namespace SolarflowServer.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<UserManager<ViewAccount>> _viewUserManagerMock;
        private readonly Mock<SignInManager<ViewAccount>> _viewSignInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthenticationController _controller;
        private readonly Mock<IAuditService> _auditService;
        private readonly ApplicationDbContext _context;

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

            _viewUserManagerMock = new Mock<UserManager<ViewAccount>>(
                new Mock<IUserStore<ViewAccount>>().Object,
                null, null, null, null, null, null, null, null);

            _viewSignInManagerMock = new Mock<SignInManager<ViewAccount>>(
                _viewUserManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ViewAccount>>().Object,
                null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();

            _auditService = new Mock<IAuditService>();

            // Criando um banco de dados em memória para testes
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new ApplicationDbContext(options);

            // Criar instância do controlador
            _controller = new AuthenticationController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _viewUserManagerMock.Object,
                _viewSignInManagerMock.Object,
                _configurationMock.Object,
                _auditService.Object,
                _context);

            // Configurar o HttpContext para evitar erros de NullReference
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Configurar a chave JWT no mock de configuração
            _configurationMock.Setup(x => x["Jwt:Secret"]).Returns("SECRET-KEYSECRET-KEYSECRET-KEYSECRET-KEYSECRET-KEYSECRET-KEY");
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
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "test@example.com", Password = "Password123!" };
            var user = new ApplicationUser { Id = 1, Email = "test@example.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

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
        public async Task Login_ViewAccount_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "test@example.com", Password = "ViewPassword123!" };
            var user = new ApplicationUser { Id = 1, Email = "test@example.com" };
            var viewUser = new ViewAccount { Id = 2, Email = "test@example.com", UserName = "test@example.com", UserId = user.Id };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null); // Simula que a conta principal não foi encontrada

            _viewUserManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(viewUser); // Simula que encontrou a conta ViewAccount

            _viewSignInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<ViewAccount>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success); // Simula login bem-sucedido

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}
*/