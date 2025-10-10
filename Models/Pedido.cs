using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Ecomerce.Models;

public class Pedido
{
    [Key]
    public int Id { get; set; }

    public DateTime DataPedido { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPedido { get; set; }

    public string Status { get; set; } = "Pendente";

    [ForeignKey("UsuarioId")]
    public string UsuarioId { get; set; } = default!;
    public IdentityUser Usuario { get; set; } = default!;
    public string NomeCliente { get; set; } = default!;
    public string Endereco { get; set; } = default!;
    public string Cidade { get; set; } = default!;
    public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
}