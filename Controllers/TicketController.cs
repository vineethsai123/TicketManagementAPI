using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagementApi.Models;
using TicketManagementApi.Services;

namespace TicketManagementApi.Controllers;
// The [Authorize] attribute ensures that all endpoints in this controller require authentication.

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Get all tickets
    /// </summary>
    [Authorize(Roles="Admin,User")] // Only users with Admin or User roles can access this endpoint
    [HttpGet]
    public ActionResult<List<Ticket>> GetAllTickets()
    {
        var tickets = _ticketService.GetAllTickets();
        return Ok(tickets);
    }

    /// <summary>
    /// Get a ticket by ID
    /// </summary>
    [Authorize(Roles="Admin,User")]
    [HttpGet("{id}")]
    public ActionResult<Ticket> GetTicketById(string id)
    {
        var ticket = _ticketService.GetTicketByStringId(id);
        if (ticket == null)
            return NotFound(new { message = $"Ticket with ID {id} not found." });

        return Ok(ticket);
    }

    /// <summary>
    /// Create a new ticket
    /// </summary>
    [Authorize(Roles="Admin")]
    [HttpPost]
    public ActionResult<Ticket> CreateTicket([FromBody] Ticket ticket)
    {
        if (ticket == null)
            return BadRequest(new { message = "Ticket data is required." });

        _ticketService.AddTicket(ticket);
        return CreatedAtAction(nameof(GetTicketById), new { id = ticket.TicketId }, ticket);
    }

    /// <summary>
    /// Update an existing ticket
    /// </summary>
    [Authorize(Roles="Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateTicket(string id, [FromBody] Ticket ticket)
    {
        if (ticket == null)
            return BadRequest(new { message = "Ticket data is required." });

        var updated = _ticketService.UpdateTicketByStringId(id, ticket);
        if (!updated)
            return NotFound(new { message = $"Ticket with ID {id} not found." });

        return Ok(new { message = "Ticket updated successfully." });
    }

    /// <summary>
    /// Delete a ticket
    /// </summary>
    [Authorize(Roles="Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteTicket(string id)
    {
        var deleted = _ticketService.DeleteTicketByStringId(id);
        if (!deleted)
            return NotFound(new { message = $"Ticket with ID {id} not found." });

        return Ok(new { message = "Ticket deleted successfully." });
    }
}
