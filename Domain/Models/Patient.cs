using Domain.Models;
using Domain.ValueObjects;

public class Patient
{
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sex { get; set; }
    private string? _phoneNumber;
    public PhoneNumber? PhoneNumber
    {
        get => _phoneNumber is null ? null : new PhoneNumber(_phoneNumber);
        set => _phoneNumber = value?.Value;
    }
    public string? BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? Address { get; set; }
    public string? ShortNote { get; set; }
    public DateTime DateOfBirth { get; set; }
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}