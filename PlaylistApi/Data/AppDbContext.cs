using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlaylistApi.Models;

namespace PlaylistApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Song> Songs => Set<Song>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite guarda las fechas como texto y al leerlas EF las devuelve con Kind=Unspecified,
        // lo que haría que el JSON saliera sin la "Z" final. Este convertidor las marca como UTC
        // para que el JSON se vea igual que el de NestJS: "2026-11-03T20:15:00.000Z".
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique(); // equivale a @unique de Prisma
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.CreatedAt).HasConversion(utcConverter);
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Singer).IsRequired().HasMaxLength(150);
            entity.Property(s => s.CreatedAt).HasConversion(utcConverter);
            entity.Property(s => s.UpdatedAt).HasConversion(utcConverter);

            entity.HasOne(s => s.User)
                  .WithMany(u => u.Songs)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
