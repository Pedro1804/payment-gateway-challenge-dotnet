namespace PaymentGateway.Api.Api.Requests;

public class PostPaymentRequest
{
    /// <summary>Between 14 and 19 digits.</summary>
    /// <example>2222405343248877</example>
    public required string CardNumber { get; set; }
    /// <summary>Between 1 and 12. Together with the year, must not be in the past.</summary>
    /// <example>4</example>
    public required int ExpiryMonth { get; set; }
    /// <example>2030</example>
    public required int ExpiryYear { get; set; }
    /// <summary>One of EUR, USD or GBP.</summary>
    /// <example>GBP</example>
    public required string Currency { get; set; }
    /// <summary>Positive amount in minor currency units, e.g. 1050 for £10.50.</summary>
    /// <example>1050</example>
    public required int Amount { get; set; }
    /// <summary>3 or 4 digits.</summary>
    /// <example>123</example>
    public required string Cvv { get; set; }
}
