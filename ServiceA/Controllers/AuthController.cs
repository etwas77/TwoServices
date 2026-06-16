using Contracts.DTO;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceA.Services;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IConfiguration _configuration;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<AuthController> _logger;
        private readonly string? _backendBaseUrl;
        private readonly HttpClient? _httpClient;

        public AuthController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            JwtTokenService jwtTokenService,
            ILogger<AuthController> logger)
        {
            //_httpClientFactory = httpClientFactory;
            //_configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _backendBaseUrl = configuration["BackendApi:BaseUrl"];
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto request)
        {
            if (_httpClient == null || string.IsNullOrEmpty(_backendBaseUrl))
            {
                return BadRequest(new { message = "HttpClient or BackendBaseUrl Initialization failed" });
            }
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_backendBaseUrl}/api/auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return BadRequest(new { message = errorContent });
            }

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
                PropertyNameCaseInsensitive = true
            };
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>(options);

            _logger.LogInformation("User {Username} registered successfully", request.Username);

            return CreatedAtAction(nameof(Register), userDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if (_httpClient == null || string.IsNullOrEmpty(_backendBaseUrl))
            {
                return BadRequest(new { message = "HttpClient or BackendBaseUrl Initialization failed" });
            }

            // Call Backend to validate credentials
            var validateRequest = new ValidateCredentialsRequestDto
            {
                Username = request.Username,
                Password = request.Password
            };

            var content = new StringContent(JsonSerializer.Serialize(validateRequest), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_backendBaseUrl}/api/auth/validate", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login failed for user: {Username}", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
                PropertyNameCaseInsensitive = true
            };
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>(options);

            if (userDto == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(userDto);
            //var expiration = _jwtTokenService.GetTokenExpiration();

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            return Ok(new LoginResponseDto
            {
                Token = token,
                User = userDto
            });
        }
    }
}
