﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Ionic.Zip;
using System.Xml;
using System.Xml.XPath;

namespace Simulator
{
    public partial class Exam_Properties : Form
    {
        static string fullFilePath;
        static string filename;
        
        public Exam_Properties()
        {
            InitializeComponent();
        }

        public Exam_Properties(string filePath, string fileName)
        {
            fullFilePath = filePath;
            filename = fileName;
            InitializeComponent();
            lbl_full_path.Text = fullFilePath;
            FileInfo file = new FileInfo(fullFilePath);
            lbl_file_size.Text = Math.Round(Convert.ToDouble(file.Length / 1024.00), 2).ToString() + " KB";
            lbl_created.Text = file.CreationTime.ToShortDateString();
        }

        int numOfSections;
        int numOfQuestions;
        private void Exam_Properties_Load(object sender, EventArgs e)
        {
            string xmlPath = GlobalPathVariables.GetXmlFilePath(GlobalPathVariables.GetExamFilesFolder(filename));

            if (File.Exists(xmlPath + ".tmp"))
            {
                File.Delete(xmlPath + ".tmp");
            }

            if (!(File.Exists(xmlPath)))
            {
                using (ZipFile zip = ZipFile.Read(fullFilePath))
                {
                    foreach (ZipEntry ent in zip)
                    {
                        ent.Extract(GlobalPathVariables.GetExamFilesFolder(filename), ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }

            using (XmlReader reader = XmlReader.Create(xmlPath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "OpenExamDocument")
                    {
                        while (reader.NodeType != XmlNodeType.EndElement)
                        {
                            reader.Read();
                            if (reader.Name == "FileVersion")
                            {
                                while (reader.NodeType != XmlNodeType.EndElement)
                                {
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.Text)
                                    {
                                        lbl_file_version.Text = reader.Value;
                                    }
                                }
                                reader.Read();
                            }
                            if (reader.Name == "ExamDetails")
                            {
                                while (reader.NodeType != XmlNodeType.EndElement)
                                {
                                    reader.Read();
                                    if (reader.Name == "ExamTitle")
                                    {
                                        while (reader.NodeType != XmlNodeType.EndElement)
                                        {
                                            reader.Read();
                                            if (reader.NodeType == XmlNodeType.Text)
                                            {
                                                lbl_title.Text = reader.Value;
                                            }
                                        }
                                        reader.Read();
                                    }
                                    if (reader.Name == "TimeAllowed")
                                    {
                                        while (reader.NodeType != XmlNodeType.EndElement)
                                        {
                                            reader.Read();
                                            if (reader.NodeType == XmlNodeType.Text)
                                            {
                                                lbl_time_limit.Text = reader.Value;
                                            }
                                        }
                                        reader.Read();
                                    }
                                    if (reader.Name == "PassingScore")
                                    {
                                        while (reader.NodeType != XmlNodeType.EndElement)
                                        {
                                            reader.Read();
                                            if (reader.NodeType == XmlNodeType.Text)
                                            {
                                                lbl_passing_score.Text = reader.Value;
                                                Properties.Settings.Default.RequiredScore = Convert.ToInt32(reader.Value);
                                            }
                                        }
                                        reader.Read();
                                    }
                                }
                                reader.Read();
                            }
                        }
                    }
                }
            }

            //save the property changes
            Properties.Settings.Default.Save();

            //New try
            try
            {
                XPathDocument doc = new XPathDocument(xmlPath);
                XPathNavigator nav = doc.CreateNavigator();
                // Compile a standard XPath expression
                XPathExpression expr;
                expr = nav.Compile("OpenExamDocument/Questions/Section");
                XPathNodeIterator iterator = nav.Select(expr);
                // Iterate on the node set
                while (iterator.MoveNext())
                {
                    numOfSections += 1;
                }
                XPathExpression exp = nav.Compile("//Question");
                XPathNodeIterator iter = nav.Select(exp);
                while(iter.MoveNext())
                {
                    numOfQuestions += 1;
                }
            }
            catch (Exception ex)
            {
                GlobalPathVariables.WriteError(ex, this.Name);
            }
            lbl_section_number.Text = numOfSections.ToString();
            lbl_number_of_questions.Text = numOfQuestions.ToString();
        }
    }
}
