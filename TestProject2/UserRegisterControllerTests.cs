using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonalFinanceManagement.Controllers;
using PersonalFinanceManagement.Dtos;
using PersonalFinanceManagement.Models;
using System.Threading.Tasks;

namespace PersonalFinanceManagement.Tests
{
    [TestClass]
    public class UserRegisterControllerTests
    {
        private FinanceContext _context;
        private UserRegisterController _controller;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FinanceContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_UserRegister")
                .Options;

            _context = new FinanceContext(options);
            _controller = new UserRegisterController(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task Register_ExistingEmail_ReturnsBadRequestWithMessage()
        {
            // Arrange - Var olan kullanıcıyı ekle
            var existingUser = new User
            {
                Email = "existing@test.com",
                Name = "Test User",
                PasswordHash = "hashed123"
            };
            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            // Aynı email ile kayıt isteği
            var duplicateRequest = new UserRegisterDto
            {
                Name = "New User",
                Email = "existing@test.com",
                Password = "newpassword"
            };

            // Act
            var result = await _controller.Register(duplicateRequest);

            // Assert - Tip kontrolü
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult, "Result BadRequestObjectResult olmalı");

            // DTO'ya cast et
            var response = badRequestResult.Value as ResponseDto;
            Assert.IsNotNull(response, "ResponseDto dönmeli");
            Assert.AreEqual("Email is already in use.", response.Message);
        }

        [TestMethod]
        public async Task Register_ValidRequest_ReturnsOkWithSuccessMessage()
        {
            // Arrange - Yeni email
            var validRequest = new UserRegisterDto
            {
                Name = "New User",
                Email = "new@test.com",
                Password = "validpass123"
            };

            // Act
            var result = await _controller.Register(validRequest);

            // Assert - Tip kontrolü
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Result OkObjectResult olmalı");

            // DTO'ya cast et
            var response = okResult.Value as ResponseDto;
            Assert.IsNotNull(response, "ResponseDto dönmeli");
            Assert.AreEqual("User registered successfully.", response.Message);
        }
    }
}