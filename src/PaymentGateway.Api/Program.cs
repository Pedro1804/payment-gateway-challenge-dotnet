using System.Text.Json.Serialization;

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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
