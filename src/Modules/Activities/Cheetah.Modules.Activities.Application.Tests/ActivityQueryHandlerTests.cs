using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Activities.Application.Activities;
using Cheetah.Modules.Activities.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Activities.Application.Tests;

public class ActivityQueryHandlerTests
{
    private readonly Mock<IRepository<TestActivity, Guid>> _repo = new();

    [Fact]
    public async Task GetById_Found_ProjectsDto()
    {
        var activity = TestData.NewActivity();
        _repo.Setup(r => r.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        var handler = new GetActivityByIdQueryHandler<TestActivity, TestActivityDto>(
            _repo.Object, new TestActivityProjector());

        var dto = await handler.HandleAsync(new GetActivityByIdQuery<TestActivityDto>(activity.Id));

        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(activity.Id);
        dto.Title.ShouldBe(activity.Title);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestActivity?)null);
        var handler = new GetActivityByIdQueryHandler<TestActivity, TestActivityDto>(
            _repo.Object, new TestActivityProjector());

        var dto = await handler.HandleAsync(new GetActivityByIdQuery<TestActivityDto>(Guid.NewGuid()));

        dto.ShouldBeNull();
    }

    [Fact]
    public async Task List_AppliesSpec_AndProjects()
    {
        var items = new List<TestActivity> { TestData.NewActivity(), TestData.NewActivity() };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TestActivity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        var handler = new ListActivitiesQueryHandler<TestActivity, TestActivityDto>(
            _repo.Object, new TestActivityProjector());

        var result = await handler.HandleAsync(
            new ListActivitiesQuery<TestActivityDto>(null, ActivityStatus.Open, EntityRefKeys.Deal, null, null));

        result.Count.ShouldBe(2);
        _repo.Verify(r => r.GetAllAsync(
            It.IsAny<ISpecification<TestActivity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
