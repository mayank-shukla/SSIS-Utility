using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDSCompare
{
    public class SSISXml
    {
        public string objectexpression;
        public string objectexpressionvalue;
        //public string attrparent;
        public SSISXml()
        {
            objectexpression = "";
            objectexpressionvalue = "";
        }
    }
    public class SSISXmlTask
    {
        public string TaskName;
        public string TaskExpression;
        public SSISXmlTask()
        {
            TaskName = "";
            TaskExpression = "";
        }
    }
}
