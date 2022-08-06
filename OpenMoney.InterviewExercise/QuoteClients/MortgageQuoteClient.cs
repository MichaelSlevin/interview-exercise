using System;
using System.Linq;
using System.Threading.Tasks;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.Models.Quotes;
using OpenMoney.InterviewExercise.ThirdParties;

namespace OpenMoney.InterviewExercise.QuoteClients
{
    public interface IMortgageQuoteClient
    {
        Task<MortgageQuote> GetQuote(GetQuotesRequest getQuotesRequest);
    }

    public class MortgageQuoteClient : IMortgageQuoteClient
    {
        private readonly IThirdPartyMortgageApi _api;

        public MortgageQuoteClient(IThirdPartyMortgageApi api)
        {
            _api = api;
        }
        
        public async Task<MortgageQuote> GetQuote(GetQuotesRequest getQuotesRequest)
        {
            if (!LTVIsEligible(getQuotesRequest)) 
                return MortgageQuote.FailedMortgageQuote("Loan to value cannot be bigger than 90%");
         
            if (!(HouseValueIsEligible(getQuotesRequest)))
                return MortgageQuote.FailedMortgageQuote("Quotes cannot be provided for houses worth over £10 million");
            try 
            {
                var mortgageAmount = getQuotesRequest.HouseValue - getQuotesRequest.Deposit;
                var cheapestMonthlyPayment = await GetMonthlyPaymentForCheapestMortgage((decimal)mortgageAmount);
                return MortgageQuote.SuccessfulMortgageQuote(cheapestMonthlyPayment);
            } 
            catch (NullReferenceException e)
            {
                return MortgageQuote.FailedMortgageQuote("No quotes returned from third party");
            }
        }

        private bool HouseValueIsEligible(GetQuotesRequest getQuotesRequest)
        {   
            return getQuotesRequest.HouseValue <= 10_000_000;
        }

        private bool LTVIsEligible(GetQuotesRequest getQuotesRequest)
        {
            var loanAmount = getQuotesRequest.HouseValue - getQuotesRequest.Deposit;
            var ltv = loanAmount / getQuotesRequest.HouseValue;
            return ltv <= (decimal)0.9;
        }

        private async Task<decimal> GetMonthlyPaymentForCheapestMortgage(decimal mortgageAmount)
        {
            var request = new ThirdPartyMortgageRequest
            {
                MortgageAmount = mortgageAmount
            };

            var returnedQuotes = await _api.GetQuotes(request);

            return returnedQuotes
                .OrderBy(x=> x.MonthlyPayment)
                .FirstOrDefault()
                .MonthlyPayment;
        }

        
    }
}