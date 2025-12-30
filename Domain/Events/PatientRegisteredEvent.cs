using Domain.Entities;
using Domain.Models;
using System;

namespace Domain.Events
{
    public record PatientRegisteredEvent(Patient Patient, DateTime RegisteredAt);
}