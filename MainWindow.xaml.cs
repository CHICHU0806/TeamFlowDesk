using System;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Pages;
using WinRT.Interop;

namespace TeamFlowDesk;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetWindowIcon();

        RootNavigationView.SelectedItem = RootNavigationView.MenuItems[0];
        NavigateToPage("HomePage");
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
