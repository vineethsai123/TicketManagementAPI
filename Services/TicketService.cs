using TicketManagementApi.Models;

namespace TicketManagementApi.Services;

public interface ITicketService
{
    List<Ticket> GetAllTickets();
    Ticket? GetTicketById(int id);
    Ticket? GetTicketByStringId(string id);
    void AddTicket(Ticket ticket);
    bool UpdateTicket(int id, Ticket ticket);
    bool UpdateTicketByStringId(string id, Ticket ticket);
    bool DeleteTicket(int id);
    bool DeleteTicketByStringId(string id);
}

public class TicketService : ITicketService
{
    private List<Ticket> _tickets = new();

    public TicketService(string csvFilePath)
    {
        LoadTicketsFromCsv(csvFilePath);
    }

    public List<Ticket> GetAllTickets()
    {
        return _tickets;
    }

    public Ticket? GetTicketById(int id)
    {
        return _tickets.FirstOrDefault(t => t.TicketId == id.ToString());
    }

    public Ticket? GetTicketByStringId(string id)
    {
        return _tickets.FirstOrDefault(t => t.TicketId == id);
    }

    public void AddTicket(Ticket ticket)
    {
        _tickets.Add(ticket);
    }

    public bool UpdateTicket(int id, Ticket ticket)
    {
        var existingTicket = GetTicketById(id);
        if (existingTicket == null)
            return false;

        existingTicket.Email = ticket.Email;
        existingTicket.Type = ticket.Type;
        existingTicket.Description = ticket.Description;
        existingTicket.Status = ticket.Status;
        existingTicket.Resolution = ticket.Resolution;

        return true;
    }

    public bool UpdateTicketByStringId(string id, Ticket ticket)
    {
        var existingTicket = GetTicketByStringId(id);
        if (existingTicket == null)
            return false;

        existingTicket.Email = ticket.Email;
        existingTicket.Type = ticket.Type;
        existingTicket.Description = ticket.Description;
        existingTicket.Status = ticket.Status;
        existingTicket.Resolution = ticket.Resolution;

        return true;
    }

    public bool DeleteTicket(int id)
    {
        var ticket = GetTicketById(id);
        if (ticket == null)
            return false;

        _tickets.Remove(ticket);
        return true;
    }

    public bool DeleteTicketByStringId(string id)
    {
        var ticket = GetTicketByStringId(id);
        if (ticket == null)
            return false;

        _tickets.Remove(ticket);
        return true;
    }

    private void LoadTicketsFromCsv(string csvFilePath)
    {
        try
        {
            if (!File.Exists(csvFilePath))
            {
                Console.WriteLine($"CSV file not found at {csvFilePath}. Starting with empty ticket list.");
                return;
            }

            var lines = File.ReadAllLines(csvFilePath);
            
            if (lines.Length <= 1)
            {
                Console.WriteLine("CSV file is empty or contains only headers.");
                return;
            }

            // Parse header to get column indices (case-insensitive)
            var headerLine = lines[0];
            var headerValues = ParseCsvLine(headerLine);
            
            var ticketIdIndex = FindColumnIndex(headerValues, "ticket_id", "ticketid");
            var emailIndex = FindColumnIndex(headerValues, "email");
            var typeIndex = FindColumnIndex(headerValues, "type");
            var descriptionIndex = FindColumnIndex(headerValues, "description");
            var statusIndex = FindColumnIndex(headerValues, "status");
            var resolutionIndex = FindColumnIndex(headerValues, "resolution");

            Console.WriteLine($"CSV Columns found: TicketId={ticketIdIndex}, Email={emailIndex}, Type={typeIndex}, Description={descriptionIndex}, Status={statusIndex}, Resolution={resolutionIndex}");

            // Parse data rows
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    var line = lines[i].Trim();
                    
                    // Skip empty lines completely
                    if (string.IsNullOrWhiteSpace(line) || line == "\"\"")
                        continue;

                    var values = ParseCsvLine(line);
                    
                    // Skip if we don't have enough columns (malformed rows)
                    if (values.Length < 6)
                        continue;

                    // Skip if TicketId column is empty or if the first cell is just a quote
                    var ticketIdRaw = GetColumnValue(values, ticketIdIndex);
                    if (string.IsNullOrWhiteSpace(ticketIdRaw) || ticketIdRaw == "\"")
                        continue;

                    var ticket = new Ticket
                    {
                        TicketId = ticketIdRaw,
                        Email = GetColumnValue(values, emailIndex),
                        Type = GetColumnValue(values, typeIndex),
                        Description = GetColumnValue(values, descriptionIndex),
                        Status = GetColumnValue(values, statusIndex),
                        Resolution = GetColumnValue(values, resolutionIndex)
                    };

                    if (!string.IsNullOrEmpty(ticket.Email) && !string.IsNullOrEmpty(ticket.TicketId)) // Both email and ticket ID should be present
                    {
                        _tickets.Add(ticket);
                    }
                }
                catch (Exception ex)
                {
                    // Silently skip problematic rows
                }
            }

            Console.WriteLine($"✓ Successfully loaded {_tickets.Count} tickets from CSV file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error reading CSV file: {ex.Message}");
        }
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (c == ',' && !insideQuotes)
            {
                result.Add(current.ToString().Trim().Trim('"'));
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString().Trim().Trim('"'));
        return result.ToArray();
    }

    private int FindColumnIndex(string[] headers, params string[] columnNames)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var headerLower = headers[i].ToLower().Trim();
            foreach (var colName in columnNames)
            {
                if (headerLower == colName.ToLower())
                    return i;
            }
        }
        return -1;
    }

    private string GetColumnValue(string[] values, int index)
    {
        if (index < 0 || index >= values.Length)
            return "";
        return values[index].Trim();
    }

}
