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
            try
            {
                var mgr = new UpdateManager(new GithubSource("https://github.com/Takatochi/EDEBEO_Filt", null, false));
                VersionText.Text = "Версія: " + (mgr.IsInstalled ? mgr.CurrentVersion?.ToString() : "Dev");
            }
            catch
            {
                VersionText.Text = "Версія: невідомо";
            }
            
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
                    UpdatePanel.Visibility = Visibility.Visible;
                    
                    await updateManager.DownloadUpdatesAsync(updateInfo, progress => 
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => 
                        {
                            UpdateProgressBar.Value = progress;
                            UpdatePercentageText.Text = $"{progress}%";
                        });
                    });
                    
                    UpdateStatusText.Text = "Перезапуск програми...";
                    updateManager.ApplyUpdatesAndRestart(updateInfo);
                }
            }
            catch
            {
                // Ignore update errors so it doesn't interrupt the user
            }
        }

        private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var updateManager = new UpdateManager(new GithubSource("https://github.com/Takatochi/EDEBEO_Filt", null, false));
                var updateInfo = await updateManager.CheckForUpdatesAsync();
                
                if (updateInfo != null)
                {
                    var result = MessageBox.Show($"Знайдено нову версію: {updateInfo.TargetFullRelease.Version}\nБажаєте оновити програму зараз?", "Оновлення", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        UpdatePanel.Visibility = Visibility.Visible;
                        
                        await updateManager.DownloadUpdatesAsync(updateInfo, progress => 
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() => 
                            {
                                UpdateProgressBar.Value = progress;
                                UpdatePercentageText.Text = $"{progress}%";
                            });
                        });
                        
                        UpdateStatusText.Text = "Перезапуск програми...";
                        updateManager.ApplyUpdatesAndRestart(updateInfo);
                    }
                }
                else
                {
                    MessageBox.Show("У вас встановлена остання версія програми.", "Оновлення", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка під час перевірки оновлень: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdatePanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void BrowseInputFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Підтримувані файли (*.csv, *.xlsx)|*.csv;*.xlsx|Всі файли (*.*)|*.*",
                Title = "Оберіть файл з ЄДЕБО"
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
                StatusTextBlock.Text = "Завантаження даних...";
                ExportButton.IsEnabled = false;
                StatisticsButton.IsEnabled = false;

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
                        int phoneIndex = keys.FindIndex(k => k.IndexOf("телефон", System.StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("контактний номер", System.StringComparison.OrdinalIgnoreCase) >= 0);
                        int grantIndex = phoneIndex >= 0 ? phoneIndex + 1 : keys.Count;

                        var grantColumn = new DataGridTextColumn
                        {
                            Header = "Можливий грант",
                            Binding = new Binding("GrantStatus"),
                            FontWeight = FontWeights.Bold
                        };

                        var budgetColumn = new DataGridTextColumn
                        {
                            Header = "Претендує на бюджет",
                            Binding = new Binding("ClaimsBudget"),
                            FontWeight = FontWeights.Bold,
                            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113))
                        };

                        for (int i = 0; i < keys.Count; i++)
                        {
                            if (i == grantIndex)
                            {
                                ApplicationsDataGrid.Columns.Add(grantColumn);
                                ApplicationsDataGrid.Columns.Add(budgetColumn);
                            }
                            
                            if (keys[i].Equals("Претендує на бюджет", StringComparison.OrdinalIgnoreCase))
                                continue;

                            ApplicationsDataGrid.Columns.Add(new DataGridTextColumn
                            {
                                Header = keys[i],
                                Binding = new Binding($"RawData[{keys[i]}]")
                            });
                        }
                        
                        if (grantIndex == keys.Count)
                        {
                            ApplicationsDataGrid.Columns.Add(grantColumn);
                            ApplicationsDataGrid.Columns.Add(budgetColumn);
                        }

                        columnsAdded = true;
                    }
                }

                var proposalList = uniqueProposals.OrderBy(x => x).ToList();
                proposalList.Insert(0, "Всі пропозиції");
                ProposalComboBox.ItemsSource = proposalList;
                ProposalComboBox.SelectedIndex = 0;

                var uniqueStatuses = apps
                    .Where(a => !string.IsNullOrEmpty(a.Status))
                    .Select(a => a.Status!)
                    .Distinct()
                    .ToList();
                    
                var statusList = new List<string> { "Всі статуси", "Без відмов", "Тільки відмови" };
                statusList.AddRange(uniqueStatuses.OrderBy(x => x));
                StatusComboBox.ItemsSource = statusList;
                StatusComboBox.SelectedIndex = 0;                StatusTextBlock.Text = $"Завантажено {apps.Count} заяв. Фільтруйте та експортуйте.";
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                ExportButton.IsEnabled = _applications.Count > 0;
                StatisticsButton.IsEnabled = _applications.Count > 0;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Помилка завантаження: {ex.Message}";
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
                MessageBox.Show($"Не вдалося завантажити файл:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (StatusComboBox.Items.Count > 0)
                StatusComboBox.SelectedIndex = 0;
            if (BudgetComboBox.Items.Count > 0)
                BudgetComboBox.SelectedIndex = 0;
        }

        private bool ApplicationFilter(object item)
        {
            try
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
                if (!string.IsNullOrWhiteSpace(selectedProposal) && selectedProposal != "Всі пропозиції")
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

                // Status Filter
                string selectedStatus = StatusComboBox.SelectedItem as string ?? StatusComboBox.Text;
                if (!string.IsNullOrWhiteSpace(selectedStatus) && selectedStatus != "Всі статуси")
                {
                    if (selectedStatus == "Без відмов")
                    {
                        if (app.Status != null && (
                            app.Status.Contains("Відмовлено", StringComparison.OrdinalIgnoreCase) ||
                            app.Status.Contains("Відхилено", StringComparison.OrdinalIgnoreCase) ||
                            app.Status.Contains("Скасовано", StringComparison.OrdinalIgnoreCase)))
                        {
                            return false;
                        }
                    }
                    else if (selectedStatus == "Тільки відмови")
                    {
                        if (app.Status == null || !(
                            app.Status.Contains("Відмовлено", StringComparison.OrdinalIgnoreCase) ||
                            app.Status.Contains("Відхилено", StringComparison.OrdinalIgnoreCase) ||
                            app.Status.Contains("Скасовано", StringComparison.OrdinalIgnoreCase)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (app.Status == null || !app.Status.Contains(selectedStatus, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                }

                // Budget Filter
                if (BudgetComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem budgetItem)
                {
                    if (budgetItem.Content != null)
                    {
                        string budgetSelection = budgetItem.Content.ToString() ?? "";
                        if (budgetSelection == "Так" && app.ClaimsBudget != "Так")
                            return false;
                        if (budgetSelection == "Ні" && app.ClaimsBudget != "Ні")
                            return false;
                    }
                }
                else if (BudgetComboBox.SelectedItem is string budgetStr)
                {
                    if (budgetStr == "Так" && app.ClaimsBudget != "Так") return false;
                    if (budgetStr == "Ні" && app.ClaimsBudget != "Ні") return false;
                }

                return true;
            }
            catch
            {
                return true; // Fallback so we don't hide everything on error
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel файли (*.xlsx)|*.xlsx",
                Title = "Зберегти результат як",
                DefaultExt = ".xlsx",
                FileName = "Exported_Result.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StatusTextBlock.Text = "Збереження в Excel...";
                    ExportButton.IsEnabled = false;
                    StatisticsButton.IsEnabled = false;

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

                    StatusTextBlock.Text = $"✅ Успішно експортовано {filteredApps.Count} заяв.";
                    StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
                    MessageBox.Show("Експорт успішно завершено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"❌ Помилка експорту: {ex.Message}";
                    StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
                    MessageBox.Show($"Помилка при збереженні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ExportButton.IsEnabled = true;
                    StatisticsButton.IsEnabled = true;
                }
            }
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filteredApplications = _applicationsView.Cast<DomainApplication>();
                var useCase = new CalculateStatisticsUseCase();
                var result = useCase.Execute(filteredApplications);
                
                var statsWindow = new StatisticsWindow(result);
                statsWindow.Owner = this;
                statsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при зборі статистики: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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