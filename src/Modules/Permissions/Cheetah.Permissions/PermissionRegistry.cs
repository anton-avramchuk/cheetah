using System.Collections.Concurrent;
using System.Reflection;

namespace Cheetah.Permissions;

/// <summary>
/// In-memory реестр объявленных permissions. Регистрируется как Singleton.
/// Источники: атрибут [Permission] на типах/полях/свойствах сборок (ScanAssemblies),
/// ручная регистрация (Add), внешние модули в микросервисах (через POST /registry/sync).
/// </summary>
public sealed class PermissionRegistry
{
    private readonly ConcurrentDictionary<string, PermissionDescriptor> _items =
        new(StringComparer.Ordinal);

    public IReadOnlyCollection<PermissionDescriptor> All => _items.Values.ToArray();

    public bool Contains(string key) => _items.ContainsKey(key);

    public void Add(string key, string? description, string? module)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Permission key cannot be empty", nameof(key));
        _items[key] = new PermissionDescriptor(key, description ?? "", module ?? "");
    }

    public void Add(PermissionDescriptor descriptor)
    {
        if (descriptor is null) throw new ArgumentNullException(nameof(descriptor));
        _items[descriptor.Key] = descriptor;
    }

    /// <summary>
    /// Сканирует сборки на наличие <see cref="PermissionAttribute"/> и регистрирует найденное.
    /// Безопасно повторять.
    /// </summary>
    public void ScanAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var asm in assemblies)
        {
            var module = asm.GetName().Name ?? "";
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
            }

            foreach (var t in types)
            {
                ScanMember(t, module);
                foreach (var f in t.GetFields(BindingFlags.Static | BindingFlags.Public))
                    ScanMember(f, module);
                foreach (var p in t.GetProperties(BindingFlags.Static | BindingFlags.Public))
                    ScanMember(p, module);
            }
        }
    }

    private void ScanMember(MemberInfo member, string fallbackModule)
    {
        var attr = member.GetCustomAttribute<PermissionAttribute>();
        if (attr is not null)
            Add(attr.Key, attr.Description, attr.Module ?? fallbackModule);
    }
}
