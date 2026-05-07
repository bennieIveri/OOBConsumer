using Microsoft.EntityFrameworkCore;
using OOB.Data.Entities;

namespace OOB.Data;

public class OOBDbContext : DbContext
{
    public OOBDbContext(DbContextOptions<OOBDbContext> options) : base(options)
    {
    }

    public DbSet<TransactionResponse> TransactionResponses { get; set; }
    public DbSet<TransactionResponseNameValue> TransactionResponseNameValues { get; set; }
    public DbSet<ProcessingStatus> ProcessingStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TransactionResponse configuration
        modelBuilder.Entity<TransactionResponse>(entity =>
        {
            entity.ToTable("TransactionResponse");
            entity.HasKey(e => e.TR_Id);
            entity.Property(e => e.TR_Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TR_RequestID).IsRequired();
            entity.Property(e => e.TR_ProcessingTime).IsRequired();
            entity.Property(e => e.TR_ApplicationID).IsRequired();
            entity.Property(e => e.TR_Mode).HasMaxLength(4).IsUnicode(false).IsRequired();
            entity.Property(e => e.TR_Acquirer).HasMaxLength(64).IsUnicode(false);
            entity.Property(e => e.TR_AcquirerCycle).HasMaxLength(8).IsUnicode(false);

            entity.HasOne(e => e.ProcessingStatus)
                  .WithMany(p => p.TransactionResponses)
                  .HasForeignKey(e => e.PS_Id)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TransactionResponseNameValue configuration
        modelBuilder.Entity<TransactionResponseNameValue>(entity =>
        {
            entity.ToTable("TransactionResponseNameValue");
            entity.HasKey(e => e.TRNV_Id);
            entity.Property(e => e.TRNV_Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TRNV_Name).HasMaxLength(128).IsUnicode(false).IsRequired();
            entity.Property(e => e.TRNV_Value).HasMaxLength(2048).IsRequired();

            entity.HasOne(e => e.TransactionResponse)
                  .WithMany(t => t.NameValues)
                  .HasForeignKey(e => e.TR_Id)
                  .OnDelete(DeleteBehavior.Cascade);

            // Computed columns are read-only in EF, so we ignore them
            //entity.Ignore(e => e.GetType().GetProperty("TRNV_ValueIndexedLeft384"));
            //entity.Ignore(e => e.GetType().GetProperty("TRNV_ValueIndexedSha256"));
        });

        // ProcessingStatus configuration
        modelBuilder.Entity<ProcessingStatus>(entity =>
        {
            entity.ToTable("ProcessingStatus");
            entity.HasKey(e => e.PS_Id);
            entity.Property(e => e.PS_Id).ValueGeneratedNever(); // Not an identity column
            entity.Property(e => e.PS_Description).HasMaxLength(64).IsUnicode(false).IsRequired();
        });
    }
}
