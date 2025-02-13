using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonalFinanceManagement.Controllers;
using PersonalFinanceManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalFinanceManagement.Tests
{
    [TestClass]
    public class AdminControllerTests
    {
        private DbContextOptions<FinanceContext> _options;
        private FinanceContext _context;
        private AdminController _controller;

        [TestInitialize]
        public void Setup()
        {
            // Setup in-memory database
            _options = new DbContextOptionsBuilder<FinanceContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new FinanceContext(_options);
            _context.Database.EnsureCreated();

            _controller = new AdminController(_context);
        }


        [TestMethod]
        public async Task UpdateKey_ValidKey_ReturnsOk()
        {
            // Arrange
            var keyUpdateDto = new KeyUpdateDto
            {
                Key = "new-key-123",
                ExpiresIn = 30 // minutes
            };

            // Act
            var result = await _controller.UpdateKey(keyUpdateDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as object;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetCurrentKey_ReturnsOkWithKey()
        {
            // Arrange
            var adminSettings = new AdminSettings { RegistrationKey = "current-key-123", ExpirationTime = DateTime.UtcNow.AddMinutes(30) };
            await _context.AdminSettings.AddAsync(adminSettings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCurrentKey();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var response = okResult.Value as object;
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetCurrentKey_NotFound_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetCurrentKey();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            var response = notFoundResult.Value as object;
            Assert.IsNotNull(response);
        }
    }
}
