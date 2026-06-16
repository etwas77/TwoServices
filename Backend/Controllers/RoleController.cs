using AutoMapper;
using Backend.Repositories;
using Contracts.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleController> _logger;

        public RoleController(RoleRepository roleRepository, IMapper mapper, ILogger<RoleController> logger)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateRole([FromBody] CreateRoleDto request)        
        {
            var existingRole = await _roleRepository.GetByTypeAsync(request.RoleType);
            if (existingRole != null)
            {
                return BadRequest(new { message = "Role already exists" });
            }
            var role = new Models.Role
            {
                Type = request.RoleType
            };
            await _roleRepository.CreateAsync(role);
            _logger.LogInformation("Role {RoleType} created successfully", request.RoleType);
            return Ok(new { message = "Role created successfully" });
        }
    }
}
