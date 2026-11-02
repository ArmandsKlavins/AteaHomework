using Billing.Infrastructure.PaymentGateways;
using Billing.Infrastructure.PaymentGateways.Mocks;
using Billing.Infrastructure.Repositories;
using Billing.Logic.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<IReceiptRepository, InMemoryReceiptRepository>();
builder.Services.AddSingleton<IPaymentGateway, CardPaymentGateway>();
builder.Services.AddSingleton<IPaymentGateway, BankPaymentGateway>();
builder.Services.AddSingleton<IPaymentGatewayResolver, PaymentGatewayResolver>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Unhandled exceptions and empty error responses (e.g. unknown routes) become ProblemDetails, like the controllers' own errors
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Billing API v1"));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
