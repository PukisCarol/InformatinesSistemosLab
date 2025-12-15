using Microsoft.EntityFrameworkCore;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Data 
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Customers { get; set; }
        public DbSet<Game> Games { get; set; }

        public DbSet<Zaidimas> Zaidimai { get; set; } = null!;
        public DbSet<Zanras> Zanrai { get; set; } = null!;
        public DbSet<Atsiliepimas> Atsiliepimai { get; set; } = null!;
        public DbSet<Naudotojas> Naudotojai { get; set; } = null!;
        public DbSet<Teises> Teises { get; set; } = null!;
        public DbSet<Uzsakymas> Uzsakymai { get; set; } = null!;
        public DbSet<Apmokejimas> Apmokejimai { get; set; } = null!;
        public DbSet<Siuntimas> Siuntimai { get; set; } = null!;
        public DbSet<KompiuterinisZaidimas> KompiuteriniaiZaidimai { get; set; } = null!;
        public DbSet<StaloZaidimas> StaloZaidimai { get; set; } = null!;
        public DbSet<Nusiskundimas> Nusiskundimai { get; set; } = null!;
        public DbSet<ZanrasPriklauso> ZanrasPriklauso { get; set; } = null!; // jei naudoji many-to-many

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Jei DB lentelių pavadinimai sutampa su klasėmis – galima nieko nedaryti.
            // Jei tavo DB yra kitokių pavadinimų/laukų, pririšk čia.

            // Pvz., jei DB lentelė vadinasi "zaidimas" ir stulpeliai pagal tavo klasę:
            modelBuilder.Entity<Zaidimas>(e =>
            {
                e.ToTable("zaidimas", "public");      // jei schema public
                e.HasKey(x => x.ZaidimoId);

                // FK į Zanras
                e.HasOne(x => x.Zanras)
                 .WithMany(z => z.Zaidimai)
                 .HasForeignKey(x => x.ZanroId);

                // FK į Savininką (Naudotojas)
                e.HasOne(x => x.Savininkas)
                 .WithMany(n => n.ParduodamiNuomojamiZaidimai)
                 .HasForeignKey(x => x.SavininkoId);
            });

            modelBuilder.Entity<Zanras>(e =>
            {
                e.ToTable("zanras", "public");
                e.HasKey(x => x.ZanroId);
            });

            modelBuilder.Entity<Atsiliepimas>(e =>
            {
                e.ToTable("atsiliepimas", "public");
                e.HasKey(x => x.AtsiliepimoId);

                e.HasOne(a => a.Zaidimas)
                 .WithMany(g => g.Atsiliepimai)
                 .HasForeignKey(a => a.ZaidimoId);

                e.HasOne(a => a.Naudotojas)
                 .WithMany(n => n.Atsiliepimai)
                 .HasForeignKey(a => a.NaudotojasId);
            });

            // Many-to-many per jungtinę lentelę (jei naudoji):
            modelBuilder.Entity<ZanrasPriklauso>(e =>
            {
                e.ToTable("zanraspriklauso", "public"); // pritaikyk realų pavadinimą
                e.HasKey(x => new { x.fk_ZaidimasZaidimoId, x.fk_ZanrasZanroId });

                e.HasOne<Zaidimas>()
                 .WithMany()
                 .HasForeignKey(x => x.fk_ZaidimasZaidimoId);

                e.HasOne<Zanras>()
                 .WithMany()
                 .HasForeignKey(x => x.fk_ZanrasZanroId);
            });
        }
    }

}
 