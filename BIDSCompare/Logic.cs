using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;

namespace BIDSCompare
{
    public class Logic
    {
        public event ProgressChangedEventHandler ProgressChanged,ProgressChanged2;

        protected virtual void OnProgressChanged(int progress)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(progress, null));
            }
        }

        private List<string> varcomparereport= new List<string>();
        private List<TaskReport> advtaskcomparereport;
        private List<VariableReport> advvarcomparereport,advvarcomparereportdup;// = new List<VariableReport>();
        #region XML1Variables
        List<string> variablenodes = new List<string>();
        List<string> tasknodes = new List<string>();
        Dictionary<string, Task> tasklist = new Dictionary<string, Task>();
        public Dictionary<string, Variable> variablelist = new Dictionary<string, Variable>();
        
        List<KeyValuePair<string, string>> templist = new List<KeyValuePair<string, string>>();
        public List<DFT> dftlist = new List<DFT>();
        #endregion
        #region XML2Variables
        List<string> variablenodes2 = new List<string>();
        List<string> tasknodes2 = new List<string>();
        Dictionary<string, Task> tasklist2 = new Dictionary<string, Task>();
        public Dictionary<string, Variable> variablelist2 = new Dictionary<string, Variable>();
        
        List<KeyValuePair<string, string>> templist2 = new List<KeyValuePair<string, string>>();
        public List<DFT> dftlist2 = new List<DFT>();
        #endregion
        public void ReloadLogicxml1()
        {
            variablelist.Clear();
            variablenodes.Clear();
            tasklist.Clear();
            tasknodes.Clear();
            templist.Clear();
            dftlist.Clear();
        }
        public void ReloadLogicxml2()
        {
            variablelist2.Clear();
            variablenodes2.Clear();
            tasklist2.Clear();
            tasknodes2.Clear();
            templist2.Clear();
            dftlist2.Clear();
        }
        #region MethodsXML1
        public int CalcVar(string str)
        {
            XDocument xdoc;
          
            variablenodes.Clear();
            xdoc = XDocument.Parse(str);
            variablenodes.AddRange(
                xdoc.Elements().Nodes().
                Where(nodes => nodes.ToString().StartsWith("<DTS:Variable")).
                Select(nodes => nodes.ToString())
                );
            xdoc = null;
            variablenodes.ForEach(nodes =>
                {
                    xdoc = null;
                    xdoc = XDocument.Parse(nodes);
                    xdoc.Descendants().Elements().Where(elem => elem.Name.LocalName != "Envelope").
                        ToList().ForEach(elm =>
                    {
                        elm.Attributes().ToList().ForEach(xattr =>
                        {
                            
                            templist.Add(new KeyValuePair<string, string>(xattr.Value, elm.Value));
                        });
                    });
                    variablelist.Add(
                        templist.Where(x => x.Key == "ObjectName").Select(y => y.Value).
                        FirstOrDefault(),
                        new Variable()
                            {
                                objectExpression = (templist.Where(x => x.Key == "Expression").
                                     Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "Expression").Select(y => y.Value).FirstOrDefault(),
                                objectExpressionValue = (templist.Where(x => x.Key == "8").
                                     Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "8").Select(y => y.Value).FirstOrDefault(),
                                DTSID = (templist.Where(x => x.Key == "DTSID").
                                Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "DTSID").Select(y => y.Value).FirstOrDefault()
                            });
                    templist.Clear();
                });
    
            return variablenodes.Count();
        }
        private void AddTaskNodes(string str)
        {
            XDocument xdoc;
            xdoc = XDocument.Parse(str);
            xdoc.Elements().Nodes().ToList().ForEach(nodes =>
                {
                    if (nodes.ToString().StartsWith("<DTS:Executable"))
                        AddTaskNodes(nodes.ToString());
                    tasknodes.Add(nodes.ToString());
                });
        }
        public int CalTask(string str)
        {
            
            AddTaskNodes(str);
            XDocument xdoc;
            xdoc = null;
            tasknodes.RemoveAll(nodes => !(nodes.ToString().StartsWith("<DTS:Executable")));
            templist.Clear();
            int currentProgress = 1;
            
            #region TaskNodes
            tasknodes.ForEach(nodes =>
                {
                    currentProgress = currentProgress + 10;
                    if (currentProgress > 10000)
                    {
                        currentProgress = 1;
                    }
                    xdoc = XDocument.Parse(nodes);
                    xdoc.Elements().Attributes().ToList().ForEach(attr =>
                        {
                            if (attr.Name.LocalName.ToString() == "ExecutableType")
                                templist.Add(new KeyValuePair<string, string>(attr.Name.LocalName.ToString(), attr.Value));
                        });


                    if (xdoc.Descendants().Elements().ToList().Exists(
                        elm =>
                            elm.Attributes().ToList().
                            Exists(attr => attr.Value == "CreationName"
                                && elm.Value.ToString().StartsWith("SSIS.Pipeline."))
                        ))
                    {
                        xdoc.Descendants().Elements().
                            ToList().ForEach(elms =>
                            {
                                currentProgress = currentProgress + 1;
                                if (currentProgress > 10000)
                                {
                                    currentProgress = 1;
                                }

                                //OnProgressChanged(currentProgress);
                                elms.Attributes().ToList().ForEach(xattr =>
                                {
                                    if (xattr.Value == "DTSID")
                                    {
                                        //templist.Add(xattr.Value, elm.Value);
                                        templist.Add(new KeyValuePair<string, string>(xattr.Value, elms.Value));
                                    }
                                });
                            });
                        currentProgress = currentProgress + 10;
                        if (currentProgress > 10000)
                        {
                            currentProgress = 1;
                        }
                                
                        xdoc.Descendants("components").Descendants("component").ToList().ForEach(desc =>
                            {
                                xdoc.Descendants().Elements().
                            ToList().ForEach(elm =>
                            {
                                currentProgress = currentProgress + 10;
                                if (currentProgress > 10000)
                                {
                                    currentProgress = 1;
                                }
                                elm.Attributes().ToList().ForEach(xattr =>
                                {
                                    if (xattr.Value == "ObjectName")
                                    {
                                        //templist.Add(xattr.Value, elm.Value);
                                        templist.Add(new KeyValuePair<string, string>(xattr.Value, elm.Value));
                                    }
                                    if (xattr.Name.LocalName == "ExecutableType")
                                    {
                                        templist.Add(new KeyValuePair<string, string>(xattr.Name.LocalName.ToString(), xattr.Value));
                                    }
                                });
                            });
                                desc.Attributes().ToList().ForEach(attr =>
                                {
                                    if (attr.Name.LocalName.ToString() == "componentClassID" || attr.Name.LocalName.ToString() == "id")
                                        templist.Add(new KeyValuePair<string, string>(attr.Name.LocalName.ToString(), attr.Value));
                                });
                                desc.Descendants("properties").Elements().ToList().ForEach(prop =>
                                    {
                                        prop.Attributes().ToList().ForEach(attr =>
                                            {
                                                if (attr.Value == "OpenRowset" || attr.Value == "OpenRowsetVariable"
                                                        || attr.Value == "SqlCommand" || attr.Value == "SqlCommandVariable"
                                                    || attr.Value == "Disabled" || attr.Value == "8")
                                                {
                                                    //templist.Add(attr.Value, prop.Value);
                                                    templist.Add(new KeyValuePair<string, string>(attr.Value, prop.Value));
                                                }
                                                if (attr.Name.LocalName.ToString() == "SqlStatementSource" ||
                                                    attr.Name.LocalName.ToString() == "IsStoredProc" ||
                                                    attr.Name.LocalName.ToString() == "ExecutableType")
                                                {
                                                    templist.Add(new KeyValuePair<string, string>(attr.Name.LocalName, attr.Value));
                                                }
                                            });
                                    });
                                desc.Attributes().ToList().ForEach(attr =>
                                {
                                    currentProgress = currentProgress + 10;
                                    if (currentProgress > 10000)
                                    {
                                        currentProgress = 1;
                                    }
                                    if (attr.Name.LocalName.ToString() == "name" || attr.Name.LocalName.ToString() == "ExecutableType")
                                    {
                                        //templist.Add(attr.Name.LocalName.ToString(), attr.Value);
                                        templist.Add(new KeyValuePair<string, string>(attr.Name.LocalName.ToString(), attr.Value));
                                    }
                                });
                                dftlist.Add(
                                    new DFT(
                                                (templist.Where(x => x.Key == "ObjectName").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "ObjectName").Select(y => y.Value).FirstOrDefault(),
                                                (templist.Where(x => x.Key == "name").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "name").Select(y => y.Value).FirstOrDefault(),
                                                (templist.Where(x => x.Key == "OpenRowset").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "OpenRowset").Select(y => y.Value).FirstOrDefault(),
                                                (templist.Where(x => x.Key == "OpenRowsetVariable").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "OpenRowsetVariable").Select(y => y.Value).FirstOrDefault(),
                                                (templist.Where(x => x.Key == "SqlCommand").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "SqlCommand").Select(y => y.Value).FirstOrDefault(),
                                                (templist.Where(x => x.Key == "SqlCommandVariable").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "SqlCommandVariable").Select(y => y.Value).FirstOrDefault(),
                                               (templist.Where(x => x.Key == "SqlStatementSource").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "SqlStatementSource").Select(y => y.Value).FirstOrDefault(),
                                               (templist.Where(x => x.Key == "8").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "8").Select(y => y.Value).FirstOrDefault(),
                                               (templist.Where(x => x.Key == "Disabled").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? 0 : int.Parse(templist.Where(x => x.Key == "Disabled").Select(y => y.Value).FirstOrDefault()),
                                                (templist.Where(x => x.Key == "IsStoredProc").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "IsStoredProc").Select(y => y.Value).FirstOrDefault(),
                                                (templist.Where(x => x.Key == "ExecutableType").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "ExecutableType").Select(y => y.Value).FirstOrDefault(),
                                                   (templist.Where(x => x.Key == "DTSID").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "DTSID").Select(y => y.Value).FirstOrDefault(),
                                                (templist.Where(x => x.Key == "componentClassID").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "componentClassID").Select(y => y.Value).FirstOrDefault(),
                                                    (templist.Where(x => x.Key == "id").
                                                    Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "id").Select(y => y.Value).FirstOrDefault()
                                            ));
                                templist.Clear();
                            });
                    }
                    else
                    {
                     
                        xdoc.Descendants().Elements().
                            ToList().ForEach(elm =>
                            {
                                currentProgress = currentProgress + 1;
                                if (currentProgress > 10000)
                                {
                                    currentProgress = 1;
                                }
                              
                                OnProgressChanged(currentProgress);
                                elm.Attributes().ToList().ForEach(xattr =>
                                {
                                    if (xattr.Value == "ObjectName" || xattr.Value == "8" || xattr.Value == "Disabled" ||
                                        xattr.Value == "DTSID" || xattr.Value == "CreationName" ||
                                        xattr.Value == "OpenRowset" || xattr.Value == "OpenRowsetVariable" ||
                                        xattr.Value == "SqlCommand" || xattr.Value == "SqlCommandVariable")
                                    {
                                        //templist.Add(xattr.Value, elm.Value);
                                        templist.Add(new KeyValuePair<string, string>(xattr.Value, elm.Value));
                                    }
                                    if (xattr.Name.LocalName.ToString() == "SqlStatementSource" ||
                                        xattr.Name.LocalName.ToString() == "IsStoredProc" ||
                                        xattr.Name.LocalName.ToString() == "ExecutableType")
                                    {
                                        //templist.Add(xattr.Name.LocalName.ToString(), xattr.Value);
                                        templist.Add(new KeyValuePair<string, string>(xattr.Name.LocalName.ToString(), xattr.Value));
                                    }
                                });
                            });
                        dftlist.Add(
                               new DFT(
                                           (templist.Where(x => x.Key == "ObjectName").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "ObjectName").Select(y => y.Value).FirstOrDefault(),
                                           (templist.Where(x => x.Key == "name").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "name").Select(y => y.Value).FirstOrDefault(),
                                           (templist.Where(x => x.Key == "OpenRowset").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "OpenRowset").Select(y => y.Value).FirstOrDefault(),
                                           (templist.Where(x => x.Key == "OpenRowsetVariable").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "OpenRowsetVariable").Select(y => y.Value).FirstOrDefault(),
                                           (templist.Where(x => x.Key == "SqlCommand").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "SqlCommand").Select(y => y.Value).FirstOrDefault(),
                                           (templist.Where(x => x.Key == "SqlCommandVariable").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "SqlCommandVariable").Select(y => y.Value).FirstOrDefault(),
                                          (templist.Where(x => x.Key == "SqlStatementSource").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "SqlStatementSource").Select(y => y.Value).FirstOrDefault(),
                                          (templist.Where(x => x.Key == "8").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "8").Select(y => y.Value).FirstOrDefault(),
                                          (templist.Where(x => x.Key == "Disabled").
                                               Select(y => y.Value).FirstOrDefault()) == null ? 0 : int.Parse(templist.Where(x => x.Key == "Disabled").Select(y => y.Value).FirstOrDefault()),
                                           (templist.Where(x => x.Key == "IsStoredProc").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "IsStoredProc").Select(y => y.Value).FirstOrDefault(),
                                           (templist.Where(x => x.Key == "ExecutableType").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "ExecutableType").Select(y => y.Value).FirstOrDefault(),
                                           (templist.Where(x => x.Key == "DTSID").
                                               Select(y => y.Value).FirstOrDefault()) == null ? "" : templist.Where(x => x.Key == "DTSID").Select(y => y.Value).FirstOrDefault(),
                                               ""/*componentClassID*/,
                                               ""/*componentID*/
                                       ));
                        templist.Clear();
                    }
                });
            #endregion
            return (dftlist.Count());
        }
        #endregion
        #region MethodsXML2
        private void AddTaskNodes2(string str)
        {
            XDocument xdoc;
            xdoc = XDocument.Parse(str);
            xdoc.Elements().Nodes().ToList().ForEach(nodes =>
            {
                if (nodes.ToString().StartsWith("<DTS:Executable"))
                    AddTaskNodes2(nodes.ToString());
                tasknodes2.Add(nodes.ToString());
            });
        }
        public int CalcVar2(string str)
        {
            XDocument xdoc;
          
            variablenodes2.Clear();
            xdoc = XDocument.Parse(str);
            variablenodes2.AddRange(
                xdoc.Elements().Nodes().
                Where(nodes => nodes.ToString().StartsWith("<DTS:Variable")).
                Select(nodes => nodes.ToString())
                );
            xdoc = null;
            variablenodes2.ForEach(nodes =>
            {
                xdoc = XDocument.Parse(nodes);
                xdoc.Descendants().Elements().Where(elem => elem.Name.LocalName != "Envelope").
                    ToList().ForEach(elm =>
                    {
                        elm.Attributes().ToList().ForEach(xattr =>
                        {
                            // templist2.Add(xattr.Value , elm.Value);
                            templist2.Add(new KeyValuePair<string, string>(xattr.Value, elm.Value));
                        });
                    });
                variablelist2.Add(
                    templist2.Where(x => x.Key == "ObjectName").Select(y => y.Value).
                    FirstOrDefault(),
                    new Variable()
                    {
                        objectExpression = (templist2.Where(x => x.Key == "Expression").
                             Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "Expression").Select(y => y.Value).FirstOrDefault(),
                        objectExpressionValue = (templist2.Where(x => x.Key == "8").
                             Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "8").Select(y => y.Value).FirstOrDefault(),
                        DTSID = (templist2.Where(x => x.Key == "DTSID").
                        Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "DTSID").Select(y => y.Value).FirstOrDefault()
                    });
                templist2.Clear();
            });
          
            return variablenodes2.Count();
        }
        public int CalTask2(string str)
        {
            int currentProgress = 1;
            AddTaskNodes2(str);
            XDocument xdoc;
            xdoc = null;
            tasknodes2.RemoveAll(nodes => !(nodes.ToString().StartsWith("<DTS:Executable")));
            templist2.Clear();
            //tasknodes2.RemoveAt(0);
            tasknodes2.ForEach(nodes =>
            {
                xdoc = XDocument.Parse(nodes);

                xdoc.Elements().Attributes().ToList().ForEach(attr =>
                {
                    if (attr.Name.LocalName.ToString() == "ExecutableType")
                        templist2.Add(new KeyValuePair<string, string>(attr.Name.LocalName.ToString(), attr.Value));
                });
                if (xdoc.Descendants().Elements().ToList().Exists(
                    elm =>
                        elm.Attributes().ToList().
                        Exists(attr => attr.Value == "CreationName"
                            && elm.Value.ToString().StartsWith("SSIS.Pipeline."))
                    ))
                {
                    xdoc.Descendants().Elements().
                           ToList().ForEach(elms =>
                           {
                               currentProgress = currentProgress + 1;
                               if (currentProgress > 10000)
                               {
                                   currentProgress = 1;
                               }

                               //OnProgressChanged(currentProgress);
                               elms.Attributes().ToList().ForEach(xattr =>
                               {
                                   if (xattr.Value == "DTSID")
                                   {
                                       //templist.Add(xattr.Value, elm.Value);
                                       templist2.Add(new KeyValuePair<string, string>(xattr.Value, elms.Value));
                                   }
                               });
                           });
                    currentProgress = currentProgress + 10;
                    if (currentProgress > 10000)
                    {
                        currentProgress = 1;
                    }
                              
                    xdoc.Descendants("components").Descendants("component").ToList().ForEach(desc =>
                    {
                        xdoc.Descendants().Elements().
                    ToList().ForEach(elm =>
                    {
                       
                              
                        elm.Attributes().ToList().ForEach(xattr =>
                        {
                            if (xattr.Value == "ObjectName")
                            {
                                //templist2.Add(xattr.Value, elm.Value);
                                templist2.Add(new KeyValuePair<string, string>(xattr.Value, elm.Value));
                            }
                            if (xattr.Name.LocalName == "ExecutableType")
                            {
                                templist2.Add(new KeyValuePair<string, string>(xattr.Name.LocalName.ToString(), xattr.Value));
                            }
                        });
                    });
                        desc.Attributes().ToList().ForEach(attr =>
                        {
                            currentProgress = currentProgress + 10;
                            if (currentProgress > 10000)
                            {
                                currentProgress = 1;
                            }

                            if (attr.Name.LocalName.ToString() == "componentClassID" || attr.Name.LocalName.ToString() == "id")
                                templist2.Add(new KeyValuePair<string, string>(attr.Name.LocalName.ToString(), attr.Value));
                        });
                        desc.Descendants("properties").Elements().ToList().ForEach(prop =>
                        {
                            prop.Attributes().ToList().ForEach(attr =>
                            {
                                currentProgress = currentProgress + 10;
                                if (currentProgress > 10000)
                                {
                                    currentProgress = 1;
                                }
                          
                                if (attr.Value == "OpenRowset" || attr.Value == "OpenRowsetVariable"
                                        || attr.Value == "SqlCommand" || attr.Value == "SqlCommandVariable"
                                    || attr.Value == "Disabled" || attr.Value == "8")
                                {
                                    //templist2.Add(attr.Value, prop.Value);
                                    templist2.Add(new KeyValuePair<string, string>(attr.Value, prop.Value));
                                }
                                if (attr.Name.LocalName.ToString() == "SqlStatementSource" ||
                                    attr.Name.LocalName.ToString() == "IsStoredProc" ||
                                    attr.Name.LocalName.ToString() == "ExecutableType")
                                {
                                    templist2.Add(new KeyValuePair<string, string>(attr.Name.LocalName, attr.Value));
                                }
                            });
                        });
                        desc.Attributes().ToList().ForEach(attr =>
                        {
                            currentProgress = currentProgress + 10;
                            if (currentProgress > 10000)
                            {
                                currentProgress = 1;
                            }
                          
                            if (attr.Name.LocalName.ToString() == "name" || attr.Name.LocalName.ToString() == "ExecutableType")
                            {
                                //templist2.Add(attr.Name.LocalName.ToString(), attr.Value);
                                templist2.Add(new KeyValuePair<string, string>(attr.Name.LocalName.ToString(), attr.Value));
                            }
                        });
                        dftlist2.Add(
                            new DFT(
                                        (templist2.Where(x => x.Key == "ObjectName").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "ObjectName").Select(y => y.Value).FirstOrDefault(),
                                        (templist2.Where(x => x.Key == "name").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "name").Select(y => y.Value).FirstOrDefault(),
                                        (templist2.Where(x => x.Key == "OpenRowset").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "OpenRowset").Select(y => y.Value).FirstOrDefault(),
                                        (templist2.Where(x => x.Key == "OpenRowsetVariable").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "OpenRowsetVariable").Select(y => y.Value).FirstOrDefault(),
                                        (templist2.Where(x => x.Key == "SqlCommand").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "SqlCommand").Select(y => y.Value).FirstOrDefault(),
                                        (templist2.Where(x => x.Key == "SqlCommandVariable").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "SqlCommandVariable").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "SqlStatementSource").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "SqlStatementSource").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "8").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "8").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "Disabled").
                                            Select(y => y.Value).FirstOrDefault()) == null ? 0 : int.Parse(templist2.Where(x => x.Key == "Disabled").Select(y => y.Value).FirstOrDefault()),
                                        (templist2.Where(x => x.Key == "IsStoredProc").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "IsStoredProc").Select(y => y.Value).FirstOrDefault(),
                                        (templist2.Where(x => x.Key == "ExecutableType").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "ExecutableType").Select(y => y.Value).FirstOrDefault(),
                                            (templist2.Where(x => x.Key == "DTSID").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "DTSID").Select(y => y.Value).FirstOrDefault(),
                                        (templist2.Where(x => x.Key == "componentClassID").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "componentClassID").Select(y => y.Value).FirstOrDefault(),
                                            (templist2.Where(x => x.Key == "id").
                                            Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "id").Select(y => y.Value).FirstOrDefault()
                                    ));
                        templist2.Clear();
                    });
                }
                else
                {
                    xdoc.Descendants().Elements().
                        ToList().ForEach(elm =>
                        {
                            currentProgress = currentProgress + 10;
                            if (currentProgress > 10000)
                            {
                                currentProgress = 1;
                            }
                          
                            elm.Attributes().ToList().ForEach(xattr =>
                            {
                                if (xattr.Value == "ObjectName" || xattr.Value == "8" || xattr.Value == "Disabled" ||
                                    xattr.Value == "DTSID" || xattr.Value == "CreationName" ||
                                    xattr.Value == "OpenRowset" || xattr.Value == "OpenRowsetVariable" ||
                                    xattr.Value == "SqlCommand" || xattr.Value == "SqlCommandVariable")
                                {
                                    //templist2.Add(xattr.Value, elm.Value);
                                    templist2.Add(new KeyValuePair<string, string>(xattr.Value, elm.Value));
                                }
                                if (xattr.Name.LocalName.ToString() == "SqlStatementSource" ||
                                    xattr.Name.LocalName.ToString() == "IsStoredProc" ||
                                    xattr.Name.LocalName.ToString() == "ExecutableType")
                                {
                                    //templist2.Add(xattr.Name.LocalName.ToString(), xattr.Value);
                                    templist2.Add(new KeyValuePair<string, string>(xattr.Name.LocalName.ToString(), xattr.Value));
                                }
                            });
                        });
                    dftlist2.Add(
                           new DFT(
                                       (templist2.Where(x => x.Key == "ObjectName").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "ObjectName").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "name").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "name").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "OpenRowset").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "OpenRowset").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "OpenRowsetVariable").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "OpenRowsetVariable").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "SqlCommand").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "SqlCommand").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "SqlCommandVariable").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "SqlCommandVariable").Select(y => y.Value).FirstOrDefault(),
                                      (templist2.Where(x => x.Key == "SqlStatementSource").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "SqlStatementSource").Select(y => y.Value).FirstOrDefault(),
                                      (templist2.Where(x => x.Key == "8").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "8").Select(y => y.Value).FirstOrDefault(),
                                      (templist2.Where(x => x.Key == "Disabled").
                                           Select(y => y.Value).FirstOrDefault()) == null ? 0 : int.Parse(templist2.Where(x => x.Key == "Disabled").Select(y => y.Value).FirstOrDefault()),
                                       (templist2.Where(x => x.Key == "IsStoredProc").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "IsStoredProc").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "ExecutableType").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "ExecutableType").Select(y => y.Value).FirstOrDefault(),
                                       (templist2.Where(x => x.Key == "DTSID").
                                           Select(y => y.Value).FirstOrDefault()) == null ? "" : templist2.Where(x => x.Key == "DTSID").Select(y => y.Value).FirstOrDefault(),
                                           ""/*componentClassID*/,
                                           ""/*componentID*/
                                   ));
                    templist2.Clear();
                }
            });

            return (dftlist2.Count());
        }
        #endregion
        public List<VariableReport> CompareVariables()
        {
            varcomparereport = null;
            varcomparereport = new List<string>();
            advvarcomparereport = null;
            advvarcomparereport = new List<VariableReport>();
            advvarcomparereportdup = new List<VariableReport>();
            try
            {
                variablelist.ToList().ForEach(var1 =>
                    {
                        if (!(variablelist2.ToList().Exists(var2 => var2.Value.DTSID == var1.Value.DTSID)))
                        {
                            //varcomparereport.Add("Variable " + var1.Key + " is missing in XML2");
                            advvarcomparereport.Add(new VariableReport()
                            {
                                VariableName = var1.Key ,
                                ChangeType = "ElementMissing",
                                XML1 = "Present In XML1",
                                XML2 = "Missing In XML2"
                            });
                        }
                        variablelist2.ToList().ForEach(var2 =>
                            {
                                if (var1.Value.DTSID == var2.Value.DTSID)
                                {
                                    if (var1.Key.ToString() != var2.Key.ToString())
                                    {
                                        //varcomparereport.Add("Variable Name < " + var1.Key.ToString() + " > In XML1 Changed to < " + var2.Key.ToString() + " > In XML2");
                                        if (!(advvarcomparereport.Exists(row =>
                                        (
                                        (row.VariableName == (var1.Key.ToString() + " / " + var2.Key.ToString()))
                                        &&
                                        (row.ChangeType == "VariableName")
                                        &&
                                        ((row.XML1 == var1.Key.ToString()) || (row.XML1 == var2.Key.ToString()))
                                        &&
                                        ((row.XML2 == var2.Key.ToString()) || (row.XML2 == var1.Key.ToString()))
                                        ))))
                                        {
                                            advvarcomparereport.Add(new VariableReport()
                                            {
                                                VariableName = var1.Key.ToString() + " / " + var2.Key.ToString(),
                                                ChangeType = "VariableName",
                                                XML1 = var1.Key.ToString(),
                                                XML2 = var2.Key.ToString()
                                            });
                                        }
                                    }
                                }
                                if ((var1.Value.DTSID == var2.Value.DTSID) && (var1.Key.ToString() == var2.Key.ToString()))
                                {
                                    if (var1.Value.objectExpression != var2.Value.objectExpression)
                                    {
                                       //varcomparereport.Add("Variable < " + var1.Key.ToString() + " > In XML1 Expression Changed To <" + var1.Value.objectExpression + "> From <" + var2.Value.objectExpression + "> In XML2");
                                        if (!(advvarcomparereport.Exists(row =>
                                       (
                                       ((row.VariableName == var1.Key.ToString()) || (row.VariableName == var2.Key.ToString()))
                                       &&
                                       (row.ChangeType == "Expression")
                                       &&
                                       ((row.XML1 == var1.Value.objectExpression.ToString()) || (row.XML1 == var2.Value.objectExpression.ToString()))
                                       &&
                                       ((row.XML2 == var2.Value.objectExpression.ToString()) || (row.XML2 == var1.Value.objectExpression.ToString()))
                                       ))))
                                        {
                                            advvarcomparereport.Add(new VariableReport()
                                            {
                                                VariableName = var1.Key.ToString(),
                                                ChangeType = "Expression",
                                                XML1 = var1.Value.objectExpression.ToString(),
                                                XML2 = var2.Value.objectExpression.ToString()
                                            });
                                        }
                                    }
                                    if (var1.Value.objectExpressionValue != var2.Value.objectExpressionValue)
                                    {
                                        //varcomparereport.Add("Variable < " + var1.Key.ToString() + " > In XML1 ExpressionValue Changed To <" + var1.Value.objectExpressionValue + "> From <" + var2.Value.objectExpressionValue + "> In XML2");
                                        if (!(advvarcomparereport.Exists(row =>
                                      (
                                      (row.VariableName == var2.Key.ToString())
                                      &&
                                      (row.ChangeType == "ExpressionValue")
                                      &&
                                      (row.XML1 == var2.Value.objectExpressionValue.ToString())
                                      &&
                                      (row.XML2 == var2.Value.objectExpressionValue.ToString())
                                      ))))
                                        {
                                            advvarcomparereport.Add(new VariableReport()
                                            {
                                                VariableName = var1.Key.ToString(),
                                                ChangeType = "ExpressionValue",
                                                XML1 = var1.Value.objectExpressionValue.ToString(),
                                                XML2 = var2.Value.objectExpressionValue.ToString()
                                            });
                                        }
                                    }
                                }
                            });
                    });
                variablelist2.ToList().ForEach(var2 =>
                {
                    if (!(variablelist.ToList().Exists(var1 => var1.Value.DTSID == var2.Value.DTSID)))
                    {
                        //varcomparereport.Add("Variable " + var2.Key + " is missing in XML1");
                        if(!(advvarcomparereport .Exists (row =>
                            (row.VariableName == var2.Key && row.ChangeType == "ElementMissing"))))
                        {
                            advvarcomparereport.Add(new VariableReport()
                            {
                                VariableName = var2.Key,
                                ChangeType = "ElementMissing",
                                XML1 = "Missing In XML1",
                                XML2 = "Present In XML2"
                            });
                        }
                    }
                    variablelist.ToList().ForEach(var1 =>
                    {
                        if (var2.Value.DTSID == var1.Value.DTSID)
                        {
                            if (var2.Key.ToString() != var1.Key.ToString())
                            {
                                if (!(advvarcomparereport.Exists(row =>
                                    (
                                    (row.VariableName == (var1.Key.ToString() + " / " + var2.Key.ToString()))
                                    &&
                                    (row.ChangeType == "VariableName")
                                    &&
                                    ((row.XML1 == var1.Key.ToString()) || (row.XML1 == var2.Key.ToString()))
                                    &&
                                    ((row.XML2 == var2.Key.ToString()) || (row.XML2 == var1.Key.ToString()))
                                    ))))
                                {
                                    advvarcomparereport.Add(new VariableReport()
                                    {
                                        VariableName = var1.Key.ToString() + " / " + var2.Key.ToString(),
                                        ChangeType = "VariableName",
                                        XML1 = var1.Key.ToString(),
                                        XML2 = var2.Key.ToString()
                                    });
                                }
                                //varcomparereport.Add("Variable Name < " + var2.Key.ToString() + " > In XML2 Changed to < " + var1.Key.ToString() + " > In XML1");
                            }
                        }
                        if ((var2.Value.DTSID == var1.Value.DTSID) && (var2.Key.ToString() == var1.Key.ToString ()))
                        {
                            if (var2.Value.objectExpression != var1.Value.objectExpression)
                            {
                                if (!(advvarcomparereport.Exists(row =>
                                    (
                                    ((row.VariableName == var1.Key.ToString()) || (row.VariableName == var2.Key.ToString()))
                                    &&
                                    (row.ChangeType == "Expression")
                                    &&
                                    ((row.XML1 == var1.Value.objectExpression.ToString()) || (row.XML1 == var2.Value.objectExpression.ToString()))
                                    &&
                                    ((row.XML2 == var2.Value.objectExpression.ToString()) || (row.XML2 == var1.Value.objectExpression.ToString()))
                                    ))))
                                {
                                    advvarcomparereport.Add(new VariableReport()
                                    {
                                        VariableName = var1.Key.ToString(),
                                        ChangeType = "Expression",
                                        XML1 = var1.Value.objectExpression.ToString(),
                                        XML2 = var2.Value.objectExpression.ToString()
                                    });
                                }
                                //varcomparereport.Add("Variable < " + var2.Key.ToString() + " > In XML2 Expression Changed To <" + var2.Value.objectExpression + "> From <" + var1.Value.objectExpression + "> In XML1");
                            }
                            if (var2.Value.objectExpressionValue != var1.Value.objectExpressionValue)
                            {
                                if (!(advvarcomparereport.Exists(row =>
                                   (
                                   (row.VariableName == var2.Key.ToString())
                                   &&
                                   (row.ChangeType == "ExpressionValue")
                                   &&
                                   (row.XML1 == var2.Value.objectExpressionValue .ToString())
                                   &&
                                   (row.XML2 == var2.Value.objectExpressionValue.ToString())
                                   ))))
                                {
                                    advvarcomparereport.Add(new VariableReport()
                                    {
                                        VariableName = var1.Key.ToString(),
                                        ChangeType = "ExpressionValue",
                                        XML1 = var1.Value.objectExpressionValue.ToString(),
                                        XML2 = var2.Value.objectExpressionValue.ToString()
                                    });
                                }
                                //varcomparereport.Add("Variable < " + var2.Key.ToString() + " > In XML2 ExpressionValue Changed To <" + var2.Value.objectExpressionValue + "> From <" + var1.Value.objectExpressionValue + "> In XML1");
                            }
                        }
                    });
                });
                advvarcomparereportdup = advvarcomparereport.Distinct().ToList();
                advvarcomparereportdup.ForEach(x =>
                    {
                        advvarcomparereport.RemoveAll(y =>
                            (
                            y.VariableName.ToString() == x.VariableName.ToString()
                            &&
                            y.ChangeType.ToString() == x.ChangeType.ToString()
                            &&
                            (y.XML1.ToString() == x.XML2.ToString() || y.XML2.ToString() == x.XML1.ToString())
                            ));            
                    });
                
                return advvarcomparereport;
            }
            catch
            {
                return null;
            }
            finally { varcomparereport = null; advvarcomparereport = null; advvarcomparereportdup = null; }
        }
        public List<TaskReport> CompareTasks()
        {
            varcomparereport = null;
            varcomparereport = new List<string>();
            advtaskcomparereport = null;
            advtaskcomparereport = new List<TaskReport>();
            try
            {
                
                dftlist.ForEach(task1 =>
                    {
                        if (!(dftlist2.Exists(task2 => task1.componentobj[0].DTSID == task2.componentobj[0].DTSID)))
                        {
                           
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                    ChangeType = "ElementMissing",
                                    XML1 = "Present In XML1",
                                    XML2 = "Missing In XML2"
                                });

                            //varcomparereport.Add("Task <" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + "> In XML1 is Missing In XML2");
                        }
                        dftlist2.ForEach(task2 =>
                            {
                                //if (task2.componentobj[0].componentClassID.ToString().Trim() == task2.componentobj[0].componentClassID.ToString().Trim())
                                //{
                                //    if ((task1.componentobj[0].componentName.ToString() != task2.componentobj[0].componentName.ToString())
                                //        && (task1.componentobj[0].componentID.ToString() == task2.componentobj[0].componentID.ToString())
                                //        )
                                //    {
                                //        advtaskcomparereport.Add(new TaskReport()
                                //        {
                                //            TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                //            ChangeType = "ComponentName",
                                //            XML1 = task1.componentobj[0].componentName.ToString(),
                                //            XML2 = task2.componentobj[0].componentName.ToString()
                                //        });
                                //    }
                                //}
                                if ((task2.componentobj[0].DTSID == task1.componentobj[0].DTSID) && task2.componentobj[0].DTSID.ToString().Trim() != "" && task1.componentobj[0].DTSID.ToString().Trim() != "")
                                {
                                    if (task1.objectName != task2.objectName)
                                    {
                                       
                                            advtaskcomparereport.Add(new TaskReport()
                                            {
                                                TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                                ChangeType = "TaskName",
                                                XML1 = task1.objectName,
                                                XML2 = task2.objectName
                                            });
                                        
                                     //   varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_ObjectName In XML1 Changed From <" + task1.objectName + "> To <" + task2.objectName + "> In XML2");
                                    }
                                    if (task1.componentobj[0].IsDisabled != task2.componentobj[0].IsDisabled)
                                    {
                                        advtaskcomparereport.Add(new TaskReport()
                                        {
                                            TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                            ChangeType = "IsDisabled",
                                            XML1 = (task1.componentobj[0].IsDisabled == 0 ? "False" : "True"),
                                            XML2 = (task2.componentobj[0].IsDisabled == 0 ? "False" : "True")
                                        });
                                        //   varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_IsDisabled In XML1 Changed From <" + (task1.componentobj[0].IsDisabled == 0 ? "False" : "True") + "> To <" + (task2.componentobj[0].IsDisabled == 0 ? "False" : "True") + "> In XML2");
                                    }
                                    if (task1.componentobj[0].IsStoredProc != task2.componentobj[0].IsStoredProc)
                                    {
                                        advtaskcomparereport.Add(new TaskReport()
                                        {
                                            TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                            ChangeType = "IsStoredProc",
                                            XML1 = task1.componentobj[0].IsStoredProc,
                                            XML2 = task1.componentobj[0].IsStoredProc,
                                        });
                                        //   varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_IsStoredProc In XML1 Changed From <" + task1.componentobj[0].IsStoredProc + "> To <" + task2.componentobj[0].IsStoredProc + "> In XML2");
                                    }
                                }
                                if((task1.componentobj[0].componentClassID == task2.componentobj[0].componentClassID) 
                                    &&
                                    (task1.componentobj[0].componentName.ToString() == task2.componentobj[0].componentName.ToString())
                                    )
                                {
                                    if (task1.componentobj[0].OpenRowset != task2.componentobj[0].OpenRowset)
                                    {
                                        
                                            advtaskcomparereport.Add(new TaskReport()
                                            {
                                                TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                                ChangeType = "OpenRowset",
                                                XML1 = task1.componentobj[0].OpenRowset,
                                                XML2 = task2.componentobj[0].OpenRowset
                                            });
                                       
                                      //  varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_OpenRowset In XML1 Changed From <" + task1.componentobj[0].OpenRowset + "> To <" + task2.componentobj[0].OpenRowset + "> In XML2");
                                    }
                                    if (task1.componentobj[0].OpenRowsetVariable != task2.componentobj[0].OpenRowsetVariable)
                                    {
                                        
                                            advtaskcomparereport.Add(new TaskReport()
                                            {
                                                TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                                ChangeType = "OpenRowsetVariable",
                                                XML1 = task1.componentobj[0].OpenRowsetVariable,
                                                XML2 = task2.componentobj[0].OpenRowsetVariable
                                            });
                                       
                                       // varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_OpenRowset In XML1 Changed From <" + task1.componentobj[0].OpenRowsetVariable + "> To <" + task2.componentobj[0].OpenRowsetVariable + "> In XML2");
                                    }
                                    if (task1.componentobj[0].SqlCommand != task2.componentobj[0].SqlCommand)
                                    {
                                        advtaskcomparereport.Add(new TaskReport()
                                        {
                                            TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                            ChangeType = "SqlCommand",
                                            XML1 = task1.componentobj[0].SqlCommand,
                                            XML2 = task2.componentobj[0].SqlCommand
                                        });
                                      //  varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_SqlCommand In XML1 Changed From <" + task1.componentobj[0].SqlCommand + "> To <" + task2.componentobj[0].SqlCommand + "> In XML2");
                                    }
                                    if (task1.componentobj[0].SqlCommandVariable != task2.componentobj[0].SqlCommandVariable)
                                    {
                                        advtaskcomparereport.Add(new TaskReport()
                                        {
                                            TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                            ChangeType = "SqlCommandVariable",
                                            XML1 = task1.componentobj[0].SqlCommandVariable,
                                            XML2 = task2.componentobj[0].SqlCommandVariable
                                        });
                                      //  varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_SqlCommandVariable In XML1 Changed From <" + task1.componentobj[0].SqlCommandVariable + "> To <" + task2.componentobj[0].SqlCommandVariable + "> In XML2");
                                    }
                                    if (task1.componentobj[0].objectExpression != task2.componentobj[0].objectExpression)
                                    {
                                        advtaskcomparereport.Add(new TaskReport()
                                        {
                                            TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                            ChangeType = "objectExpression",
                                            XML1 = task1.componentobj[0].objectExpression,
                                            XML2 = task2.componentobj[0].objectExpression
                                        });
                                     //   varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_objectExpression In XML1 Changed From <" + task1.componentobj[0].objectExpression + "> To <" + task2.componentobj[0].objectExpression + "> In XML2");
                                    }
                                    if (task1.componentobj[0].objectExpressionValue != task2.componentobj[0].objectExpressionValue)
                                    {
                                        advtaskcomparereport.Add(new TaskReport()
                                        {
                                            TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                                            ChangeType = "objectExpressionValue",
                                            XML1 = task1.componentobj[0].objectExpressionValue,
                                            XML2 = task2.componentobj[0].objectExpressionValue
                                        });
                                      //  varcomparereport.Add("<" + task1.objectName + "> / <" + task1.componentobj[0].componentName + ">_objectExpressionValue In XML1 Changed From <" + task1.componentobj[0].objectExpressionValue + "> To <" + task2.componentobj[0].objectExpressionValue + "> In XML2");
                                    }
                                   
                                }
                            });
                    });
                dftlist2.ForEach(task2 =>
                {
                    if (!(dftlist.Exists(task1 => task2.componentobj[0].DTSID == task1.componentobj[0].DTSID)))
                    {
                        
                            advtaskcomparereport.Add(new TaskReport()
                            {
                                TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                ChangeType = "ElementMissing",
                                XML1 = "Missing In XML1",
                                XML2 = "Present In XML2"
                            });
                       
                       // varcomparereport.Add("Task <" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + "> In XML2 is Missing In XML1");
                    }
                    dftlist.ForEach(task1 =>
                    {
                        //if (task1.componentobj[0].componentClassID.ToString().Trim() == task2.componentobj[0].componentClassID.ToString().Trim())
                        //{
                        //    if ((task2.componentobj[0].componentName.ToString() != task1.componentobj[0].componentName.ToString())
                        //        &&
                        //        (task2.componentobj[0].componentID.ToString() == task1.componentobj[0].componentID.ToString())
                        //        )
                        //    {
                        //        advtaskcomparereport.Add(new TaskReport()
                        //        {
                        //            TaskName = "<" + task1.objectName + "> Or <" + task1.componentobj[0].componentName + ">",
                        //            ChangeType = "ComponentName",
                        //            XML1 = task2.componentobj[0].componentName.ToString(),
                        //            XML2 = task1.componentobj[0].componentName.ToString()
                        //        });
                        //    }
                        //}
                        if ((task1.componentobj[0].DTSID == task2.componentobj[0].DTSID) && task1.componentobj[0].DTSID.ToString() != "" && task2.componentobj[0].DTSID.ToString() != "")
                        {
                            if (task2.objectName != task1.objectName)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "TaskName",
                                    XML1 = task1.objectName,
                                    XML2 = task2.objectName
                                });
                                // varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_ObjectName In XML2 Changed From <" + task2.objectName + "> To <" + task1.objectName + "> In XML1");
                            }
                            if (task2.componentobj[0].IsDisabled != task1.componentobj[0].IsDisabled)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "IsDisabled",
                                    XML1 = (task1.componentobj[0].IsDisabled == 0 ? "False" : "True"),
                                    XML2 = (task2.componentobj[0].IsDisabled == 0 ? "False" : "True")
                                });
                                // varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_IsDisabled In XML2 Changed From <" + (task2.componentobj[0].IsDisabled == 0 ? "False" : "True") + "> To <" + (task1.componentobj[0].IsDisabled == 0 ? "False" : "True") + "> In XML1");
                            }
                            if (task2.componentobj[0].IsStoredProc != task1.componentobj[0].IsStoredProc)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "IsStoredProc",
                                    XML1 = task1.componentobj[0].IsStoredProc,
                                    XML2 = task1.componentobj[0].IsStoredProc,
                                });
                                // varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_IsStoredProc In XML2 Changed From <" + task2.componentobj[0].IsStoredProc + "> To <" + task1.componentobj[0].IsStoredProc + "> In XML1");
                            }
                        }
                        if((task2.componentobj[0].componentClassID == task1.componentobj[0].componentClassID)
                            &&
                            (task2.componentobj[0].componentName == task1.componentobj[0].componentName)
                            )
                        {
                            if (task2.componentobj[0].OpenRowset != task1.componentobj[0].OpenRowset)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "OpenRowset",
                                    XML1 = task1.componentobj[0].OpenRowset,
                                    XML2 = task2.componentobj[0].OpenRowset
                                });
                               // varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_OpenRowset In XML2 Changed From <" + task2.componentobj[0].OpenRowset + "> To <" + task1.componentobj[0].OpenRowset + "> In XML1");
                            }
                            if (task2.componentobj[0].OpenRowsetVariable != task1.componentobj[0].OpenRowsetVariable)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "OpenRowsetVariable",
                                    XML1 = task1.componentobj[0].OpenRowsetVariable,
                                    XML2 = task2.componentobj[0].OpenRowsetVariable
                                });
                               // varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_OpenRowset In XML2 Changed From <" + task2.componentobj[0].OpenRowsetVariable + "> To <" + task1.componentobj[0].OpenRowsetVariable + "> In XML1");
                            }
                            if (task2.componentobj[0].SqlCommand != task1.componentobj[0].SqlCommand)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "SqlCommand",
                                    XML1 = task1.componentobj[0].SqlCommand,
                                    XML2 = task2.componentobj[0].SqlCommand
                                });
                                //varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_SqlCommand In XML2 Changed From <" + task2.componentobj[0].SqlCommand + "> To <" + task1.componentobj[0].SqlCommand + "> In XML1");
                            }
                            if (task2.componentobj[0].SqlCommandVariable != task1.componentobj[0].SqlCommandVariable)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "SqlCommandVariable",
                                    XML1 = task1.componentobj[0].SqlCommandVariable,
                                    XML2 = task2.componentobj[0].SqlCommandVariable
                                });
                               // varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_SqlCommandVariable In XML2 Changed From <" + task2.componentobj[0].SqlCommandVariable + "> To <" + task1.componentobj[0].SqlCommandVariable + "> In XML1");
                            }
                            if (task2.componentobj[0].objectExpression != task1.componentobj[0].objectExpression)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "objectExpression",
                                    XML1 = task1.componentobj[0].objectExpression,
                                    XML2 = task2.componentobj[0].objectExpression
                                });
                              //  varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_objectExpression In XML2 Changed From <" + task2.componentobj[0].objectExpression + "> To <" + task1.componentobj[0].objectExpression + "> In XML1");
                            }
                            if (task2.componentobj[0].objectExpressionValue != task1.componentobj[0].objectExpressionValue)
                            {
                                advtaskcomparereport.Add(new TaskReport()
                                {
                                    TaskName = "<" + task2.objectName + "> Or <" + task2.componentobj[0].componentName + ">",
                                    ChangeType = "objectExpressionValue",
                                    XML1 = task1.componentobj[0].objectExpressionValue,
                                    XML2 = task2.componentobj[0].objectExpressionValue
                                });
                               // varcomparereport.Add("<" + task2.objectName + "> / <" + task2.componentobj[0].componentName + ">_objectExpressionValue In XML2 Changed From <" + task2.componentobj[0].objectExpressionValue + "> To <" + task1.componentobj[0].objectExpressionValue + "> In XML1");
                            }
                           
                        }
                    });
                });
                return advtaskcomparereport;
            }
            catch(Exception err)
            {
                return null;
            }
            finally
            {
                varcomparereport = null;
            }

        }
        public override string ToString()
        {
            return base.ToString().Trim ();
        }
    }
}
