using Billing.Infrastructure.PaymentGateways;
using Billing.Infrastructure.PaymentGateways.Mocks;
using Billing.Infrastructure.Repositories;
using Billing.Logic.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<IPaymentGateway, CardPaymentGateway>();
builder.Services.AddSingleton<IPaymentGateway, BankPaymentGateway>();
builder.Services.AddSingleton<IPaymentGatewayResolver, PaymentGatewayResolver>();
builder.Services.AddScoped<IOrderService, OrderService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Billing API v1"));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
