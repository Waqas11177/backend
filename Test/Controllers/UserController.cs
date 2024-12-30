using Microsoft.AspNetCore.Mvc;
using DAL.Model;
using DAL.Repositeries;
using Test.ViewModel;

namespace Test.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly RepositeryContext _dbContext;

        public UserController(ILogger<UserController> logger, RepositeryContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        [HttpPost,Route("Users")]
        public IActionResult Users(UserViewModel model)
        {
            var userdata = _dbContext.Users.Where(x => x.UserName == model.UserName).FirstOrDefault();
            if (userdata != null)
            {
                return BadRequest("User already Exits in System");
            }
            else 
            {
                User user = new User();
                user.UserName=model.UserName;
                user.LastName=model.LastName;
                user.Password=model.Password;
                user.Email=model.Email;
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
                return Ok(model);
            }

        }
    }
}
