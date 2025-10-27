using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecomerce.Models
{
    public class Cupom
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O código é obrigatório.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O código deve ter entre 3 e 50 caracteres.")]
        [Display(Name = "Código do Cupom")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "O tipo de desconto é obrigatório.")]
        [Display(Name = "Tipo de Desconto")]
        public TipoDesconto TipoDesconto { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Valor do Desconto")]
        public decimal Valor { get; set; }

        [Display(Name = "Mínimo do Pedido")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ValorMinimoPedido { get; set; } 

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Data de Expiração")]
        public DateTime? DataExpiracao { get; set; }

        [Display(Name = "Limite de Uso")]
        public int? LimiteUsos { get; set; }

        [Display(Name = "Usos Atuais")]
        public int UsosAtuais { get; set; } = 0;
    }

    public enum TipoDesconto
    {
        Porcentagem = 1,
        Fixo = 2 
    }
}