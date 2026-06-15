using Contracts.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ServiceA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomerController> _logger;
        private readonly string _backendBaseUrl;

        public CustomerController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            ILogger<CustomerController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _backendBaseUrl = _configuration["BackendApi:BaseUrl"] ?? "http://localhost:5148";
        }

        // PUT: api/customer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(string id, [FromQuery] bool active)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // First, retrieve the customer from Backend
                var getResponse = await client.GetAsync($"{_backendBaseUrl}/api/customer/{id}");

                if (!getResponse.IsSuccessStatusCode)
                {
                    if (getResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return NotFound($"Customer with ID '{id}' not found");
                    }

                    _logger.LogWarning("Failed to retrieve customer {Id} from Backend API. Status: {StatusCode}", id, getResponse.StatusCode);
                    return StatusCode((int)getResponse.StatusCode, "Failed to retrieve customer from backend service");
                }

                // Deserialize the customer
                var customerDto = await getResponse.Content.ReadFromJsonAsync<CustomerDto>();

                if (customerDto == null)
                {
                    _logger.LogError("Failed to deserialize customer {Id} from Backend API", id);
                    return StatusCode(500, "Failed to process customer data from backend service");
                }

                // Update the IsActive field
                customerDto.IsActive = active;

                // Send the updated customer back to Backend
                var putResponse = await client.PutAsJsonAsync($"{_backendBaseUrl}/api/customer/{id}", customerDto);

                if (putResponse.IsSuccessStatusCode)
                {
                    return NoContent();
                }

                if (putResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound($"Customer with ID '{id}' not found");
                }

                if (putResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorMessage = await putResponse.Content.ReadAsStringAsync();
                    return BadRequest(errorMessage);
                }

                _logger.LogWarning("Failed to update customer {Id} in Backend API. Status: {StatusCode}", id, putResponse.StatusCode);
                return StatusCode((int)putResponse.StatusCode, "Failed to update customer in backend service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Backend API to update customer {Id}", id);
                return StatusCode(500, "An error occurred while communicating with the backend service");
            }
        }

    }
}
