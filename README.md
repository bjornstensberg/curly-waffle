
1. `docker compose up -d` (Database)
2. Start "Producer" and "Consumer".
3. Go to `/Producer/WebApp.http`
4. Call the "works" endpoint. Tenant Id will be displayed in the Consumer console.
5. Call the "fails" endpoint. Tenant Id will be "Marten" in the Consumer console.
