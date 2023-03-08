# Reproduction of Inefficient Sub-Select in EFCore 7.0

See this GitHub issue: https://github.com/dotnet/efcore/issues/17622

## Output Query for EFCore7 M2M

```sql
      SELECT p.id, p.created_utc, p.name, t.product_id, t.media_id, t.id, t.created_utc, t.name
      FROM products AS p
      LEFT JOIN (
          SELECT p0.product_id, p0.media_id, m.id, m.created_utc, m.name
          FROM product_media AS p0
          INNER JOIN media AS m ON p0.media_id = m.id
      ) AS t ON p.id = t.product_id
      ORDER BY p.id, t.product_id, t.media_id
```

## To run:

```
docker compose up -d
dotnet run
curl http://localhost:5256/init & curl http://localhost:5256/products | jq .
```