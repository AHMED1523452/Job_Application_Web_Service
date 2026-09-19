using System;
using System.Collections.Generic;

namespace JobApplication.Infrastructure.Settings
{
    public class RoleSeedSettings
    {
        public List<RoleInfo> Roles { get; set; } = new();
    }

    public class RoleInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
