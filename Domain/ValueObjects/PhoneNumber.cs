using System;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public record PhoneNumber(string Value)
    {
        private static readonly Regex _phoneRegex = new(@"^\+?[0-9]{10,15}$");

        public bool IsValid => !string.IsNullOrWhiteSpace(Value) && _phoneRegex.IsMatch(Value);

        public override string ToString() => Value;
    }
}