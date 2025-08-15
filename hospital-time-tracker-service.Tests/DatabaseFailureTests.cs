using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hospital_time_tracker_service.Controllers;
using hospital_time_tracker_service.Data;
using hospital_time_tracker_service.Models;
using Moq;

namespace hospital_time_tracker_service.Tests
{
    public class DatabaseFailureTests
    {
        [Fact]
        public async Task Scan_SaveChangesException_ReturnsInternalServerError()
        {
            var options = new DbContextOptionsBuilder<HospitalContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new HospitalContext(options);
            var mockContext = new Mock<HospitalContext>(options);
            mockContext.Setup(x => x.SaveChangesAsync(default))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var controller = new HospitalController(mockContext.Object);
            var request = new ScanRequest { PatientId = "P001", Location = "main-entrance" };
            var result = await controller.Scan(request);
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_QueryException_ReturnsInternalServerError()
        {
            var options = new DbContextOptionsBuilder<HospitalContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
private DbContextOptions<HospitalContext> CreateDbContextOptions()
        {
            return new DbContextOptionsBuilder<HospitalContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Scan_SaveChangesException_ReturnsInternalServerError()
        {
            var options = CreateDbContextOptions();

            using var context = new HospitalContext(options);
            var mockContext = new Mock<HospitalContext>(options);
            mockContext.Setup(x => x.SaveChangesAsync(default))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var controller = new HospitalController(mockContext.Object);
            var request = new ScanRequest { PatientId = "P001", Location = "main-entrance" };
            var result = await controller.Scan(request);
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_QueryException_ReturnsInternalServerError()
        {
            var options = CreateDbContextOptions();

            var mockContext = new Mock<HospitalContext>(options);
            var mockDbSet = new Mock<DbSet<Visit>>();
            mockDbSet.As<IQueryable<Visit>>().Setup(x => x.Provider)
                .Throws(new InvalidOperationException("Database error"));
            mockContext.Setup(x => x.Visits).Returns(mockDbSet.Object);

            var controller = new HospitalController(mockContext.Object);
            var result = await controller.GetReports(null, null);
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetAllVisits_QueryException_ReturnsInternalServerError()
        {
            var options = CreateDbContextOptions();

            var mockContext = new Mock<HospitalContext>(options);
            var mockDbSet = new Mock<DbSet<Visit>>();
            mockDbSet.As<IQueryable<Visit>>().Setup(x => x.Provider)
                .Throws(new InvalidOperationException("Database error"));
            mockContext.Setup(x => x.Visits).Returns(mockDbSet.Object);

            var controller = new HospitalController(mockContext.Object);
            var result = await controller.GetReports(null, null);
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetAllVisits_QueryException_ReturnsInternalServerError()
        {
            var options = new DbContextOptionsBuilder<HospitalContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockContext = new Mock<HospitalContext>(options);
            var mockDbSet = new Mock<DbSet<Visit>>();
            mockDbSet.As<IQueryable<Visit>>().Setup(x => x.Provider)
                .Throws(new InvalidOperationException("Database error"));
            mockContext.Setup(x => x.Visits).Returns(mockDbSet.Object);

            var controller = new HospitalController(mockContext.Object);
            var result = await controller.GetAllVisits();
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }
    }
}