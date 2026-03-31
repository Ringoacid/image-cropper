using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ImageCropper.Behaviors;

// ListViewの複数選択をViewModelのコレクションとバインドするためのビヘイビア
public class MultiSelectionBehavior
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(MultiSelectionBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsChanged));

    public static IList GetSelectedItems(DependencyObject obj)
    {
        return (IList)obj.GetValue(SelectedItemsProperty);
    }

    public static void SetSelectedItems(DependencyObject obj, IList value)
    {
        obj.SetValue(SelectedItemsProperty, value);
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListView listView) return;

        // イベントハンドラの重複登録を避けるために一度解除
        listView.SelectionChanged -= OnListViewSelectionChanged;

        // 新しいコレクションをビューに反映
        if (e.NewValue is IList newSelectedItems)
        {
            listView.SelectedItems.Clear();
            foreach (var item in newSelectedItems)
            {
                listView.SelectedItems.Add(item);
            }
        }

        // イベントハンドラを登録
        listView.SelectionChanged += OnListViewSelectionChanged;
    }

    private static void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView) return;

        // Viewの変更をViewModelに通知
        IList viewModelSelectedItems = GetSelectedItems(listView);
        if (viewModelSelectedItems != null)
        {
            // ListViewのSelectedItemsを元にViewModelのコレクションを更新
            viewModelSelectedItems.Clear();
            foreach (var item in listView.SelectedItems)
            {
                viewModelSelectedItems.Add(item);
            }
        }
    }
}