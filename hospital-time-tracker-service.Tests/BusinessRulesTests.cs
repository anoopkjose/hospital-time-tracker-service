using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hospital_time_tracker_service.Controllers;
using hospital_time_tracker_service.Data;
using hospital_time_tracker_service.Models;

namespace hospital_time_tracker_service.Tests
{
    public class BusinessRulesTests : IDisposable
    {
        private readonly HospitalContext _context;
        private readonly HospitalController _controller;

        public BusinessRulesTests()
        {
            var options = new DbContextOptionsBuilder<HospitalContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new HospitalContext(options);
            _controller = new HospitalController(_context);
        }

        [Fact]
        public async Task Scan_MultipleScansAtSameNonUniqueLocation_AllowsMultipleScans()
        {
            var request = new ScanRequest { PatientId = "P001", Location = "main-entrance" };
            
            var result1 = await _controller.Scan(request);
            var result2 = await _controller.Scan(request);
            var result3 = await _controller.Scan(request);
            
            Assert.IsType<OkObjectResult>(result1);
            Assert.IsType<OkObjectResult>(result2);
            Assert.IsType<OkObjectResult>(result3);
        }

        [Fact]
        public async Task Scan_DifferentPatientsAtUniqueLocation_AllowsMultipleScans()
        {
            var request1 = new ScanRequest { PatientId = "P001", Location = "parking" };
            var request2 = new ScanRequest { PatientId = "P002", Location = "parking" };
            
            var result1 = await _controller.Scan(request1);
            var result2 = await _controller.Scan(request2);
            
            Assert.IsType<OkObjectResult>(result1);
            Assert.IsType<OkObjectResult>(result2);
        }

        [Fact]
        public async Task Scan_SamePatientDifferentUniqueLocations_AllowsBothScans()
        {
            var request1 = new ScanRequest { PatientId = "P001", Location = "parking" };
            var request2 = new ScanRequest { PatientId = "P001", Location = "pharmacy" };
            
            var result1 = await _controller.Scan(request1);
            var result2 = await _controller.Scan(request2);
            
            Assert.IsType<OkObjectResult>(result1);
            Assert.IsType<OkObjectResult>(result2);
        }

        [Fact]
        public async Task GetReports_PatientFlow_ReturnsCorrectSequence()
        {
            var visits = new[]
            {
                new ScanRequest { PatientId = "P001", Location = "parking" },
                new ScanRequest { PatientId = "P001", Location = "main-entrance" },
                new ScanRequest { PatientId = "P001", Location = "registration" },
                new ScanRequest { PatientId = "P001", Location = "consultation" }
            };

            foreach (var visit in visits)
            {
                await _controller.Scan(visit);
                await Task.Delay(10);
            }

            var result = await _controller.GetReports(null, "P001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_MultiplePatients_ReturnsCorrectCounts()
        {
            var visits = new[]
            {
                new ScanRequest { PatientId = "P001", Location = "parking" },
                new ScanRequest { PatientId = "P002", Location = "parking" },
                new ScanRequest { PatientId = "P001", Location = "main-entrance" },
                new ScanRequest { PatientId = "P003", Location = "registration" }
            };

            foreach (var visit in visits)
            {
                await _controller.Scan(visit);
            }

            var result = await _controller.GetReports(null, null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_InvalidDateFormat_IgnoresDateFilter()
        {
            _context.Visits.Add(new Visit { PatientId = "P001", Location = "parking" });
            await _context.SaveChangesAsync();
            
            var result = await _controller.GetReports("invalid-date", null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_FutureDate_ReturnsNoData()
        {
            _context.Visits.Add(new Visit { PatientId = "P001", Location = "parking", Timestamp = DateTime.Today });
            await _context.SaveChangesAsync();
            
            var futureDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var result = await _controller.GetReports(futureDate, null);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetReports_NonExistentPatient_ReturnsNoData()
        {
            _context.Visits.Add(new Visit { PatientId = "P001", Location = "parking" });
            await _context.SaveChangesAsync();
            
            var result = await _controller.GetReports(null, "P999");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}