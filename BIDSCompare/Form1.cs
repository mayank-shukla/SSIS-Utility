using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Text.RegularExpressions;

namespace BIDSCompare
{
    public partial class frmCompare : Form
    {

        #region VariableDeclaration
        ToolTip tp = new ToolTip();
        List<string> lst = new List<string>();
        List<string> lsttask = new List<string>();
        List<SSISXml> attrtask = new List<SSISXml>();
        List<SSISXmlTask> componenttask = new List<SSISXmlTask>();
        List<SSISXmlTask> componenttaskdisp = new List<SSISXmlTask>();
        List<SSISXml> attr = new List<SSISXml>();
        bool IsXMLLoadFirstTime = true;
        bool IsXMLLoadFirstTimeTask = true;
        Dictionary<string, SSISXml> xmldictTask = new Dictionary<string, SSISXml>();
        Dictionary<string, SSISXml> xmldictdisplayTask = new Dictionary<string, SSISXml>();

        Dictionary<string, SSISXml> xmldict = new Dictionary<string, SSISXml>();
        Dictionary<string, SSISXml> xmldictdisplay = new Dictionary<string, SSISXml>();
        Dictionary<string, string> xmltemp = new Dictionary<string, string>();
        XDocument xdoc, xdocTask, xdocTaskTemp, xdocTaskComponent, xdocAnalyze, xdocAnalyzeTemp;
        public string xmlstring;
        #endregion
        bool ascendinggridveiw2 = true;
        bool ascendinggridview3 = true;
        private List<VariableReportToDisplay> varReportobj = new List<VariableReportToDisplay>();
        private List<TaskReport> varTaskReportobj = new List<TaskReport>();
        public string strVariableCount = string.Empty, strTasksCount = string.Empty, strFileName = string.Empty, strFileName1 = string.Empty, strFileName2 = string.Empty;
        public string strVariableCount2 = string.Empty, strTasksCount2 = string.Empty;
        public bool xml1loaded = false, xml2loaded = false, isLoadRunning = false;
        int xmlchoice = 0;
        Logic logicobj = new Logic();
        List<string> variablereport = new List<string>();
        List<string> taskreport = new List<string>();

        Wait waitobj;
        private List<VariableReport> varreport_load = new List<VariableReport>();
        private List<TaskReport> taskreport_load = new List<TaskReport>();
        private bool IsFirstTime_load = true;
        // delegate for the UI updater 
        public delegate void UpdateUIDelegate(bool IsDataLoaded);

        List<AnalyzeTaskReport> AnalyzeTaskReport_obj = new List<AnalyzeTaskReport>();
        List<AnalyzeTaskResultSet> AnalyzeTaskResultSet_obj = new List<AnalyzeTaskResultSet>();
        List<AnalyzeTaskParameter> AnalyzeTaskParameter_obj = new List<AnalyzeTaskParameter>();
        string htmltext;
        public frmCompare()
        {
            InitializeComponent();
            // To report progress from the background worker we need to set this property
            bwParseXML.WorkerReportsProgress = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Normal;

        }




        private void button2_Click(object sender, EventArgs e)
        {

            xmlchoice = 1;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "XML File| *.xml";
            openFileDialog1.ShowDialog();
            strFileName1 = openFileDialog1.FileName;
            txtFileName.Text = strFileName1;
        }


