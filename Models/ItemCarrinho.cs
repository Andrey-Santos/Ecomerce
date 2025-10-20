// Models/ItemCarrinho.cs
using System.ComponentModel.DataAnnotations;

namespace Ecomerce.Models
{
    public class ItemCarrinho
    {
        [Key]
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public Produto Produto { get; set; }
        public int VariacaoId { get; set; } 
        public string NomeVariacao { get; set; } = string.Empty; 
    }
}