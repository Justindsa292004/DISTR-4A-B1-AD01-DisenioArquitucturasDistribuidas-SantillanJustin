using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api
{
    public class Bodega
    {
        [Key]
        public int IdBodega { get; set; }

        [Required(ErrorMessage = "El nombre de la bodega es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        [StringLength(100)]
        public string Ciudad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(250)]
        public string Direccion { get; set; } = string.Empty;
    }

    public class Existencia
    {
        [Key]
        public int IdExistencia { get; set; }

        // FK LÓGICA: Referencia al producto en Catalogo.Api (CatalogoDB)
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser válido.")]
        public int IdProducto { get; set; }

        // FK FÍSICA: Referencia a Bodega en InventarioDB
        [Required]
        public int IdBodega { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        public Bodega? Bodega { get; set; }
    }

    public class MovimientoInventario
    {
        [Key]
        public int IdMovimiento { get; set; }

        // FK LÓGICA: Referencia al producto en Catalogo.Api (CatalogoDB)
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser válido.")]
        public int IdProducto { get; set; }

        // FK FÍSICA: Referencia a Bodega en InventarioDB
        [Required]
        public int IdBodega { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio (Entrada o Salida).")]
        [StringLength(50)]
        public string Tipo { get; set; } = "Entrada";

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public Bodega? Bodega { get; set; }
    }

    public class ReservaStock
    {
        [Key]
        public int IdReserva { get; set; }

        // FK LÓGICA: Referencia al producto en Catalogo.Api (CatalogoDB)
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser válido.")]
        public int IdProducto { get; set; }

        // FK FÍSICA: Referencia a Bodega en InventarioDB
        [Required]
        public int IdBodega { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad a reservar debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";

        public Bodega? Bodega { get; set; }
    }

    public class InventarioDbContext : DbContext
    {
        public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options) { }

        public DbSet<Bodega> Bodegas { get; set; }
        public DbSet<Existencia> Existencias { get; set; }
        public DbSet<MovimientoInventario> Movimientos { get; set; }
        public DbSet<ReservaStock> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bodega>().ToTable("Bodega");
            modelBuilder.Entity<Existencia>().ToTable("Existencia");
            modelBuilder.Entity<MovimientoInventario>().ToTable("MovimientoInventario");
            modelBuilder.Entity<ReservaStock>().ToTable("ReservaStock");

            // Configuración de Llaves Foráneas Físicas hacia Bodega (misma BD InventarioDB)
            modelBuilder.Entity<Existencia>()
                .HasOne(e => e.Bodega)
                .WithMany()
                .HasForeignKey(e => e.IdBodega);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Bodega)
                .WithMany()
                .HasForeignKey(m => m.IdBodega);

            modelBuilder.Entity<ReservaStock>()
                .HasOne(r => r.Bodega)
                .WithMany()
                .HasForeignKey(r => r.IdBodega);
        }
    }
}