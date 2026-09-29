using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.ApiModels.Basket
{
    public class CartResponseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
       // public decimal Total => Items.Sum(t => t.TotalPrice);
        public int UserId { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = [];
    }
}
