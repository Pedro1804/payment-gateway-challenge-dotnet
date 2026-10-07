using System.Text.Json.Serialization;

using Microsoft.OpenApi.Models;

using PaymentGateway.Api.Api.Responses;
using PaymentGateway.Api.Domain.Ports;
using PaymentGateway.Api.Domain.Services;
using PaymentGateway.Api.Infrastructure.Bank;
using PaymentGateway.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = RejectedPaymentResponse.From);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Gateway API",
        Version = "v1",
        Description = "Lets merchants process card payments through an acquiring bank and retrieve past payments."
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"));
});
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<PaymentProcessor>();
builder.Services.AddScoped<PaymentRetriever>();
builder.Services.AddHttpClient<IAcquiringBank, AcquiringBankClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Bank:BaseUrl"]!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
