namespace PaymentGateway.Api.Domain;

public sealed class InvalidPaymentException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
