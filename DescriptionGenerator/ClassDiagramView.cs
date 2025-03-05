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
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Core.Layout;
using Shape = Microsoft.Msagl.Drawing.Shape;
using Edge = Microsoft.Msagl.Drawing.Edge;
using static DescriptionGenerator.ClassDiagramView;
using System.Windows.Forms;
using System.IO;

namespace DescriptionGenerator
{
    /// <summary>
    /// Interaction logic for ClassDiagramView.xaml
    /// </summary>
    public partial class ClassDiagramView
    {
        public enum RelationshipType
        {
            Inheritance,
            Composition,
            Aggregation,
            Dependency,
            Association
        }

        public ClassDiagramView()
        {
            graphViewer = new GViewer();
            _graph = new Graph();
            _relationshipVisibility = new Dictionary<RelationshipType, bool> {
                { RelationshipType.Inheritance, true },
                { RelationshipType.Composition, true },
                { RelationshipType.Aggregation, true },
                { RelationshipType.Association, true },
                { RelationshipType.Dependency, true }
            };
            _edgesByRelationship = new Dictionary<RelationshipType, List<Edge>> {
                { RelationshipType.Inheritance, new List<Edge>() },
                { RelationshipType.Composition, new List<Edge>() },
                { RelationshipType.Aggregation, new List<Edge>() },
                { RelationshipType.Association, new List<Edge>() },
                { RelationshipType.Dependency, new List<Edge>() }
            };
            graphViewer.LayoutEditingEnabled = true;

            graphViewer.LayoutAlgorithmSettingsButtonVisible = true;
        }

        private Graph _graph;
        private List<ClassInfo> _classes;
        private List<RelationshipInfo> _relationships;
        private Dictionary<RelationshipType, bool> _relationshipVisibility;
        private Dictionary<RelationshipType, List<Edge>> _edgesByRelationship;
        public GViewer graphViewer { get; set; }

        public string JsonData
        {
            get { return _jsonData; }
            set
            {
                _jsonData = value;
                UpdateData();
            }
        }

        private string _jsonData;

        public void SetLayoutAlgorithm(LayoutAlgorithmSettings settings)
        {
            if (_graph != null)
            {
                System.Diagnostics.Debug.WriteLine(settings.ToString());
                _graph.LayoutAlgorithmSettings = settings;
            }
        }

        public void UpdateData()
        {
            if (!string.IsNullOrEmpty(JsonData))
            {
                var diagramRoot = JsonSerializer.Deserialize<DiagramRoot>(JsonData);
                if (diagramRoot?.Diagram != null)
                {
                    _classes = diagramRoot.Diagram.Classes ?? [];
                    _relationships = diagramRoot.Diagram.Relationships ?? [];
                }
            }
        }

        public void RenderDiagram()
        {
            // Log out the first 10 lines of the json data for debugging
            System.Diagnostics.Debug.WriteLine("JSON Data:" + ((JsonData.Length > 50) ? JsonData[..50] : JsonData));
            if (_classes == null || _relationships == null)
            {
                return;
            }

            _graph = new Graph();
            foreach (var classInfo in _classes)
            {
                DrawClass(classInfo);

                // Log out the class name for debugging
                System.Diagnostics.Debug.WriteLine("Class: " + classInfo.ClassName);
            }
            foreach (var relationship in _relationships)
            {
                DrawRelationship(relationship);

                // Log out the class name for debugging
                System.Diagnostics.Debug.WriteLine(relationship.SourceClass + " " + relationship.RelationshipType + " " + relationship.TargetClass);
            }
            graphViewer.Graph = _graph;
        }

        private void DrawRelationship(RelationshipInfo relationship)
        {
            if (relationship.RelationshipType == null)
            {
                throw new ArgumentNullException(nameof(relationship.RelationshipType), "RelationshipType cannot be null");
            }

            Edge edge = _graph.AddEdge(relationship.SourceClass, relationship.TargetClass);
            var relationshipType = ParseRelationshipType(relationship.RelationshipType);
            _edgesByRelationship[relationshipType].Add(edge);
            StyleRelationshipEdge(edge, relationshipType);
        }

        private RelationshipType ParseRelationshipType(string typeStr)
        {
            switch (typeStr.ToLowerInvariant())
            {
                case "inheritance":
                case "extends":
                case "inherits":
                    return RelationshipType.Inheritance;

                case "composition":
                case "composedof":
                    return RelationshipType.Composition;

                case "aggregation":
                case "aggregates":
                case "has":
                    return RelationshipType.Aggregation;

                case "dependency":
                case "depends":
                case "uses":
                    return RelationshipType.Dependency;

                case "association":
                case "associates":
                case "references":
                default:
                    return RelationshipType.Association;
            }
        }

