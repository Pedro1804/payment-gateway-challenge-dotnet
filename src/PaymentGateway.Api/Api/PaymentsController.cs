using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Api.Requests;
using PaymentGateway.Api.Api.Responses;
using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;
using PaymentGateway.Api.Domain.Services;
using PaymentGateway.Api.Infrastructure.Persistence;

namespace PaymentGateway.Api.Api;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly PaymentProcessor _paymentProcessor;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        PaymentsRepository paymentsRepository,
        PaymentProcessor paymentProcessor,
        TimeProvider timeProvider,
        ILogger<PaymentsController> logger)
    {
        _paymentsRepository = paymentsRepository;
        _paymentProcessor = paymentProcessor;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync(
        PostPaymentRequest request, CancellationToken cancellationToken)
    {
        CardPayment cardPayment;
        try
        {
            cardPayment = request.ToCardPayment(DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime));
        }
        catch (InvalidPaymentException exception)
        {
            _logger.LogInformation("Payment rejected because of an invalid {Field}", exception.Field);
            ModelState.AddModelError(exception.Field, exception.Message);
            return RejectedPaymentResponse.From(ControllerContext);
        }

        try
        {
            var decision = await _paymentProcessor.ProcessAsync(cardPayment, cancellationToken);
            return CreatedAtAction(nameof(GetPayment), new { id = decision.Id }, decision.ToPostPaymentResponse());
        }
        catch (AcquiringBankUnavailableException)
        {
            return Problem(title: "Acquiring bank unavailable", statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpGet("{id:guid}")]
    public ActionResult<PostPaymentResponse?> GetPayment(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        if (payment is null)
        {
            return NotFound();
        }

        return Ok(payment);
    }
}
