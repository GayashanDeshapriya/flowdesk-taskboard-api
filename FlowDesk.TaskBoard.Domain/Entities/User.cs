using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowDesk.TaskBoard.Domain.Enums;

namespace FlowDesk.TaskBoard.Domain.Entities
{
    public class User : BaseEntity
    {
        private User() { }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public SystemRole Role { get; set; }

        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();

        public User(string fullName, string email, SystemRole role)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            FullName = fullName.Trim();
            Email = email.Trim().ToLowerInvariant();
            Role = role;
        }

        public void UpdateProfile(string fullName, string email, SystemRole role)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            FullName = fullName.Trim();
            Email = email.Trim().ToLowerInvariant();
            Role = role;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}
