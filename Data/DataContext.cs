using Microsoft.EntityFrameworkCore;
using Shop.Models;

namespace Shop.Data
{
    public class DataContext : DbContext 
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; } // busca no banco uma tabela chamada "Products" e vai mapear os itens conforme a model "Product" 
        // DbSet é responsável pela comunicação com o banco para realizar o CRUD
        public DbSet<Category> Categories { get; set; }

        public DbSet<User> Users { get; set; }
    }
}