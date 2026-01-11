# Project Reflection: KYC Aggregation Service
## 1. Design Decisions
### Persistent Caching Strategy
I chose to use SQLite (via NeoSmart.Caching.Sqlite) as the persistent back-end for IDistributedCache. SQLite was chosen for this test because it provides persistence without requiring you to set up external infrastructure. In a production environment I would perhaps use Redis.
- Reliability: I implemented a SemaphoreSlim in the CacheService to prevent API-hammering. This ensures that if multiple requests for the same expired key arrive simultaneously, only one call is made to the external API while other threads wait for the result.
### Error Handling
- Global Exception Handling: Used the IExceptionHandler as a centralized way to catch and log system errors while returning clean errors to the user.
- Error recovery: Integrated Polly with the HttpClient to handle temporary network glitches or rate limits using exponential backoff.
### Separation of Concerns (SOLID)
I utilized the Store Pattern (IContactDetailsStore, etc.) to isolate the controller from the details of caching and API clients. This keeps the controller clean and focuses purely on request handling.
## 2. Areas for Improvement
- Cache Invalidation: Currently, data is cached with a fixed expiration. In a production scenario, cached data could be re-validated using conditional gets (ETag/Last-modified) from the API.
- Integration Testing: In addition to unit tests, I would add integration tests using WebApplicationFactory to verify the full request-response pipeline.
## 3. Tools Used
- NSwag: I used NSwag for client and controller generation to ensure that the code remains in sync with the external API specifications.
- AutoMapper: Used to decouple external data models from our internal models.
## 4. API specification
- There is a difference in the API specification part of the test instructions compared to what the Customer Data API returns. In the ContactDetails component the _address_ field is returned as _addresses_ in the API. I used the _addresses_ field.
- There were no instructions on how to return phone numbers/addresses and email addresses. For phone numbers and email addresses I decided to return them all, comma separated, but with the preferred one first. For addresses I decided to just return the first one with the address fields concatenated depending on available fields.