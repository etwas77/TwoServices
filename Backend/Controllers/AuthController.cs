using AutoMapper;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using Contracts.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly RoleRepository _roleRepository;
        private readonly PasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserRepository userRepository,
            RoleRepository roleRepository,
            PasswordHasher passwordHasher,
            IMapper mapper,
            ILogger<AuthController> logger
            )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto request)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            var role = await _roleRepository.GetByTypeAsync(RoleType.User);
            if(role == null)
            {
                return BadRequest(new { message = "Role not found" });
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Roles = new List<Role> { role }
            };
            var userDto = _mapper.Map<UserDto>(user);

            await _userRepository.CreateAsync(user);
            _logger.LogInformation("User {Username} registered successfully", request.Username);
            return CreatedAtAction(nameof(Register), userDto);
        }

        [HttpPost("validate")]
        public async Task<ActionResult<UserDto>> ValidateCredentials([FromBody] ValidateCredentialsRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null)
            {
                _logger.LogWarning("Login failed for user: {Username} - User not found", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for user: {Username} - Invalid password", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            _logger.LogInformation("User {Username} validated successfully", request.Username);

            return Ok(new { User = _mapper.Map<UserDto>(user), message = "User is valid" });
        }
    }

}
