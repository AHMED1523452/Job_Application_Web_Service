using FluentValidation;
using JobApplication.Application.Auth.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Auth.Handlers
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.requestDTO.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.requestDTO.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
