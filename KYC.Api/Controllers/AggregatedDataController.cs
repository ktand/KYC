using AutoMapper;
using KYC.Controllers.Examples;
using KYC.Service.Controllers;
using KYC.Service.Stores;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using ControllerBase = KYC.Service.Controllers.ControllerBase;

namespace KYC.Controllers;

[ApiController]
[Produces("application/json")]
public class AggregatedDataController(
    IPersonalDetailsStore personalDetailsStore,
    IContactDetailsStore contactDetailsStore,
    IKnowYourCustomerFormDataStore formDataStore,
    IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Get aggregated KYC data
    /// </summary>
    [SwaggerOperation(
        Summary = "Get aggregated KYC data",
        OperationId = "GetAggregatedKycData",
        Tags = ["KYC Aggregation"]
    )]
    [SwaggerResponse(200, "Successful response", typeof(AggregatedKycData))]
    [SwaggerResponse(404, "Customer data not found", typeof(ErrorResponse))]
    [SwaggerResponse(500, "Internal server error", typeof(ErrorResponse))]
    [SwaggerResponseExample(200, typeof(AggregatedKycDataExample))]
    [SwaggerResponseExample(404, typeof(NotFoundExample))]
    [SwaggerResponseExample(500, typeof(InternalServerErrorExample))]
    public override async Task<ActionResult<AggregatedKycData>> GetAggregatedKycData(string ssn, CancellationToken cancellationToken = default)
    {
        var personalDetailsTask = personalDetailsStore.GetPersonalDetails(ssn, cancellationToken);
        var contactDetailsTask = contactDetailsStore.GetContactDetails(ssn, cancellationToken);
        var formDataTask = formDataStore.GetLatestFormData(ssn, cancellationToken);

        var personalDetails = await personalDetailsTask;
        var contactDetails = await contactDetailsTask;
        var formData = await formDataTask;

        // We need data from all three stores to build the response otherwise return 404
        if (personalDetails is null || contactDetails is null || formData is null)
        {
            return NotFound(new ErrorResponse { Error = "Customer data not found" });
        }

        return mapper.Map<AggregatedKycData>((ssn, personalDetails, contactDetails, formData));
    }
}