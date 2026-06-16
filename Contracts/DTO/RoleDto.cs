using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.DTO
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public RoleType Type { get; set; } = RoleType.User;
    }

    public enum RoleType
    {
        Admin,
        User
    }
}
