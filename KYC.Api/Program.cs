using KYC.ExceptionHandling;
using KYC.Profiles;
using KYC.Service.Caching;
using KYC.Service.ExternalClients;
using KYC.Service.Stores;
using NeoSmart.Caching.Sqlite;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSingleton(typeof(ICacheService<>), typeof(CacheService<>));

// Add Http Client with a retry policy
builder.Services.AddHttpClient<CustomerDataClient>().AddTransientHttpErrorPolicy(policyBuilder =>
    policyBuilder.WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // Use exponential backoff
    )
);

builder.Services.AddTransient<IContactDetailsStore, ContactDetailsStore>();
builder.Services.AddTransient<IPersonalDetailsStore, PersonalDetailsStore>();
builder.Services.AddTransient<IKnowYourCustomerFormDataStore, KnowYourCustomerFormDataStore>();

// Cache configuration
// SQLite (Persistent)
builder.Services.AddSqliteCache(options => { options.CachePath = "kyc_cache.db"; }, new SQLitePCL.SQLite3Provider_e_sqlite3());

// In-Memory (Non-persistent)
// builder.Services.AddDistributedMemoryCache();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c => { c.EnableAnnotations(); });
}

// Configure AutoMapper
builder.Services.AddAutoMapper(cfg => { },
    typeof(AggregatedKycDataProfile),
    typeof(AddressProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Generates the JSON at /swagger/v1/swagger.json
    app.UseSwaggerUI(); // Generates the UI at /swagger/index.html
}

app.UseExceptionHandler(_ => { });

app.UseHttpsRedirection();

app.MapControllers();

app.Run();