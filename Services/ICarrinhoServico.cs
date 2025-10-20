using Ecomerce.Models;

namespace Ecomerce.Services
{
    public interface ICarrinhoServico
    {
        Task<string> AdicionarItem(int variacaoId, int quantidade);
        void RemoverItem(int variacaoId); 
        List<ItemCarrinho> ObterItens();
        Task<List<ItemCarrinho>> ObterDetalhesDoCarrinho();
        void LimparCarrinho();
        decimal ObterTotal(List<ItemCarrinho> itens);
    }
}