        private void StyleRelationshipEdge(Edge edge, object type)
        {
            switch (type)
            {
                case RelationshipType.Inheritance:
                    edge.Attr.ArrowheadAtTarget = ArrowStyle.Default;
                    edge.Attr.ArrowheadLength = 10;
                    edge.LabelText = "inherits";
                    break;

                case RelationshipType.Composition:
                    edge.Attr.ArrowheadAtSource = ArrowStyle.Diamond;
                    edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
                    edge.Attr.LineWidth = 2;
                    edge.LabelText = "composed of";
                    break;

                case RelationshipType.Aggregation:
                    edge.Attr.ArrowheadAtSource = ArrowStyle.Diamond;
                    edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
                    edge.LabelText = "aggregates";
                    break;

                case RelationshipType.Association:
                    edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                    edge.LabelText = "associates";
                    break;

                case RelationshipType.Dependency:
                    edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                    edge.Attr.AddStyle(Microsoft.Msagl.Drawing.Style.Dashed);
                    edge.LabelText = "depends on";
                    break;
            }
        }

        private void DrawClass(ClassInfo classInfo)
        {
            Microsoft.Msagl.Drawing.Node classNode = _graph.AddNode(classInfo.ClassName);
            classNode.LabelText = FormatClassLabel(classInfo);
            classNode.Attr.Shape = Shape.Box;
            classNode.Attr.XRadius = 5;
            classNode.Attr.YRadius = 5;
            classNode.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightBlue;
            classNode.UserData = classInfo;
        }

        private string FormatClassLabel(ClassInfo classInfo)
        {
            if (classInfo == null)
                return "Invalid class data";

            var sb = new StringBuilder();

            // Add class name with class type and abstract indicator
            string classPrefix = classInfo.IsAbstract ? "abstract " : "";
            string classTypeDisplay = string.IsNullOrEmpty(classInfo.ClassType) ? "class" : classInfo.ClassType.ToLower();
            sb.AppendLine($"{classPrefix}{classTypeDisplay} {classInfo.ClassName}");

            // Add separator line
            sb.AppendLine(new string('-', Math.Max(20, (classInfo.ClassName?.Length ?? 0) + 10)));

            // Add attributes section
            sb.AppendLine("Attributes:");
            if (classInfo.Attributes != null && classInfo.Attributes.Any())
            {
                foreach (var attribute in classInfo.Attributes.OrderBy(a => a.Visibility))
                {
                    string staticIndicator = attribute.IsStatic ? "static " : "";
                    string visibilitySymbol = GetVisibilitySymbol(attribute.Visibility);
                    sb.AppendLine($"{visibilitySymbol} {staticIndicator}{attribute.AttributeType} {attribute.AttributeName}");
                }
            }
            else
            {
                sb.AppendLine("  None");
            }

            // Add separator before methods
            sb.AppendLine();

            // Add methods section
            sb.AppendLine("Methods:");
            if (classInfo.Methods != null && classInfo.Methods.Any())
            {
                foreach (var method in classInfo.Methods.OrderBy(m => m.Visibility))
                {
                    string staticIndicator = method.IsStatic ? "static " : "";
                    string visibilitySymbol = GetVisibilitySymbol(method.Visibility);

                    var parameters = method.Parameters != null && method.Parameters.Any()
                        ? string.Join(", ", method.Parameters.Select(p => $"{p.ParameterType} {p.ParameterName}"))
                        : string.Empty;

                    sb.AppendLine($"{visibilitySymbol} {staticIndicator}{method.ReturnType} {method.MethodName}({parameters})");
                }
            }
            else
            {
                sb.AppendLine("  None");
            }

            return sb.ToString();
        }

        // Helper method to convert visibility string to symbol
        private string GetVisibilitySymbol(string visibility)
        {
            if (string.IsNullOrEmpty(visibility))
                return "?";

            return visibility.ToLower() switch
            {
                "public" => "+",
                "private" => "-",
                "protected" => "#",
                "internal" => "~",
                "protected internal" => "#~",
                "private protected" => "-#",
                _ => visibility // Return the original if not recognized
            };
        }

        public void SetRelationshipVisibility(
            RelationshipType inheritance, bool showInheritance,
            RelationshipType composition, bool showComposition,
            RelationshipType aggregation, bool showAggregation,
            RelationshipType association, bool showAssociation,
            RelationshipType dependency, bool showDependency)
        {
            _relationshipVisibility[RelationshipType.Inheritance] = showInheritance;
            _relationshipVisibility[RelationshipType.Composition] = showComposition;
            _relationshipVisibility[RelationshipType.Aggregation] = showAggregation;
            _relationshipVisibility[RelationshipType.Association] = showAssociation;
            _relationshipVisibility[RelationshipType.Dependency] = showDependency;

            // Update edge visibility
            UpdateEdgeVisibility();

            // Refresh the viewer
            graphViewer.Invalidate();
        }

        private void UpdateEdgeVisibility()
        {
            foreach (var relationshipType in _edgesByRelationship.Keys)
            {
                bool isVisible = _relationshipVisibility[relationshipType];
                foreach (var edge in _edgesByRelationship[relationshipType])
                {
                    edge.IsVisible = isVisible;
                }
            }
        }

        public void ZoomIn()
        {
            graphViewer.ZoomF *= 1.2;
        }

        public void ZoomOut()
        {
            graphViewer.ZoomF /= 1.2;
        }

        public void ZoomToFit()
        {
            graphViewer.FitGraphBoundingBox();
        }

        public GViewer GetWpfControl()
        {
            return graphViewer;
        }

        public void ExportToImage(string filePath)
        {
            // TODO: Implememnt image export
            throw new NotImplementedException();
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