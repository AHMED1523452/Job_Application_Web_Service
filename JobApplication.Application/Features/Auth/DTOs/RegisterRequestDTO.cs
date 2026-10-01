using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Auth.DTOs
{
    public class RegisterRequestDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Address { get; set; }
    }
}
