using System.ComponentModel.DataAnnotations;

namespace hospital_time_tracker_service.Models
{
    public class Visit
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; } = string.Empty;
        
        [Required]
        public string Location { get; set; } = string.Empty;
        
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        
        public string ScanType { get; set; } = "normal";
        
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}