using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Ecomerce.Models
{
    public class ItemCarrinho
    {
        [Key]
        public int ProdutoId { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoTabela { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PrecoPromocional { get; set; }

        public int Quantidade { get; set; }
        
        [JsonIgnore] 
        public Produto Produto { get; set; } 
        
        public int VariacaoId { get; set; } 
        public string NomeVariacao { get; set; } = string.Empty; 
    }
}