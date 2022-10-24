using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    public class WhenCallingPostCompleted
    {
        private IFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        }

        [Test, DomainAutoData]
        public void And_Has_Ukprn_And_ValidationError_Then_Return_Provider_Completed_View(
            CompletedViewModel model,
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            controller.ModelState.AddModelError("AddApprentice", "AddApprentice");

            var actual = controller.PostCompleted(routeModel, model);

            var actualModel = actual as ViewResult;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual(ViewNames.ProviderCompleted, actualModel.ViewName);
        }

        [Test, DomainAutoData]
        public void And_No_Ukprn_And_ValidationError_Then_Return_Employer_Completed_View(
            CompletedViewModel model,
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            controller.ModelState.AddModelError("AddApprentice", "AddApprentice");

            var actual = controller.PostCompleted(routeModel, model);

            var actualModel = actual as ViewResult;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual(ViewNames.EmployerCompleted, actualModel.ViewName);
        }
        
        [Test]
        [MoqInlineAutoData(CompletedReservationWhatsNext.RecruitAnApprentice)]
        [MoqInlineAutoData(CompletedReservationWhatsNext.AddAnApprentice)]
        [MoqInlineAutoData(CompletedReservationWhatsNext.Homepage)]
        [MoqInlineAutoData(CompletedReservationWhatsNext.FindApprenticeshipTraining)]
        public void And_Has_Ukprn_Then_The_Request_Is_Redirected_Based_On_The_Selection(
            CompletedReservationWhatsNext selection,
            CompletedViewModel model,
            ReservationsRouteModel routeModel,
            [Frozen] IOptions<ReservationsWebConfiguration> config,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelperMock,
            [NoAutoProperties] ReservationsController controller)
        {
            model.WhatsNext = selection;
            routeModel.EmployerAccountId = null;
            model.CohortRef = string.Empty;
            var providerRecruitUrl = _fixture.Create<string>();
            var addApprenticeUrl = _fixture.Create<string>();
            var homeUrl = _fixture.Create<string>();

            externalUrlHelperMock
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Id == routeModel.UkPrn.ToString() && 
                        parameters.SubDomain == "recruit")))
                .Returns(providerRecruitUrl);
            externalUrlHelperMock
                .Setup(helper => helper.GenerateAddApprenticeUrl(routeModel.Id.Value,
                    routeModel.AccountLegalEntityPublicHashedId, model.CourseId, model.UkPrn,
                    model.StartDate, "", routeModel.EmployerAccountId, 
                    false, string.Empty, string.Empty, model.JourneyData))
                .Returns(addApprenticeUrl);
            externalUrlHelperMock
                .Setup(helper => helper.GenerateDashboardUrl(null))
                .Returns(homeUrl);
            
            var actual = controller.PostCompleted(routeModel, model);

            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            
            switch (selection)
            {
                case (CompletedReservationWhatsNext.RecruitAnApprentice):
                    Assert.AreEqual(providerRecruitUrl,result.Url);
                    break;

                case (CompletedReservationWhatsNext.FindApprenticeshipTraining):
                    Assert.AreEqual(config.Value.FindApprenticeshipTrainingUrl,result.Url);
                    break;

                case (CompletedReservationWhatsNext.AddAnApprentice):
                    Assert.AreEqual(addApprenticeUrl, result.Url);
                    break;

                case (CompletedReservationWhatsNext.Homepage):
                    Assert.AreEqual(homeUrl,result.Url);
                    break;

                default: 
                    Assert.Fail();
                    break;
            }
        }

        [Test]
        [MoqInlineAutoData(CompletedReservationWhatsNext.RecruitAnApprentice)]
        [MoqInlineAutoData(CompletedReservationWhatsNext.AddAnApprentice)]
        [MoqInlineAutoData(CompletedReservationWhatsNext.Homepage)]
        [MoqInlineAutoData(CompletedReservationWhatsNext.FindApprenticeshipTraining)]
        public void And_No_Ukprn_Then_The_Request_Is_Redirected_Based_On_The_Selection(
            CompletedReservationWhatsNext selection,
            CompletedViewModel model,
            ReservationsRouteModel routeModel,
            [Frozen] IOptions<ReservationsWebConfiguration> config,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelperMock,
            [NoAutoProperties] ReservationsController controller)
        {
            model.WhatsNext = selection;
            routeModel.UkPrn = null;
            model.CohortRef = string.Empty;
            model.UkPrn = null;
            var employerRecruitUrl = _fixture.Create<string>();
            var addApprenticeUrl = _fixture.Create<string>();
            var homeUrl = _fixture.Create<string>();

            externalUrlHelperMock
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Id == routeModel.EmployerAccountId &&
                        parameters.SubDomain == "recruit" &&
                        parameters.Folder == "accounts")))
                .Returns(employerRecruitUrl);
            externalUrlHelperMock
                .Setup(helper => helper.GenerateAddApprenticeUrl(routeModel.Id.Value,
                    routeModel.AccountLegalEntityPublicHashedId, model.CourseId, model.UkPrn,
                    model.StartDate, "", routeModel.EmployerAccountId, 
                    false, string.Empty, string.Empty, model.JourneyData))
                .Returns(addApprenticeUrl);
            externalUrlHelperMock
                .Setup(helper => helper.GenerateDashboardUrl(routeModel.EmployerAccountId))
                .Returns(homeUrl);
            
            var actual = controller.PostCompleted(routeModel, model);

            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            
            switch (selection)
            {
                case (CompletedReservationWhatsNext.RecruitAnApprentice):
                    Assert.AreEqual(employerRecruitUrl,result.Url);
                    break;

                case (CompletedReservationWhatsNext.FindApprenticeshipTraining):
                    Assert.AreEqual(config.Value.FindApprenticeshipTrainingUrl,result.Url);
                    break;

                case (CompletedReservationWhatsNext.AddAnApprentice):
                    Assert.AreEqual(addApprenticeUrl, result.Url);
                    break;

                case (CompletedReservationWhatsNext.Homepage):
                    Assert.AreEqual(homeUrl,result.Url);
                    break;

                default: 
                    Assert.Fail();
                    break;
            }
        }

        [Test]
        [DomainAutoData]
        public void Then_When_There_Is_A_Cohort_Ref_The_Add_Apprentice_Link_Includes_The_Reference(
            CompletedViewModel model,
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelperMock,
            ReservationsController controller)
        {
            //Arrange
            model.WhatsNext = CompletedReservationWhatsNext.AddAnApprentice;
            var addApprenticeUrl = _fixture.Create<string>();
            externalUrlHelperMock
                .Setup(helper => helper.GenerateAddApprenticeUrl(routeModel.Id.Value,
                    routeModel.AccountLegalEntityPublicHashedId, model.CourseId, model.UkPrn,
                    model.StartDate, model.CohortRef, routeModel.EmployerAccountId, 
                    false, string.Empty, string.Empty, model.JourneyData))
                .Returns(addApprenticeUrl);
            
            //Act
            var actual = controller.PostCompleted(routeModel, model);
            
            //Assert
            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(addApprenticeUrl, result.Url);
        }


        [Test]
        [DomainAutoData]
        public void Then_When_There_Is_No_Cohort_Ref_But_ProviderId_The_Add_Apprentice_Link_Includes_The_Reference_For_Employer(
            CompletedViewModel model,
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelperMock,
            ReservationsController controller)
        {
            //Arrange
            model.WhatsNext = CompletedReservationWhatsNext.AddAnApprentice;
            model.CohortRef = string.Empty;
            routeModel.UkPrn = null;
            var addApprenticeUrl = _fixture.Create<string>();

            externalUrlHelperMock
                .Setup(helper => helper.GenerateAddApprenticeUrl(routeModel.Id.Value,
                    routeModel.AccountLegalEntityPublicHashedId, model.CourseId, model.UkPrn,
                    model.StartDate, model.CohortRef, routeModel.EmployerAccountId, 
                    true, string.Empty, string.Empty, model.JourneyData))
                .Returns(addApprenticeUrl);

            //Act
            var actual = controller.PostCompleted(routeModel, model);

            //Assert
            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(addApprenticeUrl, result.Url);
        }
    }
}
