using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using TeamFlowDesk.Pages;
using WinRT.Interop;

namespace TeamFlowDesk;

public sealed partial class MainWindow : Window
{
    private bool _isSplashStarted;

    public MainWindow()
    {
        InitializeComponent();
        SetWindowIcon();

        RootNavigationView.SelectedItem = RootNavigationView.MenuItems[0];
        NavigateToPage("HomePage");

        DispatcherQueue.TryEnqueue(async () => await PlaySplashAsync());
    }

    private async Task PlaySplashAsync()
    {
        if (_isSplashStarted)
        {
            return;
        }

        _isSplashStarted = true;

        await Task.Delay(120);

        var showStoryboard = new Storyboard();
        AddDoubleAnimation(
            showStoryboard,
            SplashContent,
            nameof(UIElement.Opacity),
            0,
            1,
            420,
            new CubicEase { EasingMode = EasingMode.EaseOut });
        AddDoubleAnimation(
            showStoryboard,
            SplashScaleTransform,
            nameof(ScaleTransform.ScaleX),
            0.86,
            1,
            420,
            new CubicEase { EasingMode = EasingMode.EaseOut });
        AddDoubleAnimation(
            showStoryboard,
            SplashScaleTransform,
            nameof(ScaleTransform.ScaleY),
            0.86,
            1,
            420,
            new CubicEase { EasingMode = EasingMode.EaseOut });

        await BeginStoryboardAsync(showStoryboard);
        await Task.Delay(350);

        var transitionStoryboard = new Storyboard();
        AddDoubleAnimation(
            transitionStoryboard,
            SplashOverlay,
            nameof(UIElement.Opacity),
            1,
            0,
            380,
            new QuadraticEase { EasingMode = EasingMode.EaseIn });
        AddDoubleAnimation(
            transitionStoryboard,
            RootNavigationView,
            nameof(UIElement.Opacity),
            0,
            1,
            380,
            new QuadraticEase { EasingMode = EasingMode.EaseOut });

        await BeginStoryboardAsync(transitionStoryboard);

        SplashOverlay.Visibility = Visibility.Collapsed;
        RootNavigationView.Opacity = 1;
        RootNavigationView.IsHitTestVisible = true;
    }

    private static void AddDoubleAnimation(
        Storyboard storyboard,
        DependencyObject target,
        string propertyName,
        double from,
        double to,
        int durationMilliseconds,
        EasingFunctionBase easingFunction)
    {
        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
            EasingFunction = easingFunction
        };

        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, propertyName);
        storyboard.Children.Add(animation);
    }

    private static Task BeginStoryboardAsync(Storyboard storyboard)
    {
        var completionSource = new TaskCompletionSource<bool>();

        storyboard.Completed += (_, _) => completionSource.TrySetResult(true);
        storyboard.Begin();

        return completionSource.Task;
    }

    private void SetWindowIcon()
    {
        var windowHandle = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");

        if (!File.Exists(iconPath))
        {
            iconPath = Path.Combine(AppContext.BaseDirectory, "AppX", "Assets", "AppIcon.ico");
        }

        if (File.Exists(iconPath))
        {
            appWindow.SetIcon(iconPath);
        }
    }

    private void RootNavigationView_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavigateToPage("SettingsPage");
            return;
        }

        if (args.SelectedItem is not NavigationViewItem selectedItem)
        {
            return;
        }

        var pageTag = selectedItem.Tag?.ToString();

        if (string.IsNullOrWhiteSpace(pageTag))
        {
            return;
        }

        NavigateToPage(pageTag);
    }

    private void NavigateToPage(string pageTag)
    {
        Type pageType = pageTag switch
        {
            "HomePage" => typeof(HomePage),
            "ProjectsPage" => typeof(ProjectsPage),
            "TasksPage" => typeof(TasksPage),
            "MembersPage" => typeof(MembersPage),
            "EquipmentPage" => typeof(EquipmentPage),
            "AiRecordsPage" => typeof(AiRecordsPage),
            "ReportsPage" => typeof(ReportsPage),
            "SettingsPage" => typeof(SettingsPage),
            "AboutPage" => typeof(AboutPage),
            _ => typeof(HomePage)
        };

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }

        RootNavigationView.Header = pageTag switch
        {
            "HomePage" => "管理驾驶舱",
            "ProjectsPage" => "项目管理",
            "TasksPage" => "任务管理",
            "MembersPage" => "人员管理",
            "EquipmentPage" => "器材管理",
            "AiRecordsPage" => "AI协作记录",
            "ReportsPage" => "周报与复盘",
            "SettingsPage" => "系统设置",
            "AboutPage" => "关于系统",
            _ => "TeamFlowDesk"
        };
    }
}
