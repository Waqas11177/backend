using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DAL.Model;
using DAL.Repositeries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Test.ViewModel;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        public IConfiguration _configuration;
        private readonly RepositeryContext _dbContext;

        public LoginController(ILogger<LoginController> logger, RepositeryContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _configuration = configuration;
        }
        private User AuthenticateUser(string UserName, string Password)
        {
            APIResponse res = new APIResponse();
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
            {
                res.Message = "Username and Password cannot be empty";
                return null;
            }
            var user = _dbContext.Users.Where(x => x.UserName == UserName && x.Password == Password).FirstOrDefault();
            if (user == null)
            {
                res.Message = "Invalid Credential!";

            }
            else
            {
                res.Message = "Login Successfully";
            }
            return user;

        }
        private string GenerateToken(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("User Cannot be null or empty", nameof(username));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password Cannot be null or empty", nameof(password));
            }
            var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Email, password),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]);
            var securityKey = new SymmetricSecurityKey(key);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost, Route("Login")]
        public IActionResult Login(LoginViewModel model)
        {
            APIResponse res = new APIResponse();
            IActionResult response = Unauthorized();
            var User = AuthenticateUser(model.UserName,model.Password);
            if (User!=null)
            {
                var token = GenerateToken(model.UserName,model.Password);
                response =Ok(new {access_token=token });
                return response;
            }
            else
            {
                res.Message = "Invalid Credential!";
                return BadRequest();
            }
        }
    }
}
