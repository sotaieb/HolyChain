using System.Collections.ObjectModel;

namespace St.HolyChain.Core.Abstractions;

public interface IDataCollection 
{
    TItem? Get<TItem>(string key);

    void Set<TItem>(string key, TItem instance);

    bool ContainsKey(string key);

    ReadOnlyDictionary<string, object> Data { get; }
}