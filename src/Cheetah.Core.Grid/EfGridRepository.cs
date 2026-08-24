using System.Linq.Expressions;
using System.Reflection;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Mapping.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Grid;

public class EfGridRepository<TDbContext, TEntity, TKey> : EfRepository<TDbContext, TEntity, TKey>, IGridRepository<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : Entity<TKey>
{
    private static readonly MethodInfo OrderByMethod = typeof(Queryable)
        .GetMethods()
        .First(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Length == 2);

    private static readonly MethodInfo OrderByDescendingMethod = typeof(Queryable)
        .GetMethods()
        .First(m => m.Name == nameof(Queryable.OrderByDescending) && m.GetParameters().Length == 2);

    private static readonly MethodInfo ThenByMethod = typeof(Queryable)
        .GetMethods()
        .First(m => m.Name == nameof(Queryable.ThenBy) && m.GetParameters().Length == 2);

    private static readonly MethodInfo ThenByDescendingMethod = typeof(Queryable)
        .GetMethods()
        .First(m => m.Name == nameof(Queryable.ThenByDescending) && m.GetParameters().Length == 2);

    private readonly IObjectMapper _mapper;
    private readonly ILogger _logger;

    public EfGridRepository(TDbContext dbContext, IObjectMapper mapper, ILogger logger)
        : base(dbContext)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async ValueTask<GridResult<TViewModel>> GetGridAsync<TViewModel>(
        GridRequest request,
        CancellationToken ct = default)
        where TViewModel : class
    {
        var queryable = AsNoTrackingQueryable();

        // Imена полей фильтра/сортировки приходят из ViewModel (например, "patientName"),
        // но Where/OrderBy должны строиться по сущности (TEntity) — после ProjectTo EF
        // не может транслировать выражения. Достаём проекционную лямбду Mapster и
        // используем её для разворачивания имён ViewModel в выражения по TEntity.
        var projection = GetProjectionLambda<TViewModel>();

        // 1. Apply filtering
        if (request.Filter is not null)
        {
            var filterExpression = GridFilterExpressions.Build<TEntity>(request.Filter, projection, _logger);
            if (filterExpression is not null)
                queryable = queryable.Where(filterExpression);
        }

        // 2. Get total count (after filtering, before pagination)
        var total = await queryable.CountAsync(ct);

        // 3. Apply sorting. Пагинация без ORDER BY недетерминирована, а метки времени
        // одного SaveChanges совпадают у всей пачки — поэтому порядок всегда завершается Id.
        queryable = ApplySorting(queryable, request.Sort, projection);

        // 4. Apply pagination. Размер страницы нормализуется: клиент не должен уметь
        // выгрузить таблицу целиком (pageSize=0 или заведомо огромное значение).
        var pageSize = NormalizePageSize(request.PageSize);
        var page = Math.Max(1, request.Page);
        queryable = queryable.Skip((page - 1) * pageSize).Take(pageSize);

        // 5. Project to ViewModel and execute
        var projectedQuery = _mapper.ProjectTo<TViewModel>(queryable);
        var data = await projectedQuery.ToListAsync(ct);

        return new GridResult<TViewModel>
        {
            Data = data,
            Total = total
        };
    }

    private LambdaExpression? GetProjectionLambda<TViewModel>()
    {
        try
        {
            var empty = Array.Empty<TEntity>().AsQueryable();
            var projected = _mapper.ProjectTo<TViewModel>(empty);
            if (projected.Expression is MethodCallExpression { Arguments.Count: >= 2 } mc)
            {
                return mc.Arguments[1] switch
                {
                    UnaryExpression { Operand: LambdaExpression lambda } => lambda,
                    LambdaExpression lambda => lambda,
                    _ => null
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Grid: failed to extract projection expression for {Entity} -> {ViewModel}",
                typeof(TEntity).Name, typeof(TViewModel).Name);
        }
        return null;
    }

    public async ValueTask<TViewModel?> GetByIdAsync<TViewModel>(TKey id, CancellationToken ct = default)
    {
        var query = AsNoTrackingQueryable().Where(x=>x.Id != null && x.Id.Equals(id));

        return await _mapper.ProjectTo<TViewModel>(query).FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Приводит размер страницы к <c>[1, <see cref="GridRequest.MaxPageSize"/>]</c>:
    /// неположительный — к значению по умолчанию, слишком большой — к максимуму.
    /// </summary>
    internal static int NormalizePageSize(int pageSize) => pageSize <= 0
        ? GridRequest.DefaultPageSize
        : Math.Min(pageSize, GridRequest.MaxPageSize);

    private static IQueryable<TEntity> ApplySorting(
        IQueryable<TEntity> queryable,
        List<SortDescriptor> sortDescriptors,
        LambdaExpression? projection)
    {
        var isFirst = true;

        foreach (var sort in sortDescriptors)
        {
            if (string.IsNullOrEmpty(sort.Field))
                continue;

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = GridFilterExpressions.Property(parameter, sort.Field, projection);

            if (property is null)
                continue;

            var lambda = Expression.Lambda(property, parameter);
            var isDescending = string.Equals(sort.Dir, "desc", StringComparison.OrdinalIgnoreCase);

            var method = isFirst
                ? (isDescending ? OrderByDescendingMethod : OrderByMethod)
                : (isDescending ? ThenByDescendingMethod : ThenByMethod);

            var genericMethod = method.MakeGenericMethod(typeof(TEntity), property.Type);

            queryable = (IQueryable<TEntity>)genericMethod.Invoke(null, [queryable, lambda])!;
            isFirst = false;
        }

        // Тай-брейкер: без него строки с одинаковым значением сортировки распределяются
        // по страницам произвольно — постраничный обход дублирует и теряет записи.
        return isFirst
            ? queryable.OrderBy(e => e.Id)
            : ((IOrderedQueryable<TEntity>)queryable).ThenBy(e => e.Id);
    }
}

public class EfGridRepository<TDbContext, TEntity> : EfGridRepository<TDbContext, TEntity, Guid>, IGridRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : Entity<Guid>
{
    public EfGridRepository(TDbContext dbContext, IObjectMapper mapper, ILogger logger)
        : base(dbContext, mapper, logger)
    {
    }
}
