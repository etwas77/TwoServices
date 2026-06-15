using Backend.Models;
using Backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;


namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerRepository _repository;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerRepository repository, ILogger<CustomerController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Customer customer)
        {
            await _repository.CreateAsync(customer);
            _logger.LogInformation("Created a new customer with ID: {CustomerId}", customer.Id);
            return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, Customer customer)
        {
            if(string.IsNullOrEmpty(id) || customer == null)
            {
                return BadRequest();
            }

            // Check if IDs match (if customer.Id is provided)
            if (!string.IsNullOrWhiteSpace(customer.Id) && customer.Id != id)
            {
                return BadRequest(new { error = "Customer ID in URL does not match ID in body" });
            }           

            var existingCustomer = await _repository.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                return NotFound();
            }
            customer.Id = id; // Ensure the ID is set correctly
            await _repository.UpdateAsync(id, customer);
            return NoContent();
        }

        // GET: api/<CustomerController>
        [HttpGet]
        public async Task<ActionResult<List<Customer>>> Get()
        {
            try
            {
                var customers = await _repository.GetAllAsync();
                return Ok(customers);
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
        public async Task<ActionResult<Customer>> Get(string id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
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
