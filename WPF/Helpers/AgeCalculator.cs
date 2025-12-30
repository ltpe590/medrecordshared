using System;

namespace WPF.Helpers
{
    public static class AgeCalculator
    {
        public static int Calculate(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}