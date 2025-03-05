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
    ""DiagramName"": ""ManufacturingDataSystem"",
    ""Classes"": [
      {
        ""ClassName"": ""User"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Attributes"": [
          {
            ""AttributeName"": ""email"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""passwordHash"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""department"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""phoneNumber"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""role"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          }
        ],
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""getEmail"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""getDepartment"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""getPhoneNumber"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""getRole"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""public""
          }
        ]
      },
      {
        ""ClassName"": ""AuthenticationService"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Attributes"": [
          {
            ""AttributeName"": ""userRepository"",
            ""AttributeType"": ""UserRepository"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          }
        ],
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""authenticate"",
            ""ReturnType"": ""User"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""email"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""password"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""hashPassword"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""private"",
            ""Parameters"": [
              {
                ""ParameterName"": ""password"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""verifyPassword"",
            ""ReturnType"": ""boolean"",
            ""Visibility"": ""private"",
            ""Parameters"": [
              {
                ""ParameterName"": ""password"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""hash"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""requestPasswordReset"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""email"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""resetPassword"",
            ""ReturnType"": ""boolean"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""email"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""resetToken"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""newPassword"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""registerUser"",
            ""ReturnType"": ""User"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""email"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""password"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""department"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""phoneNumber"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""role"",
                ""ParameterType"": ""String""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""UserRepository"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""getUserByEmail"",
            ""ReturnType"": ""User"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""email"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""saveUser"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""user"",
                ""ParameterType"": ""User""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""updateUser"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""user"",
                ""ParameterType"": ""User""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""DataProcessingService"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Attributes"": [
          {
            ""AttributeName"": ""materialProcessor"",
            ""AttributeType"": ""MaterialProcessor"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""finishedGoodsProcessor"",
            ""AttributeType"": ""FinishedGoodsProcessor"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""seizoProcessor"",
            ""AttributeType"": ""SeizoProcessor"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          }
        ],
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""processData"",
            ""ReturnType"": ""Map<String, DataSet>"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""dataCategory"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""inputData"",
                ""ParameterType"": ""DataSet""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""MaterialProcessor"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""process"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""inputData"",
                ""ParameterType"": ""DataSet""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""FinishedGoodsProcessor"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""process"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""inputData"",
                ""ParameterType"": ""DataSet""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""SeizoProcessor"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""process"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""inputData"",
                ""ParameterType"": ""DataSet""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""ReportGenerator"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""generateReport"",
            ""ReturnType"": ""Report"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""reportType"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""data"",
                ""ParameterType"": ""DataSet""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""BOMService"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Attributes"": [
          {
            ""AttributeName"": ""bomRepository"",
            ""AttributeType"": ""BOMRepository"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          }
        ],
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""getBOM"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""bomType"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""identifier"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""generateKVAReport"",
            ""ReturnType"": ""Report"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""bomData"",
                ""ParameterType"": ""DataSet""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""BOMRepository"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""getTechnicalBOM"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""productID"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""getActualBOM"",
            ""ReturnType"": ""DataSet"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""jobID"",
                ""ParameterType"": ""String""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""APIService"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": false,
        ""Attributes"": [
          {
            ""AttributeName"": ""apiLink"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""apiUser"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""apiPassword"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          },
          {
            ""AttributeName"": ""apiToken"",
            ""AttributeType"": ""String"",
            ""IsStatic"": false,
            ""Visibility"": ""private""
          }
        ],
        ""Methods"": [
          {
            ""IsStatic"": false,
            ""MethodName"": ""authenticateAndGetToken"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""public""
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""get"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""endpoint"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""post"",
            ""ReturnType"": ""String"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""endpoint"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""data"",
                ""ParameterType"": ""String""
              }
            ]
          },
          {
            ""IsStatic"": false,
            ""MethodName"": ""setApiCredentials"",
            ""ReturnType"": ""void"",
            ""Visibility"": ""public"",
            ""Parameters"": [
              {
                ""ParameterName"": ""apiLink"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""apiUser"",
                ""ParameterType"": ""String""
              },
              {
                ""ParameterName"": ""apiPassword"",
                ""ParameterType"": ""String""
              }
            ]
          }
        ]
      },
      {
        ""ClassName"": ""DataSet"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": true
      },
      {
        ""ClassName"": ""Report"",
        ""ClassType"": ""Class"",
        ""IsAbstract"": true
      }
    ],
    ""Relationships"": [
      {
        ""RelationshipType"": ""Association"",
        ""SourceClass"": ""AuthenticationService"",
        ""TargetClass"": ""User"",
        ""RelationshipName"": ""authenticates"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      },
      {
        ""RelationshipType"": ""Dependency"",
        ""SourceClass"": ""ReportGenerator"",
        ""TargetClass"": ""DataSet"",
        ""RelationshipName"": ""uses"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      },
      {
        ""RelationshipType"": ""Dependency"",
        ""SourceClass"": ""ReportGenerator"",
        ""TargetClass"": ""Report"",
        ""RelationshipName"": ""generates"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      },
      {
        ""RelationshipType"": ""Association"",
        ""SourceClass"": ""BOMService"",
        ""TargetClass"": ""BOMRepository"",
        ""RelationshipName"": ""uses"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      },
      {
        ""RelationshipType"": ""Dependency"",
        ""SourceClass"": ""BOMService"",
        ""TargetClass"": ""Report"",
        ""RelationshipName"": ""generates"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      },
      {
        ""RelationshipType"": ""Composition"",
        ""SourceClass"": ""DataProcessingService"",
        ""TargetClass"": ""MaterialProcessor"",
        ""RelationshipName"": ""has"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      },
      {
        ""RelationshipType"": ""Composition"",
        ""SourceClass"": ""DataProcessingService"",
        ""TargetClass"": ""FinishedGoodsProcessor"",
        ""RelationshipName"": ""has"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      },
      {
        ""RelationshipType"": ""Composition"",
        ""SourceClass"": ""DataProcessingService"",
        ""TargetClass"": ""SeizoProcessor"",
        ""RelationshipName"": ""has"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      },
      {
        ""RelationshipType"": ""Association"",
        ""SourceClass"": ""AuthenticationService"",
        ""TargetClass"": ""UserRepository"",
        ""RelationshipName"": ""uses"",
        ""SourceCardinality"": ""1"",
        ""TargetCardinality"": ""1""
      }
    ]
  }
}
";
        diagramDisplay.SetJsonData(DiagramJson);
    }
}