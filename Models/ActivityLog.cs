using System;

namespace MiniERPsystem.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string? UserEmail { get; set; }
        public string? Action { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? IpAddress { get; set; }
    }
}
