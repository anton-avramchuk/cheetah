using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Leads.Application.Leads;
using Cheetah.Modules.Leads.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Leads.Application.Tests;

public class LeadQueryHandlerTests
{
    private readonly Mock<IRepository<TestLead, Guid>> _repo = new();

    [Fact]
    public async Task GetById_Found_ProjectsDto()
    {
        var lead = TestData.NewLead();
        _repo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        var handler = new GetLeadByIdQueryHandler<TestLead, TestLeadDto>(_repo.Object, new TestLeadProjector());

        var dto = await handler.HandleAsync(new GetLeadByIdQuery<TestLeadDto>(lead.Id));

        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(lead.Id);
        dto.FullName.ShouldBe(lead.FullName);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestLead?)null);
        var handler = new GetLeadByIdQueryHandler<TestLead, TestLeadDto>(_repo.Object, new TestLeadProjector());

        (await handler.HandleAsync(new GetLeadByIdQuery<TestLeadDto>(Guid.NewGuid()))).ShouldBeNull();
    }

    [Fact]
    public async Task List_AppliesSpec_AndProjects()
    {
        var items = new List<TestLead> { TestData.NewLead(), TestData.NewLead() };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TestLead>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        var handler = new ListLeadsQueryHandler<TestLead, TestLeadDto>(_repo.Object, new TestLeadProjector());

        var result = await handler.HandleAsync(
            new ListLeadsQuery<TestLeadDto>(LeadWellKnownIds.StatusNew, LeadWellKnownIds.SourceWeb, null));

        result.Count.ShouldBe(2);
        _repo.Verify(r => r.GetAllAsync(
            It.IsAny<ISpecification<TestLead>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
