using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkAuditTrail
{
    class Program
    {
        private static ServiceProvider _serviceProvider;
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=Application.db"));
            _serviceProvider = services.BuildServiceProvider();


            RunApplication();
            UpdateValues();
        }

        private static void UpdateValues()
        {
            var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var list = applicationDbContext.Item.ToList();

            foreach (var item in list)
            {
                item.Name = System.Guid.NewGuid().ToString();
            }

            applicationDbContext.SaveChanges();
        }

        private static void RunApplication()
        {
            var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            for (int i = 0; i < 1; i++)
            {
                Item entity = new Item { Name = System.Guid.NewGuid().ToString() };
                applicationDbContext.Item.Add(entity);
            }

            applicationDbContext.SaveChanges();
        }
    }
}
