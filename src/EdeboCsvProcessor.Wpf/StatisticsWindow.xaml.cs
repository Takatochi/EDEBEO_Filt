using System.Windows;
using EdeboCsvProcessor.Domain.Entities;

namespace EdeboCsvProcessor.Wpf
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow(StatisticsResult stats)
        {
            InitializeComponent();
            
            // Total vs Today row mapping
            TotalAppsText.Text = stats.TotalApplications.ToString();
            TodayAppsText.Text = stats.TodayApplications.ToString();
            
            TotalOriginalsText.Text = stats.TotalOriginals.ToString();
            TodayOriginalsText.Text = stats.TodayOriginals.ToString();
            
            TotalContractsText.Text = stats.TotalContracts.ToString();
            TodayContractsText.Text = stats.TodayContracts.ToString();
            
            TotalContractAppsText.Text = stats.TotalContractApplications.ToString();
            TodayContractAppsText.Text = stats.TodayContractApplications.ToString();
            
            // Priorities
            Prio1Text.Text = stats.Priority1Count.ToString();
            Prio2Text.Text = stats.Priority2Count.ToString();
            Prio3Text.Text = stats.Priority3Count.ToString();

            // Lists
            StatusList.ItemsSource = stats.ApplicationsByStatus;
            ProposalList.ItemsSource = stats.ApplicationsByProposal;
        }
    }
}
