using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BIDSCompare
{
    public class Variable
    {
        public string objectExpression;
        public string objectExpressionValue;
        public string DTSID;
        public Variable()
        {
            objectExpression = "";
            objectExpressionValue = "";
            DTSID = "";
        }
    }
    public class VariableReportToDisplay
    {
        public string VariableName;
        public string ChangeType;
        public string XML1;
        public string XML2;
        public VariableReportToDisplay()
        {
            VariableName = "";
            ChangeType = "";
            XML1 = "";
            XML2 = "";
        }
    }
    
}
