using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.DTO
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<RoleDto> Roles { get; set; } = new();
    }
}
