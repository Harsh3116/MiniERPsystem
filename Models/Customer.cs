using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniERPsystem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string CustomerName { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public ICollection<Sale>? Sales { get; set; }
    }
}
