using DotnetToLambda.Core.Models;
using DotnetToLambda.Core.Infrastructure;
using DotnetToLambda.Core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databaseConnection =
    new DatabaseConnection(builder.Configuration.GetConnectionString("DatabaseConnection")!);

builder.Services.AddSingleton(databaseConnection);

builder.Services.AddDbContext<BookingContext>(options =>
    options.UseMySQL(databaseConnection.ToString()));

builder.Services.AddTransient<IBookingRepository, BookingRepository>();
builder.Services.AddHttpClient<ICustomerService, CustomerService>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingContext>();
    context.Database.Migrate();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
