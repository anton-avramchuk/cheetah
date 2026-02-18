using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using __Prefix__.ModuleName.ApiClient;
using __Prefix__.ModuleName.Contracts.Requests;
using __Prefix__.ModuleName.Contracts.Response;
using __Prefix__.ModuleName.Frontend.Models;
using __Prefix__.ModuleName.Frontend.Services;
using Moq;
using Shouldly;

namespace __Prefix__.ModuleName.ApiClient.Tests;

public class SampleEntityCrudServiceTests
{
    private readonly Mock<ISampleEntitiesService> _serviceMock;
    private readonly SampleEntityCrudService _crudService;

    public SampleEntityCrudServiceTests()
    {
        _serviceMock = new Mock<ISampleEntitiesService>();
        _crudService = new SampleEntityCrudService(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedEntities()
    {
        // Arrange
        var entities = new List<SampleEntityViewModel>
        {
            new(Guid.NewGuid(), "Entity 1", "Description 1"),
            new(Guid.NewGuid(), "Entity 2", "Description 2")
        };

        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _crudService.GetAllAsync(new GridRequest());

        // Assert
        result.Total.ShouldBe(2);
        result.Data.ShouldNotBeNull();
        result.Data.Count.ShouldBe(2);
        result.Data[0].Name.ShouldBe("Entity 1");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnFormModel()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new SampleEntityViewModel(id, "Test Entity", "Test Description");

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
            .ReturnsAsync((SampleEntityViewModel?)null);

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
        var model = new SampleEntityFormModel { Name = "New Entity", Description = "Description" };

        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateSampleEntityRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _crudService.CreateAsync(model);

        // Assert
        result.ShouldBe(expectedId);
        _serviceMock.Verify(s => s.CreateAsync(
            It.Is<CreateSampleEntityRequest>(r => r.Name == "New Entity"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallServiceWithCorrectData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var model = new SampleEntityFormModel { Id = id, Name = "Updated", Description = "Updated Desc" };

        _serviceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateSampleEntityRequest>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _crudService.UpdateAsync(id, model);

        // Assert
        _serviceMock.Verify(s => s.UpdateAsync(
            id,
            It.Is<UpdateSampleEntityRequest>(r => r.Name == "Updated"),
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
