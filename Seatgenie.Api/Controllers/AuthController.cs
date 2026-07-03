using Microsoft.AspNetCore.Mvc;
using Seatgenie.Api.Models;
using Seatgenie.Api.Services;

namespace Seatgenie.Api.Controllers;

/// <summary>Sign-in, sign-out and current-user session (NextAuth / Microsoft Entra).</summary>
[Tags("Auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Get current session.</summary>
    [HttpGet("/auth/session")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSession(CancellationToken ct)
        => await _auth.GetSessionAsync(ct) is { } session ? Ok(session) : Unauthenticated();

    /// <summary>Begin sign-in. Delegated to the external identity provider (NextAuth / Entra).</summary>
    [HttpPost("/auth/signin")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken ct)
        => await _auth.GetUserAuthAsync(request.Provider, ct) is { } session ? Ok(session) : Unauthenticated();

    /// <summary>Sign out.</summary>
    [HttpPost("/auth/signout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SignOutSession()
        => Ok(new { message = "Signed out." });

    /// <summary>Get my profile.</summary>
    [HttpGet("/me")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
        => await _auth.GetMeAsync(ct) is { } user ? Ok(user) : Unauthenticated();

    /// <summary>Update my profile.</summary>
    [HttpPatch("/me")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMe([FromBody] UserUpdate input, CancellationToken ct)
        => await _auth.UpdateMeAsync(input, ct) is { } user ? Ok(user) : Unauthenticated();

    private IActionResult Unauthenticated()
        => Unauthorized(new Error { Code = "UNAUTHORIZED", Message = "Not authenticated." });
}
