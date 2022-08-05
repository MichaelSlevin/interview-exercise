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
            if (IsMortgageRequestIneligible(getQuotesRequest))
            {
                return null;
            }
            var mortgageAmount = getQuotesRequest.HouseValue - getQuotesRequest.Deposit;
            var cheapestMonthlyPayment = await GetMonthlyPaymentForCheapestMortgage((decimal)mortgageAmount);
            
            return new MortgageQuote
            {
                MonthlyPayment = cheapestMonthlyPayment
            };
        }

        private Boolean IsMortgageRequestIneligible(GetQuotesRequest getQuotesRequest)
        {   
            var loanToValueFraction = getQuotesRequest.Deposit / getQuotesRequest.HouseValue;
            return loanToValueFraction < (decimal)0.1;
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