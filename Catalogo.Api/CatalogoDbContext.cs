using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api
{
    public class Categoria
    {
        [Key]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }
    }

    public class Marca
    {
        [Key]
        public int IdMarca { get; set; }

        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país de origen es obligatorio.")]
        [StringLength(100)]
        public string PaisOrigen { get; set; } = string.Empty;
    }

    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        [Required]
        public int IdCategoria { get; set; }

        [Required]
        public int IdMarca { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El SKU es obligatorio.")]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Activo";

        // Propiedades de navegación para las FK físicas
        public Categoria? Categoria { get; set; }
        public Marca? Marca { get; set; }
    }

    public class CatalogoDbContext : DbContext
    {
        public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : base(options) { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Producto> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<Marca>().ToTable("Marca");
            modelBuilder.Entity<Producto>().ToTable("Producto");

            // Configuración de Llaves Foráneas Físicas en CatalogoDB
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.IdCategoria);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Marca)
                .WithMany()
                .HasForeignKey(p => p.IdMarca);
        }
    }
}