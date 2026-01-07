using Core.DTOs.UseCases.Estoque;
using Core.Entidades;

namespace Core.Interfaces.Handlers.Estoques
{
    /// <summary>
    /// Interface para o handler de cadastro de estoque
    /// </summary>
    public interface ICadastrarEstoqueHandler
    {
        /// <summary>
        /// Manipula a operação de cadastro de estoque
        /// </summary>
        /// <param name="request">DTO com os dados do estoque a ser cadastrado</param>
        /// <returns>Estoque cadastrado</returns>
        Task<Estoque> Handle(CadastrarEstoqueUseCaseDto request);
    }
}
