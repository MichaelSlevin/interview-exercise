using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using OpenMoney.InterviewExercise.Models;
using OpenMoney.InterviewExercise.QuoteClients;
using OpenMoney.InterviewExercise.ThirdParties;
using Xunit;

namespace OpenMoney.InterviewExercise.Tests
{
    public class MortgageQuoteClientFixture
    {
        private readonly Mock<IThirdPartyMortgageApi> _apiMock = new();

        [Fact]
        public async Task GetQuote_ShouldReturnNull_IfHouseValue_Over10Mill()
        {
            const decimal deposit = 1_100_000;
            const decimal houseValue = 10_000_001;
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.Null(quote);
        }

        [Fact]
        public async Task GetQuote_ShouldReturnAQuote_IfHouseValue_Equal10Mill()
        {
            const decimal deposit = 1_000_000;
            const decimal houseValue = 10_000_00;


            _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyMortgageRequest>()))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyMortgageResponse { MonthlyPayment = 300m }
                });
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.Equal(300m, (decimal)quote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuote_ShouldReturn_AQuote_IfHouseValue_Under10Mill()
        {
            const decimal deposit = 1_000_000;
            const decimal houseValue = 9_999_999;

            _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyMortgageRequest>()))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyMortgageResponse { MonthlyPayment = 300m }
                });
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.Equal(300m, (decimal)quote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuote_ShouldCall_Api_WithCorrectMortgageValue()
        {
            const decimal deposit = 10_000;
            const decimal houseValue = 100_000;
            var expected = (decimal)(Math.Round(houseValue - deposit, 2));

            _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyMortgageRequest>()))
                .ReturnsAsync(new[]
                {
                    new ThirdPartyMortgageResponse { MonthlyPayment = 300m }
                });
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            _apiMock.Verify((x)=> x.GetQuotes(It.Is<ThirdPartyMortgageRequest>(req => req.MortgageAmount == expected)));
        }

        [Fact]
        public async Task GetQuote_ShouldReturn_theCheapestAvailableQuote()
        {
            const decimal deposit = 10_000;
            const decimal houseValue = 100_000;

            var cheapMortgage = new ThirdPartyMortgageResponse { MonthlyPayment = 299m };
            var expensiveMortgage = new ThirdPartyMortgageResponse { MonthlyPayment = 300m };

            var responses = new List<ThirdPartyMortgageResponse> {
                cheapMortgage,
                expensiveMortgage
            };

            _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyMortgageRequest>()))
                .ReturnsAsync(responses);
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.Equal(cheapMortgage.MonthlyPayment, quote.MonthlyPayment);
        }
    }
}