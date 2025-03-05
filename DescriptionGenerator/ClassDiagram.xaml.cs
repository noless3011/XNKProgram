using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DescriptionGenerator.ClassDiagramView;

namespace DescriptionGenerator
{
    /// <summary>
    /// Interaction logic for ClassDiagram.xaml
    /// </summary>
    public partial class ClassDiagram : UserControl
    {
        private ClassDiagramView diagramGenerator;
        private string? currentJsonPath;

        public ClassDiagram()
        {
            InitializeComponent();
            diagramGenerator = new();
            contentPresenter.Child = diagramGenerator.GetWpfControl();
        }

        public void SetJsonData(string jsonData)
        {
            currentJsonPath = null;
            diagramGenerator.JsonData = jsonData;
            diagramGenerator.RenderDiagram();
        }

        private async void btnOpenJson_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select Class Structure JSON File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    currentJsonPath = openFileDialog.FileName;
                    string jsonContent = File.ReadAllText(currentJsonPath);

                    // Show loading indicator
                    loadingPanel.Visibility = Visibility.Visible;

                    // Generate diagram asynchronously to prevent UI freezing
                    await Task.Run(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            diagramGenerator.JsonData = jsonContent;
                            UpdateLayoutAlgorithm();
                        });
                    });

                    // Hide loading indicator
                    loadingPanel.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading JSON file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    loadingPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnExportImage_Click(object sender, RoutedEventArgs e)
        {
            if (diagramGenerator.JsonData != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|BMP Image (*.bmp)|*.bmp",
                    Title = "Save Class Diagram"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        // Export the diagram as an image
                        diagramGenerator.ExportToImage(saveFileDialog.FileName);
                        MessageBox.Show("Diagram exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting diagram: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No diagram to export. Please open a JSON file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cmbLayout_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (diagramGenerator != null)
                UpdateLayoutAlgorithm();
        }

        private void UpdateLayoutAlgorithm()
        {
            if (diagramGenerator.JsonData != null)
            {
                switch (cmbLayout.SelectedIndex)
                {
                    case 0: // Sugiyama Layout
                        diagramGenerator.SetLayoutAlgorithm(new SugiyamaLayoutSettings
                        {
                            NodeSeparation = 25,
                            LayerSeparation = 50,
                            EdgeRoutingSettings = { EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.Spline }
                        });
                        break;

                    case 1: // Force Directed
                        diagramGenerator.SetLayoutAlgorithm(new FastIncrementalLayoutSettings
                        {
                            AvoidOverlaps = true,
                            NodeSeparation = 15,
                            EdgeRoutingSettings = { EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.Spline }
                        });
                        break;

                    case 2: // MDS Layout
                        diagramGenerator.SetLayoutAlgorithm(new MdsLayoutSettings
                        {
                            ScaleX = 1.0,
                            ScaleY = 1.0,
                            EdgeRoutingSettings = { EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.Spline }
                        });
                        break;
                }
                diagramGenerator.RenderDiagram();
            }
        }

        private void RelationshipFilter_Changed(object sender, RoutedEventArgs e)
        {
            if (diagramGenerator == null) return;
            if (diagramGenerator.JsonData != null)
            {
                diagramGenerator.SetRelationshipVisibility(
                    RelationshipType.Inheritance, chkShowInheritance.IsChecked ?? true,
                    RelationshipType.Composition, chkShowComposition.IsChecked ?? true,
                    RelationshipType.Aggregation, chkShowAggregation.IsChecked ?? true,
                    RelationshipType.Association, chkShowAssociation.IsChecked ?? true,
                    RelationshipType.Dependency, chkShowDependency.IsChecked ?? true
                );
            }
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            diagramGenerator.ZoomIn();
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            diagramGenerator.ZoomOut();
        }

        private void btnZoomFit_Click(object sender, RoutedEventArgs e)
        {
            diagramGenerator.ZoomToFit();
        }
    }
}