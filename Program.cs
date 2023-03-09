using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Database>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetService<Database>();

  context!.Database.EnsureDeleted();
  context!.Database.EnsureCreated();
  Seeder.Seed(context!);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapPost("/products/add", async (Product product, Database db) =>
{
  var tracking = db.Products!.Add(product);
  await db.SaveChangesAsync();
  return tracking.Entity;
})
.WithName("AddProduct");

app.MapPost("/media/add", async (Media media, Database db) =>
{
  var tracking = db.Media!.Add(media);
  await db.SaveChangesAsync();
  return tracking.Entity;
})
.WithName("AddMedia");

app.MapGet("/products/", async (Database db) =>
{
  var products = await db.Products
      .TagWith("LOAD_PRODUCTS")
      .Include(p => p.Media)
      .ToListAsync();

  return products;
})
.WithName("GetProducts");

app.MapGet("/products/{id:int}", async (int id, Database db) =>
{
  var product = await db.Products
      .TagWith("LOAD_PRODUCT")
      .Where(p => p.Id == id)
      .Include(p => p.Media)
      .FirstOrDefaultAsync();

  return product;
})
.WithName("GetProduct");

app.Run();

// DB entity classes
public abstract class Entity
{
  public int Id { get; set; }
  public string Name { get; set; } = "";
  public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
}

public class Product : Entity
{
  public virtual List<Media>? Media { get; set; }
}

public class Media : Entity
{
  [JsonIgnore]
  public virtual List<Product>? Products { get; set; }
}

// Database context setup.
public class Database : DbContext
{
  private string _connectionString = "server=127.0.0.1;port=8765;database=sandbox-ef;user id=postgres;password=postgres;include error detail=true";

  [NotNull]
  public DbSet<Product>? Products { get; set; }

  [NotNull]
  public DbSet<Media>? Media { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (string.IsNullOrEmpty(_connectionString))
    {
      throw new InvalidOperationException(
          "Cannot configure the database; no connection string provided."
      );
    }

    optionsBuilder
        //.UseLazyLoadingProxies()
        .UseNpgsql(
            _connectionString,
            npgsqlOptionsAction: options => options.UseAdminDatabase("postgres"))
        // Postgres does not like capital letters so switch to snake_case
        .UseSnakeCaseNamingConvention()
        .EnableDetailedErrors(true)
        .EnableSensitiveDataLogging(true)
        .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
  }
}