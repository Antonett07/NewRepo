using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.DB.Controllers;
using RaceDay.DB.Data;
using RaceDay.DB.Models;
using Xunit;
using static RaceDay.DB.Models.Entities;

namespace RaceDay.Test
{
    public class UnitTest1
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetEvents_ReturnsOkResult_WithListOfEvents()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Roles.Add(new Role { RoleID = 1, RoleName = "Organiser" });
            context.Users.Add(new User { UserID = 1, RoleID = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", PasswordHash = "hash" });
            context.Events.Add(new Event { EventID = 1, OrganiserID = 1, EventName = "Soweto Marathon", Location = "Soweto", EventDate = DateTime.Now.AddDays(10) });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            // Act
            var result = await controller.GetEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);
            Assert.Single(events);
        }
    }
}