using Backend.Models;
using Backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Contracts.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;


namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerRepository _repository;
        private readonly ILogger<CustomerController> _logger;
        private readonly IMapper _mapper;

        public CustomerController(CustomerRepository repository, ILogger<CustomerController> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CustomerDto customerDto)
        {
            var customerEntity = _mapper.Map<Customer>(customerDto);
            await _repository.CreateAsync(customerEntity);
            _logger.LogInformation("Created a new customer with ID: {CustomerId}", customerEntity.Id);
            return CreatedAtAction(nameof(Get), new { id = customerEntity.Id }, customerDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, CustomerDto customerDto)
        {
            if(string.IsNullOrEmpty(id) || customerDto  == null)
            {
                return BadRequest();
            }

            // Check if IDs match (if customer.Id is provided)
            if (!string.IsNullOrWhiteSpace(customerDto.Id) && customerDto.Id != id)
            {
                return BadRequest(new { error = "Customer ID in URL does not match ID in body" });
            }           

            var existingCustomer = await _repository.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                return NotFound();
            }
            var customerEntity = _mapper.Map<Customer>(customerDto);
            customerEntity.Id = id; // Ensure the ID is set correctly
            await _repository.UpdateAsync(id, customerEntity);
            return NoContent();
        }

        // GET: api/<CustomerController>
        [HttpGet]
        public async Task<ActionResult<List<CustomerDto>>> Get()
        {
            try
            {
                var customers = await _repository.GetAllAsync();
                var customersDto = _mapper.Map<List<CustomerDto>>(customers);
                return Ok(customersDto);
            }
            catch (MongoException ex) 
            {
                _logger.LogError(ex, "A MongoDB error occurred while retrieving customers.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving customers.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> Get(string id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            var customerDto = _mapper.Map<CustomerDto>(customer);
            return Ok(customerDto);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<CustomerDto>> GetByName(string name)
        {
            var customer = await _repository.GetByNameAsync(name);
            if (customer == null)
            {
                return NotFound();
            }
            var customerDto = _mapper.Map<CustomerDto>(customer);
            return Ok(customerDto);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(string id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
