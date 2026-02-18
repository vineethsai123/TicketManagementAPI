namespace TicketManagementApi.Models;

public class Ticket
{
    public string? TicketId { get; set; }
    public string? Email { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Resolution { get; set; }
}
