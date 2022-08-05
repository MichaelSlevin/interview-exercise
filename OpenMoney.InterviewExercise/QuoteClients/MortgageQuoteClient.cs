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
            
            var request = new ThirdPartyMortgageRequest
            {
                MortgageAmount = (decimal) mortgageAmount
            };
            var response = await _api.GetQuotes(request);

            var cheapestQuote = response.OrderBy(x=> x.MonthlyPayment).FirstOrDefault();

            return new MortgageQuote
            {
                MonthlyPayment = (float) cheapestQuote.MonthlyPayment
            };
        }

        private Boolean IsMortgageRequestIneligible(GetQuotesRequest getQuotesRequest)
        {   
            var loanToValueFraction = getQuotesRequest.Deposit / getQuotesRequest.HouseValue;
            return loanToValueFraction < 0.1d;
        }
    }
}