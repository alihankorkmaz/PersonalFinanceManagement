using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonalFinanceManagement.Controllers;
using PersonalFinanceManagement.Dtos;
using PersonalFinanceManagement.Models;
using System.Threading.Tasks;

namespace PersonalFinanceManagement.Tests
{
    [TestClass]
    public class AdminRegisterControllerTests
    {
        private SqliteConnection _connection;
        private DbContextOptions<FinanceContext> _options;
        private FinanceContext _context;
        private AdminRegisterController _controller;

        [TestInitialize]
        public void Setup()
        {
            // SQLite in-memory database bağlantısı
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // DbContextOptions oluştur
            _options = new DbContextOptionsBuilder<FinanceContext>()
                .UseSqlite(_connection)
                .Options;

            // DbContext'i oluştur ve database'i migrate et
            _context = new FinanceContext(_options);
            _context.Database.EnsureCreated(); // Database'i oluştur

            // Controller'ı oluştur
            _controller = new AdminRegisterController(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _connection.Close();
            _connection.Dispose();
        }

        [TestMethod]
        public async Task Register_NewEmail_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var validRequest = new AdminRegisterDto
            {
                Name = "New Admin",
                Email = "newadmin@test.com", // Benzersiz email
                Password = "securepassword"
            };

            // Act
            var result = await _controller.Register(validRequest);

            // Assert - Tip kontrolü
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "OkObjectResult dönmedi! Result: " + result?.GetType().Name);

            // ResponseDto kontrolü
            var response = okResult.Value as ResponseDto;
            Assert.IsNotNull(response, "ResponseDto null! Controller'da ResponseDto kullanılıyor mu?");
            Assert.AreEqual("Admin registered successfully.", response.Message);

            // Veritabanında kaydı kontrol et
            var adminInDb = await _context.Admins.FirstOrDefaultAsync(a => a.Email == validRequest.Email);
            Assert.IsNotNull(adminInDb, "Admin veritabanına eklenmedi!");
            Assert.AreEqual(validRequest.Name, adminInDb.Name);
        }

        [TestMethod]
        public async Task Register_ExistingEmailInAdmins_ReturnsBadRequest()
        {
            // Arrange - Var olan admin ekle
            var existingAdmin = new Admin
            {
                Email = "admin@test.com",
                Name = "Existing Admin",
                PasswordHash = "admin123"
            };
            await _context.Admins.AddAsync(existingAdmin);
            await _context.SaveChangesAsync();

            var duplicateRequest = new AdminRegisterDto
            {
                Name = "New Admin",
                Email = "admin@test.com", // Aynı email
                Password = "newadmin123"
            };

            // Act
            var result = await _controller.Register(duplicateRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult, "BadRequestObjectResult dönmedi! Result: " + result?.GetType().Name);

            var response = badRequestResult.Value as ResponseDto;
            Assert.IsNotNull(response, "ResponseDto null! Controller'da ResponseDto kullanılıyor mu?");
            Assert.AreEqual("Email is already in use.", response.Message);
        }

        [TestMethod]
        public async Task Register_ExistingEmailInUsers_ReturnsBadRequest()
        {
            // Arrange - Var olan user ekle
            var existingUser = new User
            {
                Email = "user@test.com",
                Name = "Existing User",
                PasswordHash = "user123"
            };
            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            var duplicateRequest = new AdminRegisterDto
            {
                Name = "New Admin",
                Email = "user@test.com", // User'ın email'i
                Password = "admin123"
            };

            // Act
            var result = await _controller.Register(duplicateRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult, "BadRequestObjectResult dönmedi! Result: " + result?.GetType().Name);

            var response = badRequestResult.Value as ResponseDto;
            Assert.IsNotNull(response, "ResponseDto null! Controller'da ResponseDto kullanılıyor mu?");
            Assert.AreEqual("Email is already in use.", response.Message);
        }
    }
}