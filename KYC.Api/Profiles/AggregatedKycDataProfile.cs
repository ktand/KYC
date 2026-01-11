using AutoMapper;
using KYC.Service.Controllers;
using KYC.Service.ExternalClients;

namespace KYC.Profiles;

public class AggregatedKycDataProfile : Profile
{
    public AggregatedKycDataProfile()
    {
        CreateMap<(string Ssn, PersonalDetails Personal, ContactDetails Contact, KYCForm FormData), AggregatedKycData>()
            // SSN mapping
            .ForMember(d => d.Ssn, o =>
                o.MapFrom(s => s.Ssn))

            // PersonalDetails mapping
            .ForMember(d => d.First_name, o =>
                o.MapFrom(s => s.Personal.First_name))
            .ForMember(d => d.Last_name, o =>
                o.MapFrom(s => s.Personal.Sur_name))

            // ContactDetails mapping
            
            // Map the first address only
            .ForMember(d => d.Address, o =>
                o.MapFrom(s => s.Contact.Addresses.FirstOrDefault()))

            // Map phone numbers as a semicolon separated string. The preferred number will be the first one in the list and the rest will be sorted alphabetically.
            .ForMember(d => d.Phone_number, o =>
                o.MapFrom(s => string.Join(';', s.Contact.Phone_numbers.OrderByDescending(e => e.Preferred).ThenBy(e => e.Number).Select(e => e.Number))))

            // Map email addresses as a semicolon separated string. The preferred email address will be the first one in the list and the rest will be sorted alphabetically.
            .ForMember(d => d.Email, o =>
                o.MapFrom(s => string.Join(';', s.Contact.Emails.OrderByDescending(e => e.Preferred).ThenBy(e => e.Email_address).Select(e => e.Email_address))))

            // KYCForm mapping
            .ForMember(d => d.Tax_country, o =>
                o.MapFrom((s, d) => s.FormData.Items.FirstOrDefault(i => i.Key == "tax_country")?.Value))
            .ForMember(d => d.Income, o =>
                o.MapFrom((s, d) => s.FormData.Items.FirstOrDefault(i => i.Key == "annual_income")?.Value))
            .ForMember(d => d.AdditionalProperties, o => o.Ignore());
    }
}