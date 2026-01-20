using Amazon.SQS;
using Application.Interfaces;
using Domain.Interfaces;
using Infra.Data;
using Infra.Repositories;
using Infra.Services;
using Jobs.Workers;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Database Configuration - PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// AWS SQS Configuration
builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var config = new AmazonSQSConfig
    {
        ServiceURL = builder.Configuration["AWS:SQS:ServiceUrl"]
    };
    // Credenciais fake para LocalStack
    var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");
    return new AmazonSQSClient(credentials, config);
});

// Dependency Injection
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IQueueService, SqsQueueService>();

// Register Workers
builder.Services.AddHostedService<ProductRegistrationWorker>();
builder.Services.AddHostedService<ProductValidationWorker>();

var host = builder.Build();

// Run migrations on startup
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

host.Run();
