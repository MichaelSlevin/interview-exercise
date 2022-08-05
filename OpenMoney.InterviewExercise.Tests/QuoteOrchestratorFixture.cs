using System;
using System.Diagnostics;
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

            const decimal deposit = 10_000;
            const decimal houseValue = 100_000;
            
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

            const decimal deposit = 10_000;
            const decimal houseValue = 100_000;
            
            var request = new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            };

            _homeInsuranceClientMock
                .Setup(m => m.GetQuote(request))
                .ReturnsAsync(new HomeInsuranceQuote
                {
                    MonthlyPayment = 600
                });
            
            var response = await orchetrator.GetQuotes(request);
            
            Assert.Equal(600, response.HomeInsuranceQuote.MonthlyPayment);
        }

        [Fact]
        public async Task GetQuotes_Perfomance_Testing()
        {
            var sw = new Stopwatch();
            var orchetrator = new QuoteOrchestrator(
                _homeInsuranceClientMock.Object,
                _mortgageClientMock.Object);

            const decimal deposit = 10_000;
            const decimal houseValue = 100_000;
            
            var request = new GetQuotesRequest
            {
                Deposit = deposit,
                HouseValue = houseValue
            };

            _homeInsuranceClientMock
                .Setup(m => m.GetQuote(request))
                .ReturnsAsync(new HomeInsuranceQuote
                {
                    MonthlyPayment = 600
                }, new TimeSpan( 0, 0, 5));

            _mortgageClientMock
                .Setup(m => m.GetQuote(request))
                .ReturnsAsync(new MortgageQuote
                {
                    MonthlyPayment = 700
                }, new TimeSpan( 0, 0, 6));
            
            sw.Start();

            var response = await orchetrator.GetQuotes(request);

            sw.Stop();
            Console.WriteLine($"The number of miliseconds it took to complete this method was: ${sw.ElapsedMilliseconds}");
            sw.Reset();
            

        }
    }
}