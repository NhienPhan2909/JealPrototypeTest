# Performance Considerations

### Optimization Strategies

**Batch Processing:**
- Process stock items in batches of 50 to balance memory and performance
- Use `SaveChangesAsync()` after each batch instead of per-item
- Parallel image downloads (max 5 concurrent) to reduce sync duration

**Database Indexing:**
- Index on `easycars_stock_number` for fast duplicate detection
- Index on `sync_logs.started_at DESC` for quick recent log retrieval
- Composite index on `(dealership_id, data_source)` for filtered queries

**Caching:**
- Cache JWT tokens until 1 minute before expiry (9 minutes effective)
- Cache dealership credentials for duration of sync operation
- Consider Redis cache for frequently accessed sync status (future enhancement)

**Async Operations:**
- All I/O operations async (database, API calls, image downloads)
- Use `ConfigureAwait(false)` in library code to avoid deadlocks
- Implement cancellation token support for long-running operations

**Query Optimization:**
- Use EF Core `AsNoTracking()` for read-only queries
- Select only required fields in projections
- Avoid N+1 queries with `Include()` for related entities

---

### Scalability

**Horizontal Scaling:**
- Stateless API design supports multiple instances behind load balancer
- Hangfire supports distributed execution with SQL Server storage
- No in-memory session state

**Background Job Concurrency:**
- Configure max concurrent jobs: `MaxWorkerCount = Environment.ProcessorCount`
- Prevent overlapping sync jobs for same dealership: `[DisableConcurrentExecution]`
- Allow concurrent sync across different dealerships

**Database Connection Pooling:**
- Configure connection pool: `MinPoolSize=5;MaxPoolSize=100`
- Use connection pooling for EF Core DbContext
- Monitor connection usage and adjust as needed

**Rate Limiting:**
- Implement API rate limiting to prevent abuse: 5 manual syncs/hour/dealership
- Throttle image downloads: max 5 concurrent downloads
- Respect EasyCars API rate limits (documented in their API guide)

---

### Performance Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| Stock Sync Duration | < 2 minutes for 500 vehicles | End-to-end sync time |
| Lead Sync Duration | < 30 seconds for 100 leads | End-to-end sync time |
| API Response Time | < 500ms (P95) | /api/admin/easycars/* endpoints |
| Image Download | < 10 seconds for 10 images | Parallel download and upload |
| Database Query Time | < 100ms (P95) | EF Core query duration |
| Sync Failure Rate | < 2% | Failed syncs / total syncs |

---
