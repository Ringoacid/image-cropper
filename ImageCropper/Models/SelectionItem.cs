namespace ImageCropper.Models;

public class SelectionItem<T> : IEquatable<SelectionItem<T>?> where T : IEquatable<T>
{
    public T Item { get; set; }
    public string DisplayName { get; set; }

    public SelectionItem(T item, string displayName)
    {
        Item = item;
        DisplayName = displayName;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SelectionItem<T>);
    }

    public bool Equals(SelectionItem<T>? other)
    {
        return other is not null &&
               EqualityComparer<T>.Default.Equals(Item, other.Item) &&
               DisplayName == other.DisplayName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Item, DisplayName);
    }

    public static bool operator ==(SelectionItem<T>? left, SelectionItem<T>? right)
    {
        return EqualityComparer<SelectionItem<T>>.Default.Equals(left, right);
    }

    public static bool operator !=(SelectionItem<T>? left, SelectionItem<T>? right)
    {
        return !(left == right);
    }
}
