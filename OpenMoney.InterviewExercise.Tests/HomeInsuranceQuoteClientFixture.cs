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
        public async Task GetQuote_ShouldReturn_AFailedQuote_IfHouseValue_Over10Mill()
        {
            const decimal houseValue = 10_000_001;
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.False(quote.Succeeded);
            Assert.Equal("Quotes cannot be provided for house worth over £10 million", quote.FailureReason);
            Assert.Null(quote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuote_ShouldReturn_ASuccessfulQuote_IfHouseValue_EqualTo10Mill()
        {
            const decimal houseValue = 10_000_000;
            
             _apiMock
                .Setup(api => api.GetQuotes(It.Is<ThirdPartyHomeInsuranceRequest>(r =>
                    r.ContentsValue == 50_000 && r.HouseValue == houseValue)))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 30 }
                });
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.Equal(30m, quote.MonthlyPayment);
            Assert.True(quote.Succeeded);
        }

        [Fact]
        public async Task GetQuote_ShouldReturn_ASuccessfulQuote_WhenHouseValueUnder10Mill()
        {
            const decimal houseValue = 9_999_999;

            _apiMock
                .Setup(api => api.GetQuotes(It.Is<ThirdPartyHomeInsuranceRequest>(r =>
                    r.ContentsValue == 50_000 && r.HouseValue == houseValue)))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyHomeInsuranceResponse { MonthlyPayment = 30 }
                });
            
            var mortgageClient = new HomeInsuranceQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                HouseValue = houseValue
            });
            
            Assert.Equal(30m, quote.MonthlyPayment);
            Assert.True(quote.Succeeded);
        }
    }
}