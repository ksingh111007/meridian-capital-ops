using Meridian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer("Server=unused;Database=unused;")
    .Options;
using var db = new AppDbContext(options);
Console.Write(db.Database.GenerateCreateScript());
