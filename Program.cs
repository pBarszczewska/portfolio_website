using BookingApi.Data;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;

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

    // make sure database exists
    context.Database.EnsureCreated();

    // create default admin if none exists
    if (!context.Users.Any(u => u.IsAdmin))
    {
        context.Users.Add(new BookingApi.Models.User
        {
            Username = "admin",
            Password = "P@ul1na", 
            IsAdmin = true
        });
        context.SaveChanges();
        Console.WriteLine("Default admin account created");
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();







