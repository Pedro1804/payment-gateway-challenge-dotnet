using System.Net;

using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Infrastructure.Bank;

public sealed class AcquiringBankClient(HttpClient httpClient, ILogger<AcquiringBankClient> logger) : IAcquiringBank
{
    public async Task<BankAuthorization> AuthorizeAsync(CardPayment payment, CancellationToken cancellationToken)
    {
        using var response = await PostToBankAsync(BankPaymentRequest.From(payment), cancellationToken);
        EnsureBankAnsweredOk(response);
        var answer = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(cancellationToken);
        return answer!.ToBankAuthorization();
    }

    private async Task<HttpResponseMessage> PostToBankAsync(
        BankPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.PostAsJsonAsync("payments", request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning("Acquiring bank is unreachable: {ErrorType}", exception.GetType().Name);
            throw new AcquiringBankUnavailableException("Acquiring bank is unreachable.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Acquiring bank did not answer in time");
            throw new AcquiringBankUnavailableException("Acquiring bank did not answer in time.", exception);
        }
    }

    private void EnsureBankAnsweredOk(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return;
        }

        logger.LogWarning("Acquiring bank answered with status {StatusCode}", (int)response.StatusCode);
        throw new AcquiringBankUnavailableException(
            $"Acquiring bank answered with status {(int)response.StatusCode}.");
    }
}
