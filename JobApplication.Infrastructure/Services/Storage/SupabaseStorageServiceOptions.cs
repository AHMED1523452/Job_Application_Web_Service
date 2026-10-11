using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Infrastructure.Services.Storage
{
    public class SupabaseStorageServiceOptions
    {
        public string Endpoint { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

    }
}
