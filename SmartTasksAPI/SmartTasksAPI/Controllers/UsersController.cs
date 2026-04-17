using Microsoft.AspNetCore.Mvc;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await userService.GetAllAsync();
            return Ok(users);
        }

    }
}
