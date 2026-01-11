using KYC.Service.Caching;
using KYC.Service.Extensions;
using KYC.Service.ExternalClients;

namespace KYC.Service.Stores;

public class ContactDetailsStore(CustomerDataClient customerDataClient, ICacheService<ContactDetails?> cache) : IContactDetailsStore
{
    public Task<ContactDetails?> GetContactDetails(string ssn, CancellationToken cancellationToken) => cache
        .GetOrAddAsync(ssn, async () => await customerDataClient
                .ContactDetailsAsync(ssn, cancellationToken)
                .GetOrReturnNullOnNotFound(),
            cancellationToken: cancellationToken);
}