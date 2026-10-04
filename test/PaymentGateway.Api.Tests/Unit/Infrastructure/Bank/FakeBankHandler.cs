using System.Net;
using System.Text;

namespace PaymentGateway.Api.Tests.Unit.Infrastructure.Bank;

public sealed class FakeBankHandler : HttpMessageHandler
{
    private readonly Func<HttpResponseMessage> _respond;

    private FakeBankHandler(Func<HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    public HttpMethod? ReceivedMethod { get; private set; }

    public Uri? ReceivedUri { get; private set; }

    public string? ReceivedBody { get; private set; }

    public static FakeBankHandler Answering(HttpStatusCode status, string jsonBody = "{}") =>
        new(() => new HttpResponseMessage(status)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        });

    public static FakeBankHandler Failing(Exception exception) => new(() => throw exception);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ReceivedMethod = request.Method;
        ReceivedUri = request.RequestUri;
        ReceivedBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return _respond();
    }
}
