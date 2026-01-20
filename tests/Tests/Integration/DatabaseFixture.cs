using Infra.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Integration;

public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }
    
    public DatabaseFixture()
    {
        // Usa o banco PostgreSQL rodando no Docker
        var connectionString = "Host=localhost;Port=5432;Database=audittrace;Username=postgres;Password=postgres";
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        Context = new ApplicationDbContext(optionsBuilder.Options);
        
        // Garante que o banco está criado e limpo para os testes
        Context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        // Limpa os dados após os testes
        Context.Products.RemoveRange(Context.Products);
        Context.SaveChanges();
        Context.Dispose();
    }
    
    public void CleanDatabase()
    {
        Context.Products.RemoveRange(Context.Products);
        Context.SaveChanges();
    }
}
