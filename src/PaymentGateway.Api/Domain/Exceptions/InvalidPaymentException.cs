namespace PaymentGateway.Api.Domain.Model;

public sealed class InvalidPaymentException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
