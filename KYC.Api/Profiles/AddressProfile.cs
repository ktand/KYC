using AutoMapper;
using KYC.Service.ExternalClients;

namespace KYC.Profiles;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<Address, string>().ConstructUsing(address => FormatAddress(address));
    }
    
    /// <summary>
    /// A simple function for formatting addresses. It attempts to present addresses
    /// in a clear format, based on whether the Address.State field is populated or not.
    /// </summary>
    /// <param name="address"></param>
    /// <returns>Formatted address</returns>
    private static string FormatAddress(Address address)
    {
        string middleSection;

        // Check if State is missing (implies EU/International style like "123 45 Stockholm")
        if (string.IsNullOrWhiteSpace(address.State))
        {
            // Format: "Postal_code City"
            middleSection = string.Join(" ", 
                new[] { address.Postal_code, address.City }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        }
        else
        {
            // Format: "City, State Postal_code"
            var stateAndZip = string.Join(" ", 
                new[] { address.State, address.Postal_code }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            middleSection = string.Join(", ", 
                new[] { address.City, stateAndZip }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        // Combine: Street, Middle Section, Country
        return string.Join(", ", 
            new[] { address.Street, middleSection, address.Country }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}