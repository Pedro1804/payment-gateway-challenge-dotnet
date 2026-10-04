namespace PaymentGateway.Api.Domain.Ports;

public sealed class AcquiringBankUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
