using Microsoft.EntityFrameworkCore;
using Pizzaria.Models;

namespace Pizzaria.Data
{
    public interface IAppDbContext
    {
        DbSet<Usuario> Usuarios { get; set; }
        DbSet<Administrador> Administradores { get; set; }
        DbSet<Pedido> Pedidos { get; set; }
        DbSet<ItemPedido> ItensPedido { get; set; }
    }
}