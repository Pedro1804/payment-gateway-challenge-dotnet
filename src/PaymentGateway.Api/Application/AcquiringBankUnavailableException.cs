namespace PaymentGateway.Api.Application;

public sealed class AcquiringBankUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
