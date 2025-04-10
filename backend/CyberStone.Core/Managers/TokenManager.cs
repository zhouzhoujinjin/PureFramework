using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CyberStone.Core.Entities;
using CyberStone.Core.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CyberStone.Core.Managers
{
  public class JwtIssuerOptions
  {
    /// <summary>
    /// 4.1.1.  "iss" (Issuer) Claim - The "iss" (issuer) claim identifies the principal that issued the JWT.
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// 4.1.2.  "sub" (Subject) Claim - The "sub" (subject) claim identifies the principal that is the subject of the JWT.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// 4.1.3.  "aud" (Audience) Claim - The "aud" (audience) claim identifies the recipients that the JWT is intended for.
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    /// 4.1.4.  "exp" (Expiration Time) Claim - The "exp" (expiration time) claim identifies the expiration time on or after which the JWT MUST NOT be accepted for processing.
    /// </summary>
    public DateTime Expiration => IssuedAt.Add(ValidFor);

    /// <summary>
    /// 4.1.5.  "nbf" (Not Before) Claim - The "nbf" (not before) claim identifies the time before which the JWT MUST NOT be accepted for processing.
    /// </summary>
    public DateTime NotBefore { get; set; } = DateTime.Now;

    /// <summary>
    /// 4.1.6.  "iat" (Issued At) Claim - The "iat" (issued at) claim identifies the time at which the JWT was issued.
    /// </summary>
    public DateTime IssuedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Set the timespan the token will be valid for (default is 120 min)
    /// </summary>
    public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(120);

    /// <summary>
    /// "jti" (JWT ID) Claim (default ID is a GUID)
    /// </summary>
    public Func<Task<string>> JtiGenerator =>
      () => Task.FromResult(Guid.NewGuid().ToString());

    /// <summary>
    /// The signing key to use when generating tokens.
    /// </summary>
    public SigningCredentials SigningCredentials { get; set; }

    public string SecretKey { get; set; }
  }

  public class TokenManager
  {
    private readonly JwtIssuerOptions jwtIssuerOptions;
    private readonly UserManager userManager;

    public TokenManager(UserManager userManager, IOptions<JwtIssuerOptions> jwtIssuerOptionsAccessor)
    {
      jwtIssuerOptions = jwtIssuerOptionsAccessor.Value;
      this.userManager = userManager;
    }

    /// <summary>
    /// 只需要 AccessToken，无需 RefreshToken
    /// </summary>
    /// <param name="user"></param>
    /// <param name="otherClaimsAction"></param>
    /// <returns></returns>
    public AuthenticationTokens GenerateTokens(UserEntity user, Action<UserEntity, List<Claim>>? otherClaimsAction = null)
    {
      return new AuthenticationTokens
      {
        AccessToken = GenerateAccessToken(user, otherClaimsAction)
      };
    }

    /// <summary>
    /// 生成 AccessToken 及 RefreshToken
    /// AccessToken 中会包含用户名（nameId）和用户Id(id)
    /// </summary>
    /// <param name="user"></param>
    /// <param name="otherClaimsAction"></param>
    /// <returns></returns>
    public async Task<AuthenticationTokens> GenerateTokensAsync(UserEntity user, Action<UserEntity, List<Claim>>? otherClaimsAction = null)
    {
      return new AuthenticationTokens
      {
        AccessToken = GenerateAccessToken(user, otherClaimsAction),
        RefreshToken = await GenerateRefreshTokenAsync(user)
      };
    }

    public string GenerateAccessToken(UserEntity user, Action<UserEntity, List<Claim>>? otherClaimsAction = null)
    {
      var claims = new List<Claim> {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("userId", user.Id.ToString()),
        new Claim("userName", user.UserName!),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName!)
      };

      if (otherClaimsAction != null)
      {
        otherClaimsAction.Invoke(user, claims);
      }

      var userClaimsIdentity = new ClaimsIdentity(claims);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        IssuedAt = DateTime.UtcNow,
        Expires = DateTime.UtcNow.Add(jwtIssuerOptions.ValidFor),
        Issuer = jwtIssuerOptions.Issuer,
        Subject = userClaimsIdentity,
        Audience = jwtIssuerOptions.Audience,
        SigningCredentials = jwtIssuerOptions.SigningCredentials
      };

      var tokenHandler = new JwtSecurityTokenHandler();
      var securityToken = tokenHandler.CreateToken(tokenDescriptor);
      var serializeToken = tokenHandler.WriteToken(securityToken);

      return serializeToken;
    }

    public async Task<string> GenerateRefreshTokenAsync(UserEntity user)
    {
      await userManager.RemoveAuthenticationTokenAsync(user, "Default", "RefreshToken");
      var newRefreshToken = await userManager.GenerateUserTokenAsync(user, "Default", "RefreshToken");
      await userManager.SetAuthenticationTokenAsync(user, "Default", "RefreshToken", newRefreshToken);
      return newRefreshToken;
    }

    public async Task<bool> VerifyRefreshTokenAsync(UserEntity user, string refreshToken)
    {
      return await userManager.VerifyUserTokenAsync(user, "Default", "RefreshToken", refreshToken);
    }

    public async Task<string> RefreshAccessToken(UserEntity user, string refreshToken)
    {
      var isValid = await VerifyRefreshTokenAsync(user, refreshToken);
      return isValid ? GenerateAccessToken(user) : null;
    }
  }
}