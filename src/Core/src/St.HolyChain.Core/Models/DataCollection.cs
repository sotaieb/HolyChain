using System.Collections.ObjectModel;
using St.HolyChain.Core.Abstractions;

namespace St.HolyChain.Core.Models;

public class DataCollection : IDataCollection
{
    private readonly Dictionary<string, object> _data = new();
    public ReadOnlyDictionary<string, object> Data => new(_data);
    public TItem? Get<TItem>(string key)
    {
        if (_data.TryGetValue(key, out var value))
        {
            return (TItem)value;
        }

        return default;
    }

    public void Set<TItem>(string key, TItem instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        _data[key] = instance;
    }

    public bool ContainsKey(string key) => _data.ContainsKey(key);
}