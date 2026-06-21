namespace Cheetah.AspNetCore.Blazor.Dialogs;

public sealed class DialogFormContext
{
    private readonly Dictionary<string, bool> _fields = new();

    public event Action? OnChanged;

    public bool IsValid => _fields.Count == 0 || _fields.Values.All(v => v);

    public void SetField(string id, bool valid)
    {
        if (_fields.TryGetValue(id, out var current) && current == valid)
            return;
        _fields[id] = valid;
        OnChanged?.Invoke();
    }

    public void UnregisterField(string id)
    {
        if (_fields.Remove(id))
            OnChanged?.Invoke();
    }
}
