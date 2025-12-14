namespace Infraestrutura.Logs.DTOs
{
    public class LogEntryDto
    {
        public string? Nivel { get; set; } = string.Empty;
        public string? Classe { get; set; } = string.Empty;
        public string? Metodo { get; set; } = string.Empty;
        public string Etapa { get; set; } = string.Empty;
        public string? CorrelationId { get; set; } = string.Empty;
        public string? TraceId { get; set; }
        public string? SpanId { get; set; }
        public object Dados { get; set; } = new object();
        public DateTime Timestamp { get; set; }
        public string? Usuario { get; set; } = string.Empty;
    }
}