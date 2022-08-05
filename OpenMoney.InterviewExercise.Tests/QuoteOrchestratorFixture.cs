using System.Threading.Tasks;
using Moq;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.Models.Quotes;
using OpenMoney.InterviewExercise.QuoteClients;
using Xunit;

namespace OpenMoney.InterviewExercise.Tests
{
    public class QuoteOrchestratorFixture
    {
        private readonly Mock<IMortgageQuoteClient> _mortgageClientMock = new();
        private readonly Mock<IHomeInsuranceQuoteClient> _homeInsuranceClientMock = new();
        
        [Fact]
        public async Task GetQuotes_ShouldPassCorrectValuesToMortgageClient_AndReturnQuote()
        {
            var orchetrator = new QuoteOrchestrator(
                _homeInsuranceClientMock.Object,
                _mortgageClientMock.Object);

            const float deposit = 10_000;
            const float houseValue = 100_000;
            
            var request = new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            };

            _mortgageClientMock
                .Setup(m => m.GetQuote(request))
                .ReturnsAsync(new MortgageQuote
                {
                    MonthlyPayment = 700
                });
            
            var response = await orchetrator.GetQuotes(request);
            
            Assert.Equal(700, response.MortgageQuote.MonthlyPayment);
        }
        
        [Fact]
        public async Task GetQuotes_ShouldPassCorrectValuesToHomeInsuranceClient_AndReturnQuote()
        {
            var orchetrator = new QuoteOrchestrator(
                _homeInsuranceClientMock.Object,
                _mortgageClientMock.Object);

            const float deposit = 10_000;
            const float houseValue = 100_000;
            
            var request = new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            };

            _homeInsuranceClientMock
                .Setup(m => m.GetQuote(request))
                .Returns(new HomeInsuranceQuote
                {
                    MonthlyPayment = 600
                });
            
            var response = await orchetrator.GetQuotes(request);
            
            Assert.Equal(600, response.HomeInsuranceQuote.MonthlyPayment);
        }
    }
}