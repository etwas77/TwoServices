using AutoMapper;
using Backend.Repositories;
using Contracts.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly ItemRepository _itemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ItemController> _logger;

        public ItemController(ItemRepository itemRepository, IMapper mapper, ILogger<ItemController> logger)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateItem([FromBody] ItemDto itemDto)
        {
            var existingItem = await _itemRepository.GetByNameAsync(itemDto.Name);
            if (existingItem != null)
            {
                return BadRequest(new { message = "Item already exists" });
            }
            var item = new Models.Item
            {
                Name = itemDto.Name,
                Quantity = itemDto.Quantity
            };
            await _itemRepository.CreateAsync(item);
            _logger.LogInformation("Item {ItemName} created successfully", itemDto.Name);
            return Ok(new { message = "Item created successfully" });
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<ItemDto>> GetByName(string name)
        {
            var item = await _itemRepository.GetByNameAsync(name);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }
            var itemDto = _mapper.Map<ItemDto>(item);
            return Ok(itemDto);
        }
    }
}
