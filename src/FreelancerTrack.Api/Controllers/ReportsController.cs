using FreelancerTrack.Application.Features.Reports.DTOs;
using FreelancerTrack.Application.Features.Reports.Queries;
using Microsoft.AspNetCore.Mvc;

namespace FreelancerTrack.Api.Controllers;

public class ReportsController : ApiControllerBase
{
    [HttpGet("cash-flow")]
    [ProducesResponseType(typeof(NetCashFlowReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNetCashFlow([FromQuery] DateTimeOffset? fromDate = null, [FromQuery] DateTimeOffset? toDate = null)
    {
        var result = await Mediator.Send(new GetNetCashFlowReportQuery(fromDate, toDate));
        return HandleResult(result);
    }

    [HttpGet("ar-aging")]
    [ProducesResponseType(typeof(AccountsReceivableAgingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsReceivableAging()
    {
        var result = await Mediator.Send(new GetAccountsReceivableAgingQuery());
        return HandleResult(result);
    }

    [HttpGet("project-profitability")]
    [ProducesResponseType(typeof(ProjectProfitabilityReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectProfitability()
    {
        var result = await Mediator.Send(new GetProjectProfitabilityQuery());
        return HandleResult(result);
    }
}
