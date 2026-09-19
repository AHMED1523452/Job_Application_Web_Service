using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application
{
    public static class DependencyInjection_Application
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection_Application).Assembly;

            // MediatR — auto-discovers all IRequestHandler implementations
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // FluentValidation — auto-discovers all AbstractValidator<T> implementations
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
