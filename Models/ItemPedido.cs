using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecomerce.Models;

public class ItemPedido
{
    [Key]
    public int Id { get; set; }

    public int Quantidade { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PrecoUnitario { get; set; }

    public int ProdutoId { get; set; }
    [ForeignKey("ProdutoId")]
    public Produto Produto { get; set; } = default!;

    public int PedidoId { get; set; }
    [ForeignKey("PedidoId")]
    public Pedido Pedido { get; set; } = default!;
}