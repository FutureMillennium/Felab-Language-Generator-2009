using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;

namespace Language_Generator
{
    public partial class MainForm : Form
    {
        String strDocument = "Untitled";
        String strDocPath = "";
        bool blDocChanged = false;
        String strDefinitions = "Untitled";
        String strDefPath = "";
        bool blDefChanged = false;
        String strDictionary = "Untitled";
        String strDicPath = "";
        bool blDicChanged = false;
        Random random = new Random();

        bool blDefChangedSince = false;
        int intWarn = 0;
        String strErrorDetail = "";

        ArrayList curContent = new ArrayList();

        public enum MessageBeepType
        {
            Default = -1,
            Ok = 0x00000000,
            Error = 0x00000010,
            Question = 0x00000020,
            Warning = 0x00000030,
            Information = 0x00000040,
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MessageBeep(
            MessageBeepType type
        );

        // Function: Read string from a file
        private string ReadFile(string file)
        {
            StreamReader reader = new StreamReader(file, Encoding.Default);
            string data = reader.ReadToEnd();
            reader.Close();

            return data;
        }

        // Function: Save string to a file
        private void SaveFile(string file, string data)
        {
            StreamWriter writer = new StreamWriter(file);
            writer.Write(data);
            writer.Close();
        }

        private void UpdateNames()
        {
            if (blDefChanged)
            {
                tlDefinitions.Text = strDefinitions + "*";
                this.Text = strDefinitions + "* - " + this.ProductName;
            }
            else
            {
                tlDefinitions.Text = strDefinitions;
                this.Text = strDefinitions + " - " + this.ProductName;
            }

            if (blDicChanged)
                tlDictionary.Text = strDictionary + "*";
            else
                tlDictionary.Text = strDictionary;

            if (blDocChanged)
                tlResult.Text = strDocument + "*";
            else
                tlResult.Text = strDocument;
        }



        public MainForm()
        {
            InitializeComponent();
        }

        // Quit
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Definitions file drag
        private void txtDefinitions_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.All;
        }

        // Form load
        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateNames();

            //ToolStripManager.LoadSettings(this);
        }

        // Definitions changed
        private void txtDefinitions_TextChanged(object sender, EventArgs e)
        {
            if (!blDefChanged)
            {
                blDefChanged = true;
                blDefChangedSince = true;
                UpdateNames();
            }
            else if (!blDefChangedSince)
                blDefChangedSince = true;
        }

