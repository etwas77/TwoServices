using Backend.Models;
using Backend.Repositories;
using Microsoft.AspNetCore.Mvc;


namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerRepository _repository;

        public CustomerController(CustomerRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Customer customer)
        {
            await _repository.CreateAsync(customer);
            return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, Customer customer)
        {
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
            var customers = await _repository.GetAllAsync();
            return Ok(customers);
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
