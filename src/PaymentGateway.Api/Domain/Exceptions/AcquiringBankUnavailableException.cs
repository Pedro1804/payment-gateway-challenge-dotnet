namespace PaymentGateway.Api.Domain.Exceptions;

public sealed class AcquiringBankUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
