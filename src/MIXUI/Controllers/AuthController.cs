using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MIXUI.Dtos;
using MIXUI.Entities;
using MIXUI.Helpers;

namespace MIXUI.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppSettings _appSettings;

        public AuthController(
            ILogger<AuthController> logger,
            UserManager<AppUser> userManager, 
            IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        private async Task<bool> DoCheckToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var validationParameters = new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };
            SecurityToken validatedToken = null;
            ClaimsPrincipal principal = null;

            try
            {
                principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch(SecurityTokenException ex)
            {
                _logger.LogWarning($"Token was found to be invalid. Message: {ex.Message}");
                return false; 
            }
            
            var userIdClaim = principal.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value;
            var foundUser = await _userManager.FindByIdAsync(userIdClaim) != null;
            if (!foundUser) 
            {
                _logger.LogWarning($"Token was valid, but user claim not in database. User: {userIdClaim}");
            }
            return foundUser;
        }

        [HttpGet("check_token")]
        public async Task<IActionResult> CheckToken(string token) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new 
            {
                Valid = await DoCheckToken(token),
            });
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Post([FromBody]LoginDto credentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identity = await GetClaimsIdentity(credentials.Username, credentials.Password);
            if (identity == null)
            {
                return BadRequest(Errors.AddErrorToModelState("LoginFailure", "Invalid username or password.", ModelState));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Id = identity.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id).Value,
                credentials.Username,
                Token = tokenString,
            });
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
                {
                    new Claim(Constants.Strings.JwtClaimIdentifiers.Id, userToVerify.Id),
                }));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }
    }
}