        // Dictionary copy selection to Results
        private void txtDictionary_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                /*if (txtDictionary.SelectionLength == 0)
                {
                    txtResult.SelectionStart = txtDictionary.GetFirstCharIndexOfCurrentLine();
                    txtResult.SelectionLength = txtResult.Lines[txtResult.GetLineFromCharIndex(txtResult.SelectionStart)].Length;
                }
                else {
                    txtResult.SelectionStart = txtDictionary.SelectionStart;
                    txtResult.SelectionLength = txtDictionary.SelectionLength;
                }*/
                txtResult.SelectionStart = txtResult.GetFirstCharIndexFromLine(txtDictionary.GetLineFromCharIndex(txtDictionary.SelectionStart));
                txtResult.SelectionLength = txtResult.Lines[txtResult.GetLineFromCharIndex(txtResult.SelectionStart)].Length;
                txtResult.ScrollToCaret();
            }
            catch { }
        }

        // Results copy selection to Dictionary
        private void txtResult_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                /*if (txtResult.SelectionLength == 0)
                {
                    txtDictionary.SelectionStart = txtResult.GetFirstCharIndexOfCurrentLine();
                    txtDictionary.SelectionLength = txtDictionary.Lines[txtDictionary.GetLineFromCharIndex(txtResult.SelectionStart)].Length;
                }
                else
                {
                    txtDictionary.SelectionStart = txtResult.SelectionStart;
                    txtDictionary.SelectionLength = txtResult.SelectionLength;
                }*/
                txtDictionary.SelectionStart = txtDictionary.GetFirstCharIndexFromLine(txtResult.GetLineFromCharIndex(txtResult.SelectionStart));
                txtDictionary.SelectionLength = txtDictionary.Lines[txtDictionary.GetLineFromCharIndex(txtDictionary.SelectionStart)].Length;
                txtDictionary.ScrollToCaret();
            }
            catch { }
        }

        // Select all
        private void selectallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBox tmpText;

            tmpText = txtDefinitions;

            if (txtDictionary.Focused)
                tmpText = txtDictionary;
            else if (txtResult.Focused)
                tmpText = txtResult;

            tmpText.SelectionStart = 0;
            tmpText.SelectionLength = tmpText.Text.Length;
        }

        // Dictionary selection changed
        private void txtDictionary_KeyUp(object sender, KeyEventArgs e)
        {
            txtDictionary_MouseUp(null, null);
        }

        // Results selection changed
        private void txtResult_KeyUp(object sender, KeyEventArgs e)
        {
            txtResult_MouseUp(null, null);
        }

        // Generate sample
        private void tlGenerateSample_Click(object sender, EventArgs e)
        {
            intWarn = 0;

            ParseDefinitions();
            tlSampleText.Text = GenerateWord();
            switch (intWarn) {
                case 1:
                    stStatus.Text = "Warning: Couldn't find group " + strErrorDetail;
                    stStatus.Image = global::Language_Generator.Properties.Resources.Warning;
                    break;
                case 2:
                    stStatus.Text = "Error: No valid group definitions or word masks";
                    stStatus.Image = global::Language_Generator.Properties.Resources.Critical;
                    break;
                default:
                    stStatus.Text = "Sample generated successfully";
                    stStatus.Image = global::Language_Generator.Properties.Resources.OK;
                    break;
            }
        }

        // ---------------------------------------------------------------------
        // Generate a word
        // ---------------------------------------------------------------------
        private string GenerateWord()
        {
            if (curContent.Count > 1)
            {
                ArrayList thisPatch;
                String strMask;

                thisPatch = (ArrayList)curContent[curContent.Count - 1];

                strMask = (string)thisPatch[random.Next(thisPatch.Count)];

                return GenerateWordByMask(strMask);
            }
            else
            {
                intWarn = 2;
                return "Error";
            }
        }

        // ---------------------------------------------------------------------
        // Generate a word by mask
        // ---------------------------------------------------------------------
        private string GenerateWordByMask(string strMask)
        {
            int i, j;
            ArrayList thisPatch;
            string strWord = "", strCur;
            bool blFound;

            for (i = 0; i < strMask.Length; i++)
            {
                strCur = strMask.Substring(i, 1);
                blFound = false;

                for (j = 0; j < curContent.Count - 1; j++)
                {
                    thisPatch = (ArrayList)curContent[j];
                    if (strCur == (string)thisPatch[0] && thisPatch.Count > 1)
                    {
                        strWord += thisPatch[random.Next(thisPatch.Count - 1) + 1];
                        blFound = true;
                        break;
                    }
                }

                if (!blFound)
                {
                    intWarn = 1;
                    strErrorDetail = strCur;
                }
            }

            return strWord;
        }

        // ---------------------------------------------------------------------
        // Parse the definitions
        // ---------------------------------------------------------------------
        private void ParseDefinitions()
        {
            if (blDefChangedSince)
            {
                int i, j, curPatch = 0, tmpSep, tmpSepValue;
                bool blHaveContent = false;
                ArrayList thisPatch;
                String tmpLine;

                curContent.Clear();

                for (i = 0; i < txtDefinitions.Lines.Length; i++)
                {
                    tmpLine = txtDefinitions.Lines[i].Trim();

                    if (tmpLine == "")
                    {
                        if (blHaveContent)
                        {
                            curPatch++;
                            blHaveContent = false;
                        }
                    }
                    else if (tmpLine.Substring(0, 1) == ";")
                        continue;
                    else
                    {
                        if (!blHaveContent)
                        {
                            curContent.Add(new ArrayList());
                            blHaveContent = true;
                        }

                        thisPatch = (ArrayList)curContent[curPatch];

                        tmpSep = tmpLine.IndexOf(" ");
                        if (tmpSep > -1)
                        {
                            try
                            {
                                tmpSepValue = Convert.ToInt32(tmpLine.Substring(tmpSep + 1));
                            }
                            catch
                            {
                                tmpSepValue = 1;
                            }
                            tmpLine = tmpLine.Substring(0, tmpSep);
                            for (j = 0; j < tmpSepValue; j++)
                            {
                                thisPatch.Add(tmpLine);
                            }
                        }
                        else
                        {
                            thisPatch.Add(tmpLine);
                        }
                    }
                }

                blDefChangedSince = false;
            }
        }

        // Form closing
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ToolStripManager.SaveSettings(this);
        }

        // Split1 double click
        private void splitContainer1_DoubleClick(object sender, EventArgs e)
        {
            //splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
        }

        // Definitions: Open
        private void tlOpenDef_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = strDocPath;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                OpenDefinitions(openFileDialog1.FileName);
        }

        // Definitions changed
        private void OpenDefinitions(string file)
        {   
            txtDefinitions.Text = ReadFile(file);
            strDefPath = file;
            strDefinitions = System.IO.Path.GetFileName(strDefPath);

            blDefChanged = false;

            UpdateNames();

            stStatus.Text = "Definitions file opened successfully";
            stStatus.Image = global::Language_Generator.Properties.Resources.OK;
        }

        // Dictionary changed
        private void txtDictionary_TextChanged(object sender, EventArgs e)
        {
            if (!blDicChanged)
            {
                blDicChanged = true;
                UpdateNames();
            }
        }

        // Results changed
        private void txtResult_TextChanged(object sender, EventArgs e)
        {
            if (!blDocChanged)
            {
                blDocChanged = true;
                UpdateNames();
                //this.Text = strDocument + "* - " + PRODUCT_NAME;
            }
        }

        // Dictionary: Open
        private void tlOpenDic_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = strDocPath;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                OpenDictionary(openFileDialog1.FileName);
        }

        // Open Dictionary file
        private void OpenDictionary(string file)
        {
            txtDictionary.Text = ReadFile(file);
            strDicPath = file;
            strDictionary = System.IO.Path.GetFileName(strDicPath);

            blDicChanged = false;

            UpdateNames();

            stStatus.Text = "Dictionary file opened successfully";
            stStatus.Image = global::Language_Generator.Properties.Resources.OK;
        }

        // Test Definitions
        private void tlTest_Click(object sender, EventArgs e)
        {
            ParseDefinitions();

            if (curContent.Count > 1)
            {
                ArrayList thisPatch;
                int i, intErrN = 0;
                string strErrors = "", tmpDictionary = "", tmpResult = "";

                thisPatch = (ArrayList)curContent[curContent.Count - 1];

                strDictionary = "Test";
                strDicPath = "";
                strDocument = "Test Result";
                strDocPath = "";

                for (i = 0; i < thisPatch.Count; i++)
                {
                    intWarn = 0;

                    tmpDictionary += (string)thisPatch[i] + "\r\n";
                    tmpResult += GenerateWordByMask((string)thisPatch[i]) + "\r\n";

                    if (intWarn != 0)
                    {
                        intErrN++;
                        strErrors += strErrorDetail;
                    }
                }

                txtDictionary.Text = tmpDictionary;
                txtResult.Text = tmpResult;

                if (intErrN == 0)
                {
                    stStatus.Text = "Test successful";
                    stStatus.Image = global::Language_Generator.Properties.Resources.OK;
                }
                else
                {
                    stStatus.Text = intErrN + " warnings: couldn't find groups " + strErrors;
                    stStatus.Image = global::Language_Generator.Properties.Resources.Warning;
                }

                UpdateNames();
            }
            else
            {
                stStatus.Text = "Error: No valid group definitions or word masks";
                stStatus.Image = global::Language_Generator.Properties.Resources.Critical;
            }
        }

        // Results: Open
        private void tlOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = strDocPath;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                OpenResults(openFileDialog1.FileName);
        }

        // Open Results file
        private void OpenResults(string file)
        {
            txtResult.Text = ReadFile(file);
            strDocPath = file;
            strDocument = System.IO.Path.GetFileName(strDocPath);

            blDocChanged = false;

            //this.Text = strDocument + " - " + PRODUCT_NAME;
            UpdateNames();

            stStatus.Text = "Language file opened successfully";
            stStatus.Image = global::Language_Generator.Properties.Resources.OK;
        }

        private void newLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckDefinitionsSave())
            {
                txtDefinitions.Text = "";

                blDefChanged = false;
                strDefinitions = "Untitled";
                strDefPath = "";

                UpdateNames();
            }
        }

        private bool CheckDefinitionsSave()
        {
            if (blDefChanged && !(txtDefinitions.Text == "" && strDefPath == ""))
            {
                switch (MessageBox.Show("Definitions file " + strDocument + " not saved.\n\nDo you wish to save?", this.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        //saveToolStripMenuItem_Click(null, null);
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private bool CheckDictionarySave()
        {
            if (blDicChanged && !(txtDictionary.Text == "" && strDicPath == ""))
            {
                switch (MessageBox.Show("Dictionary file " + strDocument + " not saved.\n\nDo you wish to save?", this.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        //saveToolStripMenuItem_Click(null, null);
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private bool CheckResultSave()
        {
            if (blDocChanged && !(txtResult.Text == "" && strDocPath == ""))
            {
                switch (MessageBox.Show("Result file " + strDocument + " not saved.\n\nDo you wish to save?", this.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        //saveToolStripMenuItem_Click(null, null);
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAboutBox frmAbout = new frmAboutBox();
            frmAbout.ShowDialog(this);
        }

        private void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            txtDictionary.Text = "";

            blDicChanged = false;
            strDictionary = "Untitled";
            strDicPath = "";

            UpdateNames();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtResult.Text = "";

            blDocChanged = false;
            strDocument = "Untitled";
            strDocPath = "";

            UpdateNames();
        }
    }
}
