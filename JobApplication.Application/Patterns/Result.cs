using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Patterns
{
    public class Result
    {
        public static async Task<GenericResult<T>> SuccessAsync<T>(T data, string message = "")
        {
            return await Task.FromResult(new GenericResult<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            });
        }

        public static async Task<GenericResult<T>> FailureAsync<T>(string message)
        {
            return await Task.FromResult(new GenericResult<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default
            });
        }
    }
}
