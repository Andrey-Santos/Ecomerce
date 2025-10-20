using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ecomerce.Models;

namespace Ecomerce.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; } = default!;
        public DbSet<Variacao> Variacoes { get; set; } = default!;  
        public DbSet<ItemPedido> ItensPedido { get; set; } = default!;
    }
}