using KYC.Service.ExternalClients;

namespace KYC.Service.Stores;

public interface IContactDetailsStore
{
    Task<ContactDetails?> GetContactDetails(string ssn, CancellationToken cancellationToken);
}