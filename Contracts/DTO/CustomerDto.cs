using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.DTO
{
    public class CustomerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
    }
}
