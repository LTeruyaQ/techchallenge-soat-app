namespace Core.DTOs.Responses.Veiculo
{
    public class VeiculoResponse
    {
        public Guid Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public string Ano { get; set; } = string.Empty;
        public string? Anotacoes { get; set; }
        public Guid? ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
