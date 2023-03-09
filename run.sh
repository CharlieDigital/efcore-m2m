curl -X POST http://localhost:5256/groups \
  -H 'Content-Type: application/json' \
  -d '["05278e84-1348-4800-a660-ed06793a6d67", "0a6c29de-9d9d-4a09-9124-ee980d4f9c0b", "100fc8c3-d02a-4df9-a9c6-eb1b5887a86e"]' | jq .