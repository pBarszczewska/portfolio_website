using BookingApi.Data;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<EmailService>();

// SqLite DB connection
    builder.Services.AddDbContext<BookingContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

// MailJet
    builder.Services.Configure<MailjetSettings>(
    builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingContext>();

    // for azure it has to migrate db not only check if exists
    context.Database.Migrate();

    // create default admin if none exists
    if (!context.Users.Any(u => u.IsAdmin))
    {
        var hasher = new PasswordHasher<BookingApi.Models.User>();

        var admin = new BookingApi.Models.User
        {
            Username = "admin",
            Email = "pu.barszczewska@gmail.com", 
            IsAdmin = true
        };
        
        admin.PasswordHash = hasher.HashPassword(admin, "P@ul1na");

        context.Users.Add(admin);
        context.SaveChanges();
        Console.WriteLine("Default admin account created");
    }
}


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.UseRouting();
app.MapControllers();

app.Run();







