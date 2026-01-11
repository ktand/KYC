using KYC.Service.Caching;
using KYC.Service.Extensions;
using KYC.Service.ExternalClients;

namespace KYC.Service.Stores;

public class PersonalDetailsStore(CustomerDataClient customerDataClient, ICacheService<PersonalDetails?> cache) : IPersonalDetailsStore
{
    public Task<PersonalDetails?> GetPersonalDetails(string ssn, CancellationToken cancellationToken) => cache
        .GetOrAddAsync(ssn, async () => await customerDataClient
                .PersonalDetailsAsync(ssn, cancellationToken)
                .GetOrReturnNullOnNotFound(),
             cancellationToken: cancellationToken);
}