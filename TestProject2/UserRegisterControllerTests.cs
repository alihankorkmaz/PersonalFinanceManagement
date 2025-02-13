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
                .UseInMemoryDatabase(databaseName: "TestDb_UserRegister_" + Guid.NewGuid())
                .Options;

            _context = new FinanceContext(options);
            _context.Database.EnsureDeleted();
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
            // Arrange - add existing user
            var existingUser = new User
            {
                Email = "existing@test.com",
                Name = "Test User",
                PasswordHash = "hashed123"
            };
            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            // Veritabanına eklenen kullanıcıyı kontrol et
            var addedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == existingUser.Email);
            Assert.IsNotNull(addedUser, "User with the specified email should exist in the database.");

            // Request with existing email
            var duplicateRequest = new UserRegisterDto
            {
                Name = "New User",
                Email = "existing@test.com",
                Password = "newpassword"
            };

            // Act - duplicate request with existing email
            var result = await _controller.Register(duplicateRequest);

            // BadRequest olup olmadığını kontrol et
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult, "Result should be BadRequestObjectResult");

            // Cast to DTO
            var response = badRequestResult.Value as ResponseDto;
            Assert.IsNotNull(response, "ResponseDto should be returned");
            Assert.AreEqual("Email is already in use.", response.Message);
        }


        [TestMethod]
        public async Task Register_ValidRequest_ReturnsOkWithSuccessMessage()
        {
            // Arrange - no existing user
            var validRequest = new UserRegisterDto
            {
                Name = "New User",
                Email = "new@test.com",
                Password = "validpass123"
            };

            // Act
            var result = await _controller.Register(validRequest);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Result should be OkObjectResult");

            // Cast to DTO
            var response = okResult.Value as ResponseDto;
            Assert.IsNotNull(response, "ResponseDto should be returned");
            Assert.AreEqual("User registered successfully.", response.Message);
        }

        [TestMethod]
        public async Task GetUserByEmail_ReturnsCorrectUser()
        {
            // Arrange - create and save a user
            var user = new User { Name = "Test User", Email = "testuser@test.com", PasswordHash = "pass123" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act - retrieve the user
            var retrievedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "testuser@test.com");

            // Assert
            Assert.IsNotNull(retrievedUser, "User should be found");
            Assert.AreEqual(user.Name, retrievedUser.Name);
        }

        [TestMethod]
        public async Task UpdateUser_ChangesUserName()
        {
            // Arrange - create a user
            var user = new User { Name = "Old Name", Email = "update@test.com", PasswordHash = "pass123" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act - update user name
            user.Name = "New Name";
            await _context.SaveChangesAsync();  // Update çağrısına gerek yok, çünkü user zaten takip ediliyor

            // Assert
            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "update@test.com");

            Assert.IsNotNull(updatedUser, "User should exist");
            Assert.AreEqual("New Name", updatedUser.Name, "User name should be updated");
        }


        [TestMethod]
        public async Task DeleteUser_RemovesUserFromDatabase()
        {
            // Arrange - create a user
            var user = new User { Name = "To Delete", Email = "delete@test.com", PasswordHash = "pass123" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act - delete user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var deletedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "delete@test.com");

            // Assert
            Assert.IsNull(deletedUser, "User should be deleted");
        }
    }
}
