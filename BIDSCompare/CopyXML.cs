using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BIDSCompare
{
    public partial class CopyXML : Form
    {
        frmCompare form1obj = new frmCompare();
        public int xmlchoice ;
        public bool isSearch = false;
        public string strXMLContent;
        private string strVariableCount,strVariableCount2;
        private string strTasksCount,strTasksCount2;
        Logic logicobj;
        public CopyXML()
        {
           
            InitializeComponent();
        }
        public CopyXML(frmCompare obj, bool isSearchForm)
        {
            form1obj = obj;
            isSearch = isSearchForm;
            InitializeComponent();
        }
        public CopyXML(frmCompare obj, ref Logic objlogicobj)
        {
            form1obj = obj;
            logicobj = objlogicobj;
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        private void button2_Click(object sender, EventArgs e)
        {
     
          
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            strXMLContent = "";
            if (isSearch)
            {
                strXMLContent = txt_InputXML.Text.Replace("©", "").Trim();
                if (form1obj.IsXmlValid(strXMLContent, "S"))
                {
                    if (MessageBox.Show("Do You Wish To Save this File", "Save File", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        saveFileDialog1.Filter = "XML File| *.xml";
                        saveFileDialog1.ShowDialog();
                    }
                    form1obj.xmlstring = txt_InputXML.Text.Replace("©", "").Trim();
                    MessageBox.Show("XML Loaded Successfully");
                    form1obj.pnlBrowseCopy.Enabled = false;
                    form1obj.tbControl.TabPages.Add(form1obj.tabPage6);
                    this.Dispose();
                }
                else
                {
                    MessageBox.Show("Error While Loading XML");
                }
            }
            {
                progressBar1.Visible = true;
                
                if (xmlchoice == 1)
                {
                    strXMLContent = txt_InputXML.Text.Replace("©", "").Trim();
                    if (MessageBox.Show("Do You Wish To Save this File", "Save File", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        saveFileDialog1.Filter = "XML File| *.xml";
                        saveFileDialog1.ShowDialog();
                    }
                    if (form1obj.IsXmlValid(strXMLContent, "S"))
                    {
                        form1obj.xml1loaded = true;
                        bwParseXML.RunWorkerAsync();


                    }
                    else
                    {
                        MessageBox.Show("Error While Loading XML");
                    }

                }
                if (xmlchoice == 2)
                {
                    strXMLContent = txt_InputXML.Text.Replace("©", "").Trim();
                    if (MessageBox.Show("Do You Wish To Save this File", "Save File", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        saveFileDialog1.Filter = "XML File| *.xml";
                        saveFileDialog1.ShowDialog();
                    }
                    if (form1obj.IsXmlValid(strXMLContent, "S"))
                    {
                        form1obj.xml2loaded = true;
                        bwParseXML.RunWorkerAsync();

                    }
                    else
                    {
                        MessageBox.Show("Error While Loading XML");
                    }

                }
            }
        }

        private void bwParseXML_DoWork(object sender, DoWorkEventArgs e)
        {
            bwParseXML.WorkerReportsProgress = true;

            logicobj.ProgressChanged += (s, pe) => bwParseXML.ReportProgress(pe.ProgressPercentage);
            if (xmlchoice == 1)
            {

                strVariableCount = "Variables >> " + logicobj.CalcVar(strXMLContent);


                strTasksCount = "Tasks >> " + logicobj.CalTask(strXMLContent);
            }
            else if (xmlchoice == 2)
            {
                strVariableCount2 = "Variables >> " + logicobj.CalcVar2(strXMLContent);


                strTasksCount2 = "Tasks >> " + logicobj.CalTask2(strXMLContent);
            }
     
        }

        private void bwParseXML_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void bwParseXML_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (xmlchoice == 1)
            {
                form1obj.strVariableCount = strVariableCount;
                form1obj.strTasksCount = strTasksCount;
            }
            if (xmlchoice == 2)
            {
                form1obj.strVariableCount2 = strVariableCount2;
                form1obj.strTasksCount2 = strTasksCount2;
            }
           
            form1obj.SetControls(xmlchoice);
            
            this.Dispose();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.File.WriteAllText(saveFileDialog1.FileName, txt_InputXML.Text.Replace("©", "").Trim());
            MessageBox.Show("File Saved Successfully");
        }
        //public void setXML(string str)
        //{
        //    if (xmlchoice == 1)
        //    {
        //        xmlstr1 = str;
        //        if (IsXmlValid(xmlstr1, "S"))
        //        {
        //            lbl_One_Var.Text = "Variables >> " + logicobj.CalcVar(xmlstr1);
        //            lbl_one_task.Text = "Tasks >> " + logicobj.CalTask(xmlstr1);
        //            btnBrowseXMl2.Enabled = false;
        //            btnBrowseXML1.Enabled = false;
        //            xml1loaded = true;
        //            lbl_xml1_name.Text = "";
        //        }
        //        else
        //        {
        //            MessageBox.Show("Error While Loading XML");
        //        }
        //        //button1.Enabled = false;
        //        //button2.Enabled = false;
        //    }
        //    if (xmlchoice == 2)
        //    {
        //        xmlstr2 = str;
        //        if (IsXmlValid(xmlstr2, "S"))
        //        {

        //            lbl_two_var.Text = "Variables >> " + logicobj.CalcVar2(xmlstr2);
        //            lbl_two_task.Text = "Tasks >> " + logicobj.CalTask2(xmlstr2);

        //            xml2loaded = true;
        //            lbl_xml2_name.Text = "";
        //        }
        //        else
        //        {
        //            MessageBox.Show("Error While Loading XML");
        //        }
        //        // button1.Enabled = false;
        //        //button2.Enabled = false;
        //    }
        //}
       
    }
}
