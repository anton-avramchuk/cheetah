using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Catalog.Application.Exceptions;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Application.Categories;

// ── Создание ─────────────────────────────────────────────────────────────────────────────────

/// <summary>Создать категорию (опционально дочернюю к существующей).</summary>
public sealed record CreateCategoryCommand(string Name, Guid? ParentId, int Order = 0) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCategoryCommand, Guid>))]
public sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly IRepository<ProductCategory, Guid> _repository;

    public CreateCategoryCommandHandler(IRepository<ProductCategory, Guid> repository)
        => _repository = repository;

    public async ValueTask<Guid> HandleAsync(CreateCategoryCommand command, CancellationToken ct = default)
    {
        ProductCategory? parent = null;
        if (command.ParentId is { } parentId)
        {
            parent = await _repository.GetByIdAsync(parentId, ct)
                ?? throw new CatalogValidationException($"Parent category '{parentId}' not found");
        }

        var category = ProductCategory.Create(command.Name, parent, command.Order);
        _repository.Add(category);
        await _repository.SaveChangesAsync(ct);
        return category.Id;
    }
}

// ── Обновление ───────────────────────────────────────────────────────────────────────────────

/// <summary>Переименовать категорию и/или изменить порядок.</summary>
public sealed record UpdateCategoryCommand(Guid Id, string Name, int Order) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCategoryCommand>))]
public sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly IRepository<ProductCategory, Guid> _repository;

    public UpdateCategoryCommandHandler(IRepository<ProductCategory, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(UpdateCategoryCommand command, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CatalogValidationException($"Category '{command.Id}' not found");

        category.Rename(command.Name);
        category.Reorder(command.Order);
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Удаление ─────────────────────────────────────────────────────────────────────────────────

/// <summary>Удалить категорию.</summary>
public sealed record DeleteCategoryCommand(Guid Id) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteCategoryCommand>))]
public sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly IRepository<ProductCategory, Guid> _repository;

    public DeleteCategoryCommandHandler(IRepository<ProductCategory, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(DeleteCategoryCommand command, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CatalogValidationException($"Category '{command.Id}' not found");

        _repository.Delete(category);
        await _repository.SaveChangesAsync(ct);
    }
}
