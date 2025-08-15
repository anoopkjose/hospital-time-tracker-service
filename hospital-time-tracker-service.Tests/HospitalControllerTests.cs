using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hospital_time_tracker_service.Controllers;
using hospital_time_tracker_service.Data;
using hospital_time_tracker_service.Models;

namespace hospital_time_tracker_service.Tests
{
    public class HospitalControllerTests : IDisposable
    {
        private readonly HospitalContext _context;
        private readonly HospitalController _controller;

        public HospitalControllerTests()
        {
            var options = new DbContextOptionsBuilder<HospitalContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new HospitalContext(options);
            _controller = new HospitalController(_context);
        }

        [Fact]
        public async Task Scan_ValidRequest_ReturnsOk()
        {
            var request = new ScanRequest { PatientId = "P001", Location = "parking" };
            var result = await _controller.Scan(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Scan_NonConsecutiveDuplicateLocation_AllowsMultipleVisits()
        {
            var request1 = new ScanRequest { PatientId = "P001", Location = "parking" };
            var request2 = new ScanRequest { PatientId = "P001", Location = "main-entrance" };
            var request3 = new ScanRequest { PatientId = "P001", Location = "parking" };
            
            await _controller.Scan(request1);
            await _controller.Scan(request2);
            var result = await _controller.Scan(request3);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            
            // Verify all three visits exist
            var visits = await _context.Visits.Where(v => v.PatientId == "P001").ToListAsync();
            Assert.Equal(3, visits.Count);
        }

        [Fact]
        public async Task Scan_EmptyPatientId_ReturnsBadRequest()
        {
            var request = new ScanRequest { PatientId = "", Location = "parking" };
            var result = await _controller.Scan(request);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
        }

        [Fact]
        public async Task Scan_EmptyLocation_ReturnsBadRequest()
        {
            var request = new ScanRequest { PatientId = "P001", Location = "" };
            var result = await _controller.Scan(request);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
        }

        [Fact]
        public async Task Scan_InvalidLocation_ReturnsBadRequest()
        {
            var request = new ScanRequest { PatientId = "P001", Location = "invalid" };
            var result = await _controller.Scan(request);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
        }

        [Fact]
        public async Task Scan_ConsecutiveDuplicateScans_OverwritesPrevious()
        {
            var request = new ScanRequest { PatientId = "P001", Location = "parking" };
            await _controller.Scan(request);
            var result = await _controller.Scan(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            
            // Verify only one visit exists
            var visits = await _context.Visits.Where(v => v.PatientId == "P001").ToListAsync();
            Assert.Single(visits);
        }

        [Fact]
        public async Task Scan_WithCustomTimestamp_UsesProvidedTime()
        {
            var customTime = new DateTimeOffset(2024, 1, 15, 14, 30, 0, TimeSpan.FromHours(-5));
            var request = new ScanRequest 
            { 
                PatientId = "P001", 
                Location = "parking",
                Timestamp = customTime
            };
            
            var result = await _controller.Scan(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            
            var visit = await _context.Visits.FirstAsync(v => v.PatientId == "P001");
            Assert.Equal(customTime, visit.Timestamp);
        }

        [Fact]
        public async Task Scan_WithoutTimestamp_UsesCurrentTime()
        {
            var beforeScan = DateTimeOffset.UtcNow;
            var request = new ScanRequest { PatientId = "P001", Location = "parking" };
            
            var result = await _controller.Scan(request);
            var afterScan = DateTimeOffset.UtcNow;
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            
            var visit = await _context.Visits.FirstAsync(v => v.PatientId == "P001");
            Assert.True(visit.Timestamp >= beforeScan && visit.Timestamp <= afterScan);
        }

        [Fact]
        public async Task GetReports_NoData_ReturnsEmptyResult()
        {
            var result = await _controller.GetReports(null, null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_WithData_ReturnsData()
        {
            _context.Visits.Add(new Visit { PatientId = "P001", Location = "parking" });
            await _context.SaveChangesAsync();
            
            var result = await _controller.GetReports(null, null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_FilterByDate_ReturnsFilteredData()
        {
            var today = new DateTimeOffset(DateTime.Today);
            _context.Visits.Add(new Visit { PatientId = "P001", Location = "parking", Timestamp = today });
            await _context.SaveChangesAsync();
            
            var result = await _controller.GetReports(today.ToString("yyyy-MM-dd"), null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_FilterByPatientId_ReturnsFilteredData()
        {
            _context.Visits.Add(new Visit { PatientId = "P001", Location = "parking" });
            await _context.SaveChangesAsync();
            
            var result = await _controller.GetReports(null, "P001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllVisits_NoData_ReturnsEmptyList()
        {
            var result = await _controller.GetAllVisits();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllVisits_WithData_ReturnsAllVisits()
        {
            _context.Visits.Add(new Visit { PatientId = "P001", Location = "parking" });
            _context.Visits.Add(new Visit { PatientId = "P002", Location = "main-entrance" });
            await _context.SaveChangesAsync();
            
            var result = await _controller.GetAllVisits();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task HealthCheck_DatabaseConnected_ReturnsHealthy()
        {
            var result = await _controller.HealthCheck();
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Theory]
        [InlineData("parking")]
        [InlineData("main-entrance")]
        [InlineData("registration")]
        [InlineData("consultation")]
        [InlineData("lab")]
        [InlineData("radiology")]
        [InlineData("pharmacy")]
        [InlineData("exit")]
        public async Task Scan_AllValidLocations_ReturnsOk(string location)
        {
            var request = new ScanRequest { PatientId = "P001", Location = location };
            var result = await _controller.Scan(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Scan_InvalidPatientIds_ReturnsBadRequest(string patientId)
        {
            var request = new ScanRequest { PatientId = patientId, Location = "parking" };
            var result = await _controller.Scan(request);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid-location")]
        [InlineData("PARKING")]
        public async Task Scan_InvalidLocations_ReturnsBadRequest(string location)
        {
            var request = new ScanRequest { PatientId = "P001", Location = location };
            var result = await _controller.Scan(request);
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}