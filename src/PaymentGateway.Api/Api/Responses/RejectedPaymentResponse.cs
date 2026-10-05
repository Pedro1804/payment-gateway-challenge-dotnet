using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Api.Responses;

public static class RejectedPaymentResponse
{
    public static ActionResult From(ActionContext context)
    {
        var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
        problem.Extensions["paymentStatus"] = nameof(PaymentStatus.Rejected);
        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }
}
