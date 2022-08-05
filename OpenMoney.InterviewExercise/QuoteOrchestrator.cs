using System.Collections.Generic;
using System.Threading.Tasks;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.Models.Quotes;
using OpenMoney.InterviewExercise.QuoteClients;

namespace OpenMoney.InterviewExercise
{
    public class QuoteOrchestrator
    {
        private readonly IHomeInsuranceQuoteClient _homeInsuranceQuoteClient;
        private readonly IMortgageQuoteClient _mortgageQuoteClient;

        public QuoteOrchestrator(
            IHomeInsuranceQuoteClient homeInsuranceQuoteClient,
            IMortgageQuoteClient mortgageQuoteClient)
        {
            _homeInsuranceQuoteClient = homeInsuranceQuoteClient;
            _mortgageQuoteClient = mortgageQuoteClient;
        }

        public async Task<GetQuotesResponse> GetQuotes(GetQuotesRequest request)
        {
            var getMortgageQuote = _mortgageQuoteClient.GetQuote(request);
            var getHomeInsuranceQuote = _homeInsuranceQuoteClient.GetQuote(request);

            var tasks = new List<Task>{
                getMortgageQuote,
                getHomeInsuranceQuote
            };

            await Task.WhenAll(tasks);

            return new GetQuotesResponse
            {
                MortgageQuote = await getMortgageQuote,
                HomeInsuranceQuote = await getHomeInsuranceQuote
            };
        }
    }
}