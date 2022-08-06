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
            {
                return null;
            }
            if (!(HouseValueIsEligible(getQuotesRequest)))
            {
                return new MortgageQuote {
                    Succeeded = false,
                    FailureReason = "Quotes cannot be provided for houses worth over £10 million"
                };
            }
            try 
            {
                var mortgageAmount = getQuotesRequest.HouseValue - getQuotesRequest.Deposit;
                var cheapestMonthlyPayment = await GetMonthlyPaymentForCheapestMortgage((decimal)mortgageAmount);
                return new MortgageQuote
                {
                    MonthlyPayment = cheapestMonthlyPayment
                };
            } 
            catch (NullReferenceException e)
            {
                return null;
            }
        }

        private bool HouseValueIsEligible(GetQuotesRequest getQuotesRequest)
        {   
            return getQuotesRequest.HouseValue <= 10_000_000;
        }

        private bool LTVIsEligible(GetQuotesRequest getQuotesRequest)
        {
            return (getQuotesRequest.Deposit / getQuotesRequest.HouseValue) >= (decimal)0.1;
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