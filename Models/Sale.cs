using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniERPsystem.Models   
{
    public class Sale
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }

        public decimal TotalAmount { get; set; }

        public List<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
