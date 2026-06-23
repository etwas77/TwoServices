using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.DTO
{
    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public List<ItemDto> Items { get; set; } = new();
    }
}
