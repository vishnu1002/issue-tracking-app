using IssueTrackingAPI.DTO.DashboardDTO;
using IssueTrackingAPI.Repository.DashboardRepo.DashboardRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardRepo _dashboardRepo;

    public DashboardController(IDashboardRepo dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    //
    // Get Dashboard Statistics
    // GET: api/dashboard/stats
    //
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats_DTO>> GetDashboardStats([FromQuery] string? fromDate = null, [FromQuery] string? toDate = null)
    {
        try
        {
            var stats = await _dashboardRepo.GetDashboardStatsAsync(fromDate, toDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving dashboard statistics", error = ex.Message });
        }
    }

    //
    // Get Ticket Trends
    // GET: api/dashboard/trends?days=30
    //
    [HttpGet("trends")]
    public async Task<ActionResult<List<TicketTrend_DTO>>> GetTicketTrends([FromQuery] int days = 30)
    {
        try
        {
            if (days <= 0 || days > 365)
                return BadRequest(new { message = "Days must be between 1 and 365" });

            var trends = await _dashboardRepo.GetTicketTrendsAsync(days);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving ticket trends", error = ex.Message });
        }
    }

    //
    // Get Representative Performance
    // GET: api/dashboard/performance
    //
    [HttpGet("performance")]
    public async Task<ActionResult<List<RepresentativePerformance_DTO>>> GetRepresentativePerformance()
    {
        try
        {
            var performance = await _dashboardRepo.GetRepresentativePerformanceAsync();
            return Ok(performance);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving representative performance", error = ex.Message });
        }
    }
}
