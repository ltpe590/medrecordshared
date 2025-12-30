namespace WPF.ViewModels
{
    public class PatientViewModel
    {
        public int PatientId { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }

        public string DisplayName => Name;
        public int Age => AgeCalculator.Calculate(DateOfBirth);
    }
}