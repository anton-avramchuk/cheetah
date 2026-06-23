using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.Grid;
using Cheetah.Core.Specification;
using Cheetah.Modules.Teams.Application.Exceptions;
using Cheetah.Modules.Teams.Application.Teams;
using Cheetah.Modules.Teams.DomainEvents;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Teams.Application.Tests;

public class TeamHandlerTests
{
    private readonly Mock<IRepository<TestTeam, Guid>> _repo = new();
    private readonly Mock<IGridRepository<TestTeam, Guid>> _grid = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly TestTeamFactory _factory = new();
    private readonly TestTeamProjector _projector = new();

    [Fact]
    public async Task Create_adds_saves_and_publishes()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TestTeam>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new CreateTeamCommandHandler<TestTeam, TestCreateTeamRequest>(_factory, _repo.Object, _eventBus.Object);

        var id = await handler.HandleAsync(new CreateTeamCommand<TestCreateTeamRequest>(TestData.CreateRequest("Alpha")));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TestTeam>(t => t.Name == "Alpha")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is TeamCreatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_with_duplicate_name_throws()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TestTeam>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateTeamCommandHandler<TestTeam, TestCreateTeamRequest>(_factory, _repo.Object, _eventBus.Object);

        await Should.ThrowAsync<TeamsValidationException>(() =>
            handler.HandleAsync(new CreateTeamCommand<TestCreateTeamRequest>(TestData.CreateRequest())).AsTask());
    }

    [Fact]
    public async Task Update_renames_team()
    {
        var team = TestData.NewTeam("Old");
        _repo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(team);
        var handler = new UpdateTeamCommandHandler<TestTeam, TestUpdateTeamRequest>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new UpdateTeamCommand<TestUpdateTeamRequest>(
            team.Id, new TestUpdateTeamRequest { Id = team.Id, Name = "New" }));

        team.Name.ShouldBe("New");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_missing_throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestTeam?)null);
        var handler = new DeactivateTeamCommandHandler<TestTeam>(_repo.Object, _eventBus.Object);

        await Should.ThrowAsync<TeamsValidationException>(() =>
            handler.HandleAsync(new DeactivateTeamCommand(Guid.NewGuid())).AsTask());
    }

    [Fact]
    public async Task Delete_removes_team()
    {
        var team = TestData.NewTeam();
        _repo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(team);
        var handler = new DeleteTeamCommandHandler<TestTeam>(_repo.Object);

        await handler.HandleAsync(new DeleteTeamCommand(team.Id));

        _repo.Verify(r => r.Delete(team), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMember_mutates_aggregate_and_publishes()
    {
        var team = TestData.NewTeam();
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestTeam>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        var handler = new AddTeamMemberCommandHandler<TestTeam>(_repo.Object, _eventBus.Object);
        var memberId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        await handler.HandleAsync(new AddTeamMemberCommand(team.Id, memberId, roleId));

        team.Members.ShouldHaveSingleItem().MemberId.ShouldBe(memberId);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is TeamMemberAddedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeMemberRole_for_missing_member_throws_validation()
    {
        var team = TestData.NewTeam();
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestTeam>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        var handler = new ChangeTeamMemberRoleCommandHandler<TestTeam>(_repo.Object, _eventBus.Object);

        await Should.ThrowAsync<TeamsValidationException>(() =>
            handler.HandleAsync(new ChangeTeamMemberRoleCommand(team.Id, Guid.NewGuid(), Guid.NewGuid())).AsTask());
    }

    [Fact]
    public async Task RemoveMember_mutates_aggregate()
    {
        var team = TestData.NewTeam();
        var memberId = Guid.NewGuid();
        team.AddMember(memberId, Guid.NewGuid());
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestTeam>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        var handler = new RemoveTeamMemberCommandHandler<TestTeam>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new RemoveTeamMemberCommand(team.Id, memberId));

        team.Members.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetById_projects_with_members()
    {
        var team = TestData.NewTeam();
        team.AddMember(Guid.NewGuid(), Guid.NewGuid());
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestTeam>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        var handler = new GetTeamByIdQueryHandler<TestTeam, TestTeamDto>(_repo.Object, _projector);

        var dto = await handler.HandleAsync(new GetTeamByIdQuery<TestTeamDto>(team.Id));

        dto.ShouldNotBeNull();
        dto!.Members.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task GetById_missing_returns_null()
    {
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestTeam>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestTeam?)null);
        var handler = new GetTeamByIdQueryHandler<TestTeam, TestTeamDto>(_repo.Object, _projector);

        var dto = await handler.HandleAsync(new GetTeamByIdQuery<TestTeamDto>(Guid.NewGuid()));

        dto.ShouldBeNull();
    }

    [Fact]
    public async Task Grid_delegates_to_grid_repository()
    {
        _grid.Setup(r => r.GetGridAsync<TestTeamGridViewModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<TestTeamGridViewModel>(
                new[] { new TestTeamGridViewModel { Id = Guid.NewGuid(), Name = "Alpha", IsActive = true } }, 1));
        var handler = new GetTeamsGridQueryHandler<TestTeam, TestTeamGridViewModel>(_grid.Object);

        var result = await handler.HandleAsync(
            new GetTeamsGridQuery<TestTeamGridViewModel>(1, 10, new List<SortDescriptor>(), null));

        result.Total.ShouldBe(1);
    }
}
