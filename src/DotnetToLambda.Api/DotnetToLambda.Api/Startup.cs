using DotnetToLambda.Core.Models;
using DotnetToLambda.Core.Infrastructure;
using DotnetToLambda.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetToLambda.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var databaseConnection =
                new DatabaseConnection(this.Configuration.GetConnectionString("DatabaseConnection"));
            
            services.AddSingleton<DatabaseConnection>(databaseConnection);
            
            services.AddDbContext<BookingContext>(options =>
                options.UseMySQL(databaseConnection.ToString()));

            services.AddTransient<IBookingRepository, BookingRepository>();
            services.AddHttpClient<ICustomerService, CustomerService>();
            
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<BookingContext>();
                context.Database.Migrate();   
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
