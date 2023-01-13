namespace DemoMinimalAPI.Models
{
    public class Fornecedor
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? NIF { get; set; }
        public bool Activo { get; set; }

    }
}
