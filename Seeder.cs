public static class Seeder
{
  private static readonly Random _random = System.Random.Shared;

  public static void Seed(Database database)
  {
    // Create 10000 media
    var media = Enumerable
      .Range(1, 10001)
      .Select(i => new Media()
      {
        Id = Guid.NewGuid(),
        Name = $"Image {i}"
      })
      .ToArray();

    database.Media.AddRange(media);

    // Create 2000 products; randomly pick a few media per product.
    var products = Enumerable
      .Range(1, 2001)
      .Select(i =>
      {
        var productMedia = Enumerable
          .Range(2, _random.Next(4, 10))
          .Select(j =>
          {
            var mediaIndex = _random.Next(0, 10000);

            return media[mediaIndex];
          }).ToList();

        return new Product()
        {
          Id = Guid.NewGuid(),
          Name = $"Product {i}",
          Media = productMedia
        };
      }).ToArray();

    database.Products.AddRange(products);

    // Create 500 product groups
    var groups = Enumerable
      .Range(1, 501)
      .Select(i =>
      {
        var groupProducts = Enumerable
          .Range(1, _random.Next(3, 5))
          .Select(j =>
          {
            var productIndex = _random.Next(0, 2000);

            return products[productIndex];
          }).ToList();

        return new ProductGroup()
        {
          Id = Guid.NewGuid(),
          Name = $"Product Group {i}",
          Products = groupProducts
        };
      });

    database.ProductGroups.AddRange(groups);

    database.SaveChanges();
  }
}

/*
        var productMedia = Enumerable
          .Range(2, _random.Next(4, 10))
          .Select(j =>
          {
            var mediaIndex = _random.Next(0, 100);

            return media[mediaIndex];
          }).ToList();
          */