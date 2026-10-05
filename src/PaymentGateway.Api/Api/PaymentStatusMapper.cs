using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Api;

public static class PaymentStatusMapper
{
    public static string ToResponseStatus(this PaymentStatus status) => status switch
    {
        PaymentStatus.Authorized => "Authorized",
        PaymentStatus.Declined => "Declined",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Only processed payments are returned.")
    };
}