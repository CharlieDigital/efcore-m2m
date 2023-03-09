```sql
EXPLAIN ANALYZE
SELECT t.id, t.created_utc, t.name, t0.media_id, t0.products_id, t0.id, t0.created_utc, t0.name
FROM (
    SELECT p.id, p.created_utc, p.name
    FROM products AS p
    WHERE p.id = 10
    LIMIT 1
) AS t
LEFT JOIN (
    SELECT m.media_id, m.products_id, m0.id, m0.created_utc, m0.name
    FROM media_product AS m
    INNER JOIN media AS m0 ON m.media_id = m0.id
) AS t0 ON t.id = t0.products_id
ORDER BY t.id, t0.media_id, t0.products_id
```

```
Sort  (cost=27.05..27.08 rows=11 width=96) (actual time=0.922..0.925 rows=9 loops=1)
  Sort Key: p.id, m.media_id, m.products_id
  Sort Method: quicksort  Memory: 26kB
  ->  Nested Loop Left Join  (cost=4.80..26.86 rows=11 width=96) (actual time=0.655..0.834 rows=9 loops=1)
        ->  Limit  (cost=0.15..8.17 rows=1 width=44) (actual time=0.260..0.261 rows=1 loops=1)
              ->  Index Scan using pk_products on products p  (cost=0.15..8.17 rows=1 width=44) (actual time=0.258..0.259 rows=1 loops=1)
                    Index Cond: (id = 10)
        ->  Nested Loop  (cost=4.64..18.57 rows=11 width=52) (actual time=0.391..0.566 rows=9 loops=1)
              ->  Bitmap Heap Scan on media_product m  (cost=4.37..15.04 rows=11 width=8) (actual time=0.207..0.261 rows=9 loops=1)
                    Recheck Cond: (p.id = products_id)
                    Heap Blocks: exact=4
                    ->  Bitmap Index Scan on ix_media_product_products_id  (cost=0.00..4.36 rows=11 width=0) (actual time=0.164..0.164 rows=9 loops=1)
                          Index Cond: (products_id = p.id)
              ->  Index Scan using pk_media on media m0  (cost=0.28..0.32 rows=1 width=44) (actual time=0.031..0.031 rows=1 loops=9)
                    Index Cond: (id = m.media_id)
Planning Time: 1.299 ms
Execution Time: 1.449 ms
```

----

```sql
EXPLAIN ANALYZE
SELECT t.id, t.created_utc, t.name, mp.media_id, mp.products_id, m.id, m.created_utc, m.name
FROM (
    SELECT p.id, p.created_utc, p.name
    FROM products AS p
    WHERE p.id = 10
    LIMIT 1
) AS t
LEFT JOIN media_product AS mp
  ON t.id = mp.products_id
LEFT JOIN media m
  ON mp.media_id = m.id
ORDER BY t.id, mp.media_id, mp.products_id
```

```
Sort  (cost=27.05..27.08 rows=11 width=96) (actual time=0.337..0.339 rows=9 loops=1)
  Sort Key: p.id, mp.media_id, mp.products_id
  Sort Method: quicksort  Memory: 26kB
  ->  Nested Loop Left Join  (cost=4.80..26.86 rows=11 width=96) (actual time=0.161..0.309 rows=9 loops=1)
        ->  Nested Loop Left Join  (cost=4.52..23.33 rows=11 width=52) (actual time=0.135..0.150 rows=9 loops=1)
              ->  Limit  (cost=0.15..8.17 rows=1 width=44) (actual time=0.086..0.087 rows=1 loops=1)
                    ->  Index Scan using pk_products on products p  (cost=0.15..8.17 rows=1 width=44) (actual time=0.084..0.084 rows=1 loops=1)
                          Index Cond: (id = 10)
              ->  Bitmap Heap Scan on media_product mp  (cost=4.37..15.04 rows=11 width=8) (actual time=0.043..0.055 rows=9 loops=1)
                    Recheck Cond: (p.id = products_id)
                    Heap Blocks: exact=4
                    ->  Bitmap Index Scan on ix_media_product_products_id  (cost=0.00..4.36 rows=11 width=0) (actual time=0.035..0.035 rows=9 loops=1)
                          Index Cond: (products_id = p.id)
        ->  Index Scan using pk_media on media m  (cost=0.28..0.32 rows=1 width=44) (actual time=0.017..0.017 rows=1 loops=9)
              Index Cond: (id = mp.media_id)
Planning Time: 0.463 ms
Execution Time: 0.650 ms
```