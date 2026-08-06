using EgyMediChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SystemUser> SystemUsers => Set<SystemUser>();
    public DbSet<AuthRefreshToken> AuthRefreshTokens => Set<AuthRefreshToken>();
    public DbSet<RegistrationRequest> RegistrationRequests => Set<RegistrationRequest>();
    public DbSet<EntityDocument> EntityDocuments => Set<EntityDocument>();
    public DbSet<Factory> Factories => Set<Factory>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
    public DbSet<MedicineProduct> MedicineProducts => Set<MedicineProduct>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<UnitCode> UnitCodes => Set<UnitCode>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<PublicVerificationScan> PublicVerificationScans => Set<PublicVerificationScan>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EntityLicense> EntityLicenses => Set<EntityLicense>();
    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<ReportJob> ReportJobs => Set<ReportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Keep constraints loose on purpose (nullable-friendly, minimal FK cascade restrictions)
        // so the API stays forgiving for a fast-moving frontend integration.

        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.Factory).WithMany().HasForeignKey(r => r.FactoryId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.Warehouse).WithMany().HasForeignKey(r => r.WarehouseId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.Pharmacy).WithMany().HasForeignKey(r => r.PharmacyId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RegistrationRequest>()
            .HasOne(r => r.SystemUser).WithMany().HasForeignKey(r => r.SystemUserId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EntityDocument>()
            .HasOne(d => d.RegistrationRequest).WithMany(r => r.Documents)
            .HasForeignKey(d => d.RegistrationRequestId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Pharmacy>()
            .HasOne(p => p.DefaultWarehouse).WithMany().HasForeignKey(p => p.DefaultWarehouseId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Batch>()
            .HasOne(b => b.MedicineProduct).WithMany(m => m.Batches).HasForeignKey(b => b.MedicineProductId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Batch>()
            .HasOne(b => b.Factory).WithMany(f => f.Batches).HasForeignKey(b => b.FactoryId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<UnitCode>()
            .HasOne(u => u.Batch).WithMany(b => b.UnitCodes).HasForeignKey(u => u.BatchId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Shipment>()
    .HasOne(s => s.Batch)
    .WithMany(b => b.Shipments)
    .HasForeignKey(s => s.BatchId)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Shipment>()
      .HasOne<Factory>()
      .WithMany()
      .HasForeignKey(s => s.SourceFactoryId)
      .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Shipment>()
            .HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(s => s.SourceWarehouseId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Shipment>()
            .HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(s => s.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Shipment>()
            .HasOne<Pharmacy>()
            .WithMany()
            .HasForeignKey(s => s.DestinationPharmacyId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InventoryStock>()
            .HasOne(i => i.Batch).WithMany(b => b.InventoryStocks).HasForeignKey(i => i.BatchId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<InventoryStock>()
            .HasOne<Warehouse>().WithMany().HasForeignKey(i => i.WarehouseId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<InventoryStock>()
            .HasOne<Pharmacy>().WithMany().HasForeignKey(i => i.PharmacyId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Batch).WithMany(b => b.Alerts).HasForeignKey(a => a.BatchId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Shipment).WithMany().HasForeignKey(a => a.ShipmentId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PublicVerificationScan>()
            .HasOne(s => s.UnitCode).WithMany().HasForeignKey(s => s.UnitCodeId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AuthRefreshToken>()
            .HasOne(t => t.SystemUser).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.SystemUserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
