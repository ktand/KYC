using KYC.Service.ExternalClients;

namespace KYC.Service.Stores;

public interface IPersonalDetailsStore
{
    Task<PersonalDetails?> GetPersonalDetails(string ssn, CancellationToken cancellationToken);
}