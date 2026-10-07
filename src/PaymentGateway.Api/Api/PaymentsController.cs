using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Api.Requests;
using PaymentGateway.Api.Api.Responses;
using PaymentGateway.Api.Domain.Exceptions;
using PaymentGateway.Api.Domain.Services;

namespace PaymentGateway.Api.Api;

[Route("api/payments")]
[ApiController]
[Produces("application/json")]
public class PaymentsController(
    PaymentRetriever paymentRetriever,
    PaymentProcessor paymentProcessor,
    TimeProvider timeProvider,
    ILogger<PaymentsController> logger) : Controller
{
    private DateOnly Today => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    /// <summary>Processes a card payment through the acquiring bank.</summary>
    /// <remarks>An invalid request is rejected without reaching the bank.</remarks>
    /// <response code="201">The bank authorized or declined the payment.</response>
    /// <response code="400">The payment is rejected because a field is invalid.</response>
    /// <response code="502">The acquiring bank could not be reached.</response>
    [HttpPost]
    [ProducesResponseType<PostPaymentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, "application/problem+json")]
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

    /// <summary>Retrieves a previously processed payment.</summary>
    /// <param name="id">The identifier returned when the payment was created.</param>
    /// <response code="200">The payment exists.</response>
    /// <response code="404">No payment has this identifier.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetPaymentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public ActionResult<GetPaymentResponse> GetPayment(Guid id)
    {
        var payment = paymentRetriever.Find(id);

        if (payment is null)
        {
            return NotFound();
        }

        return payment.ToGetPaymentResponse();
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
