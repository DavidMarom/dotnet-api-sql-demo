namespace MyServer.Models;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Contact
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<Company> Companies { get; set; } = [];
}

public class ContactRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
