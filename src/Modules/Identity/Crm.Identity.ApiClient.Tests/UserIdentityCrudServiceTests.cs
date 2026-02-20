using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;
using Crm.Identity.Frontend.Models;
using Crm.Identity.Frontend.Services;
using Moq;
using Shouldly;
using Xunit;

namespace Crm.Identity.ApiClient.Tests;

public class UserIdentityCrudServiceTests
{
    private readonly Mock<IIdentityService> _serviceMock;
    private readonly UserIdentityCrudService _crudService;

    public UserIdentityCrudServiceTests()
    {
        _serviceMock = new Mock<IIdentityService>();
        _crudService = new UserIdentityCrudService(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedEntities()
    {
        // Arrange
        var entities = new List<UserIdentityViewModel>
        {
            new(Guid.NewGuid(), "Entity 1", "Description 1"),
            new(Guid.NewGuid(), "Entity 2", "Description 2")
        };
        var gridResult = new GridResult<UserIdentityViewModel>(entities, entities.Count);

        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<GetAllSampleEntitiesRequest?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gridResult);

        // Act
        var result = await _crudService.GetAllAsync(new GridRequest());

        // Assert
        result.Total.ShouldBe(2);
        result.Data.ShouldNotBeNull();
        result.Data.Count().ShouldBe(2);
        result.Data.First().Name.ShouldBe("Entity 1");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnFormModel()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new UserIdentityViewModel(id, "Test Entity", "Test Description");

        _serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _crudService.GetByIdAsync(id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(id);
        result.Name.ShouldBe("Test Entity");
        result.Description.ShouldBe("Test Description");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingEntity_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserIdentityViewModel?)null);

        // Act
        var result = await _crudService.GetByIdAsync(id);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCallServiceWithCorrectData()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var model = new UserIdentityFormModel { Name = "New Entity", Description = "Description" };

        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateUserIdentityRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _crudService.CreateAsync(model);

        // Assert
        result.ShouldBe(expectedId);
        _serviceMock.Verify(s => s.CreateAsync(
            It.Is<CreateUserIdentityRequest>(r => r.Name == "New Entity"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallServiceWithCorrectData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var model = new UserIdentityFormModel { Id = id, Name = "Updated", Description = "Updated Desc" };

        _serviceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateUserIdentityRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _crudService.UpdateAsync(id, model);

        // Assert
        _serviceMock.Verify(s => s.UpdateAsync(
            id,
            It.Is<UpdateUserIdentityRequest>(r => r.Name == "Updated"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallServiceDelete()
    {
        // Arrange
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _crudService.DeleteAsync(id);

        // Assert
        _serviceMock.Verify(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
