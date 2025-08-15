using System.ComponentModel.DataAnnotations;

namespace hospital_time_tracker_service.Models
{
    /// <summary>
    /// Represents a patient visit record at a specific hospital location
    /// </summary>
    public class Visit
    {
        /// <summary>
        /// Unique identifier for the visit record
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Patient identifier from QR code scan
        /// </summary>
        [Required]
        public string PatientId { get; set; } = string.Empty;
        
        /// <summary>
        /// Hospital location where the scan occurred
        /// </summary>
        [Required]
        public string Location { get; set; } = string.Empty;
        
        /// <summary>
        /// Timestamp when the patient scanned at this location, with timezone offset support
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// Type of scan performed (default: "normal")
        /// </summary>
        public string ScanType { get; set; } = "normal";



        /// <summary>
        /// Timestamp when this record was created in the database, with timezone offset support
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}