namespace Core.DTOs.UseCases.AlertaEstoque
{
    public class CadastrarAlertaEstoqueUseCaseDto
    {
        public Guid EstoqueId { get; set; }
        public DateTime DataEnvio { get; set; }
    }
}
