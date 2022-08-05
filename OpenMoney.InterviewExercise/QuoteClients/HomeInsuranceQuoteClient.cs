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
        
        public decimal contentsValue = 50_000;

        public HomeInsuranceQuoteClient(IThirdPartyHomeInsuranceApi api)
        {
            _api = api;
        }

        public async Task<HomeInsuranceQuote> GetQuote(GetQuotesRequest getQuotesRequest)
        {
            // check if request is eligible
            if (getQuotesRequest.HouseValue > (decimal)10_000_000)
            {
                return null;
            }
            
            var request = new ThirdPartyHomeInsuranceRequest
            {
                HouseValue = (decimal) getQuotesRequest.HouseValue,
                ContentsValue = contentsValue
            };

            var response = await _api.GetQuotes(request);

            ThirdPartyHomeInsuranceResponse cheapestQuote = response.OrderBy(x=> x.MonthlyPayment).FirstOrDefault();
                       
            return new HomeInsuranceQuote
            {
                MonthlyPayment = (decimal) cheapestQuote.MonthlyPayment
            };
        }
    }
}