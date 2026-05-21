using System.Collections;
using System.Collections.Specialized;
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

    private static readonly DependencyProperty HelperProperty =
        DependencyProperty.RegisterAttached(
            "Helper",
            typeof(MultiSelectionHelper),
            typeof(MultiSelectionBehavior),
            new PropertyMetadata(null));

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListView listView) return;

        var helper = (MultiSelectionHelper)listView.GetValue(HelperProperty);
        if (helper == null)
        {
            helper = new MultiSelectionHelper(listView);
            listView.SetValue(HelperProperty, helper);
        }

        if (e.NewValue == null)
        {
            helper.Dispose();
            listView.ClearValue(HelperProperty);
        }
        else
        {
            helper.BindCollection(e.NewValue as IList);
        }
    }

    private class MultiSelectionHelper
    {
        private readonly ListView _listView;
        private INotifyCollectionChanged? _collection;
        private bool _isSyncing = false;

        public MultiSelectionHelper(ListView listView)
        {
            _listView = listView;
            _listView.SelectionChanged += OnListViewSelectionChanged;
            _listView.Unloaded += OnListViewUnloaded;
        }

        public void BindCollection(IList? collection)
        {
            if (_collection != null)
            {
                _collection.CollectionChanged -= OnCollectionChanged;
            }

            _collection = collection as INotifyCollectionChanged;

            if (_collection != null)
            {
                _collection.CollectionChanged += OnCollectionChanged;
            }

            SyncListViewToCollection(collection);
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SyncListViewToCollection(_collection as IList);
        }

        private void SyncListViewToCollection(IList? collection)
        {
            if (_isSyncing) return;
            _isSyncing = true;
            try
            {
                _listView.SelectedItems.Clear();
                if (collection != null)
                {
                    foreach (var item in collection)
                    {
                        _listView.SelectedItems.Add(item);
                    }
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSyncing) return;
            _isSyncing = true;
            try
            {
                IList viewModelSelectedItems = GetSelectedItems(_listView);
                if (viewModelSelectedItems != null)
                {
                    viewModelSelectedItems.Clear();
                    foreach (var item in _listView.SelectedItems)
                    {
                        viewModelSelectedItems.Add(item);
                    }
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void OnListViewUnloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            _listView.SelectionChanged -= OnListViewSelectionChanged;
            _listView.Unloaded -= OnListViewUnloaded;
            if (_collection != null)
            {
                _collection.CollectionChanged -= OnCollectionChanged;
                _collection = null;
            }
        }
    }
}