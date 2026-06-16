using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.DTO
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
    }
}
