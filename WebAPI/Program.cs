namespace GestorPedidoAPI.WebAPI;

using Microsoft.EntityFrameworkCore;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.WebAPI.Middlewares;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("GestorPedidoDB"));
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
        });

        var app = builder.Build();

        app.UseMiddleware<ExceptionMiddleware>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

        }
        app.UseAuthorization();
        app.MapControllers();


        app.MapGet("/", () => "API Gestor de Pedidos Rodando!")
            .WithName("GetRoot") // Nome da operação no Swagger
            .WithMetadata(new Microsoft.OpenApi.Models.OpenApiOperation
            {
                Summary = "Verifica o status da API",
                Description = "Retorna uma mensagem indicando que a API está rodando."
            })
            .Produces<string>(200, "text/plain"); // Indica o tipo de retorno e código de status



        app.Run();
    }
}
