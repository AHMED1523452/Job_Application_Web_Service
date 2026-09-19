using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Auth.DTOs
{
    public class LoginRequestDTO
    {
        public string  Email { get; set; }
        public string Password { get; set; }
    }
}
