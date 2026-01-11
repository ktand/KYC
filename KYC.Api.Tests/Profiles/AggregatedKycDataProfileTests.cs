using AutoMapper;
using FluentAssertions;
using KYC.Profiles;
using KYC.Service.Controllers;
using KYC.Service.ExternalClients;
using Xunit;

namespace KYC.Api.Tests.Profiles;

public class AggregatedKycDataProfileTests
{
    private readonly IMapper _mapper;

    public AggregatedKycDataProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
             cfg.AddProfile<AggregatedKycDataProfile>();
             cfg.AddProfile<AddressProfile>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Configuration_ShouldBeValid()
    {
        // Assert
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AggregatedKycDataProfile>();
            cfg.AddProfile<AddressProfile>();
        });
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_ShouldCorrectlyAggregateDataWithUSAddress()
    {
        // Arrange
        var ssn = "123456-7890";
        var personal = new PersonalDetails { First_name = "John", Sur_name = "Doe" };
        var contact = new ContactDetails
        {
            Addresses = new List<Address>
            {
                new()
                {
                    Street = "456 Placeholder Ave",
                    Postal_code = "00000",
                    City = "Sampletown",
                    State = "NY",
                    Country = "USA"
                }
            },
            Phone_numbers = new List<PhoneNumber>
            {
                new() { Number = "555-002", Preferred = false },
                new() { Number = "555-001", Preferred = true },
                new() { Number = "555-003", Preferred = false }
            },
            Emails = new List<Email>
            {
                new() { Email_address = "b@test.com", Preferred = false },
                new() { Email_address = "a@test.com", Preferred = true }
            }
        };
        var formData = new KYCForm
        {
            Items = new List<KYCItem>
            {
                new() { Key = "tax_country", Value = "United States" },
                new() { Key = "annual_income", Value = "1100000" }
            }
        };

        var source = (ssn, personal, contact, formData);

        // Act
        var result = _mapper.Map<AggregatedKycData>(source);

        // Assert
        result.Ssn.Should().Be(ssn);
        result.First_name.Should().Be("John");
        result.Last_name.Should().Be("Doe");
        result.Address.Should().Be("456 Placeholder Ave, Sampletown, NY 00000, USA");

        // Check the sorting of phone numbers and emails based on preferred flag
        result.Phone_number.Should().Be("555-001;555-002;555-003");
        result.Email.Should().Be("a@test.com;b@test.com");

        // KYC Form items
        result.Tax_country.Should().Be("United States");
        result.Income.Should().Be(1100000);
    }
    
    [Fact]
    public void Map_ShouldCorrectlyAggregateDataWithSwedishAddress()
    {
        // Arrange
        var ssn = "123456-7890";
        var personal = new PersonalDetails { First_name = "Anders", Sur_name = "Andersson" };
        var contact = new ContactDetails
        {
            Addresses = new List<Address>
            {
                new()
                {
                    Street = "Sveavägen 1",
                    Postal_code = "12345",
                    City = "Stockholm",
                    Country = "Sweden"
                }
            },
            Phone_numbers = new List<PhoneNumber>
            {
                new() { Number = "+4670100000", Preferred = false },
                new() { Number = "+4670100001", Preferred = true },
                new() { Number = "+4670100002", Preferred = false }
            },
            Emails = new List<Email>
            {
                new() { Email_address = "b@test.com", Preferred = false },
                new() { Email_address = "a@test.com", Preferred = true }
            }
        };
        var formData = new KYCForm
        {
            Items = new List<KYCItem>
            {
                new() { Key = "tax_country", Value = "Sweden" },
                new() { Key = "annual_income", Value = "1100000" }
            }
        };

        var source = (ssn, personal, contact, formData);

        // Act
        var result = _mapper.Map<AggregatedKycData>(source);

        // Assert
        result.Ssn.Should().Be(ssn);
        result.First_name.Should().Be("Anders");
        result.Last_name.Should().Be("Andersson");
        result.Address.Should().Be("Sveavägen 1, 12345 Stockholm, Sweden");

        // Check the sorting of phone numbers and emails based on preferred flag
        result.Phone_number.Should().Be("+4670100001;+4670100000;+4670100002");
        result.Email.Should().Be("a@test.com;b@test.com");

        // KYC Form items
        result.Tax_country.Should().Be("Sweden");
        result.Income.Should().Be(1100000);
    }

    [Fact]
    public void Map_WithEmptyCollections_ShouldSucceed()
    {
        // Arrange
        var source = (
            Ssn: "123456-7890",
            Personal: new PersonalDetails(),
            Contact: new ContactDetails
            {
                Addresses = new List<Address>(),
                Phone_numbers = new List<PhoneNumber>(),
                Emails = new List<Email>()
            },
            FormData: new KYCForm
            {
                Items = new List<KYCItem>()
            }
        );

        // Act
        var result = _mapper.Map<AggregatedKycData>(source);

        // Assert
        result.Ssn.Should().Be("123456-7890");
        result.Phone_number.Should().BeEmpty();
        result.Email.Should().BeEmpty();
        result.Tax_country.Should().BeNull();
        result.Income.Should().BeNull();
    }
}