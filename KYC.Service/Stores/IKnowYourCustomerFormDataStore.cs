using KYC.Service.ExternalClients;

namespace KYC.Service.Stores;

public interface IKnowYourCustomerFormDataStore
{
    Task<KYCForm?> GetLatestFormData(string ssn, CancellationToken cancellationToken);
}