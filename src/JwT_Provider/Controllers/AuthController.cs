using JwT_Implementation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwT_Implementation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> logger;
        private readonly JwTApiContext context;
        private readonly IConfiguration configuration;
        public AuthController(ILogger<AuthController> _logger, JwTApiContext _context, IConfiguration _configuration)
        {
            logger = _logger;
            context = _context;
            configuration = _configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> authUser(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
            {
                var userDetails = await context.UsersDetails.Where(x => x.username == username && x.password == password).FirstOrDefaultAsync();
                if (userDetails != null)
                {
                    var token=GetJwT_Token(userDetails);
                    if (token != null)
                    {
                        return Ok(new {Token=token, Username=username, UserRole=userDetails.role});
                    }
                }
            }
            return BadRequest();
        }

        private string GetJwT_Token(UserDetails userDetails)
        {
            
            var jwtTokenHandler = new JwtSecurityTokenHandler();         

            var securityKey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!));

            var credentials= new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256Signature);

            //declaring claims
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Iss,configuration["JwtSettings:Issuer"] !),
                new Claim(JwtRegisteredClaimNames.Sub,userDetails.email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, configuration["JwtSettings:Audiance"] !),
                new Claim(ClaimTypes.Role,userDetails.role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(Convert.ToDouble(configuration["JwtSettings:ExpiryMinutes"])),
                NotBefore=DateTime.Now,
                SigningCredentials = credentials,
            };

            var created_token=jwtTokenHandler.CreateToken(tokenDescriptor);
            var token=jwtTokenHandler.WriteToken(created_token);
            return token;
        }
    }
}
