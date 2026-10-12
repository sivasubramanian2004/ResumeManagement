using ResumeManagement.Api.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
namespace ResumeManagement.Api.FluentAPI
{
    public class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
    {
        public void Configure(EntityTypeBuilder<Applicant> builder)
        {
            builder.ToTable("Applicants");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.FullName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.Email)
                .HasMaxLength(254)
                .IsRequired();

            builder.HasIndex(a => a.Email)
                .IsUnique();
                

            builder.Property(a => a.Mobile)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(a => a.Location)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.ResumeKey)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(a => a.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(a => a.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder.Property(a => a.UpdatedAt)
                .HasColumnType("datetime(6)")
                .IsRequired(false);

            builder.Property(a => a.DeletedAt)
              .HasColumnType("datetime(6)")
              .IsRequired(false);
        }
    }
}