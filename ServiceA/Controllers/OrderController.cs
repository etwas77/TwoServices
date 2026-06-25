using Contracts.Config;
using Contracts.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceA.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly string? _backendBaseUrl;
        private readonly HttpClient? _httpClient;
        private readonly OrderPublisherService _orderPublisherService;
        private readonly RabbitMqSettings _rabbitMqSettings;

        public OrderController(
            ILogger<OrderController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            OrderPublisherService orderPublisherService,
            IOptions<RabbitMqSettings> rabbitMqSettingsOptions
        )
        {
            _logger = logger;
            _backendBaseUrl = configuration["BackendApi:BaseUrl"];
            _httpClient = httpClientFactory.CreateClient();
            _orderPublisherService = orderPublisherService;
            _rabbitMqSettings = rabbitMqSettingsOptions.Value;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishOrder([FromBody] OrderDto orderDto)
        {
            _logger.LogInformation("Received order: {order}", orderDto);

            if (_httpClient == null || string.IsNullOrEmpty(_backendBaseUrl))
            {
                return BadRequest(new { message = "HttpClient or BackendBaseUrl Initialization failed" });
            }

            // Validate customer by name
            var response = await _httpClient.GetAsync($"{_backendBaseUrl}/api/customer/name/{orderDto.CustomerName}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Customer validation failed for order: {order}", orderDto);
                return BadRequest(new { message = "Invalid customer" });
            }

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
                PropertyNameCaseInsensitive = true
            };
            var customerDto = await response.Content.ReadFromJsonAsync<CustomerDto>(options);

            // validate items by name
            foreach (var item in orderDto.Items)
            {
                var itemResponse = await _httpClient.GetAsync($"{_backendBaseUrl}/api/item/name/{item.Name}");
                if (!itemResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Item validation failed for item: {name}", item.Name);
                    return BadRequest(new { message = "One or more items are invalid", item = item.Name });
                }
            }

            // publish order to RabbitMQ
            await _orderPublisherService.PublishOrderAsync(orderDto, HttpContext.RequestAborted);


            return Accepted(new
            {
                message = "Order accepted and qeued for async processing",
                orderId = orderDto.Id,
                queue = _rabbitMqSettings.OrderQueue,
                status = "queued"
            });
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> Get()
        {
            if (_httpClient == null || string.IsNullOrEmpty(_backendBaseUrl))
            {
                return BadRequest(new { message = "HttpClient or BackendBaseUrl Initialization failed" });
            }
            var response = await _httpClient.GetAsync($"{_backendBaseUrl}/api/order");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve orders from backend API.");
                return BadRequest(new { message = "Failed to retrieve orders" });
            }

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
                PropertyNameCaseInsensitive = true
            };
            var ordersDto = await response.Content.ReadFromJsonAsync<List<OrderDto>>(options);

            return Ok(ordersDto);
        }
    }
}
