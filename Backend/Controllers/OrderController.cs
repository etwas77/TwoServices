using AutoMapper;
using Backend.Repositories;
using Contracts.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderRepository orderRepository, IMapper mapper, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> Get()
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync();
                var ordersDto = _mapper.Map<List<OrderDto>>(orders);
                return Ok(ordersDto);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "A MongoDB error occurred while retrieving orders.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving orders.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> Get(string id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound();
                }
                var orderDto = _mapper.Map<OrderDto>(order);
                return Ok(orderDto);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "A MongoDB error occurred while retrieving orders.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving orders.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
