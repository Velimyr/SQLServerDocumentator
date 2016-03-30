using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SQL_Docs_Generator
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                drpAuthType.Items.Add("Windows authentification");
                drpAuthType.Items.Add("SQL server authentification");
            }
        }

        private string SPListGet = "SELECT name, SCHEMA_NAME ( schema_id )as [schema]  FROM sys.objects WHERE type = 'P' ";
        private string SPGet = "sp_helptext '{0}'";
        private string SPParamGet = "select parameter_name from information_schema.parameters where specific_name='{0}'";
        private string DBQueryStr = "SELECT name FROM master.dbo.sysdatabases";

        private string WinConnectionTemplate = "Server={0};Database={1};Trusted_Connection=True;";
        private string SQLConnectionTemplate = "Server={0};Database={1};User Id={2};Password={3};";

        private string ConnectionString = "";

        //protected void btnGo_Click(object sender, EventArgs e)
        //{
        //    SqlConnection con = new SqlConnection(txtConnectionStr.Text.ToString());
        //    con.Open();     
        //    SqlCommand comm = new SqlCommand(SPListGet, con);
        //    SqlDataReader dr = comm.ExecuteReader();
        //    DataTable dt = new DataTable();

        //    dt.Load(dr);
        //    con.Close();

        //    List<SPClass> SPList = new List<SPClass>();
        //    string Res = "";
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {           
        //        SPList.Add(GetSPBody(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString()));                
        //    }
        //    SPCreatePDF(SPList);
        //    txtResult.Text = Res;         

        //}

        private SPClass GetSPBody(string name, string schema)
        {
            //Процедура отримання данных про stored procedure
            string QueryGetSP = String.Format(SPGet, name);

            SqlConnection con;
            SqlCommand comm;
            SqlDataReader dr;
            DataTable dt;
            try
            {
                con = new SqlConnection(ConnectionString);
                con.Open();
                comm = new SqlCommand(QueryGetSP, con);
                dr = comm.ExecuteReader();
                dt = new DataTable();
                dt.Load(dr);
            }
            catch (Exception err)
            {
                return null;
            }
            string SP_Text = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                SP_Text += dt.Rows[i][0].ToString();
            }
            SPClass NewSP = new SPClass();

            string Name_pattern = "(?<=CREATE PROCEDURE )(.*)";
            string Author_pattern = "(?<=-- Author:)(.*)";
            string Created_pattern = "(?<=-- Create date:)(.*)";
            string Desc_pattern = "(?<=-- Description:)(.*)";
            string Section_pattern = "(?<=-- Section:)(.*)";
            string returned_pattern = "statuses(?s)(.*)(?=/*/)";

            NewSP.name = name;
            NewSP.schema = schema;
            NewSP.author = Regex.Match(SP_Text, Author_pattern).ToString().Trim();
            NewSP.creationdate = Regex.Match(SP_Text, Created_pattern).ToString().Trim();
            NewSP.desc = Regex.Match(SP_Text, Desc_pattern).ToString().Trim();
            NewSP.returned = Regex.Match(SP_Text, returned_pattern).ToString().Trim();
            NewSP.section = Regex.Match(SP_Text, Section_pattern).ToString().Trim();

            NewSP.parameters = SPGetParams(NewSP.name, SP_Text);

            con.Close();
            return NewSP;
        }

        private List<string> SPGetParams (string name, string SP_Text)
        {
            //Пошук парамерів процедури
            string QueryGetSPParams = String.Format(SPParamGet, name);
            string param_pattern = "(?={0})(.*)";

            SqlConnection con;
            SqlCommand comm;
            SqlDataReader dr;
            DataTable dt;
            try
            {
                con = new SqlConnection(ConnectionString);
                con.Open();
                comm = new SqlCommand(QueryGetSPParams, con);
                dr = comm.ExecuteReader();
                dt = new DataTable();
                dt.Load(dr);
            }
            catch (Exception err)
            {
                return null;
            }
            List<string> paramList = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string pat = String.Format(param_pattern, dt.Rows[i][0].ToString());
                string param_value = Regex.Match(SP_Text, pat).ToString().Trim();
                paramList.Add(param_value);
            }
            con.Close();
            return paramList;
        }

        private void SPCreatePDF (List<SPClass> SPData)
        {
            try
            {
                SPData.Sort( delegate (SPClass c1, SPClass c2) { return c1.section.CompareTo(c2.section); });

                // Create a Document object
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);

                // Create a new PdfWriter object, specifying the output stream
                FileStream output = new FileStream(Server.MapPath("SQLDoc.pdf"), FileMode.Create);
                PdfWriter writer = PdfWriter.GetInstance(document, output);

                //Adding metainfo
                document.AddTitle("SQL Server Stored Procedure Documentation");
                document.AddSubject("SQL Server Stored Procedure Documentation ");
                document.AddKeywords("SQL Server, Stored Procedure, Documentation");
                document.AddAuthor("SQL Doc Creator");

                //Fonts registration
                BaseFont baseFont = BaseFont.CreateFont(Server.MapPath("~/Content/Arial.ttf"), BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font mainTextFont = new Font(baseFont, 10);
                Font SubHeaderTextFont = new Font(baseFont, 10, 1);
                Font MinTextFont = new Font(baseFont, 9, 2);
                Font HeaderTextFont = new Font(baseFont, 14, 1);
                Font SectionHeaderTextFont = new Font(baseFont, 18, 1);

                // Open the Document for writing
                document.Open();

                string Section_Name = "";
                foreach (SPClass item in SPData)
                {
                    if (Section_Name != item.section)
                    {
                        document.NewPage();
                        Paragraph SectionParagraph = new Paragraph("Section: " + item.section, SectionHeaderTextFont);
                        document.Add(SectionParagraph);
                        Section_Name = item.section;
                    }
                    //Назва процедури 
                    document.Add(new Chunk("\n")); //Відступ  
                    
                    Paragraph NameParagraph = new Paragraph("[" + item.schema + "].[" + item.name + "]", HeaderTextFont);
                    document.Add(NameParagraph);
                    //Опис процедури
                    if (chkDescr.Checked == true)
                    {
                        Paragraph DescParagraph = new Paragraph(item.desc, mainTextFont);
                        document.Add(DescParagraph);
                    }

                    //Автор і дата створення процедури
                    if (chkAuthor.Checked == true)
                    {
                        Paragraph AuthorParagraph = new Paragraph("Author: " + item.author, MinTextFont);
                        AuthorParagraph.Alignment = Element.ALIGN_RIGHT;
                        document.Add(AuthorParagraph);
                    }
                    if (chkCreated.Checked == true)
                    {
                        Paragraph CreatedParagraph = new Paragraph("Created date: " + item.creationdate, MinTextFont);
                        CreatedParagraph.Alignment = Element.ALIGN_RIGHT;
                        document.Add(CreatedParagraph);
                    }
                    //Вхідні параметри
                    if (chkParams.Checked == true)
                    {
                        document.Add(new Chunk("\n")); //Відступ  
                        Paragraph ParamLabelParagraph = new Paragraph("Incoming parameters: ", SubHeaderTextFont);
                        document.Add(ParamLabelParagraph);
                        if (item.parameters.Count > 0)
                            foreach (string ParamItem in item.parameters)
                            {
                                Paragraph ParamParagraph = new Paragraph("      " + ParamItem, mainTextFont);
                                document.Add(ParamParagraph);
                            }
                        else
                        {
                            Paragraph NoParamParagraph = new Paragraph("        No parameters", mainTextFont);
                            document.Add(NoParamParagraph);
                        }
                    }
                    //Значення що повертаються
                    if (chkStatuses.Checked == true)
                    {
                        if (item.returned != "")
                        {
                            document.Add(new Chunk("\n")); //Відступ
                            Paragraph ReturnedHeaderParagraph = new Paragraph("Returned: ", SubHeaderTextFont);
                            document.Add(ReturnedHeaderParagraph);
                            Paragraph ReturnedParagraph = new Paragraph(item.returned, mainTextFont);
                            document.Add(ReturnedParagraph);
                        }
                    }

                }
                // Close the Document - this saves the document contents to the output stream
                document.Close();


                // Write the file to the Response
                Response.ContentType = "Application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=SQLDoc.pdf");
                Response.TransmitFile(Server.MapPath("SQLDoc.pdf"));
                Response.End();
            }
            catch (Exception err)
            {
                
            }
        }

        protected void drpAuthType_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel_SQL_auth.Visible = false;
            if (drpAuthType.SelectedItem.Text.ToString()== "SQL server authentification")
            {
                panel_SQL_auth.Visible = true;
            }
        }

        protected void btnConnect_Click(object sender, EventArgs e)
        {
            string ConStr = null;
            if (txtServerName.Text.ToString()=="")
            {
                lblError.Text = "Invalid server name";
                return;
            }
            if (drpAuthType.SelectedItem.Text.ToString() == "SQL server authentification")
            {
                
                ConStr = String.Format(SQLConnectionTemplate, txtServerName.Text.ToString(),"master", txtLogin.Text.ToString(), txtPassword.Text.ToString());
            }
            else
            {
                ConStr = String.Format(WinConnectionTemplate, txtServerName.Text.ToString(),"master");
            }

            List<string> db = GetDatabaseList(ConStr);
            if (db != null)
            {
                foreach (string dbitem in db)
                {
                    drpDatabase.Items.Add(dbitem);
                }
            }
        }

        private List<string> GetDatabaseList (string defaultConnectionString)
        {
            lblError.Text = "";
            SqlConnection con;
            SqlCommand comm;
            SqlDataReader dr;
            DataTable dt;
            try
            {
                con = new SqlConnection(defaultConnectionString);
                con.Open();
                comm = new SqlCommand(DBQueryStr, con);
                dr = comm.ExecuteReader();
                dt = new DataTable();
                dt.Load(dr);
            }
            catch (Exception err)
            {
                lblError.Text = "Database connection error";
                return null;
            }

            List<string> paramList = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {               
                paramList.Add(dt.Rows[i][0].ToString());
            }
            con.Close();
            return paramList;            
        }

        protected void drpDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
           lblError.Text = "";
            
            if ((drpDatabase.SelectedItem==null) || (drpDatabase.SelectedItem.Text.ToString()==""))
            {
                 lblError.Text = "No database selected";   
                //ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('No database selected');", true);
                return;
            }
            if (drpAuthType.SelectedItem.Text.ToString() == "SQL server authentification")
            {
                ConnectionString = String.Format(SQLConnectionTemplate, txtServerName.Text.ToString(), drpDatabase.SelectedItem.Text.ToString(), txtLogin.Text.ToString(), txtPassword.Text.ToString());
            }
            else
            {
                ConnectionString = String.Format(WinConnectionTemplate, txtServerName.Text.ToString(), drpDatabase.SelectedItem.Text.ToString());
            }
            SqlConnection con;
            SqlCommand comm;
            SqlDataReader dr;
            DataTable dt;
            try
            {
                con = new SqlConnection(ConnectionString);
                con.Open();
                comm = new SqlCommand(SPListGet, con);
                dr = comm.ExecuteReader();
                dt = new DataTable();

                dt.Load(dr);
                con.Close();
            }
            catch (Exception err)
            {
                lblError.Text = "Database connection error";
                return;
            }

            List<SPClass> SPList = new List<SPClass>();
            string Res = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                SPList.Add(GetSPBody(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString()));
            }

            if (SPList.Count > 0)
            {
                SPCreatePDF(SPList);
            }
            
        }
    }
}