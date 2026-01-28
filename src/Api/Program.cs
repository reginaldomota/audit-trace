using Amazon.SQS;
using Application.Interfaces;
using Application.Services;
using Audit.Extensions;
using Domain.Interfaces;
using Infra.Data;
using Infra.Repositories;
using Infra.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Audit Service (registrar ANTES dos serviços de aplicação)
builder.Services.AddAuditForApi(builder.Configuration);

// Dependency Injection
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IQueueService, SqsQueueService>();

// Habilita proxies automáticos para auditoria (DEVE ser chamado APÓS registrar TODOS os serviços)
builder.Services.EnableAuditProxies();

// CORS (opcional)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    // Migrate ApplicationDbContext
    var appDbContext = services.GetRequiredService<ApplicationDbContext>();
    appDbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Desabilitado para desenvolvimento local

// Middleware de Auditoria
app.UseAudit();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
