public static class Seeder
{
  private static readonly Random _random = System.Random.Shared;

  public static void Seed(Database database)
  {
    // Create 100 media
    var media = Enumerable
      .Range(1, 101)
      .Select(i => new Media()
      {
        Id = i,
        Name = $"Image {i}"
      })
      .ToArray();

    database.Media.AddRange(media);

    // Create 50 products; randomly pick a few media per product.
    var products = Enumerable
      .Range(1, 51)
      .Select(i =>
      {
        var productMedia = Enumerable
          .Range(2, _random.Next(4, 10))
          .Select(j =>
          {
            var mediaIndex = _random.Next(0, 100);

            return media[mediaIndex];
          }).ToList();

        return new Product()
        {
          Id = i,
          Name = $"Product {i}",
          Media = productMedia
        };
      });

    database.Products.AddRange(products);
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