using Microsoft.EntityFrameworkCore;

using ClaimWise.Domain.Entities;

namespace ClaimWise.Infrastructure.Data

{

    public class ClaimWiseDbContext : DbContext

    {

        public ClaimWiseDbContext(DbContextOptions<ClaimWiseDbContext> options)

            : base(options) { }

        // Entity sets

        public DbSet<Agent> Agent { get; set; }

        public DbSet<Hospital> Hospital { get; set; }

        public DbSet<Treatment> Treatment { get; set; }

        public DbSet<PolicyType> PolicyType { get; set; }

        public DbSet<Policyholder> Policyholder { get; set; }

        public DbSet<Dependent> Dependent { get; set; }

        public DbSet<Claim> Claim { get; set; }

        public DbSet<EligibilityCheck> EligibilityCheck { get; set; }

        public DbSet<Payout> Payout { get; set; }

        public DbSet<ClaimsReport> ClaimsReport { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<LoginAuditLog> LoginAuditLogs { get; set; }

        public DbSet<DocumentAccessLog> DocumentAccessLogs { get; set; }

        public DbSet<ClaimActionLog> ClaimActionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {

            base.OnModelCreating(modelBuilder);

            // Primary keys

            modelBuilder.Entity<Agent>().HasKey(a => a.AgentID);

            modelBuilder.Entity<Policyholder>().HasKey(p => p.PolicyholderID);

            modelBuilder.Entity<Dependent>().HasKey(d => d.DependentID);

            modelBuilder.Entity<PolicyType>().HasKey(pt => pt.PolicyTypeID);

            modelBuilder.Entity<Hospital>().HasKey(h => h.HospitalID);

            modelBuilder.Entity<Treatment>().HasKey(t => t.TreatmentID);

            modelBuilder.Entity<Claim>().HasKey(c => c.ClaimID);

            modelBuilder.Entity<EligibilityCheck>().HasKey(e => e.CheckID);

            modelBuilder.Entity<Payout>().HasKey(p => p.PayoutID);

            modelBuilder.Entity<ClaimsReport>().HasKey(cr => cr.ReportID);

            modelBuilder.Entity<User>().HasKey(u => u.UserID);

            modelBuilder.Entity<RefreshToken>().HasKey(rt => rt.TokenID);

            modelBuilder.Entity<LoginAuditLog>().HasKey(l => l.LogID);

            modelBuilder.Entity<DocumentAccessLog>().HasKey(l => l.LogID);

            modelBuilder.Entity<ClaimActionLog>().HasKey(l => l.LogID);

            // ✅ Explicit table mapping

            modelBuilder.Entity<ClaimActionLog>().ToTable("ClaimActionLog");

            // ✅ Policyholder mapping

            modelBuilder.Entity<Policyholder>(entity =>

            {

                entity.ToTable("Policyholder");

                entity.Property(p => p.Name)

                    .IsRequired()

                    .HasMaxLength(100);

                entity.Property(p => p.PolicyTypeID).IsRequired();

                entity.Property(p => p.CoverageDetails)

                    .IsRequired()

                    .HasMaxLength(500);

                entity.Property(p => p.AgentID).IsRequired();

                entity.Property(p => p.Region)

                    .IsRequired()

                    .HasMaxLength(100);

                entity.Property(p => p.ProductType)

                    .IsRequired()

                    .HasMaxLength(100);

                entity.Property(p => p.PhoneNumber)

                    .HasMaxLength(20); // ✅ Nullable by default

                entity.Property(p => p.BankReferenceNumber)

                   .HasMaxLength(20);
                // ✅ Relationships

                entity.HasOne(p => p.PolicyType)

                    .WithMany(pt => pt.Policyholders)

                    .HasForeignKey(p => p.PolicyTypeID)

                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Agent)

                    .WithMany(a => a.Policyholders)

                    .HasForeignKey(p => p.AgentID)

                    .OnDelete(DeleteBehavior.Restrict);

            });

            // ✅ Audit fields: ClaimActionLog

            modelBuilder.Entity<ClaimActionLog>(entity =>

            {

                entity.Property(e => e.Action).HasMaxLength(500);

                entity.Property(e => e.PerformedBy).HasMaxLength(100);

                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            });

            // ✅ Audit fields: EligibilityCheck

            modelBuilder.Entity<EligibilityCheck>(entity =>

            {

                entity.Property(e => e.RuleApplied).HasMaxLength(500);

                entity.Property(e => e.Result).HasMaxLength(100);

                entity.Property(e => e.Reason).HasMaxLength(500);

                entity.Property(e => e.CheckedBy).HasMaxLength(100);

                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            });

            // ✅ Audit fields: Payout

            modelBuilder.Entity<Payout>(entity =>

            {

                entity.Property(p => p.ApprovalStatus).HasMaxLength(50);

                entity.Property(p => p.PayoutStatus).HasMaxLength(50);

                entity.Property(p => p.TransferStatus).HasMaxLength(50);

                entity.Property(p => p.ApprovedByLevel1).HasMaxLength(100);

                entity.Property(p => p.ApprovedByLevel2).HasMaxLength(100);

                entity.Property(p => p.BankReferenceNumber).HasMaxLength(100);

            });

            // ✅ Constraints for ClaimsReport

            modelBuilder.Entity<ClaimsReport>(entity =>

            {

                entity.Property(r => r.Type)

                    .IsRequired()

                    .HasMaxLength(50);

                entity.Property(r => r.Insights)

                    .IsRequired();

                entity.Property(r => r.GeneratedDate)

                    .IsRequired();

            });

            // ✅ Audit fields: LoginAuditLog

            modelBuilder.Entity<LoginAuditLog>(entity =>

            {

                entity.Property(l => l.IPAddress)

                    .HasMaxLength(45)

                    .IsRequired(false); // ✅ Nullable

                entity.Property(l => l.UserAgent)

                    .HasMaxLength(300)

                    .IsRequired(false); // ✅ Nullable

                entity.Property(l => l.Timestamp)

                    .HasDefaultValueSql("GETUTCDATE()");

            });

            // ✅ Relationships

            modelBuilder.Entity<LoginAuditLog>()

                .HasOne(l => l.User)

                .WithMany(u => u.LoginAuditLogs)

                .HasForeignKey(l => l.UserID)

                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Dependent>()

                .HasOne(d => d.Policyholder)

                .WithMany(p => p.Dependents)

                .HasForeignKey(d => d.PolicyholderID)

                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Claim>()

                .HasOne(c => c.Policyholder)

                .WithMany(p => p.Claims)

                .HasForeignKey(c => c.PolicyholderID)

                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Claim>()

                .HasOne(c => c.Hospital)

                .WithMany(h => h.Claims)

                .HasForeignKey(c => c.HospitalID)

                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Claim>()

                .HasOne(c => c.Treatment)

                .WithMany(t => t.Claims)

                .HasForeignKey(c => c.TreatmentID)

                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EligibilityCheck>()

                .HasOne(e => e.Claim)

                .WithMany(c => c.EligibilityChecks)

                .HasForeignKey(e => e.ClaimID)

                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payout>()

                .HasOne(p => p.Claim)

                .WithMany(c => c.Payouts)

                .HasForeignKey(p => p.ClaimID)

                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClaimsReport>()

                .HasOne(r => r.Claim)

                .WithMany(c => c.ClaimsReports)

                .HasForeignKey(r => r.ClaimID)

                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClaimsReport>()

                .HasOne(r => r.Policyholder)

                .WithMany(p => p.ClaimsReports)

                .HasForeignKey(r => r.PolicyholderID)

                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()

                .HasOne(rt => rt.User)

                .WithMany(u => u.RefreshTokens)

                .HasForeignKey(rt => rt.UserID);

            modelBuilder.Entity<ClaimActionLog>()

                .HasOne(l => l.Claim)

                .WithMany(c => c.ClaimActionLogs)

                .HasForeignKey(l => l.ClaimID)

                .OnDelete(DeleteBehavior.Cascade);
            // ✅ User entity mapping

            modelBuilder.Entity<User>(entity =>

            {

                entity.ToTable("User");

                entity.Property(e => e.Username)

                    .IsRequired()

                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)

                    .IsRequired()

                    .HasMaxLength(255);

                entity.Property(e => e.Role)

                    .IsRequired()

                    .HasMaxLength(50);

            });

        }

        // ✅ Seeding Lance as Admin

        public void SeedLanceAdmin()

        {

            if (!Users.Any(u => u.Username == "Lance"))

            {

                Users.Add(new User

                {

                    Username = "Lance",

                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lance@123"),

                    Role = "Admin"

                });

                SaveChanges();

            }

        }

        public void SeedPratheekManager()
        {
            if (!Users.Any(u => u.Username == "Pratheek"))
            {
                Users.Add(new User
                {
                    Username = "Pratheek",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pratheek@123"),
                    Role = "Manager"
                });

                SaveChanges();
            }
        }



    }

}

