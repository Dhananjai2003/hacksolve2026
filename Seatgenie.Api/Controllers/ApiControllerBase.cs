using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;

namespace Seatgenie.Api.Controllers;

/// <summary>Base class for SeatGenie API controllers with shared response helpers.</summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>404 with the standard <see cref="Error"/> payload.</summary>
    protected IActionResult NotFoundError(string message = "Resource not found.")
        => NotFound(new Error { Code = "NOT_FOUND", Message = message });

    /// <summary>Return 200 with the value, or 404 when it is null.</summary>
    protected IActionResult OkOrNotFound<T>(T? value, string message = "Resource not found.")
        => value is null ? NotFoundError(message) : Ok(value);
}
