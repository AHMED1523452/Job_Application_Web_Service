using FluentValidation;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Application.Interfaces;
using MediatR;
using JobApplication.Application.Features.Auth.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, GenericResult<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly IIdentityService service;

        public VerifyEmailHandler(IUnitOfWork uow,
                                  IPasswordHasher hasher,
                                  IIdentityService service)
        {
            _uow = uow;
            _hasher = hasher;
            this.service = service;
        }

        public async Task<GenericResult<string>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<string>();

            var codes = await _uow.Repository<VerificationCode>().GetAllAsync<VerificationCode>(
                                predicate: c => c.Email == request.Request.Email && !c.IsUsed,
                                selector: c => c,
                                cancellationToken: cancellationToken);

            var codeRecord = codes.OrderByDescending(c => c.CreatedAt).FirstOrDefault();

            if (codeRecord == null)
            {
                result.IsSuccess = false;
                result.Message = "No verification code found for this email.";
                return result;
            }

            if (codeRecord.ExpiresAt < DateTime.UtcNow)
            {
                result.IsSuccess = false;
                result.Message = "Verification code expired.";
                return result;
            }

            var ok = await _hasher.VerifyAsync(codeRecord.CodeHash, request.Request.Code, cancellationToken);
            if (!ok)
            {
                result.IsSuccess = false;
                result.Message = "Invalid verification code.";
                return result;
            }

            codeRecord.IsUsed = true;

            bool confirmed = await service.ConfirmEmailAsync(request.Request.Email, cancellationToken);
            if (!confirmed)
            {
                result.Message = "There is no user with this email";
                result.IsSuccess = false;
                return result;
            }
            await _uow.Repository<VerificationCode>().EditAsync(codeRecord, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            result.IsSuccess = true;
            result.Message = "Email verified. You can now login.";
            return result;
        }
    }

    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Request.Code)
                .NotEmpty().WithMessage("Verification code is required.")
                .Length(6).WithMessage("Verification code must be 6 digits.");
        }
    }
}
