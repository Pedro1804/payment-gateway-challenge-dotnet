using System.Net;
using System.Text;

namespace PaymentGateway.Api.Tests.Unit.Infrastructure.Bank;

public sealed class FakeBankHandler : HttpMessageHandler
{
    private Func<HttpResponseMessage> _respond =
        () => throw new InvalidOperationException("The fake bank was not told how to answer.");

    public HttpMethod? ReceivedMethod { get; private set; }

    public Uri? ReceivedUri { get; private set; }

    public string? ReceivedBody { get; private set; }

    public void Answers(HttpStatusCode status, string jsonBody = "{}") =>
        _respond = () => new HttpResponseMessage(status)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };

    public void Fails(Exception exception) => _respond = () => throw exception;

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
