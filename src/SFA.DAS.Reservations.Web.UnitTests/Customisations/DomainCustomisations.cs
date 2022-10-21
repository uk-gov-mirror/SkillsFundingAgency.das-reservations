using AutoFixture;
using AutoFixture.AutoMoq;

namespace SFA.DAS.Reservations.Web.UnitTests.Customisations
{
    public class DomainCustomisations : CompositeCustomization
    {
        public DomainCustomisations() : base(
            new AutoMoqCustomization { ConfigureMembers = true })
        {
        }
    }
}