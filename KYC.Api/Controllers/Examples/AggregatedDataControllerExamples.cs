using KYC.Service.Controllers;
using Swashbuckle.AspNetCore.Filters;

namespace KYC.Controllers.Examples;

// Error examples
public abstract class BaseErrorExample(string message) : IExamplesProvider<ErrorResponse>
{
    public ErrorResponse GetExamples() => new() { Error = message };
}

public class NotFoundExample() : BaseErrorExample("Customer data not found for the provided SSN.");
public class InternalServerErrorExample() : BaseErrorExample("An unexpected error occurred while processing the request.");

// Aggregated data example
public class AggregatedKycDataExample : IExamplesProvider<AggregatedKycData>
{
    public AggregatedKycData GetExamples()
    {
        return new AggregatedKycData
        {
            Ssn = "19801115-1234",
            First_name = "Lars",
            Last_name = "Larsson",
            Address = "Smågatan 1, 123 22 Malmö",
            Phone_number= "+46 70 123 45 67",
            Email = "lars.larsson@example.com",
            Tax_country = "SE",
            Income = 550000
        };
    }
}
