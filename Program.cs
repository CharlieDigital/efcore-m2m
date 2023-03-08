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
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/init", async(Database db) => {
    var product1 = await db.Products.AddAsync(new Product() {
        Id = Guid.NewGuid(),
        Name = "Product 1"
    });

    var product2 = await db.Products.AddAsync(new Product() {
        Id = Guid.NewGuid(),
        Name = "Product 2"
    });

    var media1 = await db.Media.AddAsync(new Media() {
        Id = Guid.NewGuid(),
        Name = "Media 1"
    });

    var media2 = await db.Media.AddAsync(new Media() {
        Id = Guid.NewGuid(),
        Name = "Media 2"
    });

    var media = new List<ProductMedia>() {
        new () {
            Product = product1.Entity,
            Media = media1.Entity
        },
        new () {
            Product = product1.Entity,
            Media = media2.Entity
        },
        new () {
            Product = product2.Entity,
            Media = media1.Entity
        },
        new () {
            Product = product2.Entity,
            Media = media2.Entity
        },
    };

    await db.ProductMedia.AddRangeAsync(media);

    await db.SaveChangesAsync();
})
.WithName("Init");

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

app.MapGet("/products/{id}", async (Guid id, Database db) =>
{
    return await db.Products!.FindAsync(id);
})
.WithName("GetProductById");

app.MapGet("/products/", async (Database db) =>
{
    var products = await db.Products
        .TagWith("LOAD_PRODUCT")
        .Include(p => p.ProductMedia!)
        .ThenInclude(pm => pm.Media!)
        .ToListAsync();

    return products;
})
.WithName("GetProducts");

app.Run();

// DB entity classes
public abstract class Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
}

    public class Product : Entity
    {
        public virtual List<ProductMedia>? ProductMedia { get; set; }
    }

    public class Media : Entity
    {
        [JsonIgnore]
        public virtual List<ProductMedia>? ProductMedia { get; set; }
    }

    public class ProductMedia {
        public Guid ProductId { get; set; }
        [JsonIgnore]
        [NotNull]
        public Product? Product { get; set;}

        public Guid MediaId { get; set; }
        [NotNull]
        public Media? Media { get; set; }
    }


// Database context setup.
public class Database : DbContext
{
    private string _connectionString = "server=127.0.0.1;port=8765;database=sandbox-ef;user id=postgres;password=postgres;include error detail=true";

    [NotNull]
    public DbSet<Product>? Products { get; set; }

    [NotNull]
    public DbSet<Media>? Media { get; set; }

    [NotNull]
    public DbSet<ProductMedia>? ProductMedia { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<ProductMedia>()
            .HasKey(pm => new { pm.ProductId, pm.MediaId });

        builder.Entity<ProductMedia>()
            .HasOne(pm => pm.Product)
            .WithMany(p => p.ProductMedia)
            .HasForeignKey(pm => pm.ProductId);

        builder.Entity<ProductMedia>()
            .HasOne(pm => pm.Media)
            .WithMany(m => m.ProductMedia)
            .HasForeignKey(pm => pm.MediaId);
    }

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