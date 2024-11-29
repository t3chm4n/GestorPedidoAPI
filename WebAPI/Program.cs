namespace GestorPedidoAPI.WebAPI;

using Microsoft.EntityFrameworkCore;
using GestorPedidoAPI.Infrastructure.Persistence;

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
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapGet("/", () => "API Gestor de Pedidos Rodando!");

        app.Run();
    }
}
