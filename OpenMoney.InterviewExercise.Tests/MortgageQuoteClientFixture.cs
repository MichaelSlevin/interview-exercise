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
        public async Task GetQuote_Should_Return_AFailedQuote_IfHouseValue_Over10Mill()
        {
            const decimal deposit = 1_100_000;
            const decimal houseValue = 10_000_001;
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.False(quote.Succeeded);
            Assert.Equal("Quotes cannot be provided for houses worth over £10 million", quote.FailureReason);
            Assert.Null(quote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuote_Should_Return_ASuccessfulQuote_IfHouseValue_Equal10Mill()
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
            Assert.True(quote.Succeeded);
        }

        [Fact]
        public async Task GetQuote_Should_Return_ASuccessfulQuote_IfHouseValue_Under10Mill()
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
            Assert.True(quote.Succeeded);
        }

        [Fact]
        public async Task GetQuote_Should_Call_Api_WithCorrectMortgageValue()
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
        public async Task GetQuote_Should_Return_theCheapestAvailableQuote()
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
            Assert.True(quote.Succeeded);
        }

        [Fact]
        public async Task GetQuote_Should_Return_AFailedQuote_WhenLoanToValueIsBiggerThan_90percent()
        {
            const decimal houseValue = 100;
            const decimal deposit = (houseValue / 10) - 1;
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.False(quote.Succeeded);
            Assert.Equal("Loan to value cannot be bigger than 90%", quote.FailureReason);
            Assert.Null(quote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuote_Should_Return_ASuccessfulQuote_WhenLoanToValueIsEqualTo_90percent()
        {
            const decimal houseValue = 100;
            const decimal deposit = (houseValue / 10);

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
            Assert.True(quote.Succeeded);
        }

        [Fact]
        public async Task GetQuote_Should_Return_ASuccessfulQuote_WhenLoanToValueIsLessThan_90percent()
        {
            const decimal houseValue = 100;
            const decimal deposit = (houseValue / 10) + 1;

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
            
            Assert.Equal(300m, quote.MonthlyPayment);
            Assert.True(quote.Succeeded);
        }

        [Fact]
        public async Task GetQuote_Should_Return_AFailedQuote_WhenNoQuotesAreReturned()
        {

            const decimal houseValue = 100;
            const decimal deposit = (houseValue / 10) + 1;

             _apiMock
                .Setup(api => api.GetQuotes(It.IsAny<ThirdPartyMortgageRequest>()))
                .ReturnsAsync(new List<ThirdPartyMortgageResponse>());
            
            var mortgageClient = new MortgageQuoteClient(_apiMock.Object);
            var quote = await mortgageClient.GetQuote(new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            });
            
            Assert.False(quote.Succeeded);
            Assert.Equal("No quotes returned from third party", quote.FailureReason);
            Assert.Null(quote.MonthlyPayment);
        }
    }
}