```sql
EXPLAIN ANALYZE
SELECT
  t1.media_id, t1.products_id, t1.id, t1.created_utc, t1.name,
  t.id,
  t0.product_groups_id, t0.products_id, t0.id
FROM (
    SELECT p.id
    FROM product_groups AS p
    WHERE p.id = 10
    LIMIT 1
) AS t
INNER JOIN (
    SELECT p0.product_groups_id, p0.products_id, p1.id
    FROM product_product_group AS p0
    INNER JOIN products AS p1 ON p0.products_id = p1.id
) AS t0 ON t.id = t0.product_groups_id
INNER JOIN (
    SELECT m.media_id, m.products_id, m0.id, m0.created_utc, m0.name
    FROM media_product AS m
    INNER JOIN media AS m0 ON m.media_id = m0.id
) AS t1 ON t0.id = t1.products_id
ORDER BY t.id, t0.product_groups_id, t0.products_id, t0.id
```

```
Sort  (cost=22.17..22.22 rows=19 width=46) (actual time=0.384..0.386 rows=18 loops=1)
  Sort Key: p.id, m.products_id
  Sort Method: quicksort  Memory: 27kB
  ->  Nested Loop  (cost=1.40..21.77 rows=19 width=46) (actual time=0.205..0.341 rows=18 loops=1)
        ->  Nested Loop  (cost=1.11..15.47 rows=19 width=24) (actual time=0.178..0.234 rows=18 loops=1)
              Join Filter: (p0.products_id = m.products_id)
              ->  Nested Loop  (cost=0.83..13.60 rows=3 width=16) (actual time=0.144..0.155 rows=3 loops=1)
                    ->  Nested Loop  (cost=0.55..12.66 rows=3 width=12) (actual time=0.096..0.099 rows=3 loops=1)
                          ->  Limit  (cost=0.27..8.29 rows=1 width=4) (actual time=0.041..0.041 rows=1 loops=1)
                                ->  Index Only Scan using pk_product_groups on product_groups p  (cost=0.27..8.29 rows=1 width=4) (actual time=0.039..0.039 rows=1 loops=1)
                                      Index Cond: (id = 10)
                                      Heap Fetches: 1
                          ->  Index Only Scan using pk_product_product_group on product_product_group p0  (cost=0.28..4.33 rows=3 width=8) (actual time=0.027..0.028 rows=3 loops=1)
                                Index Cond: (product_groups_id = p.id)
                                Heap Fetches: 0
                    ->  Index Only Scan using pk_products on products p1  (cost=0.28..0.31 rows=1 width=4) (actual time=0.018..0.018 rows=1 loops=3)
                          Index Cond: (id = p0.products_id)
                          Heap Fetches: 0
              ->  Index Scan using ix_media_product_products_id on media_product m  (cost=0.29..0.55 rows=6 width=8) (actual time=0.014..0.023 rows=6 loops=3)
                    Index Cond: (products_id = p1.id)
        ->  Index Scan using pk_media on media m0  (cost=0.29..0.33 rows=1 width=22) (actual time=0.005..0.005 rows=1 loops=18)
              Index Cond: (id = m.media_id)
Planning Time: 18.393 ms
Execution Time: 0.703 ms
```

----

```sql
EXPLAIN ANALYZE
SELECT
  mp.media_id, mp.products_id, m.id, m.created_utc, m.name,
  t.id,
  ppg.product_groups_id, ppg.products_id, p.id
FROM (
    SELECT p.id
    FROM product_groups AS p
    WHERE p.id = 10
    LIMIT 1
) AS t
JOIN product_product_group AS ppg
  ON t.id = ppg.product_groups_id
JOIN products AS p
  ON ppg.products_id = p.id
JOIN media_product AS mp
  ON p.id = mp.products_id
JOIN media AS m
  ON mp.media_id = m.id
ORDER BY t.id, ppg.product_groups_id, ppg.products_id, p.id
```

```
Sort  (cost=22.17..22.22 rows=19 width=46) (actual time=0.201..0.203 rows=18 loops=1)
  Sort Key: p_1.id, mp.products_id
  Sort Method: quicksort  Memory: 27kB
  ->  Nested Loop  (cost=1.40..21.77 rows=19 width=46) (actual time=0.117..0.187 rows=18 loops=1)
        ->  Nested Loop  (cost=1.11..15.47 rows=19 width=24) (actual time=0.099..0.126 rows=18 loops=1)
              Join Filter: (ppg.products_id = mp.products_id)
              ->  Nested Loop  (cost=0.83..13.60 rows=3 width=16) (actual time=0.055..0.061 rows=3 loops=1)
                    ->  Nested Loop  (cost=0.55..12.66 rows=3 width=12) (actual time=0.040..0.041 rows=3 loops=1)
                          ->  Limit  (cost=0.27..8.29 rows=1 width=4) (actual time=0.021..0.021 rows=1 loops=1)
                                ->  Index Only Scan using pk_product_groups on product_groups p_1  (cost=0.27..8.29 rows=1 width=4) (actual time=0.020..0.020 rows=1 loops=1)
                                      Index Cond: (id = 10)
                                      Heap Fetches: 1
                          ->  Index Only Scan using pk_product_product_group on product_product_group ppg  (cost=0.28..4.33 rows=3 width=8) (actual time=0.017..0.017 rows=3 loops=1)
                                Index Cond: (product_groups_id = p_1.id)
                                Heap Fetches: 0
                    ->  Index Only Scan using pk_products on products p  (cost=0.28..0.31 rows=1 width=4) (actual time=0.006..0.006 rows=1 loops=3)
                          Index Cond: (id = ppg.products_id)
                          Heap Fetches: 0
              ->  Index Scan using ix_media_product_products_id on media_product mp  (cost=0.29..0.55 rows=6 width=8) (actual time=0.016..0.020 rows=6 loops=3)
                    Index Cond: (products_id = p.id)
        ->  Index Scan using pk_media on media m  (cost=0.29..0.33 rows=1 width=22) (actual time=0.003..0.003 rows=1 loops=18)
              Index Cond: (id = mp.media_id)
Planning Time: 2.601 ms
Execution Time: 0.309 ms
```
