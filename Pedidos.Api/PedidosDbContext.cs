using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Api
{
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "Los nombres del cliente son obligatorios.")]
        [StringLength(150)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20)]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;
    }

    public class Pedido
    {
        [Key]
        public int IdPedido { get; set; }

        // FK FÍSICA: Referencia a Cliente en PedidosDB
        [Required]
        public int IdCliente { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Creado";

        public Cliente? Cliente { get; set; }
        public List<DetallePedido> Detalles { get; set; } = new();
    }

    public class DetallePedido
    {
        [Key]
        public int IdDetalle { get; set; }

        // FK FÍSICA: Referencia a Pedido en PedidosDB
        [Required]
        public int IdPedido { get; set; }

        // FK LÓGICA: Referencia a Producto en Catalogo.Api (CatalogoDB)
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser válido.")]
        public int IdProducto { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [JsonIgnore]
        public Pedido? Pedido { get; set; }
    }

    public class PedidosDbContext : DbContext
    {
        public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>().ToTable("Cliente");
            modelBuilder.Entity<Pedido>().ToTable("Pedido");
            modelBuilder.Entity<DetallePedido>().ToTable("DetallePedido");

            // Configuración de Llaves Foráneas Físicas en PedidosDB
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.IdCliente);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdPedido);
        }
    }
}