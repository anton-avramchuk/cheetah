using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

/// <summary>
/// Базовый запрос создания роли. Identity — базовый модуль: реальное приложение
/// наследует этот тип своим конкретным record-ом (можно добавить дополнительные поля)
/// и передаёт его в <c>AddCrmIdentity(...).WithRoles&lt;...&gt;()</c>.
/// </summary>
public abstract record CreateRoleRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
