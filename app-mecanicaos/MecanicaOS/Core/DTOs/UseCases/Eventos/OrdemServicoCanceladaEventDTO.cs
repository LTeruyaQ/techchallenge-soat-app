namespace Core.DTOs.UseCases.Eventos
{
    public class OrdemServicoCanceladaEventDTO
    {
        public Guid OrdemServicoId { get; set; }

        public OrdemServicoCanceladaEventDTO(Guid ordemServicoId)
        {
            OrdemServicoId = ordemServicoId;
        }
    }
}
