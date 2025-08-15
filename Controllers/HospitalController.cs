using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hospital_time_tracker_service.Data;
using hospital_time_tracker_service.Models;

namespace hospital_time_tracker_service.Controllers
{
    [ApiController]
    [Route("api")]
    public class HospitalController : ControllerBase
    {
        private readonly HospitalContext _context;
        private readonly string[] _validLocations = { "parking", "main-entrance", "registration", "consultation", "lab", "radiology", "pharmacy", "exit" };


        public HospitalController(HospitalContext context)
        {
            _context = context;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> Scan([FromBody] ScanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PatientId) || string.IsNullOrWhiteSpace(request.Location))
                return BadRequest(new { error = "Invalid QR code", message = "Patient ID and location are required" });

            if (!_validLocations.Contains(request.Location))
                return BadRequest(new { error = "Invalid location", message = "Location not recognized" });

            try
            {
                // Check if this is the same location as the last scan for this patient
                var patientVisits = await _context.Visits
                    .Where(v => v.PatientId == request.PatientId)
                    .ToListAsync();
                var lastVisit = patientVisits
                    .OrderByDescending(v => v.Timestamp)
                    .FirstOrDefault();

                // Remove duplicate consecutive scans
                if (lastVisit != null && lastVisit.Location == request.Location)
                {
                    _context.Visits.Remove(lastVisit);
                    await _context.SaveChangesAsync();
                }

                var visit = new Visit 
                { 
                    PatientId = request.PatientId, 
                    Location = request.Location,
                    Timestamp = request.Timestamp ?? DateTimeOffset.UtcNow
                };
                _context.Visits.Add(visit);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Scan recorded successfully", timestamp = visit.Timestamp });
            }
            catch
            {
                return StatusCode(500, new { error = "Database unavailable", message = "Unable to connect to database" });
            }
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports([FromQuery] string? date, [FromQuery] string? patient_id)
        {
            try
            {
                var query = _context.Visits.AsQueryable();

                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var filterDate))
                    query = query.Where(v => v.Timestamp.Date == filterDate.Date);

                if (!string.IsNullOrEmpty(patient_id))
                    query = query.Where(v => v.PatientId == patient_id);

                var visits = await query.ToListAsync();
                visits = visits.OrderBy(v => v.Timestamp).ToList();
                var patientFlows = visits.GroupBy(v => v.PatientId)
                    .ToDictionary(g => g.Key, g => g.Select(v => new { location = v.Location, timestamp = v.Timestamp }).ToList());

                return Ok(new { visits, patientFlows, totalPatients = patientFlows.Count });
            }
            catch
            {
                return StatusCode(500, new { error = "Database unavailable", message = "Unable to connect to database" });
            }
        }

        [HttpGet("visits")]
        public async Task<IActionResult> GetAllVisits()
        {
            try
            {
                 var allVisits = await _context.Visits.ToListAsync();
                 var visits = allVisits.OrderByDescending(v => v.Timestamp).ToList();
                return Ok(visits);
            }
            catch
            {
                return StatusCode(500, new { error = "Database unavailable", message = "Unable to connect to database" });
            }
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return Ok(new { status = "healthy", database = "connected", timestamp = DateTimeOffset.UtcNow });
            }
            catch
            {
                return StatusCode(503, new { status = "unhealthy", database = "disconnected", timestamp = DateTimeOffset.UtcNow });
            }
        }
    }
}
