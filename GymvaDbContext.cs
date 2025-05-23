using Microsoft.EntityFrameworkCore;
using GymvaActivacionWeb.Models;

namespace GymvaActivacionWeb.Data
{
    public class GymvaDbContext : DbContext
    {
        public GymvaDbContext(DbContextOptions<GymvaDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Gimnasio> Gimnasios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("usuario");
            modelBuilder.Entity<Gimnasio>().ToTable("gimnasio");
        }
    }
}
