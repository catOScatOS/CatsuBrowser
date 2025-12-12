using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace CatsuBrowser
{
    public partial class MainWindow : Window
    {
        private const string HomeUrl = "https://www.google.com";

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView(WebView, HomeUrl);
        }

        // Initialize a WebView2 instance
        private async void InitializeWebView(Microsoft.Web.WebView2.Wpf.WebView2 webView, string url)
        {
            await webView.EnsureCoreWebView2Async();

            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

            webView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                SetStatus("Loading...");
            };

            webView.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                SetStatus(e.IsSuccess ? "Done" : $"Failed ({e.WebErrorStatus})");

                var (_, _, history) = GetActiveControls();
                var current = webView.Source?.ToString();
                if (history != null && !string.IsNullOrWhiteSpace(current))
                {
                    history.Items.Add(current);
                    history.SelectedIndex = history.Items.Count - 1;
                }
            };

            webView.Source = new Uri(url);
        }

        private void SetStatus(string text)
        {
            // Hook to a status bar TextBlock if you have one in XAML (optional)
            // Example: StatusText.Text = text;
        }

        // --- Tabs management ---
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedItem == AddTabItem)
            {
                CreateNewTab();
            }
        }

        private void CreateNewTab()
        {
            var tab = new TabItem();

            var header = new StackPanel { Orientation = Orientation.Horizontal };
            header.Children.Add(new TextBlock { Text = "New tab", Margin = new Thickness(0, 0, 5, 0) });
            var closeBtn = new Button { Content = "✖", Width = 20, Height = 20, Padding = new Thickness(0) };
            closeBtn.Click += CloseTab_Click;
            header.Children.Add(closeBtn);
            tab.Header = header;

            var dock = new DockPanel();
            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(6) };

            var backBtn = new Button { Content = "◀", Width = 30 };
            backBtn.Click += BackButton_Click;

            var forwardBtn = new Button { Content = "▶", Width = 30 };
            forwardBtn.Click += ForwardButton_Click;

            var refreshBtn = new Button { Content = "↻", Width = 30 };
            refreshBtn.Click += RefreshButton_Click;

            var homeBtn = new Button { Content = "🏠", Width = 30 };
            homeBtn.Click += HomeButton_Click;

            var address = new TextBox { Width = 420 };
            address.KeyDown += AddressTextBox_KeyDown;

            var searchBtn = new Button { Content = "Search" };
            searchBtn.Click += SearchButton_Click;

            var historyCombo = new ComboBox { Width = 220 };
            historyCombo.SelectionChanged += HistoryCombo_SelectionChanged;

            var historyDeleteBtn = new Button { Content = "Delete" };
            historyDeleteBtn.Click += HistoryDeleteButton_Click;

            var historyClearBtn = new Button { Content = "Clear all" };
            historyClearBtn.Click += HistoryClearButton_Click;

            toolbar.Children.Add(backBtn);
            toolbar.Children.Add(forwardBtn);
            toolbar.Children.Add(refreshBtn);
            toolbar.Children.Add(homeBtn);
            toolbar.Children.Add(address);
            toolbar.Children.Add(searchBtn);
            toolbar.Children.Add(historyCombo);
            toolbar.Children.Add(historyDeleteBtn);
            toolbar.Children.Add(historyClearBtn);

            DockPanel.SetDock(toolbar, Dock.Top);
            dock.Children.Add(toolbar);

            var webView = new Microsoft.Web.WebView2.Wpf.WebView2();
            dock.Children.Add(webView);

            tab.Content = dock;

            TabControl.Items.Insert(TabControl.Items.Count - 1, tab);
            TabControl.SelectedItem = tab;

            InitializeWebView(webView, HomeUrl);
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var header = (StackPanel)btn.Parent;
            var tab = header?.Parent as TabItem;
            if (tab != null)
            {
                TabControl.Items.Remove(tab);
            }
        }

        // --- Navigation ---
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var (address, webView, history) = GetActiveControls();
            if (address == null || webView == null) return;

            var input = address.Text?.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            string target = Uri.IsWellFormedUriString(input, UriKind.Absolute)
                ? input
                : $"https://www.google.com/search?q={Uri.EscapeDataString(input)}&hl=en";

            webView.Source = new Uri(target);

            if (history != null)
            {
                history.Items.Add(target);
                history.SelectedIndex = history.Items.Count - 1;
            }
        }

        private void AddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Click(sender, e);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var (_, webView, _) = GetActiveControls();
            if (webView?.CoreWebView2?.CanGoBack == true)
                webView.CoreWebView2.GoBack();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            var (_, webView, _) = GetActiveControls();
            if (webView?.CoreWebView2?.CanGoForward == true)
                webView.CoreWebView2.GoForward();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var (_, webView, _) = GetActiveControls();
            webView?.Reload();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var (address, webView, history) = GetActiveControls();
            if (webView != null)
            {
                webView.Source = new Uri(HomeUrl);
                if (address != null) address.Text = HomeUrl;
                if (history != null)
                {
                    history.Items.Add(HomeUrl);
                    history.SelectedIndex = history.Items.Count - 1;
                }
            }
        }

        private void HistoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var (_, webView, history) = GetActiveControls();
            if (webView != null && history?.SelectedItem is string url)
            {
                webView.Source = new Uri(url);
            }
        }

        // Delete selected history item in current tab
        private void HistoryDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var (_, _, history) = GetActiveControls();
            if (history == null) return;

            var index = history.SelectedIndex;
            if (index >= 0)
            {
                history.Items.RemoveAt(index);
                if (history.Items.Count > 0)
                {
                    history.SelectedIndex = Math.Min(index, history.Items.Count - 1);
                }
            }
        }

        // Clear the entire history in current tab
        private void HistoryClearButton_Click(object sender, RoutedEventArgs e)
        {
            var (_, _, history) = GetActiveControls();
            history?.Items.Clear();
        }

        // --- Helpers to get controls from the active tab ---
        private (TextBox address, Microsoft.Web.WebView2.Wpf.WebView2 webView, ComboBox history) GetActiveControls()
        {
            var selected = TabControl.SelectedItem as TabItem;
            if (selected == null || selected == AddTabItem)
            {
                return (null, null, null);
            }

            // Initial tab (named controls in XAML)
            if (ReferenceEquals(selected.Content, WebView.Parent))
            {
                return (AddressTextBox, WebView, HistoryCombo);
            }

            // Dynamically created tabs
            var dock = selected.Content as DockPanel;
            if (dock == null || dock.Children.Count < 2)
                return (null, null, null);

            var toolbar = dock.Children[0] as StackPanel;
            var webView = dock.Children[1] as Microsoft.Web.WebView2.Wpf.WebView2;

            TextBox address = null;
            ComboBox history = null;

            foreach (var child in toolbar.Children)
            {
                if (address == null && child is TextBox tb) address = tb;
                else if (child is ComboBox cb && history == null) history = cb;
            }

            return (address, webView, history);
        }
    }
}
