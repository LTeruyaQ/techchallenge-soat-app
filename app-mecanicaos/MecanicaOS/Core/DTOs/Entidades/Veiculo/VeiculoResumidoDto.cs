namespace Core.DTOs.Entidades.Veiculo
{
    public class VeiculoResumidoDto
    {
        public Guid Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Ano { get; set; } = string.Empty;
    }
}
