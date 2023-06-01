using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using server.Attributes;
using server.DTOs;
using server.Entities;
using server.Services;
using server.Utilities;

namespace server.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService mAuthService;

    public AuthController(AuthService authService)
    {
        mAuthService = authService;
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult<AuthResponse> Auth([FromBody] AuthRequest authRequest)
    {
        UserEntity? userEntity = mAuthService.UserAuthorize(authRequest.email, authRequest.password);

        AuthResponse response = new AuthResponse();

        if (userEntity == null)
        {
            // Unauthorized
            return Unauthorized();
        }
        else if (authRequest.no_token?? false)
        {
            // No token response
            return response;
        }
        else
        {
            response.access_token = mAuthService.GenerateAccessToken(userEntity.Id);
            response.refresh_token = mAuthService.GenerateRefreshToken(userEntity.Id);

            return response;
        }
    }

    [Route("refresh")]
    [HttpGet]
    [RefreshAuthorize]
    public ActionResult<AuthResponse> Refresh()
    {
        JwtSecurityToken jwtToken = (JwtSecurityToken)HttpContext.Items["JwtToken"]!;
        string? id = jwtToken.GetClaimByType("id");

        if (id == null)
        {
            return Unauthorized();
        }

        AuthResponse response = new AuthResponse();
        response.access_token = mAuthService.GenerateAccessToken(long.Parse(id));

        return response;
    }
}
