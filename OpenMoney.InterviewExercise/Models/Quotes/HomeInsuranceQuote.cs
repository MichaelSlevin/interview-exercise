namespace OpenMoney.InterviewExercise.Models.Quotes
{
    public class HomeInsuranceQuote
    {
        public decimal? MonthlyPayment { get; set; }

        public bool Succeeded { get; set;}

        #nullable enable
        public string? FailureReason { get; set; }


        public static HomeInsuranceQuote FailedHomeInsuranceQuote(string reason)
        {
            return new HomeInsuranceQuote {
                Succeeded = false,
                FailureReason = reason
            };
        }

        public static HomeInsuranceQuote SuccessfulHomeInsuranceQuote(decimal monthlyPayment)
        {
            return new HomeInsuranceQuote {
                Succeeded = true,
                MonthlyPayment = monthlyPayment
            };
        }
    }
}