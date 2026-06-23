using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Grid;
using Cheetah.Core.Specification;
using Cheetah.Modules.Teams.Application.Exceptions;
using Cheetah.Modules.Teams.Application.Members;
using Cheetah.Modules.Teams.Application.Roles;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Teams.Application.Tests;

public class TeamRoleHandlerTests
{
    private readonly Mock<IRepository<TeamRole, Guid>> _repo = new();
    private readonly Mock<IGridRepository<TeamRole, Guid>> _grid = new();

    [Fact]
    public async Task Create_adds_and_saves()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TeamRole>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new CreateTeamRoleCommandHandler(_repo.Object);

        var id = await handler.HandleAsync(new CreateTeamRoleCommand("Lead"));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TeamRole>(x => x.Name == "Lead")), Times.Once);
    }

    [Fact]
    public async Task Create_duplicate_throws()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TeamRole>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateTeamRoleCommandHandler(_repo.Object);

        await Should.ThrowAsync<TeamsValidationException>(() =>
            handler.HandleAsync(new CreateTeamRoleCommand("Lead")).AsTask());
    }

    [Fact]
    public async Task Update_renames()
    {
        var role = TeamRole.Create("Old");
        _repo.Setup(r => r.GetByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        var handler = new UpdateTeamRoleCommandHandler(_repo.Object);

        await handler.HandleAsync(new UpdateTeamRoleCommand(role.Id, "New"));

        role.Name.ShouldBe("New");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_missing_throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamRole?)null);
        var handler = new DeleteTeamRoleCommandHandler(_repo.Object);

        await Should.ThrowAsync<TeamsValidationException>(() =>
            handler.HandleAsync(new DeleteTeamRoleCommand(Guid.NewGuid())).AsTask());
    }

    [Fact]
    public async Task GetById_projects_via_grid_repository()
    {
        var id = Guid.NewGuid();
        _grid.Setup(r => r.GetByIdAsync<TeamRoleDto>(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TeamRoleDto { Id = id, Name = "Lead" });
        var handler = new GetTeamRoleByIdQueryHandler(_grid.Object);

        var dto = await handler.HandleAsync(new GetTeamRoleByIdQuery(id));

        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(id);
    }

    [Fact]
    public async Task Grid_delegates_to_grid_repository()
    {
        _grid.Setup(r => r.GetGridAsync<TeamRoleDto>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<TeamRoleDto>(new[] { new TeamRoleDto { Id = Guid.NewGuid(), Name = "Lead" } }, 1));
        var handler = new GetTeamRolesGridQueryHandler(_grid.Object);

        var result = await handler.HandleAsync(new GetTeamRolesGridQuery(1, 10, new List<SortDescriptor>(), null));

        result.Total.ShouldBe(1);
    }
}

public class TeamMemberHandlerTests
{
    private readonly Mock<IRepository<TeamMember, Guid>> _repo = new();
    private readonly Mock<IGridRepository<TeamMember, Guid>> _grid = new();

    [Fact]
    public async Task Create_adds_and_saves()
    {
        var handler = new CreateTeamMemberCommandHandler(_repo.Object);
        var userId = Guid.NewGuid();

        var id = await handler.HandleAsync(new CreateTeamMemberCommand("John", userId));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TeamMember>(x => x.Name == "John" && x.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task Update_changes_name_and_user()
    {
        var member = TeamMember.Create("Old");
        _repo.Setup(r => r.GetByIdAsync(member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        var handler = new UpdateTeamMemberCommandHandler(_repo.Object);
        var userId = Guid.NewGuid();

        await handler.HandleAsync(new UpdateTeamMemberCommand(member.Id, "New", userId));

        member.Name.ShouldBe("New");
        member.UserId.ShouldBe(userId);
    }

    [Fact]
    public async Task Delete_missing_throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember?)null);
        var handler = new DeleteTeamMemberCommandHandler(_repo.Object);

        await Should.ThrowAsync<TeamsValidationException>(() =>
            handler.HandleAsync(new DeleteTeamMemberCommand(Guid.NewGuid())).AsTask());
    }

    [Fact]
    public async Task Grid_delegates_to_grid_repository()
    {
        _grid.Setup(r => r.GetGridAsync<TeamMemberDto>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<TeamMemberDto>(new[] { new TeamMemberDto { Id = Guid.NewGuid(), Name = "John" } }, 1));
        var handler = new GetTeamMembersGridQueryHandler(_grid.Object);

        var result = await handler.HandleAsync(new GetTeamMembersGridQuery(1, 10, new List<SortDescriptor>(), null));

        result.Total.ShouldBe(1);
    }
}
