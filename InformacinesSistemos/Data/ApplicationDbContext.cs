
// /Data/ApplicationDbContext.cs
using InformacinesSistemos.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Zaidimas> Zaidimas { get; set; } = null!;
    public DbSet<Atsiliepimas> Atsiliepimas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Kiti konfigūracijos

        modelBuilder.Entity<Atsiliepimas>()
            .HasOne(a => a.Zaidimas)
            .WithMany(z => z.Atsiliepimai)
            .HasForeignKey(a => a.ZaidimoId)  // Pataisyta
            .OnDelete(DeleteBehavior.Cascade);  // Jei reikia, pridėk kitas opcijas

        // Jei yra kitas ryšys, pvz., su Naudotojas:
        modelBuilder.Entity<Atsiliepimas>()
            .HasOne(a => a.Naudotojas)
            .WithMany(n => n.Atsiliepimai)
            .HasForeignKey(a => a.NaudotojasId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kiti konfigūracijos
    }
}
