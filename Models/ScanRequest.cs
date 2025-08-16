using System.ComponentModel.DataAnnotations;

namespace hospital_time_tracker_service.Models
{
    public class ScanRequest
    {
        [Required]
        public string PatientId { get; set; } = string.Empty;
        
        [Required]
        public string Location { get; set; } = string.Empty;
        
        public DateTimeOffset? Timestamp { get; set; }
    }
}