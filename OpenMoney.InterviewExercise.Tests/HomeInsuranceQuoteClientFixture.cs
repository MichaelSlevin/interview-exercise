using System.Threading.Tasks;
using Moq;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.QuoteClients;
using OpenMoney.InterviewExercise.ThirdParties;
using Xunit;

namespace OpenMoney.InterviewExercise.Tests
{
    public class HomeInsuranceQuoteClientFixture
    {
        private readonly Mock<IThirdPartyHomeInsuranceApi> _apiMock = new();

        [Fact]
        public async Task GetQuote_ShouldReturnNull_IfHouseValue_Over10Mill()
        {
            const decimal houseValue = 10_000_001;
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.Null(quote);
        }

        [Fact]
        public async Task GetQuote_ShouldReturnAQuote_IfHouseValue_EqualTo10Mill()
        {
            const decimal houseValue = 10_000_000;
            
             _apiMock
                .Setup(api => api.GetQuotes(It.Is<ThirdPartyHomeInsuranceRequest>(r =>
                    r.ContentsValue == 50_000 && r.HouseValue == (decimal) houseValue)))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 30 }
                });
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.Equal(30m, (decimal)quote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuote_ShouldReturn_AQuote()
        {
            const decimal houseValue = 100_000;

            _apiMock
                .Setup(api => api.GetQuotes(It.Is<ThirdPartyHomeInsuranceRequest>(r =>
                    r.ContentsValue == 50_000 && r.HouseValue == (decimal) houseValue)))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 30 }
                });
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.Equal(30m, (decimal)quote.MonthlyPayment);
        }
    }
}