public static class AgeCalculator
{
    public static int FromDateOfBirth(DateTime dateOfBirth, DateTime? asOf = null)
    {
        var today = (asOf ?? DateTime.UtcNow).Date;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age < 0 ? 0 : age;
    }

    public static DateOnly ToDateOfBirth(int age, DateTime? asOf = null)
    {
        if (age < 0) age = 0;
        return DateOnly.FromDateTime((asOf ?? DateTime.UtcNow).Date.AddYears(-age));
    }
}