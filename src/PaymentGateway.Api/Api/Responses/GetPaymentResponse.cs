namespace PaymentGateway.Api.Api.Responses;

public class GetPaymentResponse
{
    /// <summary>Identifier to retrieve the payment with.</summary>
    public Guid Id { get; set; }
    /// <summary>Authorized or Declined.</summary>
    public required string Status { get; set; }
    public required string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public required string Currency { get; set; }
    /// <summary>Amount in minor currency units.</summary>
    public int Amount { get; set; }
}