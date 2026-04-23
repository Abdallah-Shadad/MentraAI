using System;

namespace MentraAI.API.Modules.Auth.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
