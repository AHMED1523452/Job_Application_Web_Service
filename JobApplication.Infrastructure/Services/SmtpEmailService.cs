using JobApplication.Application.Interfaces;
using FluentEmail.Core;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IFluentEmailFactory _emailFactory;

        public SmtpEmailService(IFluentEmailFactory emailFactory)
        {
            _emailFactory = emailFactory;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var response = await _emailFactory.Create()
                .To(to)
                .Subject(subject)
                .Body(htmlBody, isHtml: true)
                .SendAsync(cancellationToken);

            if (!response.Successful)
                throw new InvalidOperationException($"Email delivery failed: {string.Join("; ", response.ErrorMessages)}");
        }
    }
}
