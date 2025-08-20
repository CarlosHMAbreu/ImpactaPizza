using Microsoft.EntityFrameworkCore;
using Pizzaria.Models;

namespace Pizzaria.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasIndex(u => u.Email).IsUnique();
            });
            
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("Pedidos");
                entity.HasMany(p => p.Itens)
                      .WithOne()
                      .HasForeignKey(i => i.PedidoId);
            });
            
            modelBuilder.Entity<ItemPedido>(entity =>
            {
                entity.ToTable("ItensPedido");
            });
        }
    }
}