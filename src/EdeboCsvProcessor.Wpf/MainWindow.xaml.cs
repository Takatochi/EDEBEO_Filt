using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using EdeboCsvProcessor.Application.UseCases;
using EdeboCsvProcessor.Domain.Interfaces;
using EdeboCsvProcessor.Infrastructure.Csv;
using EdeboCsvProcessor.Infrastructure.Excel;
using DomainApplication = EdeboCsvProcessor.Domain.Entities.Application;
using EdeboCsvProcessor.Application.Services;
using Velopack;
using Velopack.Sources;

namespace EdeboCsvProcessor.Wpf
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<DomainApplication> _applications = new ObservableCollection<DomainApplication>();
        private ICollectionView _applicationsView;

        public MainWindow()
        {
            InitializeComponent();
            _applicationsView = CollectionViewSource.GetDefaultView(_applications);
            _applicationsView.Filter = ApplicationFilter;
            ApplicationsDataGrid.ItemsSource = _applicationsView;
            
            // Background update check
            _ = UpdateAppAsync();
        }

        private async Task UpdateAppAsync()
        {
            try
            {
                var updateManager = new UpdateManager(new GithubSource("https://github.com/Takatochi/EDEBEO_Filt", null, false));
                var updateInfo = await updateManager.CheckForUpdatesAsync();
                
                if (updateInfo != null)
                {
                    // New update available, download it
                    await updateManager.DownloadUpdatesAsync(updateInfo);
                    
                    // Apply the update and restart the app
                    updateManager.ApplyUpdatesAndRestart(updateInfo);
                }
            }
            catch
            {
                // Ignore update errors so it doesn't interrupt the user
            }
        }

        private async void BrowseInputFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "РџС–РґС‚СЂРёРјСѓРІР°РЅС– С„Р°Р№Р»Рё (*.csv, *.xlsx)|*.csv;*.xlsx|Р’СЃС– С„Р°Р№Р»Рё (*.*)|*.*",
                Title = "РћР±РµСЂС–С‚СЊ С„Р°Р№Р» Р· Р„Р”Р•Р‘Рћ"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                InputFileTextBox.Text = openFileDialog.FileName;
                await LoadDataAsync(openFileDialog.FileName);
            }
        }

        private async Task LoadDataAsync(string filePath)
        {
            try
            {
                StatusTextBlock.Text = "Р—Р°РІР°РЅС‚Р°Р¶РµРЅРЅСЏ РґР°РЅРёС…...";
                ExportButton.IsEnabled = false;

                var apps = await Task.Run(() =>
                {
                    IApplicationRepository repo;
                    if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                        repo = new ExcelApplicationRepository();
                    else
                        repo = new CsvApplicationRepository();

                    return repo.GetAll(filePath).OrderBy(a => a.SubmissionDate?.Value ?? DateTime.MaxValue).ToList();
                });

                _applications.Clear();
                var uniqueProposals = new System.Collections.Generic.HashSet<string>();
                ApplicationsDataGrid.Columns.Clear();

                bool columnsAdded = false;

                foreach (var app in apps)
                {
                    _applications.Add(app);
                    if (app.Proposal != null && !string.IsNullOrWhiteSpace(app.Proposal.Name))
                        uniqueProposals.Add(app.Proposal.Name);

                    if (!columnsAdded && app.RawData != null)
                    {
                        var keys = app.RawData.Keys.ToList();
                        int phoneIndex = keys.FindIndex(k => k.IndexOf("С‚РµР»РµС„РѕРЅ", System.StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("РєРѕРЅС‚Р°РєС‚РЅРёР№ РЅРѕРјРµСЂ", System.StringComparison.OrdinalIgnoreCase) >= 0);
                        int grantIndex = phoneIndex >= 0 ? phoneIndex + 1 : keys.Count;

                        var grantColumn = new DataGridTextColumn
                        {
                            Header = "РњРѕР¶Р»РёРІРёР№ РіСЂР°РЅС‚",
                            Binding = new Binding("GrantStatus"),
                            FontWeight = FontWeights.Bold
                        };

                        for (int i = 0; i < keys.Count; i++)
                        {
                            if (i == grantIndex)
                            {
                                ApplicationsDataGrid.Columns.Add(grantColumn);
                            }
                            
                            ApplicationsDataGrid.Columns.Add(new DataGridTextColumn
                            {
                                Header = keys[i],
                                Binding = new Binding($"RawData[{keys[i]}]")
                            });
                        }
                        
                        if (grantIndex == keys.Count)
                        {
                            ApplicationsDataGrid.Columns.Add(grantColumn);
                        }

                        columnsAdded = true;
                    }
                }

                var proposalList = uniqueProposals.OrderBy(x => x).ToList();
                proposalList.Insert(0, "Р’СЃС– РїСЂРѕРїРѕР·РёС†С–С—");
                ProposalComboBox.ItemsSource = proposalList;
                ProposalComboBox.SelectedIndex = 0;

                StatusTextBlock.Text = $"Р—Р°РІР°РЅС‚Р°Р¶РµРЅРѕ {apps.Count} Р·Р°СЏРІ. Р¤С–Р»СЊС‚СЂСѓР№С‚Рµ С‚Р° РµРєСЃРїРѕСЂС‚СѓР№С‚Рµ.";
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                ExportButton.IsEnabled = _applications.Count > 0;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"РџРѕРјРёР»РєР° Р·Р°РІР°РЅС‚Р°Р¶РµРЅРЅСЏ: {ex.Message}";
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
                MessageBox.Show($"РќРµ РІРґР°Р»РѕСЃСЏ Р·Р°РІР°РЅС‚Р°Р¶РёС‚Рё С„Р°Р№Р»:\n{ex.Message}", "РџРѕРјРёР»РєР°", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            if (_applicationsView != null)
            {
                _applicationsView.Refresh();
            }
        }

        private void FilterChanged(object sender, TextChangedEventArgs e)
        {
            if (_applicationsView != null)
            {
                _applicationsView.Refresh();
            }
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_applicationsView != null)
            {
                _applicationsView.Refresh();
            }
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            if (ProposalComboBox.Items.Count > 0)
                ProposalComboBox.SelectedIndex = 0;
        }

        private bool ApplicationFilter(object item)
        {
            if (item is not DomainApplication app) return false;

            // Date Filters
            if (DateFromPicker.SelectedDate.HasValue && app.SubmissionDate != null && app.SubmissionDate.Value < DateFromPicker.SelectedDate.Value)
                return false;

            if (DateToPicker.SelectedDate.HasValue && app.SubmissionDate != null)
            {
                var endOfDay = DateToPicker.SelectedDate.Value.AddDays(1).AddTicks(-1);
                if (app.SubmissionDate.Value > endOfDay)
                    return false;
            }

            // Proposal Filter
            string selectedProposal = ProposalComboBox.SelectedItem as string ?? ProposalComboBox.Text;
            if (!string.IsNullOrWhiteSpace(selectedProposal) && selectedProposal != "Р’СЃС– РїСЂРѕРїРѕР·РёС†С–С—")
            {
                if (app.Proposal == null || !app.Proposal.Name.Contains(selectedProposal, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Global Search Filter
            string searchText = SearchTextBox.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                bool matches = false;
                foreach (var val in app.RawData.Values)
                {
                    if (val != null && val.ToLower().Contains(searchText))
                    {
                        matches = true;
                        break;
                    }
                }
                
                if (!matches) return false;
            }

            return true;
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel С„Р°Р№Р»Рё (*.xlsx)|*.xlsx",
                Title = "Р—Р±РµСЂРµРіС‚Рё СЂРµР·СѓР»СЊС‚Р°С‚ СЏРє",
                DefaultExt = ".xlsx",
                FileName = "Exported_Result.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StatusTextBlock.Text = "Р—Р±РµСЂРµР¶РµРЅРЅСЏ РІ Excel...";
                    ExportButton.IsEnabled = false;

                    var filteredApps = _applicationsView.Cast<DomainApplication>().ToList();
                    string outputPath = saveFileDialog.FileName;

                    var orderedColumns = ApplicationsDataGrid.Columns
                        .Where(c => c.Visibility == Visibility.Visible)
                        .OrderBy(c => c.DisplayIndex)
                        .Select(c => c.Header?.ToString() ?? "")
                        .ToList();

                    await Task.Run(() =>
                    {
                        var exportUseCase = new ExportApplicationsUseCase(new ExcelExportService());
                        exportUseCase.Execute(filteredApps, outputPath, orderedColumns);
                    });

                    StatusTextBlock.Text = $"вњ… РЈСЃРїС–С€РЅРѕ РµРєСЃРїРѕСЂС‚РѕРІР°РЅРѕ {filteredApps.Count} Р·Р°СЏРІ.";
                    StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
                    MessageBox.Show("Р•РєСЃРїРѕСЂС‚ СѓСЃРїС–С€РЅРѕ Р·Р°РІРµСЂС€РµРЅРѕ!", "РЈСЃРїС–С…", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"вќЊ РџРѕРјРёР»РєР° РµРєСЃРїРѕСЂС‚Сѓ: {ex.Message}";
                    StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
                    MessageBox.Show($"РџРѕРјРёР»РєР° РїСЂРё Р·Р±РµСЂРµР¶РµРЅРЅС–:\n{ex.Message}", "РџРѕРјРёР»РєР°", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ExportButton.IsEnabled = true;
                }
            }
        }

        private void HideColumn_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem && 
                menuItem.Parent is System.Windows.Controls.ContextMenu contextMenu &&
                contextMenu.PlacementTarget is System.Windows.Controls.Primitives.DataGridColumnHeader header)
            {
                if (header.Column != null)
                {
                    header.Column.Visibility = Visibility.Collapsed;
                }
            }
        }
        
        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            var paymentWindow = new PaymentWindow
            {
                Owner = this
            };
            paymentWindow.ShowDialog();
        }
    }
}
