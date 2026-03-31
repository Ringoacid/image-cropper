using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ImageCropper.Models;

/// <summary>
/// 汎用の HashSet をラップしたコレクションです。
/// WPF のデータバインディングで使いやすくするために、
/// <see cref="INotifyCollectionChanged"/> と <see cref="INotifyPropertyChanged"/> を実装しています。
/// </summary>
public class ObservableHashSet<T> : ISet<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly HashSet<T> _set;

    public ObservableHashSet()
    => _set = new HashSet<T>();

    public ObservableHashSet(IEqualityComparer<T>? comparer)
    => _set = new HashSet<T>(comparer);

    public ObservableHashSet(IEnumerable<T> collection)
    => _set = new HashSet<T>(collection);

    public ObservableHashSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer)
    => _set = new HashSet<T>(collection, comparer);

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Count => _set.Count;
    public bool IsReadOnly => ((ICollection<T>)_set).IsReadOnly;
    public IEqualityComparer<T> Comparer => _set.Comparer;

    public bool Add(T item)
    {
        if (_set.Add(item))
        {
            OnPropertyChanged(nameof(Count));
            // HashSet は個別のコレクション変更通知を提供しないため、
            // WPF 側で一覧を再描画できるよう Reset イベントを発行します。
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return true;
        }
        return false;
    }

    void ICollection<T>.Add(T item) => Add(item);

    public bool Remove(T item)
    {
        if (_set.Remove(item))
        {
            OnPropertyChanged(nameof(Count));
            // Remove（削除）時も同様に、個別の通知を送れないので Reset を発行します。
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return true;
        }
        return false;
    }

    public void Clear()
    {
        if (_set.Count == 0)
            return;

        _set.Clear();
        OnPropertyChanged(nameof(Count));
        // セットの内容をすべてクリアします。
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item) => _set.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ISet<T> の多くの操作は、個々の変更を細かく通知するのが難しいため、
    // 変更があった場合は Count を更新して Reset を発行します。

    public void ExceptWith(IEnumerable<T> other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        if (_set.Count == 0) return;

        int before = _set.Count;
        _set.ExceptWith(other);
        if (_set.Count != before)
        {
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        int before = _set.Count;
        _set.IntersectWith(other);
        if (_set.Count != before)
        {
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        int before = _set.Count;
        _set.SymmetricExceptWith(other);
        if (_set.Count != before)
        {
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public void UnionWith(IEnumerable<T> other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));

        // 他のコレクションの要素を追加する場合、個別の Add 通知の発行は容易でないため、
        // HashSet.Add を使って新規追加の有無を判定し、変更があれば Reset を発行します。
        bool anyAdded = false;
        foreach (var item in other)
        {
            if (_set.Add(item)) anyAdded = true;
        }

        if (anyAdded)
        {
            OnPropertyChanged(nameof(Count));
            // 他のコレクションから追加された場合も、個別通知は発行せず Reset を発行します。
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    => CollectionChanged?.Invoke(this, args);
}
