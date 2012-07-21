﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using WinFormCharpWebCam;

namespace IDCardManagement
{
    public partial class Form2 : Form
    {
        public int Y { get; set; }
        public int X { get; set; }
        private IDCard idcard;
        PictureBox pictureBox1;
        Panel panel1;
        bool mouseClicked;
        string extraTableName;


        public Form2()
        {
            InitializeComponent();
            webcamStatus = 0;
        }

        //called in "new" subroutine
        public Form2(IDCard idcard)
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.idcard = idcard;
            Form2_LoadFile();
        }

        private void Form2_Load(object o, EventArgs e)
        {
            
            toolStripStatusLabel1.Text = "Click on Open to start working...";
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                fontToolStripComboBox.Items.Add(font.Name);
            }


        }

        private void loadDataGrid(String str)
        {
            try
            {
                using (SqlCeConnection c = new SqlCeConnection(str))
                {
                    Console.WriteLine("here" + str);
                    c.Open();
                    using (SqlCeDataAdapter a = new SqlCeDataAdapter(
                        "SELECT * FROM " + idcard.tableName, c))
                    {
                        DataTable t = new DataTable();
                        a.Fill(t);
                        dataGridView1.DataSource = t;
                    }
                }
            }
            catch(Exception ex){ MessageBox.Show("Invalid file format :"+ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);}
        }

        private void Form2_LoadFile()
        {
            if (idcard.backgroundImage != null) panel1.BackgroundImage = idcard.backgroundImage;
            toolStripStatusLabel1.Text = "Select Records on the right to fill form..";//"Right Click to add Fields...";
            titleLbl.Text = idcard.title;
            titleLbl.MouseDown += tmplbl_MouseDown;
            ControlMover.Init(titleLbl);
           // if (pictureBox1 != null) //ControlMover.Init(pictureBox1);
            panel1.Visible = true;
            loadDataGrid(idcard.connectionString);
            enableItems();
            
            panel1.Size = new Size(idcard.dimensions.Width * 10, idcard.dimensions.Height * 10);
            panel1.Left = ((this.Width - panel1.Width - dataGridView1.Width) / 2) - 20;
            panel1.Top = (this.Height - panel1.Height) / 2;
            rectangleShape1.Visible = true;
            rectangleShape1.SetBounds(panel1.Left + 5, panel1.Top + 5, panel1.Width, panel1.Height);
            //contextMenuStrip1.Items.Clear();
            //foreach (String str in idcard.selectedFields)
            //{
            //    ToolStripItem tmp = contextMenuStrip1.Items.Add(str);
            //    tmp.Click += tmpToolStripItem_Click;
            //}
            titleLbl.Tag = "notext";

            foreach (Control ctl in panel1.Controls)
            {
                if (ctl is Label)//(string)ctl.Tag !="notext")
                {
                    //if ((string)ctl.Tag != "notext")
                    {
                        Label tmp = new Label();
                        tmp.Tag = ctl.Text;
                        tmp.Left = ctl.Left + ctl.Width + 10;
                        tmp.BackColor = Color.Beige;
                        tmp.Top = ctl.Top;
                        tmp.MouseDown += tmplbl_MouseDown;
                        ControlMover.Init(tmp);
                        panel1.Controls.Add(tmp);
                        tmp.AutoSize = true;
                        // MessageBox.Show("name:"+ctl.Name+" tag :"+ctl.Tag);
                    }
                    //else { MessageBox.Show("found him"); }
                }
            }

        }

        private void enableItems()
        {
            printToolStripButton.Enabled = true;
            webcamToolStripButton.Enabled = true;
            saveToolStripButton.Enabled = true;
        }

        //open
        PictureBox ptmp;
        Panel pictureContainerPanel;
        String filename;
        private void openToolStripButton_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                this.Text = filename + " - IDCard Producer";
                
                ArrayList fields = new ArrayList();
                ArrayList selectedFields = new ArrayList();
                
                Image backgroundImage = null;
                string connectionString = "", tableName = "", title = "", dataSourceType = "", primaryKey = "";
                
                Size dimensions = new Size();

                panel1.Controls.Clear();
                panel1.ContextMenuStrip = contextMenuStrip1;
                
                try
                {
                    using (XmlTextReader reader = new XmlTextReader(openFileDialog1.FileName))
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                #region
                                switch (reader.Name)
                                {
                                    case "label":
                                        Label tmp = new Label();
                                        tmp.Text = reader.GetAttribute("text");
                                        tmp.Top = Convert.ToInt32(reader.GetAttribute("top"));
                                        tmp.Left = Convert.ToInt32(reader.GetAttribute("left"));
                                        panel1.Controls.Add(tmp);
                                        tmp.MouseDown += tmplbl_MouseDown;
                                        ControlMover.Init(tmp);
                                        tmp.AutoSize = true;
                                        tmp.Font = (Font)TypeDescriptor.GetConverter(typeof(Font)).ConvertFromString(reader.GetAttribute("font"));
                                        tmp.BackColor = Color.FromArgb(Convert.ToInt32(reader.GetAttribute("backcolor")));
                                        tmp.ForeColor = Color.FromArgb(Convert.ToInt32(reader.GetAttribute("forecolor")));
                                        break;
                                    case "IDpictureBox":
                                        pictureContainerPanel = new Panel();
                                        pictureContainerPanel.Tag = "IDpictureBox";
                                        pictureContainerPanel.BackgroundImage = global::IDCardManagement.Properties.Resources.avatar;
                                        pictureContainerPanel.BackgroundImageLayout = ImageLayout.Stretch;
                                        pictureContainerPanel.Left = Convert.ToInt32(reader.GetAttribute("left"));
                                        pictureContainerPanel.Top = Convert.ToInt32(reader.GetAttribute("top"));
                                        pictureContainerPanel.Height = Convert.ToInt32(reader.GetAttribute("height"));
                                        pictureContainerPanel.Width = Convert.ToInt32(reader.GetAttribute("width"));
                                        panel1.Controls.Add(pictureContainerPanel);
                                        break;

                                    case "idCard":
                                        dimensions.Height = Convert.ToInt32(reader.GetAttribute("height"));
                                        dimensions.Width = Convert.ToInt32(reader.GetAttribute("width"));
                                        String base64String;
                                        if ((base64String = reader.GetAttribute("backgroundImage")) != null)
                                        {
                                            byte[] imageBytes = Convert.FromBase64String(base64String);
                                            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                                            // Convert byte[] to Image
                                            ms.Write(imageBytes, 0, imageBytes.Length);
                                            backgroundImage = Image.FromStream(ms, true);
                                            

                                        }
                                        title = reader.GetAttribute("title");
                                        tableName = reader.GetAttribute("tableName");
                                        connectionString = reader.GetAttribute("connectionString");
                                        dataSourceType = reader.GetAttribute("dataSourceType");
                                        primaryKey = reader.GetAttribute("primaryKey");
                                        extraTableName = reader.GetAttribute("extraTableName");
                                        break;
                                    case "field":
                                        fields.Add(reader.ReadString());
                                        break;
                                    case "selectedField":
                                        selectedFields.Add(reader.ReadString());
                                        break;

                                }

                                #endregion
                            }
                        }
                    idcard = new IDCard(connectionString, dataSourceType, tableName,primaryKey, dimensions, backgroundImage, fields, selectedFields, title);
                    Form2_LoadFile();
                }catch(Exception ex){ MessageBox.Show("Invalid file format","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);}
            }
        }

      

        private void tmpToolStripItem_Click(object sender, EventArgs e)
        {

            ToolStripItem clickedItem = (ToolStripItem)sender;
            Label tmp = new Label();
            tmp.BackColor = Color.Transparent;
            tmp.Text = clickedItem.Text;
            tmp.Left = X;
            tmp.Top = Y;
            tmp.AutoSize = true;
            tmp.MouseDown += tmplbl_MouseDown;
            ControlMover.Init(tmp);
            panel1.Controls.Add(tmp);
        }

        private void tmplbl_MouseDown(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.Control)
                foreach (Control ctl in panel1.Controls)
                {
                    if (ctl is Label) { ((Label)ctl).BorderStyle = BorderStyle.None; }
                    if (ctl is PictureBox) { ((PictureBox)ctl).BorderStyle = BorderStyle.None; }
                }
            Label tmp = sender as Label;
            tmp.BorderStyle = BorderStyle.FixedSingle;
            fontToolStripComboBox.Text = tmp.Font.FontFamily.Name;
            fontSizeToolStripComboBox.Text = ((int)tmp.Font.Size).ToString();
            toolStripButton1.BackColor = tmp.ForeColor;
            toolStripButton2.BackColor = tmp.BackColor;

        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                X = Cursor.Position.X - panel1.Left;
                Y = Cursor.Position.Y - panel1.Top - 30;
                Console.WriteLine(X + "lkkn :" + Y);
            }

        }

        private void panel1_Click(object sender, EventArgs e)
        {
            foreach (Control ctl in panel1.Controls) { if (ctl is Label) { ((Label)ctl).BorderStyle = BorderStyle.None; } }

        }

        private void Form2_Click(object sender, EventArgs e)
        {
            if (panel1 != null)
                foreach (Control ctl in panel1.Controls) { if (ctl is Label) { ((Label)ctl).BorderStyle = BorderStyle.None; } }

        }

        private void fontToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Control ctl in panel1.Controls)
            {
                if (ctl is Label) if (((Label)ctl).BorderStyle == BorderStyle.FixedSingle) ((Label)ctl).Font = new Font(fontToolStripComboBox.Text, ctl.Font.Size);
            }
        }

        private void fontSizeToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Control ctl in panel1.Controls)
            {
                if (ctl is Label)
                {
                    if (((Label)ctl).BorderStyle == BorderStyle.FixedSingle)
                        ((Label)ctl).Font = new Font(ctl.Font.FontFamily.ToString(), Convert.ToInt32(fontSizeToolStripComboBox.Text));
                }
            }

        }

        private void backcolorToolStripButton_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                foreach (Control ctl in panel1.Controls)
                {
                    if (ctl is Label)
                    {
                        if (((Label)ctl).BorderStyle == BorderStyle.FixedSingle)
                            ((Label)ctl).BackColor = colorDialog1.Color;
                    }
                }

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            foreach (Control ctl in panel1.Controls) { if (ctl is Label) { ((Label)ctl).BorderStyle = BorderStyle.None; } }

        }

        private void deleteToolStripButton_Click(object sender, EventArgs e)
        {
            foreach (Control ctl in panel1.Controls)
            {
                if (ctl is Label)
                {
                    if (((Label)ctl).BorderStyle == BorderStyle.FixedSingle)
                        panel1.Controls.Remove(ctl);

                }
            }

        }

        private void forecolorToolStripButton_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                foreach (Control ctl in panel1.Controls)
                {
                    if (ctl is Label)
                    {
                        if (((Label)ctl).BorderStyle == BorderStyle.FixedSingle)
                            ((Label)ctl).ForeColor = colorDialog1.Color;
                    }
                }

        }

        private byte[] ConvertImageToByteArray(System.Drawing.Image imageToConvert,
                                       System.Drawing.Imaging.ImageFormat formatOfImage)
        {
            byte[] Ret;
            //try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    imageToConvert.Save(ms, formatOfImage);
                    Ret = ms.ToArray();
                }
            }
            //catch (Exception ex) { Console.WriteLine(ex.Message); ; }
            return Ret;
        }

        //save
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {

            //using (SqlCeConnection con = new SqlCeConnection(idcard.connectionString))
            //{

            //    string tmp = "if not exists (select * from sysobjects where name='" + idcard.tableName + "extra' and xtype='U')";
            //    con.Open();
            //    try{
            //        using (SqlCeCommand cmd = new SqlCeCommand("  create table " + idcard.tableName + "extra ( pic1 varbinary(8000) )", con))
            //    {
            //        cmd.ExecuteNonQuery();
            //    }}
            //    catch(SqlCeException ex)
            //    {
            //        Console.WriteLine("myerror :"+ex.Message);
            //    }

            //    try
            //    {
            //        using (SqlCeCommand cmd2 = new SqlCeCommand("Insert into " + idcard.tableName + "extra (pic1) Values (@Pic)", con))
            //        {

            //            cmd2.Parameters.Add("Pic", SqlDbType.Image, 0).Value =
            //                ConvertImageToByteArray(pictureBox1.BackgroundImage, System.Drawing.Imaging.ImageFormat.Jpeg);
            //            cmd2.ExecuteNonQuery();

            //        }
            //    }
            //    catch (SqlCeException ex)
            //    {
            //        Console.WriteLine("myerror2 :" + ex.Message);
            //    }

            //    using (SqlCeCommand cmd3 = new SqlCeCommand("select * from "+idcard.tableName+"extra ",con)) {
            //        SqlCeDataReader reader= cmd3.ExecuteReader();
            //        reader.Read();
            //        byte[] imageBytes = (byte[])(reader["pic1"]);
            //        MemoryStream ms = new MemoryStream(imageBytes,0,imageBytes.Length);//, );
            //        // Convert byte[] to Image
            //        ms.Write(imageBytes, 0, imageBytes.Length);
            //        panel1.BackgroundImage  = Image.FromStream(ms, true);
            //        ms.Close();
            //        //panel1.BackgroundImage = Image.FromStream(new by);//((byte[])reader["pic1"]));
            //    }
            //} 
            #region
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //    using (XmlWriter wrt = XmlWriter.Create(saveFileDialog1.FileName))
            //    {
            //        this.Text = openFileDialog1.FileName + " - IDCard Producer";
            //        wrt.WriteStartDocument();
            //        wrt.WriteStartElement("panel");
            //        foreach (Control ctl in this.panel1.Controls)
            //        {
            //            if (ctl is Label)
            //            {
            //                wrt.WriteStartElement("label");
            //                wrt.WriteAttributeString("text", ((Label)ctl).Text);
            //                wrt.WriteAttributeString("top", ((Label)ctl).Top.ToString());
            //                wrt.WriteAttributeString("left", ((Label)ctl).Left.ToString());
            //                wrt.WriteAttributeString("backcolor", ((Label)ctl).BackColor.ToArgb().ToString()); //Color.FromArgb(int);
            //                wrt.WriteAttributeString("forecolor", ((Label)ctl).ForeColor.ToArgb().ToString());
            //                wrt.WriteAttributeString("font", TypeDescriptor.GetConverter(typeof(Font)).ConvertToString(((Label)ctl).Font)); //Font font = (Font)converter.ConvertFromString(fontString);
            //                wrt.WriteEndElement();

            //            }
            //            if (ctl is PictureBox)
            //            {
            //                wrt.WriteStartElement("pictureBox");
            //                wrt.WriteAttributeString("left", ((PictureBox)ctl).Left.ToString());
            //                wrt.WriteAttributeString("top", ((PictureBox)ctl).Top.ToString());
            //                wrt.WriteAttributeString("height", ((PictureBox)ctl).Height.ToString());
            //                wrt.WriteAttributeString("width", ((PictureBox)ctl).Width.ToString());
            //                wrt.WriteEndElement();
            //            }

            //        }

            //        wrt.WriteStartElement("idCard");
            //        wrt.WriteAttributeString("connectionString", idcard.connectionString);
            //        wrt.WriteAttributeString("height", idcard.dimensions.Height.ToString());
            //        wrt.WriteAttributeString("width", idcard.dimensions.Width.ToString());
            //        wrt.WriteAttributeString("tableName", idcard.tableName);
            //        wrt.WriteAttributeString("title", label1.Text);//idcard.title
            //        string imagebase64String;
            //        using (MemoryStream ms = new MemoryStream())
            //        {
            //            idcard.backgroundImage.Save(ms, idcard.backgroundImage.RawFormat);
            //            byte[] imageBytes = ms.ToArray();
            //            imagebase64String = Convert.ToBase64String(imageBytes);
            //        }
            //        wrt.WriteAttributeString("backgroundImage", imagebase64String);
            //        foreach (string str in idcard.fields)
            //        {

            //            wrt.WriteElementString("field", str);

            //            wrt.WriteElementString("field", str);

            //        }
            //        foreach (string str in idcard.selectedFields)
            //        {
            //            wrt.WriteElementString("selectedField", str);
            //        }
            //        wrt.WriteEndElement();//idcard
            //        wrt.WriteEndElement();//panel
            //        wrt.WriteEndDocument();
            //    }
            #endregion
        }


        //new
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            // Form1 frm = new Form1(this);
            //Hide();
            //frm.Show();
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (Control ctl in panel1.Controls)
                {
                    if (ctl is Label)
                    {
                        if (((Label)ctl).BorderStyle == BorderStyle.FixedSingle)
                            ((Label)ctl).Font = fontDialog1.Font;
                    }
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewCellCollection dcc = dataGridView1.SelectedRows[0].Cells;
            //foreach()
            foreach (DataGridViewCell dc in dcc)
                foreach (Control ctl in panel1.Controls)
                    if (ctl is Label && ctl.Tag != null) 
                        if (dc.OwningColumn.HeaderCell.Value.ToString() == (ctl as Label).Tag.ToString())
                            (ctl as Label).Text = dc.Value.ToString();
            //if (ctl.Tag != null) Console.WriteLine(ctl.Tag.ToString());
        }

        //Printing..
        Bitmap MemoryImage;
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Rectangle pagearea = e.PageBounds;
            //e.Graphics.DrawImage(MemoryImage, (pagearea.Width / 2) - (panel1.Width / 2), panel1.Location.Y);
            e.Graphics.DrawImage(MemoryImage, 0, 0);

        }

        public void GetPrintArea(Panel pnl)
        {
            MemoryImage = new Bitmap(pnl.Width, pnl.Height);
            Rectangle rect = new Rectangle(0, 0, pnl.Width, pnl.Height);
            pnl.DrawToBitmap(MemoryImage, new Rectangle(0, 0, pnl.Width, pnl.Height));
        }
        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    e.Graphics.DrawImage(MemoryImage, 0, 0);
        //    base.OnPaint(e);
        //}


        public void Print()
        {

            GetPrintArea(panel1);
            printDialog1.Document = printDocument1;
            if (printDialog1.ShowDialog() == DialogResult.OK) printDocument1.Print();
        }

        private void printToolStripButton_Click(object sender, EventArgs e)
        {
            Print();
            
            //string timestamp = DateTime.Now.ToString();
            //string prevtimestamp="null";

            using (SqlCeConnection con = new SqlCeConnection(idcard.connectionString))
            {
                try
                {
                    con.Open();
                    using (SqlCeCommand cmd = new SqlCeCommand("Insert into " + extraTableName + "  Values (@id,@Pic,@printtime,@machineid,@log,@oldprinttime)", con))
                    {

                        cmd.Parameters.Add("Pic", SqlDbType.Image, 0).Value = ConvertImageToByteArray(pictureContainerPanel.BackgroundImage, System.Drawing.Imaging.ImageFormat.Jpeg);
                        cmd.Parameters.Add("id", SqlDbType.NVarChar).Value = "";
                        cmd.Parameters.Add("printtime", SqlDbType.NVarChar).Value = DateTime.Now.ToString();
                        cmd.Parameters.Add("machineid", SqlDbType.NVarChar).Value = Environment.MachineName;
                        cmd.Parameters.Add("log", SqlDbType.NVarChar).Value = "";
                        cmd.Parameters.Add("oldprinttime", SqlDbType.NVarChar).Value = "";
                        cmd.ExecuteNonQuery();

                    }
                }
            
                catch (SqlCeException ex)
                {
                    Console.WriteLine("myerror2 :" + ex.Message);
                }
        }
                    


        }

        WebCam webcam;
        int webcamStatus;
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (pictureContainerPanel != null && webcamStatus == 0)
            {
                webcam = new WebCam();
                webcam.InitializeWebCam(ref pictureContainerPanel);
                webcam.Start();
                webcamStatus = 1;
                toolStripStatusLabel1.Text = "Click on 'Capture Image' button again to capture image";

            }
            else
            {
                webcam.Stop(); webcam.Continue(); webcamStatus = 0;

            }
        }




    }
}