        public bool IsXmlValid(string str, string FileOrString)
        {
            if (FileOrString == "S")
            {
                try
                {
                    XDocument xd = XDocument.Parse(str);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            if (FileOrString == "F")
            {
                try
                {
                    XDocument xd = XDocument.Load(str);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }


        private void button5_Click(object sender, EventArgs e)
        {
            lbl_One_Var.Text = "Will Display Variable Count After XML Load";
            lbl_one_task.Text = "Will Display Task Count After XML Load";
            strVariableCount = "";
            strTasksCount = "";
            btnBrowseXML1.Enabled = true;
            dataGridView1.DataSource = null;
            xml1loaded = false;
            // Start the background worker
            rbCopyXML.Enabled = true;
            rbBrowse.Enabled = true;
            logicobj.ReloadLogicxml1();
            lbl_xml1_name.Text = "";
            txtFileName.Text = "";
            btnLoadXML.Enabled = true;
            IsFirstTime_load = true;
        }

        private void lbl_One_Var_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int i = 1;
            if (lbl_One_Var.Text != "Will Display Variable Count After XML Load")
            {
                var displayvariable = from row in logicobj.variablelist
                                      select new
                                      {
                                          SerialNumber = i++,
                                          VariableName = row.Key,
                                          VariableExpression = row.Value.objectExpression,
                                          VariableExpressionValue = row.Value.objectExpressionValue,
                                          DTSID = row.Value.DTSID
                                      };
                dataGridView1.DataSource = displayvariable.ToArray();
                dataGridView1.AutoResizeColumns(
                                DataGridViewAutoSizeColumnsMode.ColumnHeader);
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                int iGridSize = dataGridView1.Width;
                int iColumnCount = dataGridView1.Columns.Count;
                int iColumnSize = iGridSize / iColumnCount;




            }
        }

        private void lbl_one_task_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int i = 1;

            if (lbl_one_task.Text != "Will Display Task Count After XML Load")
            {
                var displayvariable = from row in logicobj.dftlist
                                      select new
                                      {
                                          SerialNumber = i++,
                                          TaskName = row.objectName,
                                          ComponentName = row.componentobj[0].componentName,
                                          ExecutableType = row.componentobj[0].ExecutableType,
                                          OpenRowSet = row.componentobj[0].OpenRowset,
                                          OpenRowSetVariable = row.componentobj[0].OpenRowsetVariable,
                                          SqlCommand = row.componentobj[0].SqlCommand,
                                          SqlCommandVariable = row.componentobj[0].SqlCommandVariable,
                                          objectExpression = row.componentobj[0].objectExpression,
                                          objectExpressionValue = row.componentobj[0].objectExpressionValue,
                                          IsDisabled = row.componentobj[0].IsDisabled == 0 ? "False" : "True",
                                          IsStoredProc = row.componentobj[0].IsStoredProc,
                                          DTSID = row.componentobj[0].DTSID
                                      };
                dataGridView1.DataSource = displayvariable.ToArray();

                dataGridView1.AutoResizeColumns(
                                DataGridViewAutoSizeColumnsMode.ColumnHeader);
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            }
        }



        private void button7_Click(object sender, EventArgs e)
        {
            lbl_two_var.Text = "Will Display Variable Count After XML Load";
            lbl_two_task.Text = "Will Display Task Count After XML Load";

            dataGridView1.DataSource = null;
            xml2loaded = false;
            logicobj.ReloadLogicxml2();
            lbl_xml2_name.Text = "";
        }

        private void lbl_two_var_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int i = 1;

            if (lbl_two_var.Text != "Will Display Variable Count After XML Load")
            {
                var displayvariable = from row in logicobj.variablelist2
                                      select new
                                      {
                                          SerialNumber = i++,
                                          VariableName = row.Key,
                                          VariableExpression = row.Value.objectExpression,
                                          VariableExpressionValue = row.Value.objectExpressionValue,
                                          DTSID = row.Value.DTSID
                                      };
                dataGridView1.DataSource = displayvariable.ToArray();
                dataGridView1.AutoResizeColumns(
                                DataGridViewAutoSizeColumnsMode.ColumnHeader);
            }
        }

        private void lbl_two_task_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int i = 1;
            if (lbl_two_task.Text != "Will Display Task Count After XML Load")
            {
                var displayvariable = from row in logicobj.dftlist2
                                      select new
                                      {
                                          SerialNumber = i++,
                                          TaskName = row.objectName,
                                          ComponentName = row.componentobj[0].componentName,
                                          ExecutableType = row.componentobj[0].ExecutableType,
                                          OpenRowSet = row.componentobj[0].OpenRowset,
                                          OpenRowSetVariable = row.componentobj[0].OpenRowsetVariable,
                                          SqlCommand = row.componentobj[0].SqlCommand,
                                          SqlCommandVariable = row.componentobj[0].SqlCommandVariable,
                                          objectExpression = row.componentobj[0].objectExpression,
                                          objectExpressionValue = row.componentobj[0].objectExpressionValue,
                                          IsDisabled = row.componentobj[0].IsDisabled == 0 ? "False" : "True",
                                          IsStoredProc = row.componentobj[0].IsStoredProc,
                                          DTSID = row.componentobj[0].DTSID
                                      };
                dataGridView1.DataSource = displayvariable.ToArray();

                dataGridView1.AutoResizeColumns(
                                DataGridViewAutoSizeColumnsMode.ColumnHeader);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            xmlchoice = 2;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "XML File| *.xml";
            openFileDialog1.ShowDialog();
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            variablereport.Clear();
            taskreport.Clear();
            varReportobj.Clear();
            varTaskReportobj.Clear();
            if (xml1loaded == false || xml2loaded == false)
            {
                MessageBox.Show("Please Load Required SSIS XMLs");
                tbControl.SelectedTab = tabPage1;
            }
            else
            {
                waitobj = new Wait();
                waitobj.ShowInTaskbar = false;
                waitobj.StartPosition = FormStartPosition.CenterScreen;
                waitobj.Text = "Preparing Comparison Report";
                waitobj.Show();
                if (IsFirstTime_load)
                {
                    varreport_load = logicobj.CompareVariables();
                    taskreport_load = logicobj.CompareTasks();
                    IsFirstTime_load = false;
                }
                var displayvariable = (from row in varreport_load
                                       select new
                                       {
                                           //Report = row
                                           VariableName = row.VariableName,
                                           ChangeType = row.ChangeType,
                                           XML1 = row.XML1,
                                           XML2 = row.XML2
                                       })
                                   .GroupBy(g => new { g.VariableName, g.ChangeType, g.XML1, g.XML2 })
                                   .Select(s => s.FirstOrDefault());
                varReportobj.AddRange(
                    from row in displayvariable
                    select new VariableReportToDisplay()
                    {
                        VariableName = row.VariableName,
                        ChangeType = row.ChangeType,
                        XML1 = row.XML1,
                        XML2 = row.XML2
                    });
                //variablereport.AddRange(logicobj.CompareVariables());
                variablereport.AddRange(
                    from row in displayvariable
                    select "<VariableName:> " + row.VariableName + " || <ChangeType> " + row.ChangeType + " || <XML1 Value> " + row.XML1 + " || <XML2 Value> " + row.XML2
                    );
                /*
                 0-VariableName
                 1-ChangeType
                 2-XML1
                 3-XML2
                 */
                // System.ComponentModel.BindingList
                dataGridView2.DataSource = displayvariable.ToArray();

                var displayvariabletask = (from row in taskreport_load
                                           select new
                                           {
                                               TaskName = row.TaskName,
                                               ChangeType = row.ChangeType,
                                               XML1 = row.XML1,
                                               XML2 = row.XML2
                                           })
                                        .GroupBy(g => new { g.TaskName, g.ChangeType, g.XML1, g.XML2 })
                                        .Select(s => s.FirstOrDefault());
                //taskreport.AddRange(logicobj.CompareTasks());
                taskreport.AddRange(
                    from row in displayvariabletask
                    select "<TaskName:> " + row.TaskName + " || <ChangeType> " + row.ChangeType + " || <XML1 Value> " + row.XML1 + " || <XML2 Value> " + row.XML2
                    );

                varTaskReportobj.AddRange(
                    from row in displayvariabletask
                    select new TaskReport()
                    {
                        TaskName = row.TaskName,
                        ChangeType = row.ChangeType,
                        XML1 = row.XML1,
                        XML2 = row.XML2
                    }
                    );
                dataGridView3.DataSource = displayvariabletask.ToArray();
                dataGridView3.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                waitobj.Close();
                waitobj.Dispose();
            }

        }



        private void dataGridView2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenuStrip1.Show();
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

            try
            {
                saveFileDialog1.Filter = "Txt File| *.txt";
                saveFileDialog1.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void contextMenuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                saveFileDialog2.Filter = "Txt File| *.txt";
                saveFileDialog2.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView3_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenuStrip2.Show();
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.File.WriteAllLines(saveFileDialog1.FileName, variablereport.ToArray());
            MessageBox.Show("Variable Report Saved Successfully");
        }

        private void saveFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.File.WriteAllLines(saveFileDialog2.FileName, taskreport.ToArray());
            MessageBox.Show("Task Report Saved Successfully");
        }

        private void bwParseXML_DoWork(object sender, DoWorkEventArgs e)
        {

            logicobj.ProgressChanged += (s, pe) => bwParseXML.ReportProgress(pe.ProgressPercentage);



            strVariableCount = "Variables >> " + logicobj.CalcVar(XDocument.Load(strFileName1).ToString());


            strTasksCount = "Tasks >> " + logicobj.CalTask(XDocument.Load(strFileName1).ToString());

        }

        private void bwParseXML_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void bwParseXML_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            lbl_One_Var.Text = strVariableCount;
            lbl_one_task.Text = strTasksCount;

            lbl_xml1_name.Text = "FileName >> " + strFileName1.Substring(strFileName1.LastIndexOf('\\') + 1);
            isLoadRunning = false;
            //Set Variables
            progressBar1.Visible = false;
            btnLoadXML.Enabled = true;
            btnBrowseXML1.Enabled = true;
            btnClearXML1.Enabled = true;
            xml1loaded = true;

        }




