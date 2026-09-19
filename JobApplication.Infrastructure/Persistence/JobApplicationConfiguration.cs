using JobApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Infrastructure.Persistence
{
    public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplications>
    {
        void IEntityTypeConfiguration<JobApplications>.Configure(EntityTypeBuilder<JobApplications> builder)
        {
            builder.Property(x => x.status)
              .HasConversion<string>();
        }
    }
}
