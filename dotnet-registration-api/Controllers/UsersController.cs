using dotnet_registration_api.Data.Entities;
using dotnet_registration_api.Data.Models;
using dotnet_registration_api.Services;
using Microsoft.AspNetCore.Mvc;
using dotnet_registration_api.Helpers;

namespace dotnet_registration_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest model)
        {
            try
            {
                var user = await _userService.Login(model);
                return Ok(user);
            }
            catch (AppException ex)
            {
                // Handling exception for invalid credentials
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handling general errors
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterRequest model)
        {
            try
            {
                var user = await _userService.Register(model);
                return Ok(user); // Changed from CreatedAtAction() to Ok()
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            try
            {
                var users = await _userService.GetAll();
                return Ok(users);
            }
            catch (Exception ex)
            {
                // Handling general errors
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            try
            {
                var user = await _userService.GetById(id);
                return Ok(user);
            }
            catch (NotFoundException ex)
            {
                // Handling user not found
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handling general errors
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<User>> Update(int id, [FromBody] UpdateRequest model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Request body cannot be null." });
            }

            // Field validation
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
            {
                return BadRequest(new { message = "Username, FirstName, and LastName are required." });
            }

            // Check if the username is already taken by another user
            var existingUser = await _userService.GetByUsername(model.Username);
            if (existingUser != null && existingUser.Id != id)
            {
                return BadRequest(new { message = "Username is already taken." });
            }

            // Validate old password if provided
            if (!string.IsNullOrEmpty(model.OldPassword))
            {
                var user = await _userService.GetById(id);
                if (user.PasswordHash != HashHelper.HashPassword(model.OldPassword))
                {
                    return BadRequest(new { message = "Old password is incorrect." });
                }
            }

            try
            {
                var user = await _userService.Update(id, model);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(user); // If update was successful
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var user = await _userService.GetById(id);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                await _userService.Delete(id);
                return Ok(); // Changed from NoContent() to Ok()
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}