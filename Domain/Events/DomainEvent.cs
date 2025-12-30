using System;

namespace Domain.Events
{
    public abstract record DomainEvent(DateTime OccurredOn);
}