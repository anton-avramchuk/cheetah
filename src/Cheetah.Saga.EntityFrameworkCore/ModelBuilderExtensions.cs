using Microsoft.EntityFrameworkCore;

namespace Cheetah.Saga.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Подключает таблицу SagaInstances к DbContext'у. Вызывается в OnModelCreating.
    /// </summary>
    public static ModelBuilder AddSagas(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SagaInstanceConfiguration());
        return modelBuilder;
    }
}
