using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DescriptionGenerator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public string DiagramJson { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this; // Set DataContext to the Window itself

        // Example JSON Data (replace with your actual JSON)
        DiagramJson = @"
{
  ""Diagram"": {
    ""DiagramName"": ""ExportImportProgramClassDiagram"",
    ""Classes"": [
      {
        ""ClassName"": ""UserManagement"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""addAccount"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""editAccount"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""deleteAccount"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""forgotPassword"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""changePassword"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""assignPermissions"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""user"",
                ""ParameterType"": ""User""
              },
              {
                ""ParameterName"": ""permissions"",
                ""ParameterType"": ""List<String>""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""login"",
            ""ReturnType"": ""boolean"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""username"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""password"",
                ""ParameterType"": ""String""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""Home"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false
      },
      {
        ""ClassName"": ""DataImport"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""connectToERP"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""apiEndpoint"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""importExcel"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""loadMCData"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""loadFGData"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""loadSeizoData"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""loadStandardBOM"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""loadJobBOM"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public""
          }
        ]
      },
      {
        ""ClassName"": ""DataProcessing"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""saveToSoftwareSystem"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""data"",
                ""ParameterType"": ""DataSet""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""processMCData"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""mcData"",
                ""ParameterType"": ""DataSet""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""processFGData"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""fgData"",
                ""ParameterType"": ""DataSet""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""processSeizoData"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""seizoData"",
                ""ParameterType"": ""DataSet""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""processStandardBOM"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""standardBOM"",
                ""ParameterType"": ""DataSet""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""processJobBOM"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""jobBOM"",
                ""ParameterType"": ""DataSet""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""CustomsReport"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""aggregateReportData"",
            ""ReturnType"": ""ReportData"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""searchCriteria"",
                ""ParameterType"": ""SearchCriteria""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""BOMComparison"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""aggregateBOMData"",
            ""ReturnType"": ""ReportData"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""searchCriteria"",
                ""ParameterType"": ""SearchCriteria""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""compareBOMs"",
            ""ReturnType"": ""ComparisonResult"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""standardBOM"",
                ""ParameterType"": ""DataSet""
              },
              {
                ""ParameterName"": ""jobBOM"",
                ""ParameterType"": ""DataSet""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""ReportExport"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""exportKVAReport"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""reportData"",
                ""ParameterType"": ""ReportData""
              },
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""exportCustomsReport15"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""reportData"",
                ""ParameterType"": ""ReportData""
              },
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""exportCustomsReport15a"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""reportData"",
                ""ParameterType"": ""ReportData""
              },
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""BOMExport"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""exportNormBOM"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""bomData"",
                ""ParameterType"": ""DataSet""
              },
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""exportBOMComparison"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""comparisonResult"",
                ""ParameterType"": ""ComparisonResult""
              },
              {
                ""ParameterName"": ""filePath"",
                ""ParameterType"": ""String""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""MasterDataManagement"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""manageCodes"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""codeType"",
                ""ParameterType"": ""String""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""CodeConversion"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""convertCode"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""sourceCode"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""targetCodeType"",
                ""ParameterType"": ""String""
              }
            ]
          }
        ]
      }
    ],
    ""Relationships"": []
  }
}
";
        diagramDisplay.JsonData = DiagramJson;
    }
}