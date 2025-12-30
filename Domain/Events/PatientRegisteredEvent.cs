using Domain.Models;

namespace Domain.Events
{
    public record PatientRegisteredEvent(Patient Patient, DateTime RegisteredAt);
}