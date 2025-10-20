// Models/Variacao.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecomerce.Models
{
    public class Variacao
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo.")]
        [Display(Name = "Estoque Disponível")]
        public int Estoque { get; set; }

        public int ProdutoId { get; set; }
        
        [ForeignKey("ProdutoId")]
        public Produto Produto { get; set; }
    }
}