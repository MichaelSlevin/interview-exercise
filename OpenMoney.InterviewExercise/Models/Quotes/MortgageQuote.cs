namespace OpenMoney.InterviewExercise.Models.Quotes
{
    public class MortgageQuote
    {
        public decimal? MonthlyPayment { get; set; }

        public bool Succeeded { get; set;}

        #nullable enable
        public string? FailureReason { get; set; }
    }
}