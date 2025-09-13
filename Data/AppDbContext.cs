using Microsoft.EntityFrameworkCore;
using Pizzaria.Models;

namespace Pizzaria.Data
{
    public class AppDbContext : DbContext // REMOVA: , IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }
        public DbSet<Pizza> Pizzas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração da tabela Usuarios
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.NomeCompleto).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Telefone).IsRequired().HasMaxLength(20);
                entity.Property(u => u.Endereco).IsRequired().HasMaxLength(200);
                entity.Property(u => u.Senha).IsRequired().HasMaxLength(100);
            });
            
            // Configuração da tabela Administradores
            modelBuilder.Entity<Administrador>(entity =>
            {
                entity.ToTable("Administradores");
                entity.HasOne(a => a.Usuario)
                      .WithMany()
                      .HasForeignKey(a => a.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(a => a.NivelAcesso).HasMaxLength(50);
            });
            
            // Configuração da tabela Pedidos
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("Pedidos");
                entity.HasOne(p => p.Usuario)
                      .WithMany()
                      .HasForeignKey(p => p.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasMany(p => p.Itens)
                      .WithOne()
                      .HasForeignKey(i => i.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configuração da tabela ItensPedido
            modelBuilder.Entity<ItemPedido>(entity =>
            {
                entity.ToTable("ItensPedido");
                entity.Property(i => i.NomePizza).HasMaxLength(100);
                entity.Property(i => i.PrecoPizza).HasColumnType("decimal(10,2)");

                // Relacionamento com Pizza
                _ = entity.HasOne(i => i.Pizza)
                      .WithMany()
                      .HasForeignKey(i => i.PizzaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configuração da tabela Pizzas
            modelBuilder.Entity<Pizza>(entity =>
            {
                entity.ToTable("Pizzas");
                entity.HasIndex(p => p.Nome).IsUnique();
                entity.Property(p => p.Preco).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Nome).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Descricao).IsRequired().HasMaxLength(500);
                entity.Property(p => p.ImagemUrl).HasMaxLength(500);
            });
        }
    }
}