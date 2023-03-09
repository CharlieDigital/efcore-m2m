```sql
EXPLAIN ANALYZE
SELECT t0.media_id, t0.products_id, t0.id, t0.created_utc, t0.name,
p.id,
t.product_groups_id, t.products_id, t.id
FROM product_groups AS p
INNER JOIN (
    SELECT p0.product_groups_id, p0.products_id, p1.id
    FROM product_product_group AS p0
    INNER JOIN products AS p1 ON p0.products_id = p1.id
) AS t ON p.id = t.product_groups_id
INNER JOIN (
    SELECT m.media_id, m.products_id, m0.id, m0.created_utc, m0.name
    FROM media_product AS m
    INNER JOIN media AS m0 ON m.media_id = m0.id
) AS t0 ON t.id = t0.products_id
WHERE p.id IN ('05278e84-1348-4800-a660-ed06793a6d67', '0a6c29de-9d9d-4a09-9124-ee980d4f9c0b', '100fc8c3-d02a-4df9-a9c6-eb1b5887a86e')
ORDER BY p.id, t.product_groups_id, t.products_id, t.id
```

```Sort  (cost=62.74..62.92 rows=71 width=130) (actual time=1.349..1.353 rows=52 loops=1)
  Sort Key: p.id, m.products_id
  Sort Method: quicksort  Memory: 38kB
  ->  Nested Loop  (cost=1.12..60.56 rows=71 width=130) (actual time=0.212..1.239 rows=52 loops=1)
        ->  Nested Loop  (cost=0.84..36.34 rows=71 width=96) (actual time=0.167..0.685 rows=52 loops=1)
              Join Filter: (p0.products_id = m.products_id)
              ->  Nested Loop  (cost=0.56..28.54 rows=11 width=64) (actual time=0.118..0.247 rows=10 loops=1)
                    ->  Nested Loop  (cost=0.28..25.05 rows=11 width=48) (actual time=0.055..0.135 rows=10 loops=1)
                          ->  Seq Scan on product_groups p  (cost=0.00..11.89 rows=3 width=16) (actual time=0.018..0.086 rows=3 loops=1)
                                Filter: (id = ANY ('{05278e84-1348-4800-a660-ed06793a6d67,0a6c29de-9d9d-4a09-9124-ee980d4f9c0b,100fc8c3-d02a-4df9-a9c6-eb1b5887a86e}'::uuid[]))
                                Rows Removed by Filter: 498
                          ->  Index Only Scan using pk_product_product_group on product_product_group p0  (cost=0.28..4.35 rows=4 width=32) (actual time=0.013..0.014 rows=3 loops=3)
                                Index Cond: (product_groups_id = p.id)
                                Heap Fetches: 0
                    ->  Index Only Scan using pk_products on products p1  (cost=0.28..0.32 rows=1 width=16) (actual time=0.010..0.010 rows=1 loops=10)
                          Index Cond: (id = p0.products_id)
                          Heap Fetches: 0
              ->  Index Scan using ix_media_product_products_id on media_product m  (cost=0.29..0.63 rows=6 width=32) (actual time=0.015..0.042 rows=5 loops=10)
                    Index Cond: (products_id = p1.id)
        ->  Index Scan using pk_media on media m0  (cost=0.29..0.34 rows=1 width=34) (actual time=0.010..0.010 rows=1 loops=52)
              Index Cond: (id = m.media_id)
Planning Time: 5.122 ms
Execution Time: 1.660 ms
```

---

```sql
EXPLAIN ANALYZE
SELECT
  mp.media_id, mp.products_id, m.id, m.created_utc, m.name,
  t.id,
  ppg.product_groups_id, ppg.products_id, p.id
FROM (
    SELECT p.id
    FROM product_groups AS p
    WHERE p.id IN ('05278e84-1348-4800-a660-ed06793a6d67', '0a6c29de-9d9d-4a09-9124-ee980d4f9c0b', '100fc8c3-d02a-4df9-a9c6-eb1b5887a86e')
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
Sort  (cost=21.94..22.01 rows=26 width=130) (actual time=1.129..1.133 rows=23 loops=1)
  Sort Key: p_1.id, mp.products_id
  Sort Method: quicksort  Memory: 31kB
  ->  Nested Loop  (cost=1.12..21.33 rows=26 width=130) (actual time=0.247..1.086 rows=23 loops=1)
        ->  Nested Loop  (cost=0.84..12.47 rows=26 width=96) (actual time=0.206..0.403 rows=23 loops=1)
              Join Filter: (ppg.products_id = mp.products_id)
              ->  Nested Loop  (cost=0.56..9.63 rows=4 width=64) (actual time=0.136..0.161 rows=4 loops=1)
                    ->  Nested Loop  (cost=0.28..8.36 rows=4 width=48) (actual time=0.109..0.113 rows=4 loops=1)
                          ->  Limit  (cost=0.00..3.96 rows=1 width=16) (actual time=0.052..0.053 rows=1 loops=1)
                                ->  Seq Scan on product_groups p_1  (cost=0.00..11.89 rows=3 width=16) (actual time=0.039..0.039 rows=1 loops=1)
                                      Filter: (id = ANY ('{05278e84-1348-4800-a660-ed06793a6d67,0a6c29de-9d9d-4a09-9124-ee980d4f9c0b,100fc8c3-d02a-4df9-a9c6-eb1b5887a86e}'::uuid[]))
                                      Rows Removed by Filter: 5
                          ->  Index Only Scan using pk_product_product_group on product_product_group ppg  (cost=0.28..4.35 rows=4 width=32) (actual time=0.054..0.056 rows=4 loops=1)
                                Index Cond: (product_groups_id = p_1.id)
                                Heap Fetches: 0
                    ->  Index Only Scan using pk_products on products p  (cost=0.28..0.32 rows=1 width=16) (actual time=0.011..0.011 rows=1 loops=4)
                          Index Cond: (id = ppg.products_id)
                          Heap Fetches: 0
              ->  Index Scan using ix_media_product_products_id on media_product mp  (cost=0.29..0.63 rows=6 width=32) (actual time=0.025..0.058 rows=6 loops=4)
                    Index Cond: (products_id = p.id)
        ->  Index Scan using pk_media on media m  (cost=0.29..0.34 rows=1 width=34) (actual time=0.027..0.027 rows=1 loops=23)
              Index Cond: (id = mp.media_id)
Planning Time: 6.639 ms
Execution Time: 1.480 ms
```
