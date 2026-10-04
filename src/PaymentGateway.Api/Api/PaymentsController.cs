using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Api.Requests;
using PaymentGateway.Api.Api.Responses;
using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;
using PaymentGateway.Api.Domain.Services;
using PaymentGateway.Api.Infrastructure.Persistence;

namespace PaymentGateway.Api.Api;

[Route("api/payments")]
[ApiController]
public class PaymentsController(
    PaymentsRepository paymentsRepository,
    PaymentProcessor paymentProcessor,
    TimeProvider timeProvider,
    ILogger<PaymentsController> logger) : Controller
{
    private DateOnly Today => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync(
        PostPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cardPayment = request.ToCardPayment(Today);
            var decision = await paymentProcessor.ProcessAsync(cardPayment, cancellationToken);
            return CreatedAtAction(nameof(GetPayment), new { id = decision.Id }, decision.ToPostPaymentResponse());
        }
        catch (InvalidPaymentException exception)
        {
            return Reject(exception);
        }
        catch (AcquiringBankUnavailableException)
        {
            return BankUnavailable();
        }
    }

    [HttpGet("{id:guid}")]
    public ActionResult<PostPaymentResponse> GetPayment(Guid id)
    {
        var payment = paymentsRepository.Get(id);

        if (payment is null)
        {
            return NotFound();
        }

        return Ok(payment);
    }

    private ActionResult Reject(InvalidPaymentException exception)
    {
        logger.LogInformation("Payment rejected because of an invalid {Field}", exception.Field);
        ModelState.AddModelError(exception.Field, exception.Message);
        return RejectedPaymentResponse.From(ControllerContext);
    }

    private ObjectResult BankUnavailable() =>
        Problem(title: "Acquiring bank unavailable", statusCode: StatusCodes.Status502BadGateway);
}
