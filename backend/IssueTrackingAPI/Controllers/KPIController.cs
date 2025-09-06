using IssueTrackingAPI.DTO.DashboardDTO;
using IssueTrackingAPI.Repository.KPIRepo.KPIRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/kpi")]
[Authorize]
public class KPIController : ControllerBase
{
    private readonly IKPIRepo _kpiRepo;

    public KPIController(IKPIRepo kpiRepo)
    {
        _kpiRepo = kpiRepo;
    }

    //
    // Get Representative KPI
    // GET: api/kpi/representative/{id}?fromDate=2024-01-01&toDate=2024-12-31
    //
    [HttpGet("representative/{id}")]
    public async Task<ActionResult<RepresentativePerformance_DTO>> GetRepresentativeKPI(
        int id, 
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentRole = User.FindFirst(ClaimTypes.Role).Value;

            // Only admins can view any representative's KPI, representatives can only view their own
            if (currentRole != "Admin" && currentUserId != id)
                return Forbid();

            var kpi = await _kpiRepo.GetRepresentativeKPIAsync(id, fromDate, toDate);
            return Ok(kpi);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving representative KPI", error = ex.Message });
        }
    }

    //
    // Get All Representatives KPI (Admin only)
    // GET: api/kpi/representatives?fromDate=2024-01-01&toDate=2024-12-31
    //
    [HttpGet("representatives")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<RepresentativePerformance_DTO>>> GetAllRepresentativesKPI(
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var kpis = await _kpiRepo.GetAllRepresentativesKPIAsync(fromDate, toDate);
            return Ok(kpis);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving representatives KPI", error = ex.Message });
        }
    }

    //
    // Get Average Resolution Time
    // GET: api/kpi/average-resolution-time?fromDate=2024-01-01&toDate=2024-12-31
    //
    [HttpGet("average-resolution-time")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> GetAverageResolutionTime(
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var avgTime = await _kpiRepo.GetAverageResolutionTimeAsync(fromDate, toDate);
            return Ok(new { averageResolutionTimeHours = avgTime });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving average resolution time", error = ex.Message });
        }
    }

    //
    // Get Total Tickets Resolved
    // GET: api/kpi/total-resolved?fromDate=2024-01-01&toDate=2024-12-31
    //
    [HttpGet("total-resolved")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> GetTotalTicketsResolved(
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var totalResolved = await _kpiRepo.GetTotalTicketsResolvedAsync(fromDate, toDate);
            return Ok(new { totalTicketsResolved = totalResolved });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving total tickets resolved", error = ex.Message });
        }
    }
}
