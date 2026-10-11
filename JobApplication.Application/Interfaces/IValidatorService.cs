using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Interfaces
{
    public interface IValidatorService
    {
        public bool IsValidFileExtension(
                        string fileName,
                        string contentType,
                        long FileSize,
                        FileCategory fileCategory);
    }
}