        private void rbCopyXML_CheckedChanged(object sender, EventArgs e)
        {

            if (rbCopyXML.Checked)
            {
                if (isLoadRunning)
                {
                    MessageBox.Show("Please wait, XML is getting loaded");
                    rbCopyXML.Checked = false;
                    return;
                }
                isLoadRunning = true;
                btnLoadXML.Enabled = false;
                btnBrowseXML1.Enabled = false;
                xmlchoice = 1;
                CopyXML copyxmlobj = new CopyXML(this, ref logicobj);
                copyxmlobj.xmlchoice = 1;
                copyxmlobj.StartPosition = FormStartPosition.CenterParent;
                copyxmlobj.ShowDialog();
                if (strVariableCount != "")
                    lbl_One_Var.Text = strVariableCount;
                if (strTasksCount != "")
                    lbl_one_task.Text = strTasksCount;
                isLoadRunning = false;
                rbCopyXML.Checked = false;
            }
        }

        private void btnLoadXML_Click(object sender, EventArgs e)
        {

            System.Windows.Forms.Button objButton = (System.Windows.Forms.Button)sender;

            if (objButton.Name == "btnLoadXML")
            {
                if (IsXmlValid(txtFileName.Text, "F"))
                {
                    logicobj.ReloadLogicxml1();
                    xmlchoice = 1;
                    isLoadRunning = true;
                    progressBar1.Visible = true;
                    btnLoadXML.Enabled = false;
                    btnBrowseXML1.Enabled = false;
                    btnClearXML1.Enabled = false;
                    bwParseXML.RunWorkerAsync();
                    rbCopyXML.Enabled = false;
                    rbBrowse.Enabled = false;
                    objButton.Enabled = false;
                }
                else
                {
                    MessageBox.Show("Error While Loading XML");
                }
            }
            else if (objButton.Name == "btnLoadXML2")
            {

                if (IsXmlValid(txtXMLPath2.Text, "F"))
                {
                    logicobj.ReloadLogicxml2();
                    xmlchoice = 2;
                    isLoadRunning = true;
                    progressBar2.Visible = true;
                    btnLoadXML2.Enabled = false;
                    btnBrowseXML2.Enabled = false;
                    btnClear2.Enabled = false;
                    bwParseXML2.RunWorkerAsync();
                    rbCopyXML2.Enabled = false;
                    rbBrowseXML2.Enabled = false;
                    objButton.Enabled = false;
                }
                else
                {
                    MessageBox.Show("Error While Loading XML");
                }
            }

        }




        private void btnBrowseXMl2_Click(object sender, EventArgs e)
        {
            xmlchoice = 2;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "XML File| *.xml";
            openFileDialog1.ShowDialog();
            strFileName = openFileDialog1.FileName;
        }




        private void rbBrowse_CheckedChanged(object sender, EventArgs e)
        {
            if (rbBrowse.Checked)
            {

                btnLoadXML.Enabled = true;
                btnBrowseXML1.Enabled = true;
                btnClearXML1.Enabled = true;
            }
            else
            {
                btnLoadXML.Enabled = false;
                btnBrowseXML1.Enabled = false;

            }
        }

        private void rbCopyXML2_CheckedChanged_1(object sender, EventArgs e)
        {

            if (rbCopyXML2.Checked)
            {
                if (isLoadRunning)
                {
                    MessageBox.Show("Please wait, XML is getting loaded");
                    rbCopyXML2.Checked = false;
                    return;
                }
                isLoadRunning = true;
                btnLoadXML2.Enabled = false;
                btnBrowseXML2.Enabled = false;
                xmlchoice = 2;
                CopyXML copyxmlobj = new CopyXML(this, ref logicobj);
                copyxmlobj.StartPosition = FormStartPosition.CenterScreen;
                copyxmlobj.xmlchoice = 2;
                copyxmlobj.ShowDialog();
                isLoadRunning = false;

                if (strVariableCount2 != "")
                    lbl_two_var.Text = strVariableCount2;
                if (strTasksCount2 != "")
                    lbl_two_task.Text = strTasksCount2;
                isLoadRunning = false;
                rbCopyXML2.Checked = false;
                copyxmlobj.Dispose();
            }
        }

        private void rbBrowseXML2_CheckedChanged(object sender, EventArgs e)
        {
            if (rbBrowseXML2.Checked)
            {
                //if (isLoadRunning)
                //{
                //    MessageBox.Show("Please wait, XML is getting loaded");
                //    rbBrowseXML2.Checked = false;
                //    return;
                //}
                btnLoadXML2.Enabled = true;
                btnBrowseXML2.Enabled = true;
                btnClear2.Enabled = true;
            }
            else
            {
                btnLoadXML2.Enabled = false;
                btnBrowseXML2.Enabled = false;
            }
        }

        private void btnBrowseXML2_Click_1(object sender, EventArgs e)
        {
            xmlchoice = 2;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "XML File| *.xml";
            openFileDialog1.ShowDialog();
            strFileName2 = openFileDialog1.FileName;
            txtXMLPath2.Text = strFileName2;
        }

