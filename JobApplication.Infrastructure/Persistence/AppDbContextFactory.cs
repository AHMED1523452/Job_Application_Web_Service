using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobApplication.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder =
            new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=AHMED\\MSSQLSERVER02;DataBase=HireSense_Management_APIs;Integrated Security = True;Trust Server Certificate = True;",
                sql => sql.UseNetTopologySuite());

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
