namespace PaymentGateway.Api.Tests.Integration;

public sealed class FixedTimeProvider(DateOnly today) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}
