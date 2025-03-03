using System;
using System.Collections.Generic;
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
using System.Text.Json;

namespace DescriptionGenerator
{
    /// <summary>
    /// Interaction logic for ClassDiagramView.xaml
    /// </summary>
    public partial class ClassDiagramView : UserControl
    {
        public ClassDiagramView()
        {
            InitializeComponent();
            this.Loaded += ClassDiagramView_Loaded;
        }

        private void ClassDiagramView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(JsonData))
            {
                RenderDiagram();
            }
        }

        private Dictionary<string, UIElement> _classElements = new Dictionary<string, UIElement>();
        private Dictionary<string, ClassInfo> _classInfoMap = new Dictionary<string, ClassInfo>();
        private Dictionary<string, Point> _classPositions = new Dictionary<string, Point>();
        private Dictionary<string, Size> _classSizes = new Dictionary<string, Size>();

        public string JsonData
        {
            get { return (string)GetValue(JsonDataProperty); }
            set { SetValue(JsonDataProperty, value); }
        }

        public static readonly DependencyProperty JsonDataProperty =
            DependencyProperty.Register("JsonData", typeof(string), typeof(ClassDiagramView),
                new PropertyMetadata(null, OnJsonDataChanged));

        private static void OnJsonDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ClassDiagramView)d;
            control.RenderDiagram();
        }

        private void RenderDiagram()
        {
            if (string.IsNullOrEmpty(JsonData))
            {
                DiagramCanvas.Children.Clear();
                _classElements.Clear();
                _classInfoMap.Clear();
                _classPositions.Clear();
                _classSizes.Clear();
                return;
            }

            try
            {
                // Clear all collections
                DiagramCanvas.Children.Clear();
                _classElements.Clear();
                _classInfoMap.Clear();
                _classPositions.Clear();
                _classSizes.Clear();

                // 1. Deserialize JSON
                var diagramRoot = JsonSerializer.Deserialize<DiagramRoot>(JsonData);
                if (diagramRoot?.Diagram?.Classes == null) return;
                var diagramData = diagramRoot.Diagram;

                // 2. First pass - create all class elements to calculate sizes
                foreach (var classInfo in diagramData.Classes)
                {
                    if (classInfo?.ClassName == null) continue;

                    var classElement = CreateClassElement(classInfo);
                    _classInfoMap[classInfo.ClassName] = classInfo;
                    _classElements[classInfo.ClassName] = classElement;

                    // Measure the element to get its size
                    classElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    _classSizes[classInfo.ClassName] = new Size(
                        classElement.DesiredSize.Width + 10, // Add some padding
                        classElement.DesiredSize.Height + 10
                    );
                }

                // 3. Position classes using a layout algorithm
                PositionClasses(diagramData.Classes);

                // 4. Add class elements to canvas at calculated positions
                foreach (var classInfo in diagramData.Classes)
                {
                    if (classInfo?.ClassName == null) continue;

                    var classElement = _classElements[classInfo.ClassName];
                    var position = _classPositions[classInfo.ClassName];

                    Canvas.SetLeft(classElement, position.X);
                    Canvas.SetTop(classElement, position.Y);
                    DiagramCanvas.Children.Add(classElement);
                }

                // 5. Render relationships after all classes are positioned
                if (diagramData.Relationships != null)
                {
                    foreach (var relationship in diagramData.Relationships)
                    {
                        if (relationship?.SourceClass == null || relationship?.TargetClass == null)
                            continue;

                        if (_classPositions.ContainsKey(relationship.SourceClass) &&
                            _classPositions.ContainsKey(relationship.TargetClass))
                        {
                            RenderRelationship(
                                relationship,
                                _classPositions[relationship.SourceClass],
                                _classPositions[relationship.TargetClass],
                                _classSizes[relationship.SourceClass],
                                _classSizes[relationship.TargetClass]
                            );
                        }
                    }
                }

                // 6. Set canvas size to fit all elements
                UpdateCanvasSize();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error parsing JSON: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rendering diagram: {ex.Message}", "Rendering Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private UIElement CreateClassElement(ClassInfo classInfo)
        {
            // Create a Grid to hold the class elements
            Grid classGrid = new Grid();
            classGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            classGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            classGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Class header with name
            Border headerBorder = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Background = Brushes.LightGray
            };

            TextBlock headerText = new TextBlock
            {
                Text = classInfo.IsAbstract ? $"«abstract» {classInfo.ClassName}" : classInfo.ClassName,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(5),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            headerBorder.Child = headerText;
            Grid.SetRow(headerBorder, 0);
            classGrid.Children.Add(headerBorder);

            // Attributes section
            Border attributesBorder = new Border
            {
                BorderThickness = new Thickness(1, 0, 1, 1),
                BorderBrush = Brushes.Black
            };

            StackPanel attributesPanel = new StackPanel();
            if (classInfo.Attributes != null && classInfo.Attributes.Count > 0)
            {
                foreach (var attr in classInfo.Attributes)
                {
                    string visibility = GetVisibilitySymbol(attr.Visibility);
                    string staticPrefix = attr.IsStatic ? "static " : "";
                    TextBlock attrText = new TextBlock
                    {
                        Text = $"{visibility} {attr.AttributeName} : {staticPrefix}{attr.AttributeType}",
                        Padding = new Thickness(5, 2, 5, 2)
                    };
                    attributesPanel.Children.Add(attrText);
                }
            }
            else
            {
                attributesPanel.Children.Add(new TextBlock
                {
                    Text = " ",
                    Padding = new Thickness(5, 2, 5, 2)
                });
            }

            attributesBorder.Child = attributesPanel;
            Grid.SetRow(attributesBorder, 1);
            classGrid.Children.Add(attributesBorder);

            // Methods section
            Border methodsBorder = new Border
            {
                BorderThickness = new Thickness(1, 0, 1, 1),
                BorderBrush = Brushes.Black
            };

            StackPanel methodsPanel = new StackPanel();
            if (classInfo.Methods != null && classInfo.Methods.Count > 0)
            {
                foreach (var method in classInfo.Methods)
                {
                    string visibility = GetVisibilitySymbol(method.Visibility);
                    string staticPrefix = method.IsStatic ? "static " : "";
                    string parameters = "";

                    if (method.Parameters != null && method.Parameters.Count > 0)
                    {
                        parameters = string.Join(", ", method.Parameters.ConvertAll(p =>
                            $"{p.ParameterName}: {p.ParameterType}"));
                    }

                    TextBlock methodText = new TextBlock
                    {
                        Text = $"{visibility} {method.MethodName}({parameters}) : {staticPrefix}{method.ReturnType}",
                        Padding = new Thickness(5, 2, 5, 2)
                    };
                    methodsPanel.Children.Add(methodText);
                }
            }
            else
            {
                methodsPanel.Children.Add(new TextBlock
                {
                    Text = " ",
                    Padding = new Thickness(5, 2, 5, 2)
                });
            }

            methodsBorder.Child = methodsPanel;
            Grid.SetRow(methodsBorder, 2);
            classGrid.Children.Add(methodsBorder);

            return classGrid;
        }

        private string GetVisibilitySymbol(string visibility)
        {
            return visibility?.ToLower() switch
            {
                "public" => "+",
                "private" => "-",
                "protected" => "#",
                "package" => "~",
                _ => "+"
            };
        }

        private void PositionClasses(List<ClassInfo> classes)
        {
            // A more sophisticated force-directed layout algorithm could be implemented here
            // For now, we'll use a simple grid layout with some spacing and centering

            // Determine the canvas size and reasonable spacing
            double canvasWidth = DiagramCanvas.ActualWidth > 0 ? DiagramCanvas.ActualWidth : 800;
            double canvasHeight = DiagramCanvas.ActualHeight > 0 ? DiagramCanvas.ActualHeight : 600;

            double horizontalSpacing = 50;
            double verticalSpacing = 50;

            // Calculate the total width and height needed for all classes
            double totalWidth = 0;
            double maxHeight = 0;
            int classCount = 0;

            foreach (var cls in classes)
            {
                if (cls?.ClassName == null || !_classSizes.ContainsKey(cls.ClassName))
                    continue;

                totalWidth += _classSizes[cls.ClassName].Width;
                maxHeight = Math.Max(maxHeight, _classSizes[cls.ClassName].Height);
                classCount++;
            }

            totalWidth += horizontalSpacing * (classCount - 1);

            // Position classes in a row
            double startX = Math.Max(0, (canvasWidth - totalWidth) / 2);
            double startY = Math.Max(0, (canvasHeight - maxHeight) / 2);

            double currentX = startX;

            // Simple layout: place classes in a row
            foreach (var cls in classes)
            {
                if (cls?.ClassName == null || !_classSizes.ContainsKey(cls.ClassName))
                    continue;

                var size = _classSizes[cls.ClassName];
                _classPositions[cls.ClassName] = new Point(currentX, startY);
                currentX += size.Width + horizontalSpacing;
            }
        }

        private void RenderRelationship(
            RelationshipInfo relationship,
            Point sourcePoint,
            Point targetPoint,
            Size sourceSize,
            Size targetSize)
        {
            // Calculate connector points on the edges of class boxes
            var sourceCenter = new Point(
                sourcePoint.X + sourceSize.Width / 2,
                sourcePoint.Y + sourceSize.Height / 2
            );

            var targetCenter = new Point(
                targetPoint.X + targetSize.Width / 2,
                targetPoint.Y + targetSize.Height / 2
            );

            // Find edge connection points
            Point sourceConnector = FindEdgeConnectionPoint(sourceCenter, sourcePoint, sourceSize, targetCenter);
            Point targetConnector = FindEdgeConnectionPoint(targetCenter, targetPoint, targetSize, sourceCenter);

            // Create line between the two classes
            Line relationshipLine = new Line
            {
                X1 = sourceConnector.X,
                Y1 = sourceConnector.Y,
                X2 = targetConnector.X,
                Y2 = targetConnector.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };
            DiagramCanvas.Children.Add(relationshipLine);

            // Add relationship type indicators based on UML specification
            switch (relationship.RelationshipType?.ToLower())
            {
                case "inheritance":
                    DrawInheritanceArrow(targetConnector, sourceConnector);
                    break;

                case "aggregation":
                    DrawAggregationDiamond(sourceConnector, targetConnector);
                    break;

                case "composition":
                    DrawCompositionDiamond(sourceConnector, targetConnector);
                    break;

                case "association":
                    DrawAssociationArrow(targetConnector, sourceConnector);
                    break;

                case "dependency":
                    DrawDependencyArrow(targetConnector, sourceConnector);
                    relationshipLine.StrokeDashArray = new DoubleCollection { 4, 2 };
                    break;

                case "implementation":
                    DrawImplementationArrow(targetConnector, sourceConnector);
                    relationshipLine.StrokeDashArray = new DoubleCollection { 4, 2 };
                    break;
            }

            // Add relationship name
            if (!string.IsNullOrEmpty(relationship.RelationshipName))
            {
                TextBlock relationshipNameText = new TextBlock
                {
                    Text = relationship.RelationshipName,
                    FontSize = 10,
                    Background = Brushes.White,
                    Padding = new Thickness(2)
                };

                Point midpoint = new Point(
                    (sourceConnector.X + targetConnector.X) / 2,
                    (sourceConnector.Y + targetConnector.Y) / 2
                );

                Canvas.SetLeft(relationshipNameText, midpoint.X - relationshipNameText.ActualWidth / 2);
                Canvas.SetTop(relationshipNameText, midpoint.Y - 15);
                DiagramCanvas.Children.Add(relationshipNameText);
            }

            // Add cardinality labels if present
            if (!string.IsNullOrEmpty(relationship.SourceCardinality))
            {
                AddCardinalityLabel(relationship.SourceCardinality, sourceConnector, targetConnector, true);
            }

            if (!string.IsNullOrEmpty(relationship.TargetCardinality))
            {
                AddCardinalityLabel(relationship.TargetCardinality, targetConnector, sourceConnector, false);
            }
        }

        private Point FindEdgeConnectionPoint(
            Point center,
            Point topLeft,
            Size size,
            Point targetCenter)
        {
            // Calculate the angle from class center to target center
            double angle = Math.Atan2(targetCenter.Y - center.Y, targetCenter.X - center.X);

            // Find intersection point with the rectangle edge
            double halfWidth = size.Width / 2;
            double halfHeight = size.Height / 2;

            // Determine edge intersection based on angle
            if (Math.Abs(Math.Tan(angle)) > halfHeight / halfWidth)
            {
                // Intersect with top or bottom edge
                int sign = Math.Sign(targetCenter.Y - center.Y);
                return new Point(
                    center.X + sign * halfHeight / Math.Tan(angle),
                    center.Y + sign * halfHeight
                );
            }
            else
            {
                // Intersect with left or right edge
                int sign = Math.Sign(targetCenter.X - center.X);
                return new Point(
                    center.X + sign * halfWidth,
                    center.Y + sign * halfWidth * Math.Tan(angle)
                );
            }
        }

        private void DrawInheritanceArrow(Point tip, Point from)
        {
            // Create an empty triangle arrowhead for inheritance
            const double arrowSize = 10;
            double angle = Math.Atan2(from.Y - tip.Y, from.X - tip.X);

            Point point1 = new Point(
                tip.X + arrowSize * Math.Cos(angle + Math.PI / 6),
                tip.Y + arrowSize * Math.Sin(angle + Math.PI / 6)
            );

            Point point2 = new Point(
                tip.X + arrowSize * Math.Cos(angle - Math.PI / 6),
                tip.Y + arrowSize * Math.Sin(angle - Math.PI / 6)
            );

            Polygon arrow = new Polygon
            {
                Points = new PointCollection { tip, point1, point2 },
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };

            DiagramCanvas.Children.Add(arrow);
        }

        private void DrawAggregationDiamond(Point start, Point end)
        {
            // Create an empty diamond for aggregation
            const double diamondSize = 10;
            double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            Point point1 = new Point(
                start.X + diamondSize * Math.Cos(angle + Math.PI / 4),
                start.Y + diamondSize * Math.Sin(angle + Math.PI / 4)
            );

            Point point2 = new Point(
                start.X + 2 * diamondSize * Math.Cos(angle),
                start.Y + 2 * diamondSize * Math.Sin(angle)
            );

            Point point3 = new Point(
                start.X + diamondSize * Math.Cos(angle - Math.PI / 4),
                start.Y + diamondSize * Math.Sin(angle - Math.PI / 4)
            );

            Polygon diamond = new Polygon
            {
                Points = new PointCollection { start, point1, point2, point3 },
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };

            DiagramCanvas.Children.Add(diamond);
        }

        private void DrawCompositionDiamond(Point start, Point end)
        {
            // Create a filled diamond for composition
            const double diamondSize = 10;
            double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            Point point1 = new Point(
                start.X + diamondSize * Math.Cos(angle + Math.PI / 4),
                start.Y + diamondSize * Math.Sin(angle + Math.PI / 4)
            );

            Point point2 = new Point(
                start.X + 2 * diamondSize * Math.Cos(angle),
                start.Y + 2 * diamondSize * Math.Sin(angle)
            );

            Point point3 = new Point(
                start.X + diamondSize * Math.Cos(angle - Math.PI / 4),
                start.Y + diamondSize * Math.Sin(angle - Math.PI / 4)
            );

            Polygon diamond = new Polygon
            {
                Points = new PointCollection { start, point1, point2, point3 },
                Fill = Brushes.Black,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };

            DiagramCanvas.Children.Add(diamond);
        }

        private void DrawAssociationArrow(Point tip, Point from)
        {
            // For association, a simple open arrow is used
            const double arrowSize = 10;
            double angle = Math.Atan2(from.Y - tip.Y, from.X - tip.X);

            Point point1 = new Point(
                tip.X + arrowSize * Math.Cos(angle + Math.PI / 6),
                tip.Y + arrowSize * Math.Sin(angle + Math.PI / 6)
            );

            Line line1 = new Line
            {
                X1 = tip.X,
                Y1 = tip.Y,
                X2 = point1.X,
                Y2 = point1.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };

            Point point2 = new Point(
                tip.X + arrowSize * Math.Cos(angle - Math.PI / 6),
                tip.Y + arrowSize * Math.Sin(angle - Math.PI / 6)
            );

            Line line2 = new Line
            {
                X1 = tip.X,
                Y1 = tip.Y,
                X2 = point2.X,
                Y2 = point2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };

            DiagramCanvas.Children.Add(line1);
            DiagramCanvas.Children.Add(line2);
        }

        private void DrawDependencyArrow(Point tip, Point from)
        {
            // Dependency arrow is similar to association but with dashed line
            DrawAssociationArrow(tip, from);
        }

        private void DrawImplementationArrow(Point tip, Point from)
        {
            // Implementation arrow is similar to inheritance but with dashed line
            DrawInheritanceArrow(tip, from);
        }

        private void AddCardinalityLabel(string cardinality, Point position, Point otherPoint, bool isSource)
        {
            TextBlock cardinalityText = new TextBlock
            {
                Text = cardinality,
                FontSize = 10,
                Background = Brushes.White,
                Padding = new Thickness(2)
            };

            double angle = Math.Atan2(otherPoint.Y - position.Y, otherPoint.X - position.X);
            double distance = isSource ? 10 : 20;

            double offsetX = distance * Math.Cos(angle + Math.PI / 2);
            double offsetY = distance * Math.Sin(angle + Math.PI / 2);

            Canvas.SetLeft(cardinalityText, position.X + offsetX - cardinalityText.ActualWidth / 2);
            Canvas.SetTop(cardinalityText, position.Y + offsetY - 10);
            DiagramCanvas.Children.Add(cardinalityText);
        }

        private void UpdateCanvasSize()
        {
            // Find the furthest point to ensure canvas can display all elements
            double maxX = 0;
            double maxY = 0;

            foreach (var className in _classPositions.Keys)
            {
                if (!_classSizes.ContainsKey(className)) continue;

                Point pos = _classPositions[className];
                Size size = _classSizes[className];

                maxX = Math.Max(maxX, pos.X + size.Width);
                maxY = Math.Max(maxY, pos.Y + size.Height);
            }

            // Add some margin
            maxX += 50;
            maxY += 50;

            // Update canvas size if necessary
            if (DiagramCanvas.Width < maxX)
                DiagramCanvas.Width = maxX;

            if (DiagramCanvas.Height < maxY)
                DiagramCanvas.Height = maxY;
        }

        // Classes to match the JSON structure
        public class DiagramRoot
        {
            public DiagramContent? Diagram { get; set; }
        }

        public class DiagramContent
        {
            public string? DiagramName { get; set; }
            public List<ClassInfo>? Classes { get; set; }
            public List<RelationshipInfo>? Relationships { get; set; }
        }

        public class ClassInfo
        {
            public string? ClassName { get; set; }
            public string? ClassType { get; set; }
            public bool IsAbstract { get; set; }
            public List<AttributeInfo>? Attributes { get; set; }
            public List<MethodInfo>? Methods { get; set; }
        }

        public class AttributeInfo
        {
            public string? AttributeName { get; set; }
            public string? AttributeType { get; set; }
            public string? Visibility { get; set; }
            public bool IsStatic { get; set; }
        }

        public class MethodInfo
        {
            public string? MethodName { get; set; }
            public string? ReturnType { get; set; }
            public string? Visibility { get; set; }
            public bool IsStatic { get; set; }
            public List<ParameterInfo>? Parameters { get; set; }
        }

        public class ParameterInfo
        {
            public string? ParameterName { get; set; }
            public string? ParameterType { get; set; }
        }

        public class RelationshipInfo
        {
            public string? RelationshipType { get; set; }
            public string? SourceClass { get; set; }
            public string? TargetClass { get; set; }
            public string? SourceCardinality { get; set; }
            public string? TargetCardinality { get; set; }
            public string? RelationshipName { get; set; }
        }
    }
}