using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;

namespace SFA.DAS.Reservations.Web.UnitTests.Filters
{
    public class WhenFilteringLevy
    {
        [Test, DomainAutoData]
        public async Task AndIsProvider_ThenContinuesToAction(
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionExecutingContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            LevyNotPermittedFilter filter)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Provider;
            
            //Act
           await filter.OnActionExecutionAsync(context, nextMethod.Object);

           //Assert
           nextMethod.Verify( x => x(), Times.Once);
           Assert.Null(context.Result);
        }

        [Test, DomainAutoData]
        public async Task AndIsANonLevyEmployer_ThenContinuesToAction(
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [ArrangeActionExecutingContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            LevyNotPermittedFilter filter)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            legalEntitiesResponse.AccountLegalEntities = legalEntities;
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            foreach (var legalEntity in legalEntitiesResponse.AccountLegalEntities) { legalEntity.IsLevy = false;}
            
            mockEncodingService
                .Setup(x => x.TryDecode(employerAccountId, EncodingType.AccountId, out decodedId))
                .Returns(true);
            
            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntitiesQuery>(y => y.AccountId == decodedId),It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);

            //Act
            await filter.OnActionExecutionAsync(context, nextMethod.Object);

            //Assert
            Assert.Null(context.Result);
            nextMethod.Verify(x => x(), Times.Once);
        }

        [Test, DomainAutoData]
        public async Task AndIsALevyEmployer_ThenRedirectsToAccessDeniedPage(
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [ArrangeActionExecutingContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            LevyNotPermittedFilter filter)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            legalEntitiesResponse.AccountLegalEntities = legalEntities;
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            foreach (var legalEntity in legalEntitiesResponse.AccountLegalEntities) { legalEntity.IsLevy = true; }
            mockEncodingService
                .Setup(x => x.TryDecode(employerAccountId, EncodingType.AccountId, out decodedId))
                .Returns(true);
            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntitiesQuery>(y => y.AccountId == decodedId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);

            //Act
            await filter.OnActionExecutionAsync(context, nextMethod.Object);

            //Assert
            Assert.NotNull(context.Result);
            Assert.True(context.Result is RedirectToRouteResult);
            Assert.AreEqual((context.Result as RedirectToRouteResult).RouteName, RouteNames.Error403);
            nextMethod.Verify(x => x(), Times.Never);
        }
    }
}
