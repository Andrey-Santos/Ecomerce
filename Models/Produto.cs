using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecomerce.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome do Produto")] 
        public string Nome { get; set; } = default!;

        [Display(Name = "Descrição Detalhada")]
        public string Descricao { get; set; } = default!;

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
        [Display(Name = "Preço")] 
        public decimal Preco { get; set; }

        [Display(Name = "URL da Imagem")]
        public string ImagemUrl { get; set; } = default!;
        
        [NotMapped]
        public IFormFile ImagemUpload { get; set; } 
    
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo.")]
        [Display(Name = "Estoque Disponível")] 
        public int Estoque { get; set; }
    }
}