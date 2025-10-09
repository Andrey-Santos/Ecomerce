using System.ComponentModel.DataAnnotations;

namespace Ecomerce.Models
{
    public class Encomenda
    {
        public int Id { get; set; }
        
        public string? UtilizadorId { get; set; }
        
        [Display(Name = "Nome Completo")]
        public string NomeCliente { get; set; } = default!;
        
        [Display(Name = "Endereço")]
        public string Endereco { get; set; } = default!;
        
        [Display(Name = "Cidade")]
        public string Cidade { get; set; } = default!;
        
        [Display(Name = "Total da Encomenda")]
        public decimal EncomendaTotal { get; set; }
        
        [Display(Name = "Data da Encomenda")]
        public DateTime DataEncomenda { get; set; } = DateTime.Now;

        public List<ItemEncomenda>? ItensEncomenda { get; set; }
    }
}