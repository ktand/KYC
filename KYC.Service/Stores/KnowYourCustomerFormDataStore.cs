using KYC.Service.Caching;
using KYC.Service.Extensions;
using KYC.Service.ExternalClients;

namespace KYC.Service.Stores;

public class KnowYourCustomerFormDataStore(CustomerDataClient customerDataClient, ICacheService<KYCForm?> cache) : IKnowYourCustomerFormDataStore
{
    public Task<KYCForm?> GetLatestFormData(string ssn, CancellationToken cancellationToken) => cache
        .GetOrAddAsync(ssn, async () => await customerDataClient
                .KycFormAsync(ssn, DateTimeOffset.UtcNow, cancellationToken)
                .GetOrReturnNullOnNotFound(),
            cancellationToken: cancellationToken);
}