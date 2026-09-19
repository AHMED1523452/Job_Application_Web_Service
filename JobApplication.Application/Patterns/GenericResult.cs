using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Patterns
{
    public class GenericResult<T> 
    {
        public string? Message { get; set; }
        public bool IsSuccess { get; set; } = true;
        public T? Data { get; set; }
    }
}
