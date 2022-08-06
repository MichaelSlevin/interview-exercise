using System.Linq;
using System.Threading.Tasks;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.Models.Quotes;
using OpenMoney.InterviewExercise.ThirdParties;

namespace OpenMoney.InterviewExercise.QuoteClients
{
    public interface IHomeInsuranceQuoteClient
    {
        Task<HomeInsuranceQuote> GetQuote(GetQuotesRequest getQuotesRequest);
    }

    public class HomeInsuranceQuoteClient : IHomeInsuranceQuoteClient
    {
        private IThirdPartyHomeInsuranceApi _api;
        
        private decimal _contentsValue = 50_000;

        public HomeInsuranceQuoteClient(IThirdPartyHomeInsuranceApi api)
        {
            _api = api;
        }

        public async Task<HomeInsuranceQuote> GetQuote(GetQuotesRequest getQuotesRequest)
        {
            if (!HouseValueIsEligible(getQuotesRequest))
            {
                return HomeInsuranceQuote.FailedHomeInsuranceQuote("Quotes cannot be provided for house worth over £10 million");
            }

            return HomeInsuranceQuote.SuccessfulHomeInsuranceQuote(
                await GetCheapestMonthlyPaymentForInsurance(getQuotesRequest)
            );
        }

        private bool HouseValueIsEligible(GetQuotesRequest getQuotesRequest) 
        {
            return getQuotesRequest.HouseValue <= (decimal)10_000_000;
        }

        private async Task<decimal> GetCheapestMonthlyPaymentForInsurance(GetQuotesRequest getQuotesRequest)
        {
            var request = new ThirdPartyHomeInsuranceRequest
            {
                HouseValue = (decimal) getQuotesRequest.HouseValue,
                ContentsValue = _contentsValue
            };

            var response = await _api.GetQuotes(request);

            return (decimal)response
                .OrderBy(x=> x.MonthlyPayment)
                .FirstOrDefault()
                .MonthlyPayment;
        }
    }
}