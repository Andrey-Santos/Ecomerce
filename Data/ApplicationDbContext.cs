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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Variacao>()
                .HasOne(v => v.Produto)
                .WithMany(p => p.Variacoes)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ItemPedido>()
                .HasOne(ip => ip.Variacao)
                .WithMany()
                .HasForeignKey(ip => ip.VariacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ItemPedido>()
                .HasOne(ip => ip.Produto)
                .WithMany()
                .HasForeignKey(ip => ip.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ItemPedido>()
                .HasOne(ip => ip.Pedido)
                .WithMany(p => p.ItensPedido)
                .HasForeignKey(ip => ip.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}