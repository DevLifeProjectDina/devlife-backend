using DevLifeBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLifeBackend.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
          
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(20);
            builder.HasIndex(e => e.Username).IsUnique();


            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(e => e.Surname)
                .IsRequired() 
                .HasMaxLength(50);


            builder.Property(e => e.Stacks)
                .IsRequired()
                .HasColumnType("text[]");


            builder.Property(e => e.ExperienceLevel)
                .IsRequired() 
                .HasConversion<string>();

         
            builder.Property(e => e.DateOfBirth)
                .IsRequired() 
                .HasColumnType("timestamp with time zone");

            builder.ToTable("Users");
        }
    }
}
