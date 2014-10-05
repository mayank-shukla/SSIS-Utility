using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BIDSCompare
{
    public class Task
    {
        public string objectExpression;
        public string objectExpressionValue;
        public int IsDisabled;
        public string IsStoredProc;
        public string creationname;
        public Task()
        {
            objectExpression = "";
            objectExpressionValue = "";
            IsDisabled = 0;
            IsStoredProc = "";
            creationname = "";
        }
    }
    public class DFT
    {
        public string objectName;
        public List<component> componentobj ;
        public DFT(string objectName,string componentName,string OpenRowset,string OpenRowsetVariable,string SqlCommand,
            string SqlCommandVariable,string objectExpression,string objectExpressionValue,int IsDisabled,string IsStoredProc,
            string ExecutableType, string DTSID, string componentClassID, string componentID)
        {
            this.objectName = objectName;
            componentobj = new List<component>();
            componentobj.Add(new component()
            {
                componentName = componentName ,
                OpenRowset = OpenRowset ,
                OpenRowsetVariable = OpenRowsetVariable ,
                SqlCommand = SqlCommand ,
                SqlCommandVariable = SqlCommandVariable ,
                objectExpression = objectExpression ,
                objectExpressionValue = objectExpressionValue ,
                IsDisabled = IsDisabled ,
                IsStoredProc = IsStoredProc,
                ExecutableType = ExecutableType,
                DTSID = DTSID ,
                componentClassID = componentClassID,
                componentID=componentID
            });
        }
    }
    public class component
    {

        public string componentName;
        public string OpenRowset;
        public string OpenRowsetVariable;
        public string SqlCommand;
        public string SqlCommandVariable;
        public string objectExpression;
        public string objectExpressionValue;
        public int IsDisabled;
        public string IsStoredProc;
        public string ExecutableType;
        public string DTSID;
        public string componentClassID;
        public string componentID;
        public component()
        {
            componentName = "";
            OpenRowset = "";
            OpenRowsetVariable = "";
            SqlCommand = "";
            SqlCommandVariable = "";
            objectExpression= "";
            objectExpressionValue= "";
            IsDisabled= 0;
            IsStoredProc= "";
            ExecutableType = "";
            DTSID = "";
            componentClassID = "";
            componentID = "";
        }
    }
    public class TaskReport
    {
        public string TaskName;
        public string ChangeType;
        public string XML1;
        public string XML2;
        public TaskReport()
        {
            TaskName = "";
            ChangeType = "";
            XML1 = "";
            XML2 = "";
        }
    }
    public class VariableReport
    {
        public string VariableName;
        public string ChangeType;
        public string XML1;
        public string XML2;
        public VariableReport()
        {
            VariableName = "";
            ChangeType = "";
            XML1 = "";
            XML2 = "";
        }
    }
    public class AnalyzeTaskReport
    {
        public string TaskName_Report;
        public string description_Report;
        public string TaskType_Report;
        public string TaskExpression_Report;
        public string TaskEvaluatedValue_Report;
        public AnalyzeTaskReport()
        {
            TaskName_Report = "";
            description_Report = "";
            TaskType_Report = "";
            TaskEvaluatedValue_Report = "";
            TaskExpression_Report = "";
        }
    }
    public class AnalyzeTaskResultSet
    {
        public string TaskName_ResultSet;
        public string ResultSetName;
        public AnalyzeTaskResultSet()
        {
            TaskName_ResultSet = "";
            ResultSetName = "";
        }
    }
    public class AnalyzeTaskParameter
    {
        public string TaskName_Parameter;
        public string ParameterName;
        public AnalyzeTaskParameter()
        {
            TaskName_Parameter = "";
            ParameterName = "";
        }
    }
}
