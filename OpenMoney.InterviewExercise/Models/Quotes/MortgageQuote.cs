namespace OpenMoney.InterviewExercise.Models.Quotes
{
    public class MortgageQuote
    {
        public decimal? MonthlyPayment { get; set; }

        public bool Succeeded { get; set;}

        #nullable enable
        public string? FailureReason { get; set; }


        public static MortgageQuote FailedMortgageQuote(string reason)
        {
            return new MortgageQuote {
                Succeeded = false,
                FailureReason = reason
            };
        }

        public static MortgageQuote SuccessfulMortgageQuote(decimal monthlyPayment)
        {
            return new MortgageQuote {
                Succeeded = true,
                MonthlyPayment = monthlyPayment
            };
        }
    }
}