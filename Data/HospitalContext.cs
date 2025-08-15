using Microsoft.EntityFrameworkCore;
using hospital_time_tracker_service.Models;

namespace hospital_time_tracker_service.Data
{
    public class HospitalContext : DbContext
    {
        public HospitalContext(DbContextOptions<HospitalContext> options) : base(options) { }

        public virtual DbSet<Visit> Visits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Visit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PatientId).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Location).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ScanType).HasMaxLength(20).HasDefaultValue("normal");
                
                entity.HasIndex(e => e.PatientId);
                entity.HasIndex(e => e.Location);
                entity.HasIndex(e => e.Timestamp);
            });
        }
    }
}