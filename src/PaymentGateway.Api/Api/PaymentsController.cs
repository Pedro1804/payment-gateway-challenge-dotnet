using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Api.Responses;
using PaymentGateway.Api.Infrastructure.Persistence;

namespace PaymentGateway.Api.Api;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;

    public PaymentsController(PaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        if (payment is null)
        {
            return NotFound();
        }

        return Ok(payment);
    }
}