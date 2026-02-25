# Error Handling & Resilience

### Retry Policies (Polly)

**EasyCars API Retry Policy:**
```csharp
services.AddHttpClient<IEasyCarsApiClient, EasyCarsApiClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx and 408
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Log.Warning("Retry {RetryAttempt} after {Delay}s due to {StatusCode}",
                    retryAttempt, timespan.TotalSeconds, outcome.Result?.StatusCode);
            }
        );
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromMinutes(1),
            onBreak: (outcome, duration) =>
            {
                Log.Error("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
            },
            onReset: () =>
            {
                Log.Information("Circuit breaker reset");
            }
        );
}
```

---

### Exception Mapping

**EasyCars API Response Codes:**
- `0 (Success)` → Success, continue processing
- `1 (AuthenticationFail)` → Throw `EasyCarsAuthenticationException` (401)
- `5 (Warning)` → Log warning, continue processing
- `7 (Failed)` → Throw `EasyCarsApiException` with error message
- `9 (SystemError)` → Throw `EasyCarsSystemException`, trigger retry

**Custom Exceptions:**
```csharp
public class EasyCarsAuthenticationException : Exception
{
    public EasyCarsAuthenticationException(string message) : base(message) { }
}

public class EasyCarsApiException : Exception
{
    public int ResponseCode { get; }
    public string? ApiErrorMessage { get; }
    
    public EasyCarsApiException(int code, string message) : base(message)
    {
        ResponseCode = code;
        ApiErrorMessage = message;
    }
}

public class EasyCarsSystemException : Exception
{
    public EasyCarsSystemException(string message, Exception? inner = null) 
        : base(message, inner) { }
}
```

---

### Sync Operation Error Handling

**Partial Failure Handling:**
- Stock sync processes items in batches of 50
- Individual item failures logged but don't stop batch
- Failed items recorded in sync log with error details
- Sync marked as "Warning" if some items failed, "Failed" if all failed

**Transaction Management:**
- Each batch processed in separate database transaction
- Rollback on critical errors (database constraint violations)
- Commit successful items even if some fail

**Notification Strategy:**
- Email notification to dealership admin on repeated failures (3+ consecutive)
- Slack/Teams webhook for system-wide EasyCars API outages
- Admin dashboard shows sync health status

---

### Monitoring & Alerting

**Application Insights / Logging:**
- Log all sync operations with duration and result
- Track API response times and error rates
- Monitor background job execution status
- Custom metrics: sync success rate, average sync duration

**Hangfire Dashboard:**
- View background job status and history
- Retry failed jobs manually
- View job parameters and execution logs
- Monitor job queue depth

**Health Checks:**
```csharp
services.AddHealthChecks()
    .AddCheck<EasyCarsApiHealthCheck>("easycars_api")
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<HangfireHealthCheck>("hangfire");
```

**Alerts:**
- Sync failure rate > 10% in 1 hour
- API response time > 5 seconds average
- Authentication failures > 5 in 10 minutes
- Circuit breaker opened

---
