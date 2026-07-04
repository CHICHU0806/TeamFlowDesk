using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace TeamFlowDesk.Services.Ui;

public static class PageInteractionService
{
    public static void RunKeepingScrollPosition(Page page, Action action)
    {
        var scrollViewer = FindFirstDescendant<ScrollViewer>(page);
        var verticalOffset = scrollViewer?.VerticalOffset ?? 0;

        try
        {
            action();
        }
        finally
        {
            if (scrollViewer is not null)
            {
                page.DispatcherQueue.TryEnqueue(() =>
                {
                    scrollViewer.ChangeView(
                        null,
                        verticalOffset,
                        null,
                        disableAnimation: true);

                    page.DispatcherQueue.TryEnqueue(() =>
                    {
                        scrollViewer.ChangeView(
                            null,
                            verticalOffset,
                            null,
                            disableAnimation: true);
                    });
                });
            }
        }
    }

    public static TItem? GetItemFromButton<TItem>(
        object sender,
        IEnumerable<TItem> source,
        Func<TItem, int> idSelector)
    {
        if (sender is not Button button || button.Tag is null)
        {
            return default;
        }

        if (!int.TryParse(button.Tag.ToString(), out var id))
        {
            return default;
        }

        return source.FirstOrDefault(item => idSelector(item) == id);
    }

    public static void ReplaceItem<TItem>(
        ObservableCollection<TItem> source,
        TItem oldItem,
        TItem newItem)
    {
        var index = source.IndexOf(oldItem);

        if (index >= 0)
        {
            source[index] = newItem;
        }
    }

    public static string GetComboBoxText(ComboBox comboBox, string fallback)
    {
        if (comboBox.SelectedItem is ComboBoxItem selectedItem &&
            selectedItem.Content is not null)
        {
            return selectedItem.Content.ToString() ?? fallback;
        }

        return fallback;
    }

    public static async Task ShowDetailDialogAsync(
        Page page,
        string title,
        IEnumerable<DetailSection> sections)
    {
        var detailPanel = new StackPanel
        {
            Spacing = 16,
            MaxWidth = 940
        };

        foreach (var section in sections)
        {
            detailPanel.Children.Add(CreateDetailBlock(section.Title, section.Content));
        }

        var scrollViewer = new ScrollViewer
        {
            Content = detailPanel,
            MaxHeight = 700,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var dialog = new ContentDialog
        {
            Title = title,
            Content = scrollViewer,
            CloseButtonText = "关闭",
            XamlRoot = page.XamlRoot
        };

        await dialog.ShowAsync();
    }

    private static StackPanel CreateDetailBlock(string title, string content)
    {
        var panel = new StackPanel
        {
            Spacing = 6
        };

        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });

        panel.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(content) ? "暂无内容" : content,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        });

        return panel;
    }

    private static T? FindFirstDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current is T match)
            {
                return match;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(current);

            for (var index = 0; index < childCount; index++)
            {
                queue.Enqueue(VisualTreeHelper.GetChild(current, index));
            }
        }

        return null;
    }
}