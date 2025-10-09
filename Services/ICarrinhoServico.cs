using Ecomerce.Models;

namespace Ecomerce.Services
{
    public interface ICarrinhoServico
    {
        void AdicionarItem(int produtoId, int quantidade);
        void RemoverItem(int produtoId);
        List<ItemCarrinho> ObterItens();
        void LimparCarrinho();
        decimal ObterTotal(List<ItemCarrinho> itens);
    }
}