        private void lbl_two_var_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int i = 1;
            if (lbl_two_var.Text != "Will Display Variable Count After XML Load")
            {
                var displayvariable = from row in logicobj.variablelist2
                                      select new
                                      {
                                          SerialNumber = i++,
                                          VariableName = row.Key,
                                          VariableExpression = row.Value.objectExpression,
                                          VariableExpressionValue = row.Value.objectExpressionValue,
                                          DTSID = row.Value.DTSID
                                      };
                dataGridView1.DataSource = displayvariable.ToArray();
                dataGridView1.AutoResizeColumns(
                                DataGridViewAutoSizeColumnsMode.ColumnHeader);
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }

        }

        private void lbl_two_task_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int i = 1;

            if (lbl_two_task.Text != "Will Display Task Count After XML Load")
            {
                var displayvariable = from row in logicobj.dftlist2
                                      select new
                                      {
                                          SerialNumber = i++,
                                          TaskName = row.objectName,
                                          ComponentName = row.componentobj[0].componentName,
                                          ExecutableType = row.componentobj[0].ExecutableType,
                                          OpenRowSet = row.componentobj[0].OpenRowset,
                                          OpenRowSetVariable = row.componentobj[0].OpenRowsetVariable,
                                          SqlCommand = row.componentobj[0].SqlCommand,
                                          SqlCommandVariable = row.componentobj[0].SqlCommandVariable,
                                          objectExpression = row.componentobj[0].objectExpression,
                                          objectExpressionValue = row.componentobj[0].objectExpressionValue,
                                          IsDisabled = row.componentobj[0].IsDisabled == 0 ? "False" : "True",
                                          IsStoredProc = row.componentobj[0].IsStoredProc,
                                          DTSID = row.componentobj[0].DTSID
                                      };
                dataGridView1.DataSource = displayvariable.ToArray();

                dataGridView1.AutoResizeColumns(
                                DataGridViewAutoSizeColumnsMode.ColumnHeader);
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            }
        }


        public void SetControls(int xmlchoice)
        {
            if (xmlchoice == 1)
            {

                progressBar1.Visible = false;
                lbl_One_Var.Text = strVariableCount;
                lbl_one_task.Text = strTasksCount;
                btnBrowseXML1.Enabled = false;
                xml1loaded = true;
                btnClearXML1.Enabled = true;

            }
            if (xmlchoice == 2)
            {

                lbl_two_var.Text = strVariableCount2;
                lbl_two_task.Text = strTasksCount2;
                btnBrowseXML1.Enabled = false;
                xml2loaded = true;
                btnClear2.Enabled = true;

            }
        }

        private void btnClear2_Click(object sender, EventArgs e)
        {
            lbl_two_var.Text = "Will Display Variable Count After XML Load";
            lbl_two_task.Text = "Will Display Task Count After XML Load";
            strVariableCount2 = "";
            strTasksCount2 = "";
            btnBrowseXML2.Enabled = true;
            dataGridView1.DataSource = null;
            xml2loaded = false;
            // Start the background worker
            rbCopyXML2.Enabled = true;
            rbBrowseXML2.Enabled = true;
            logicobj.ReloadLogicxml2();
            lbl_xml2_name.Text = "";
            txtXMLPath2.Text = "";
            IsFirstTime_load = true;
            btnLoadXML2.Enabled = true;
        }



        /// <summary>
        /// Search Browse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbSearchBrowse_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSearchBrowse.Checked)
            {
                btnBrowseSearchXML.Enabled = true;
            }
            else
            {
                btnBrowseSearchXML.Enabled = false;
            }
        }

        private void btnBrowseSearchXML_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "XML File| *.xml";
            openFileDialog1.ShowDialog();
            txtBrowsedFileName.Text = openFileDialog1.FileName;
            SetXML(openFileDialog1.FileName, "F");
        }


        private void SetXML(string filename, string strtype)
        {
            if (IsXMLLoaded(filename, strtype))
            {
                MessageBox.Show("XML loaded successfully.");
                tbControl.TabPages.Add(tabPage6);
                pnlBrowseCopy.Enabled = false;
            }

        }
        private bool IsXMLLoaded(string filename, string strtype)
        {

            XDocument xd;
            try
            {
                switch (strtype)
                {
                    case "F":
                        xd = XDocument.Load(filename);
                        xmlstring = xd.ToString();
                        break;
                    case "S":
                        xmlstring = filename.Replace("©", "");
                        xd = XDocument.Parse(filename);
                        break;
                }
                return true;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error occured while loading XML: " + ex.Message);
                return false;
            }
        }

        private void searchInXml(string str)
        {
            if (IsXMLLoadFirstTime)
            {
                xmldict.Clear();
                lst.Clear();
                xdoc = XDocument.Parse(xmlstring);
                xdoc.Elements().Nodes().ToList().ForEach(x =>
                {
                    lst.Add(x.ToString());
                });

                lst.ForEach(xml =>
                {
                    if (xml.StartsWith("<DTS:Variable"))
                        FillDict(xml);
                }
                );
                IsXMLLoadFirstTime = false;
            }
            variableSearch(str);
            display();
        }
        private void searchInXmlTask(string str)
        {
            if (IsXMLLoadFirstTimeTask)
            {
                xdocTask = XDocument.Parse(xmlstring);
                lsttask.Clear();
                componenttask.Clear();
                xdocTask.Elements().Nodes().ToList().ForEach(x =>
                {
                    lsttask.Add(x.ToString());
                });

                lsttask.ForEach(xml =>
                {
                    if (xml.StartsWith("<DTS:Executable"))
                        FillDictTask(xml);
                }
                );
                IsXMLLoadFirstTimeTask = false;
            }
            variableSearchTask(str);
            display();
        }
        private void display()
        {
            int i = 1;
            if (rbtn_variable.Checked)
            {
                var disp = from row in xmldictdisplay
                           select new
                           {
                               SerialNumber = i++,
                               VariableName = row.Key,
                               Expression = row.Value.objectexpression,
                               EvaluatedExpression = row.Value.objectexpressionvalue
                           };
                dgvFill.DataSource = disp.ToArray();
                //  dataGridView1.AutoResizeColumns();
            }
            if (rbtn_task.Checked)
            {
                var disp = from row in componenttaskdisp
                           select new
                           {
                               SerialNumber = i++,
                               TaskName = row.TaskName,
                               TaskExpression = row.TaskExpression
                           };
                dgvFill.DataSource = disp.ToArray();
                //  dataGridView1.AutoResizeColumns();
            }
            lbl_rowcount.Text = "Total Rows >> " + (i - 1);
        }
        private void variableSearch(string str)
        {
            xmldictdisplay.Clear();
            xmldict.ToList().ForEach(x =>
            {
                if (x.Value.objectexpression.ToUpper().ToString().Contains(str.ToUpper()) || x.Value.objectexpressionvalue.ToUpper().ToString().Contains(str.ToUpper()))
                {
                    xmldictdisplay.Add(x.Key, new SSISXml()
                    {
                        objectexpression = x.Value.objectexpression,
                        objectexpressionvalue = x.Value.objectexpressionvalue
                    });
                }
            });
        }
        private void variableSearchTask(string str)
        {
            componenttaskdisp.Clear();
            componenttask.ForEach(x =>
            {
                if (x.TaskExpression.ToUpper().ToString().Contains(str.ToUpper()))
                {
                    componenttaskdisp.Add(new SSISXmlTask()
                    {
                        TaskName = x.TaskName,
                        TaskExpression = x.TaskExpression
                    });
                }
            });
        }
        private void FillDict(string str)
        {
            #region Testing
            //            string xml = @"
            //            <DTS:Variable xmlns:DTS=""www.microsoft.com/SqlServer/Dts"">
            //              <DTS:Property DTS:Name=""Expression"">@[User::BatchID]</DTS:Property> 
            //              <DTS:Property DTS:Name=""EvaluateAsExpression"">-1</DTS:Property> 
            //              <DTS:Property DTS:Name=""Namespace"">User</DTS:Property> 
            //              <DTS:Property DTS:Name=""ReadOnly"">0</DTS:Property> 
            //              <DTS:Property DTS:Name=""RaiseChangedEvent"">0</DTS:Property> 
            //              <DTS:Property DTS:Name=""IncludeInDebugDump"">2345</DTS:Property> 
            //              <DTS:VariableValue DTS:DataType=""8"">0123456789012345678</DTS:VariableValue> 
            //              <DTS:Property DTS:Name=""ObjectName"">batch_BatchID</DTS:Property> 
            //              <DTS:Property DTS:Name=""DTSID"">{7751C489-688F-4E6D-B636-CEBAF6D7AE2B}</DTS:Property> 
            //              <DTS:Property DTS:Name=""Description"" /> 
            //              <DTS:Property DTS:Name=""CreationName"" /> 
            //              </DTS:Variable>
            //                ";
            #endregion
            xdoc = XDocument.Parse(str);
            xmltemp.Clear();
            xdoc.Descendants().Elements().Where(elem => elem.Name.LocalName != "Envelope").ToList().ForEach(elm =>
            {
                elm.Attributes().ToList().ForEach(xattr =>
                {
                    xmltemp.Add(xattr.Value, elm.Value);
                });
            });

            xmldict.Add(
                xmltemp.Where(x => x.Key == "ObjectName").Select(y => y.Value).FirstOrDefault(),
                new SSISXml()
                {
                    objectexpression = (xmltemp.Where(x => x.Key == "Expression").Select(y => y.Value).FirstOrDefault()) == null ? "" : xmltemp.Where(x => x.Key == "Expression").Select(y => y.Value).FirstOrDefault(),
                    objectexpressionvalue = (xmltemp.Where(x => x.Key == "8").Select(y => y.Value).FirstOrDefault()) == null ? "" : xmltemp.Where(x => x.Key == "8").Select(y => y.Value).FirstOrDefault()
                });
            xmltemp.Clear();
        }

        private void FillDictTask(string str)
        {
            
            xdocTaskComponent = XDocument.Parse(str);
            xdocTaskComponent.Descendants("components").Elements().ToList().ForEach(elem =>
            {
                xdocTaskTemp = XDocument.Parse(elem.ToString());
                xdocTaskTemp.Descendants("properties").Elements().ToList().ForEach(prop =>
                {
                    prop.Attributes().ToList().ForEach(propattr =>
                    {
                        if ((propattr.Value == "SqlCommandVariable" || propattr.Value == "SqlCommand"))
                        {
                            componenttask.Add(new SSISXmlTask()
                            {
                                TaskName = elem.Attribute("name").Value.ToString(),
                                TaskExpression = prop.Value.ToString()
                            });
                        }
                    });
                });
            });

            componenttask.RemoveAll(x => x.TaskExpression.Trim() == "");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (xmlstring == null || xmlstring.Trim() == "")
            {
                MessageBox.Show("Please Load XML First");
            }
            else
            {
                Wait waitobj2 = new Wait();
                waitobj2.ShowInTaskbar = false;
                waitobj2.StartPosition = FormStartPosition.CenterScreen;
                waitobj2.Text = "Searching Please Wait...";
                waitobj2.Show();
                if (rbtn_variable.Checked == true)
                    searchInXml(txt_search.Text.Trim());
                if (rbtn_task.Checked == true)
                    searchInXmlTask(txt_search.Text.Trim());
                waitobj2.Close();
                waitobj2.Dispose();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            txt_search.Text = "";
            txtBrowsedFileName.Text = "";
            dgvFill.DataSource = null;
            lbl_rowcount.Text = "";
            IsXMLLoadFirstTime = true;
            IsXMLLoadFirstTimeTask = true;
            pnlBrowseCopy.Enabled = true;
            xmlstring = "";
            button3.Enabled = false;
            button4.Enabled = false;
            button1.Enabled = true;
            txt_Filter.Text = "";
            dataGridView4.DataSource = null;
            dataGridView4.Refresh();
            dataGridView5.DataSource = null;
            dataGridView5.Refresh();
            dataGridView6.DataSource = null;
            dataGridView6.Refresh();
            dataGridView7.DataSource = null;
            dataGridView7.Refresh();
            dataGridView8.DataSource = null;
            dataGridView8.Refresh();
            dataGridView9.DataSource = null;
            dataGridView9.Refresh();
            tbControl.TabPages.Remove(tabPage6);
            xmldictdisplay.Clear();
            xmldictdisplayTask.Clear();
            componenttaskdisp.Clear();
        }

        private void rbSearchXML_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSearchXML.Checked == true)
            {
                CopyXML copyxmlobj = new CopyXML(this, true);
                copyxmlobj.StartPosition = FormStartPosition.CenterParent;
                copyxmlobj.ShowDialog();
            }
        }

        private void dataGridView2_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            /*
               0-VariableName
               1-ChangeType
               2-XML1
               3-XML2
               */
            // Check which column is selected, otherwise set NewColumn to null.
            Wait waitobj2 = new Wait();
            waitobj2.ShowInTaskbar = false;
            waitobj2.StartPosition = FormStartPosition.CenterScreen;
            waitobj2.Text = "Sorting...";
            waitobj2.Show();
            var displayvariable = from row in varReportobj
                                  orderby row.VariableName
                                  select new
                                  {
                                      //Report = row
                                      VariableName = row.VariableName,
                                      ChangeType = row.ChangeType,
                                      XML1 = row.XML1,
                                      XML2 = row.XML2
                                  };
            if (varReportobj.Count != 0)
            {
                switch (ascendinggridveiw2)
                {
                    case true:

                        switch (e.ColumnIndex)
                        {
                            case 0: displayvariable = from row in varReportobj
                                                      orderby row.VariableName
                                                      select new
                                                      {
                                                          //Report = row
                                                          VariableName = row.VariableName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 1: displayvariable = from row in varReportobj
                                                      orderby row.ChangeType
                                                      select new
                                                      {
                                                          //Report = row
                                                          VariableName = row.VariableName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 2: displayvariable = from row in varReportobj
                                                      orderby row.XML1
                                                      select new
                                                      {
                                                          //Report = row
                                                          VariableName = row.VariableName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 3: displayvariable = from row in varReportobj
                                                      orderby row.XML2
                                                      select new
                                                      {
                                                          //Report = row
                                                          VariableName = row.VariableName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                        };
                        ascendinggridveiw2 = false;
                        break;
                    case false:
                        switch (e.ColumnIndex)
                        {
                            case 0: displayvariable = from row in varReportobj
                                                      orderby row.VariableName descending
                                                      select new
                                                          {
                                                              //Report = row
                                                              VariableName = row.VariableName,
                                                              ChangeType = row.ChangeType,
                                                              XML1 = row.XML1,
                                                              XML2 = row.XML2
                                                          };
                                break;
                            case 1: displayvariable = from row in varReportobj
                                                      orderby row.ChangeType descending
                                                      select new
                                                      {
                                                          //Report = row
                                                          VariableName = row.VariableName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 2: displayvariable = from row in varReportobj
                                                      orderby row.XML1 descending
                                                      select new
                                                      {
                                                          //Report = row
                                                          VariableName = row.VariableName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 3: displayvariable = from row in varReportobj
                                                      orderby row.XML2 descending
                                                      select new
                                                      {
                                                          //Report = row
                                                          VariableName = row.VariableName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                        };
                        ascendinggridveiw2 = true;
                        break;

                }
            }
            dataGridView2.DataSource = null;
            dataGridView2.DataSource = displayvariable.ToArray();
            dataGridView2.Refresh();
            waitobj2.Close();
            waitobj2.Dispose();
        }

        private void dataGridView3_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            /*
              0-TaskName
              1-ChangeType
              2-XML1
              3-XML2
              */
            // Check which column is selected, otherwise set NewColumn to null.
            Wait waitobj2 = new Wait();
            waitobj2.ShowInTaskbar = false;
            waitobj2.StartPosition = FormStartPosition.CenterScreen;
            waitobj2.Text = "Sorting...";
            waitobj2.Show();
            var displayvariable = from row in varTaskReportobj
                                  orderby row.TaskName
                                  select new
                                  {
                                      //Report = row
                                      TaskName = row.TaskName,
                                      ChangeType = row.ChangeType,
                                      XML1 = row.XML1,
                                      XML2 = row.XML2
                                  };
            if (varTaskReportobj.Count != 0)
            {
                switch (ascendinggridview3)
                {
                    case true:

                        switch (e.ColumnIndex)
                        {
                            case 0: displayvariable = from row in varTaskReportobj
                                                      orderby row.TaskName
                                                      select new
                                                      {
                                                          //Report = row
                                                          TaskName = row.TaskName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 1: displayvariable = from row in varTaskReportobj
                                                      orderby row.ChangeType
                                                      select new
                                                      {
                                                          //Report = row
                                                          TaskName = row.TaskName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 2: displayvariable = from row in varTaskReportobj
                                                      orderby row.XML1
                                                      select new
                                                      {
                                                          //Report = row
                                                          TaskName = row.TaskName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 3: displayvariable = from row in varTaskReportobj
                                                      orderby row.XML2
                                                      select new
                                                      {
                                                          //Report = row
                                                          TaskName = row.TaskName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                        };
                        ascendinggridview3 = false;
                        break;
                    case false:
                        switch (e.ColumnIndex)
                        {
                            case 0: displayvariable = from row in varTaskReportobj
                                                      orderby row.TaskName descending
                                                      select new
                                                      {
                                                          //Report = row
                                                          TaskName = row.TaskName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 1: displayvariable = from row in varTaskReportobj
                                                      orderby row.ChangeType descending
                                                      select new
                                                      {
                                                          //Report = row
                                                          TaskName = row.TaskName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 2: displayvariable = from row in varTaskReportobj
                                                      orderby row.XML1 descending
                                                      select new
                                                      {
                                                          //Report = row
                                                          TaskName = row.TaskName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                            case 3: displayvariable = from row in varTaskReportobj
                                                      orderby row.XML2 descending
                                                      select new
                                                      {
                                                          //Report = row
                                                          TaskName = row.TaskName,
                                                          ChangeType = row.ChangeType,
                                                          XML1 = row.XML1,
                                                          XML2 = row.XML2
                                                      };
                                break;
                        };
                        ascendinggridview3 = true;
                        break;

                }
            }
            dataGridView3.DataSource = null;
            dataGridView3.DataSource = displayvariable.ToArray();
            dataGridView3.Refresh();
            waitobj2.Close();
            waitobj2.Dispose();
        }

        private void bwParseXML2_DoWork(object sender, DoWorkEventArgs e)
        {
            logicobj.ProgressChanged2 += (s, pe) => bwParseXML2.ReportProgress(pe.ProgressPercentage);


            strVariableCount2 = "Variables >> " + logicobj.CalcVar2(XDocument.Load(strFileName2).ToString());


            strTasksCount2 = "Tasks >> " + logicobj.CalTask2(XDocument.Load(strFileName2).ToString());

        }

        private void bwParseXML2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
        }

        private void bwParseXML2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {


            lbl_two_var.Text = strVariableCount2;
            lbl_two_task.Text = strTasksCount2;
            progressBar2.Visible = false;
            btnBrowseXML1.Enabled = false;
            xml2loaded = true;
            lbl_xml2_name.Text = "FileName >> " + strFileName2.Substring(strFileName2.LastIndexOf('\\') + 1); ;
            isLoadRunning = false;
            btnLoadXML2.Enabled = true;
            btnBrowseXML2.Enabled = true;
            btnClear2.Enabled = true;
            //}
        }

        private void frmCompare_Load(object sender, EventArgs e)
        {
            tbControl.TabPages.Remove(tabPage6);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Wait waitobj2 = new Wait();
            waitobj2.ShowInTaskbar = false;
            waitobj2.StartPosition = FormStartPosition.CenterScreen;
            waitobj2.Text = "Analyzing Please Wait...";
            waitobj2.Show();
            xdocAnalyze = XDocument.Parse(xmlstring);
            List<string> lst = new List<string>();
            List<string> lst2 = new List<string>();
            string temp = "";
            AnalyzeTaskReport_obj.Clear();
            AnalyzeTaskResultSet_obj.Clear();
            AnalyzeTaskParameter_obj.Clear();

            string ObjectName = "", Expression = "", EvalValue = "", Description = "", type = "";

            xdocAnalyze.Descendants().Nodes().ToList().ForEach(x =>
                {
                    if (x.ToString().StartsWith("<DTS:Executable"))
                        lst.Add(x.ToString());
                });

            lst.ForEach(x =>
                {
                    xdocAnalyzeTemp = XDocument.Parse(x);
                    xdocAnalyzeTemp.Descendants().ToList().ForEach(y =>
                        {
                            y.Attributes().ToList().ForEach(z =>
                                {
                                    if (z.Name.LocalName == "Name" && z.Value.ToString() == "ObjectName")
                                        ObjectName = y.Value.ToString();
                                });
                            y.Attributes().ToList().ForEach(z =>
                            {
                                if (z.Name.LocalName == "Name" && z.Value.ToString() == "Description")
                                    Description = y.Value.ToString();
                            });
                            y.Attributes().ToList().ForEach(z =>
                            {
                                if (z.Name.LocalName == "ExecutableType")
                                    type = z.Value.ToString();
                            });

                            if (y.Name.LocalName == "PropertyExpression")
                            {
                                temp =
                                y.Attributes().Where(attr => attr.Name.LocalName == "Name" && attr.Value.ToString() == "SqlStatementSource").
                                Select(attrvalue => attrvalue.Name).FirstOrDefault() == null ? "" : y.Attributes().Where(attr => attr.Name.LocalName == "Name" && attr.Value.ToString() == "SqlStatementSource").
                                Select(attrvalue => attrvalue.Name).FirstOrDefault().ToString().Trim();
                                if (temp != "")
                                    Expression = y.Value.ToString();
                            }

                            if (y.ToString().StartsWith("<SQLTask:SqlTaskData"))
                            {
                                y.Attributes().ToList().ForEach(attr =>
                                    {
                                        if (attr.Name.LocalName == "SqlStatementSource")
                                        {
                                            EvalValue = attr.Value.ToString();
                                            y.Descendants().ToList().ForEach(resultset =>
                                                {
                                                    if (resultset.ToString().StartsWith("<SQLTask:ResultBinding"))
                                                    {
                                                        if (!(AnalyzeTaskResultSet_obj.Exists(resultmatch => resultmatch.TaskName_ResultSet == ObjectName
                                                             &&
                                                             resultmatch.ResultSetName == (resultset.Attributes().Where(RS => RS.Name.LocalName == "DtsVariableName").
                                                                             Select(RSName => RSName.Value).FirstOrDefault() == null ? "" :
                                                                                 resultset.Attributes().Where(RS => RS.Name.LocalName == "DtsVariableName").
                                                                                 Select(RSName => RSName.Value).FirstOrDefault().ToString().Trim()))))
                                                        {
                                                            AnalyzeTaskResultSet_obj.Add(new AnalyzeTaskResultSet()
                                                            {
                                                                TaskName_ResultSet = ObjectName,
                                                                ResultSetName = resultset.Attributes().Where(RS => RS.Name.LocalName == "DtsVariableName").
                                                                                Select(RSName => RSName.Value).FirstOrDefault() == null ? "" :
                                                                                    resultset.Attributes().Where(RS => RS.Name.LocalName == "DtsVariableName").
                                                                                    Select(RSName => RSName.Value).FirstOrDefault().ToString().Trim()
                                                            });
                                                        }
                                                    }
                                                    if (resultset.ToString().StartsWith("<SQLTask:ParameterBinding"))
                                                    {
                                                        if (!(AnalyzeTaskParameter_obj.Exists(Parametername => Parametername.TaskName_Parameter == ObjectName
                                                            &&
                                                            Parametername.ParameterName == (resultset.Attributes().Where(RS => RS.Name.LocalName == "DtsVariableName").
                                                                            Select(RSName => RSName.Value).FirstOrDefault() == null ? "" :
                                                                                resultset.Attributes().Where(RS => RS.Name.LocalName == "DtsVariableName").
                                                                                Select(RSName => RSName.Value).FirstOrDefault().ToString().Trim()))))
                                                        {
                                                            AnalyzeTaskParameter_obj.Add(new AnalyzeTaskParameter()
                                                            {
                                                                TaskName_Parameter = ObjectName,
                                                                ParameterName = resultset.Attributes().Where(RS => RS.Name.LocalName == "DtsVariableName").
                                                                                Select(RSName => RSName.Value).FirstOrDefault() == null ? "" :
                                                                                    resultset.Attributes().Where(RS => RS.Name.LocalName == "DtsVariableName").
                                                                                    Select(RSName => RSName.Value).FirstOrDefault().ToString().Trim()
                                                            });
                                                        }
                                                    }
                                                });
                                        }
                                    });
                            }
                        });
                    AnalyzeTaskReport_obj.Add(new AnalyzeTaskReport()
                    {
                        TaskName_Report = ObjectName,
                        TaskExpression_Report = Expression,
                        TaskEvaluatedValue_Report = EvalValue,
                        description_Report = Description,
                        TaskType_Report = type
                    });
                    ObjectName = "";
                    Expression = "";
                    EvalValue = "";
                    Description = "";
                    type = "";
                });
            AnalyzeTaskReport_obj.RemoveAll(x => x.description_Report.Trim() == ""
                   && x.TaskEvaluatedValue_Report.Trim() == ""
                   && x.TaskExpression_Report.Trim() == ""
                   && x.TaskName_Report.Trim() == ""
                   && x.TaskType_Report.Trim() == "");
            AnalyzeTaskParameter_obj.RemoveAll(x => x.ParameterName.Trim() == "");
            AnalyzeTaskResultSet_obj.RemoveAll(x => x.ResultSetName.Trim() == "");
            waitobj2.Close();
            waitobj2.Dispose();
            fn_AnalyzeDispVariable();
            button3.Enabled = true;
            button4.Enabled = true;
            button1.Enabled = false;
        }
        private void fn_AnalyzeDispVariable()
        {
            int i = 1;
            xmldict.Clear();
            lst.Clear();
            xdoc = XDocument.Parse(xmlstring);
            xdoc.Elements().Nodes().ToList().ForEach(x =>
            {
                lst.Add(x.ToString());
            });

            lst.ForEach(xml =>
            {
                if (xml.StartsWith("<DTS:Variable"))
                    FillDict(xml);
            }
            );
            variableSearch("");
            var disp = from row in xmldictdisplay
                       select new
                       {
                           Sno = i++,
                           VariableName = row.Key
                       };
            dataGridView4.DataSource = disp.ToArray();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            dataGridView4.DataSource = null;
            dataGridView4.Refresh();
            int i = 1;
            var disp = from row in xmldictdisplay
                       where (System.Text.RegularExpressions.Regex.
                            IsMatch(row.Key.ToString(), txt_Filter.Text.Trim(),
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                       select new
                       {
                           Sno = i++,
                           VariableName = row.Key
                       };
            dataGridView4.DataSource = disp.ToArray();
        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                Wait waitobj2 = new Wait();
                waitobj2.ShowInTaskbar = false;
                waitobj2.StartPosition = FormStartPosition.CenterScreen;
                waitobj2.Text = "Analyzing Please Wait...";
                waitobj2.Show();
                dataGridView5.DataSource = null;
                dataGridView5.Refresh();
                dataGridView7.DataSource = null;
                dataGridView7.Refresh();
                dataGridView6.DataSource = null;
                dataGridView6.Refresh();
                dataGridView8.DataSource = null;
                dataGridView8.Refresh();
                dataGridView9.DataSource = null;
                dataGridView9.Refresh();
                fn_AnalyzeDispVariableReport(dataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Trim());
                waitobj2.Close();
                waitobj2.Dispose();
            }
        }
        private void fn_AnalyzeDispVariableReport(string variablename)
        {
            xdocTask = XDocument.Parse(xmlstring);
            lsttask.Clear();
            componenttask.Clear();
            xdocTask.Elements().Nodes().ToList().ForEach(x =>
            {
                lsttask.Add(x.ToString());
            });

            lsttask.ForEach(xml =>
            {
                if (xml.StartsWith("<DTS:Executable"))
                    FillDictTask(xml);
            }
            );
            variableSearchTask(variablename);
            var dftdisp = from row in componenttaskdisp
                          select new
                          {
                              TaskName = row.TaskName,
                              TaskExpression = row.TaskExpression
                          };
            dataGridView5.DataSource = dftdisp.ToArray();

            var taskdisp = from row in AnalyzeTaskReport_obj
                           where (Regex.IsMatch(row.TaskExpression_Report, variablename, RegexOptions.IgnoreCase)
                                    ||
                                  Regex.IsMatch(row.TaskEvaluatedValue_Report, variablename, RegexOptions.IgnoreCase))
                           select new
                           {
                               TaskName = row.TaskName_Report,
                               TaskExpression = row.TaskExpression_Report,
                               TaskEvaluated = row.TaskEvaluatedValue_Report
                           };
            dataGridView7.DataSource = taskdisp.ToArray();

            var ResultDisp = from row in AnalyzeTaskResultSet_obj
                             where (Regex.IsMatch(row.ResultSetName, variablename, RegexOptions.IgnoreCase))
                             select new
                             {
                                 TaskName = row.TaskName_ResultSet,
                                 ResultSet = row.ResultSetName
                             };

            dataGridView6.DataSource = ResultDisp.ToArray();

            var ParamDisp = from row in AnalyzeTaskParameter_obj
                            where (Regex.IsMatch(row.ParameterName, variablename, RegexOptions.IgnoreCase))
                            select new
                            {
                                TaskName = row.TaskName_Parameter,
                                Parameter = row.ParameterName
                            };
            dataGridView8.DataSource = ParamDisp.ToArray();

            var VarDisp = from row in xmldictdisplay
                          where (Regex.IsMatch(row.Value.objectexpression.ToString(), variablename, RegexOptions.IgnoreCase)
                                    ||
                                Regex.IsMatch(row.Value.objectexpressionvalue.ToString(), variablename, RegexOptions.IgnoreCase))
                          select new
                          {
                              VarName = row.Key,
                              VarExpr = row.Value.objectexpression,
                              VarExprVal = row.Value.objectexpressionvalue
                          };
            dataGridView9.DataSource = VarDisp.ToArray();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Wait waitobj2 = new Wait();
            waitobj2.ShowInTaskbar = false;
            waitobj2.StartPosition = FormStartPosition.CenterScreen;
            waitobj2.Text = "Generating Report Please Wait...";
            waitobj2.Show();
            int i = 1, j = 1, k = 1, l = 1, m = 1, n = 1;
            htmltext = "";
            htmltext = "<HTML><Head><h1><br>Report for SSIS Variables</h1></Head>";
            htmltext += "<body>";
            xmldictdisplay.ToList().ForEach(var =>
            {
                htmltext += "<br><font face=\"verdana\" color=\"#FF3300\"><<" + (n++).ToString() + ">> " + var.Key.ToString() + "</font>";
                htmltext += "<br>********************************";
                htmltext += "<br><font face=\"verdana\" color=\"#339933\"> VariableSearch </font> ";
                xmldictdisplay.ToList().ForEach(varsearch =>
                    {
                        if (varsearch.Key.ToString().Trim() != var.Key.ToString().Trim()
                            &&
                            (
                            Regex.IsMatch(varsearch.Value.objectexpression, var.Key, RegexOptions.IgnoreCase)
                            ||
                            Regex.IsMatch(varsearch.Value.objectexpressionvalue, var.Key, RegexOptions.IgnoreCase)
                            ))
                        {
                            htmltext += "<br><font face=\"verdana\" color=\"#330033\">" + (i++).ToString() + " " + varsearch.Key.ToString() + "</font>";
                        }
                    });
                htmltext += "<br>-----------------------------------";
                htmltext += "<br><font face=\"verdana\" color=\"#339933\"> DFT Tasks Search</font> ";
                componenttaskdisp.ToList().ForEach(dft =>
                    {
                        if (Regex.IsMatch(dft.TaskExpression, var.Key, RegexOptions.IgnoreCase))
                        {
                            htmltext += "<br><font face=\"verdana\" color=\"#330033\">" + (j++).ToString() + " " + dft.TaskName.ToString() + "</font>";
                        }
                    });
                htmltext += "<br>-----------------------------------";
                htmltext += "<br><font face=\"verdana\" color=\"#339933\"> Other Tasks Search</font> ";
                AnalyzeTaskReport_obj.ToList().ForEach(task =>
                    {
                        if (Regex.IsMatch(task.TaskExpression_Report, var.Key, RegexOptions.IgnoreCase)
                            ||
                            Regex.IsMatch(task.TaskEvaluatedValue_Report, var.Key, RegexOptions.IgnoreCase))
                        {
                            htmltext += "<br><font face=\"verdana\" color=\"#330033\">" + (k++).ToString() + " " + task.TaskName_Report.ToString() + "</font>";
                        }
                    });
                htmltext += "<br>-----------------------------------";
                htmltext += "<br><font face=\"verdana\" color=\"#339933\"> Result Set Search</font> ";
                AnalyzeTaskResultSet_obj.ToList().ForEach(result =>
                    {
                        if (Regex.IsMatch(result.ResultSetName, var.Key, RegexOptions.IgnoreCase))
                            htmltext += "<br><font face=\"verdana\" color=\"#330033\">" + (l++).ToString() + " " + result.TaskName_ResultSet.ToString() + "</font>";
                    });
                htmltext += "<br>-----------------------------------";
                htmltext += "<br><font face=\"verdana\" color=\"#339933\"> Parameter Search</font> ";
                AnalyzeTaskParameter_obj.ToList().ForEach(param =>
                    {
                        if (Regex.IsMatch(param.ParameterName, var.Key, RegexOptions.IgnoreCase))
                            htmltext += "<br><font face=\"verdana\" color=\"#330033\">" + (m++).ToString() + " " + param.TaskName_Parameter.ToString() + "</font>";
                    });
                i = 1; j = 1; k = 1; l = 1; m = 1;
                htmltext += "<br>========================================";
                htmltext += "<br>========================================";
            });
            htmltext += "<br><font face=\"verdana\" color=\"#339933\"> Generated On " + DateTime.Now.ToShortDateString() + "</font> ";
            htmltext += "</body></html>";
            saveFileDialog3.Filter = "HTML File| *.html";
            waitobj2.Close();
            waitobj2.Dispose();
            saveFileDialog3.ShowDialog();
        }

        private void exportToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveFileDialog3_FileOk(object sender, CancelEventArgs e)
        {
            //System.IO.File.WriteAllLines(saveFileDialog3.FileName,htmltext);
            System.IO.File.WriteAllText(saveFileDialog3.FileName, htmltext);
            MessageBox.Show("Report Saved Successfully");
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            int i = 1;
            var disp = from row in xmldictdisplay
                       where (Regex.IsMatch(row.Key, textBox1.Text.Trim(), RegexOptions.IgnoreCase))
                       select new
                       {
                           SerialNumber = i++,
                           VariableName = row.Key,
                           Expression = row.Value.objectexpression,
                           EvaluatedExpression = row.Value.objectexpressionvalue
                       };
            dgvFill.DataSource = null;
            dgvFill.DataSource = disp.ToArray();
            dgvFill.Refresh();
            lbl_rowcount.Text = "Total Rows >> " + (i - 1);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int i = 1;
            if (rbtn_variable.Checked == true)
            {
                var disp = from row in xmldictdisplay
                           where (Regex.IsMatch(row.Key, textBox1.Text.Trim(), RegexOptions.IgnoreCase))
                           select new
                           {
                               SerialNumber = i++,
                               VariableName = row.Key,
                               Expression = row.Value.objectexpression,
                               EvaluatedExpression = row.Value.objectexpressionvalue
                           };
                dgvFill.DataSource = null;
                dgvFill.DataSource = disp.ToArray();
                dgvFill.Refresh();
                lbl_rowcount.Text = "Total Rows >> " + (i - 1);
            }
            if (rbtn_task.Checked == true)
            {
                var disptask = from row in componenttaskdisp
                               where (Regex.IsMatch(row.TaskName, textBox1.Text.Trim(), RegexOptions.IgnoreCase))
                               select new
                               {
                                   SerialNumber = i++,
                                   TaskName = row.TaskName,
                                   TaskExpression = row.TaskExpression
                               };
                dgvFill.DataSource = null;
                dgvFill.DataSource = disptask.ToArray();
                dgvFill.Refresh();
                lbl_rowcount.Text = "Total Rows >> " + (i - 1);
            }
        }
    }
}
