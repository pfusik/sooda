// 
// SoodaQuery - A Sooda database query tool
// 
// Copyright (C) 2003-2004 Jaroslaw Kowalski
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// Jaroslaw Kowalski (jaak@polbox.com)
// 

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Xml;
using System.Text;

using Sooda;
using Sooda.Schema;

using Microsoft.Win32;
using System.Reflection;
using ICSharpCode.TextEditor.Document;

namespace SoodaQuery
{
    /// <summary>
    /// Summary description for MainForm.
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.StatusBar statusBar1;
        private System.Windows.Forms.ToolBar toolBar1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.MenuItem menuitemFileExit;
        private System.Windows.Forms.MenuItem menuitemFileNew;
        private System.Windows.Forms.MenuItem menuitemFileOpen;
        private System.Windows.Forms.MenuItem menuitemFileSave;
        private System.Windows.Forms.MenuItem menuitemFileSaveAs;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuitemQueryRun;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TextBox messagesTextBox;
        private System.Windows.Forms.ListView resultSet;
        private System.Windows.Forms.ToolBarButton toolBarSeparator1;
        private System.Windows.Forms.TabPage tabPageRecordSet;
        private System.Windows.Forms.TabPage tabPageTSql;
        private System.Windows.Forms.TabPage tabPageXml;
        private System.Windows.Forms.TabPage tabPageMessages;
        private System.Windows.Forms.TabPage tabPageCsv;
        private System.Windows.Forms.ToolBarButton toolBarButtonRun;
        private System.Windows.Forms.ToolBarButton toolBarButtonNew;
        private System.Windows.Forms.ToolBarButton toolBarButtonOpen;
        private System.Windows.Forms.ToolBarButton toolBarButtonSave;
        private ICSharpCode.TextEditor.TextEditorControl TextEditorControl1;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ContextMenu recordsetContextMenu;
        private System.Windows.Forms.MenuItem contextMenuItemSelectAll;
        private System.Windows.Forms.MenuItem contextMenuItemCopy;
        private ICSharpCode.TextEditor.TextEditorControl translatedSql;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private ICSharpCode.TextEditor.TextEditorControl xmlResults;
        private System.Windows.Forms.Panel panel4;
        private ICSharpCode.TextEditor.TextEditorControl csvResults;
        private System.Windows.Forms.ToolBarButton toolBarButtonConnection;
        private System.Windows.Forms.ToolBarButton toolBarButton1;
        private System.Windows.Forms.ContextMenu contextMenuDatabases;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem8;
        private System.Windows.Forms.MenuItem menuItemProjectNew;
        private System.Windows.Forms.MenuItem menuItemProjectOpen;
        private System.Windows.Forms.MenuItem menuItemProjectSave;
        private System.Windows.Forms.MenuItem menuItemProjectProperties;
        private System.Windows.Forms.MenuItem menuItem9;
        private System.Windows.Forms.TabPage tabPagePrettyPrint;
        private System.Windows.Forms.Panel panel5;
        private ICSharpCode.TextEditor.TextEditorControl soqlPrettyPrint;
        private System.ComponentModel.IContainer components;

        public MainForm()
        {
            HighlightingManager.Manager.AddSyntaxModeFileProvider(new MyResourceSyntaxModeProvider());

            InitializeComponent();

            translatedSql.Document.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy("SQL");
            soqlPrettyPrint.Document.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy("SQL");
            xmlResults.Document.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy("MyXML");
            csvResults.Document.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy("CSV");
            TextEditorControl1.Document.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy("SQL");

            //HighlightingManager.Manager.HighlightingDefinitions.Clear();

            //translatedSql.Document.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy("Default");

            SetupControl(TextEditorControl1);
            SetupControl(translatedSql);
            SetupControl(soqlPrettyPrint);
            SetupControl(xmlResults);
            SetupControl(csvResults);

            // HACK - why is this needed ?
            TextEditorControl1.ActiveTextAreaControl.Caret.RecreateCaret();
            //TextEditorControl1.Text = "select PrimaryGroup.Manager.Name,count(*) from Contact where exists (Contact where id=1) and ContactId=1 and (1=1) or (1+2<2+3) and not (1=1) and (1 in (2,3,4,'5')) and {0}=1 and true and exists (select * from Contact where id=1) group by Name order by count(*) desc";
            TextEditorControl1.Text = "select PrimaryGroup.Manager.PrimaryGroup.Name,count(*) from Contact where PrimaryGroup.Manager.ContactId > 1 group by PrimaryGroup.Manager.PrimaryGroup.Name order by PrimaryGroup.Manager.PrimaryGroup.Name";
            //TextEditorControl1.Enabled = false;

            ReloadLastProject();
            UpdateTitleBar();
            resultSet.Columns.Add("(no results)", 200, HorizontalAlignment.Left);
            TextEditorControl1.Focus();
        }

        void SetupControl(ICSharpCode.TextEditor.TextEditorControl tec)
        {
            tec.ShowEOLMarkers = false;
            tec.ShowVRuler = false;
            tec.ShowSpaces = false;
            tec.ShowLineNumbers = false;
            tec.ShowInvalidLines = false;
            tec.ShowTabs = false;
            tec.ShowMatchingBracket = true;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuitemFileNew = new System.Windows.Forms.MenuItem();
            this.menuitemFileOpen = new System.Windows.Forms.MenuItem();
            this.menuitemFileSave = new System.Windows.Forms.MenuItem();
            this.menuitemFileSaveAs = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuitemFileExit = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItemProjectNew = new System.Windows.Forms.MenuItem();
            this.menuItemProjectOpen = new System.Windows.Forms.MenuItem();
            this.menuItemProjectSave = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuItemProjectProperties = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuitemQueryRun = new System.Windows.Forms.MenuItem();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.toolBar1 = new System.Windows.Forms.ToolBar();
            this.toolBarButtonNew = new System.Windows.Forms.ToolBarButton();
            this.toolBarButtonOpen = new System.Windows.Forms.ToolBarButton();
            this.toolBarButtonSave = new System.Windows.Forms.ToolBarButton();
            this.toolBarSeparator1 = new System.Windows.Forms.ToolBarButton();
            this.toolBarButtonRun = new System.Windows.Forms.ToolBarButton();
            this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
            this.toolBarButtonConnection = new System.Windows.Forms.ToolBarButton();
            this.contextMenuDatabases = new System.Windows.Forms.ContextMenu();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.recordsetContextMenu = new System.Windows.Forms.ContextMenu();
            this.contextMenuItemSelectAll = new System.Windows.Forms.MenuItem();
            this.contextMenuItemCopy = new System.Windows.Forms.MenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageRecordSet = new System.Windows.Forms.TabPage();
            this.resultSet = new System.Windows.Forms.ListView();
            this.tabPageXml = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.xmlResults = new ICSharpCode.TextEditor.TextEditorControl();
            this.tabPageCsv = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.Panel();
            this.csvResults = new ICSharpCode.TextEditor.TextEditorControl();
            this.tabPagePrettyPrint = new System.Windows.Forms.TabPage();
            this.tabPageTSql = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.translatedSql = new ICSharpCode.TextEditor.TextEditorControl();
            this.tabPageMessages = new System.Windows.Forms.TabPage();
            this.messagesTextBox = new System.Windows.Forms.TextBox();
            this.TextEditorControl1 = new ICSharpCode.TextEditor.TextEditorControl();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.soqlPrettyPrint = new ICSharpCode.TextEditor.TextEditorControl();
            this.tabControl1.SuspendLayout();
            this.tabPageRecordSet.SuspendLayout();
            this.tabPageXml.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabPageCsv.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tabPagePrettyPrint.SuspendLayout();
            this.tabPageTSql.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabPageMessages.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem1,
                                                                                      this.menuItem2,
                                                                                      this.menuItem3});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuitemFileNew,
                                                                                      this.menuitemFileOpen,
                                                                                      this.menuitemFileSave,
                                                                                      this.menuitemFileSaveAs,
                                                                                      this.menuItem7,
                                                                                      this.menuitemFileExit});
            this.menuItem1.Text = "&File";
            // 
            // menuitemFileNew
            // 
            this.menuitemFileNew.Index = 0;
            this.menuitemFileNew.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.menuitemFileNew.Text = "&New";
            // 
            // menuitemFileOpen
            // 
            this.menuitemFileOpen.Index = 1;
            this.menuitemFileOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuitemFileOpen.Text = "&Open";
            this.menuitemFileOpen.Click += new System.EventHandler(this.menuitemProjectOpen_Click);
            // 
            // menuitemFileSave
            // 
            this.menuitemFileSave.Index = 2;
            this.menuitemFileSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.menuitemFileSave.Text = "&Save";
            // 
            // menuitemFileSaveAs
            // 
            this.menuitemFileSaveAs.Index = 3;
            this.menuitemFileSaveAs.Text = "Save &as...";
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 4;
            this.menuItem7.Text = "-";
            // 
            // menuitemFileExit
            // 
            this.menuitemFileExit.Index = 5;
            this.menuitemFileExit.Shortcut = System.Windows.Forms.Shortcut.AltF4;
            this.menuitemFileExit.Text = "&Exit";
            this.menuitemFileExit.Click += new System.EventHandler(this.menuitemFileExit_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItemProjectNew,
                                                                                      this.menuItemProjectOpen,
                                                                                      this.menuItemProjectSave,
                                                                                      this.menuItem9,
                                                                                      this.menuItemProjectProperties,
                                                                                      this.menuItem4});
            this.menuItem2.Text = "&Project";
            // 
            // menuItemProjectNew
            // 
            this.menuItemProjectNew.Index = 0;
            this.menuItemProjectNew.Text = "&New project";
            // 
            // menuItemProjectOpen
            // 
            this.menuItemProjectOpen.Index = 1;
            this.menuItemProjectOpen.Text = "&Open project from file...";
            // 
            // menuItemProjectSave
            // 
            this.menuItemProjectSave.Index = 2;
            this.menuItemProjectSave.Text = "&Save project to file...";
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 3;
            this.menuItem9.Text = "-";
            // 
            // menuItemProjectProperties
            // 
            this.menuItemProjectProperties.Index = 4;
            this.menuItemProjectProperties.Text = "&Properties...";
            this.menuItemProjectProperties.Click += new System.EventHandler(this.menuItemProjectProperties_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 5;
            this.menuItem4.Text = "-";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuitemQueryRun});
            this.menuItem3.Text = "&Query";
            // 
            // menuitemQueryRun
            // 
            this.menuitemQueryRun.Index = 0;
            this.menuitemQueryRun.Shortcut = System.Windows.Forms.Shortcut.CtrlE;
            this.menuitemQueryRun.Text = "&Run";
            this.menuitemQueryRun.Click += new System.EventHandler(this.menuitemQueryRun_Click);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 403);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(640, 22);
            this.statusBar1.TabIndex = 4;
            this.statusBar1.Text = "statusBar1";
            // 
            // toolBar1
            // 
            this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
                                                                                        this.toolBarButtonNew,
                                                                                        this.toolBarButtonOpen,
                                                                                        this.toolBarButtonSave,
                                                                                        this.toolBarSeparator1,
                                                                                        this.toolBarButtonRun,
                                                                                        this.toolBarButton1,
                                                                                        this.toolBarButtonConnection});
            this.toolBar1.DropDownArrows = true;
            this.toolBar1.ImageList = this.imageList1;
            this.toolBar1.Location = new System.Drawing.Point(0, 0);
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.ShowToolTips = true;
            this.toolBar1.Size = new System.Drawing.Size(640, 28);
            this.toolBar1.TabIndex = 3;
            this.toolBar1.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
            this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
            // 
            // toolBarButtonNew
            // 
            this.toolBarButtonNew.ImageIndex = 0;
            this.toolBarButtonNew.Text = "New";
            // 
            // toolBarButtonOpen
            // 
            this.toolBarButtonOpen.ImageIndex = 1;
            this.toolBarButtonOpen.Text = "Open";
            // 
            // toolBarButtonSave
            // 
            this.toolBarButtonSave.ImageIndex = 2;
            this.toolBarButtonSave.Text = "Save";
            // 
            // toolBarSeparator1
            // 
            this.toolBarSeparator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // toolBarButtonRun
            // 
            this.toolBarButtonRun.ImageIndex = 3;
            this.toolBarButtonRun.Text = "Run";
            // 
            // toolBarButton1
            // 
            this.toolBarButton1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // toolBarButtonConnection
            // 
            this.toolBarButtonConnection.DropDownMenu = this.contextMenuDatabases;
            this.toolBarButtonConnection.ImageIndex = 4;
            this.toolBarButtonConnection.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            this.toolBarButtonConnection.Text = "No connection";
            // 
            // contextMenuDatabases
            // 
            this.contextMenuDatabases.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                                 this.menuItem5,
                                                                                                 this.menuItem8,
                                                                                                 this.menuItem6});
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 0;
            this.menuItem5.Text = "&Connect to database...";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 1;
            this.menuItem8.Text = "&Manage databases...";
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 2;
            this.menuItem6.Text = "-";
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Silver;
            // 
            // recordsetContextMenu
            // 
            this.recordsetContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                                 this.contextMenuItemSelectAll,
                                                                                                 this.contextMenuItemCopy});
            this.recordsetContextMenu.Popup += new System.EventHandler(this.recordsetContextMenu_Popup);
            // 
            // contextMenuItemSelectAll
            // 
            this.contextMenuItemSelectAll.Index = 0;
            this.contextMenuItemSelectAll.Text = "Select &All";
            this.contextMenuItemSelectAll.Click += new System.EventHandler(this.contextMenuItemSelectAll_Click);
            // 
            // contextMenuItemCopy
            // 
            this.contextMenuItemCopy.Index = 1;
            this.contextMenuItemCopy.Text = "&Copy";
            this.contextMenuItemCopy.Click += new System.EventHandler(this.contextMenuItemCopy_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageRecordSet);
            this.tabControl1.Controls.Add(this.tabPagePrettyPrint);
            this.tabControl1.Controls.Add(this.tabPageCsv);
            this.tabControl1.Controls.Add(this.tabPageXml);
            this.tabControl1.Controls.Add(this.tabPageTSql);
            this.tabControl1.Controls.Add(this.tabPageMessages);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControl1.Location = new System.Drawing.Point(0, 155);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(640, 248);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPageRecordSet
            // 
            this.tabPageRecordSet.Controls.Add(this.resultSet);
            this.tabPageRecordSet.Location = new System.Drawing.Point(4, 22);
            this.tabPageRecordSet.Name = "tabPageRecordSet";
            this.tabPageRecordSet.Size = new System.Drawing.Size(768, 222);
            this.tabPageRecordSet.TabIndex = 0;
            this.tabPageRecordSet.Text = "Recordset";
            // 
            // resultSet
            // 
            this.resultSet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.resultSet.ContextMenu = this.recordsetContextMenu;
            this.resultSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultSet.FullRowSelect = true;
            this.resultSet.GridLines = true;
            this.resultSet.Location = new System.Drawing.Point(0, 0);
            this.resultSet.Name = "resultSet";
            this.resultSet.Size = new System.Drawing.Size(768, 222);
            this.resultSet.TabIndex = 0;
            this.resultSet.View = System.Windows.Forms.View.Details;
            this.resultSet.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // tabPageXml
            // 
            this.tabPageXml.Controls.Add(this.panel3);
            this.tabPageXml.Location = new System.Drawing.Point(4, 22);
            this.tabPageXml.Name = "tabPageXml";
            this.tabPageXml.Size = new System.Drawing.Size(632, 222);
            this.tabPageXml.TabIndex = 2;
            this.tabPageXml.Text = "XML";
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.xmlResults);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(632, 222);
            this.panel3.TabIndex = 0;
            // 
            // xmlResults
            // 
            this.xmlResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xmlResults.DockPadding.Top = 2;
            this.xmlResults.EnableFolding = false;
            this.xmlResults.Encoding = ((System.Text.Encoding)(resources.GetObject("xmlResults.Encoding")));
            this.xmlResults.IndentStyle = ICSharpCode.TextEditor.Document.IndentStyle.Auto;
            this.xmlResults.Location = new System.Drawing.Point(0, 0);
            this.xmlResults.Name = "xmlResults";
            this.xmlResults.ShowEOLMarkers = true;
            this.xmlResults.ShowInvalidLines = false;
            this.xmlResults.ShowLineNumbers = false;
            this.xmlResults.ShowSpaces = true;
            this.xmlResults.ShowTabs = true;
            this.xmlResults.ShowVRuler = true;
            this.xmlResults.Size = new System.Drawing.Size(630, 220);
            this.xmlResults.TabIndent = 8;
            this.xmlResults.TabIndex = 0;
            this.xmlResults.Load += new System.EventHandler(this.xmlResults_Load);
            // 
            // tabPageCsv
            // 
            this.tabPageCsv.Controls.Add(this.panel4);
            this.tabPageCsv.Location = new System.Drawing.Point(4, 22);
            this.tabPageCsv.Name = "tabPageCsv";
            this.tabPageCsv.Size = new System.Drawing.Size(632, 222);
            this.tabPageCsv.TabIndex = 4;
            this.tabPageCsv.Text = "CSV";
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.csvResults);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(632, 222);
            this.panel4.TabIndex = 0;
            // 
            // csvResults
            // 
            this.csvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.csvResults.DockPadding.Top = 2;
            this.csvResults.EnableFolding = false;
            this.csvResults.Encoding = ((System.Text.Encoding)(resources.GetObject("csvResults.Encoding")));
            this.csvResults.IndentStyle = ICSharpCode.TextEditor.Document.IndentStyle.Auto;
            this.csvResults.Location = new System.Drawing.Point(0, 0);
            this.csvResults.Name = "csvResults";
            this.csvResults.ShowEOLMarkers = true;
            this.csvResults.ShowInvalidLines = false;
            this.csvResults.ShowLineNumbers = false;
            this.csvResults.ShowSpaces = true;
            this.csvResults.ShowTabs = true;
            this.csvResults.ShowVRuler = true;
            this.csvResults.Size = new System.Drawing.Size(630, 220);
            this.csvResults.TabIndent = 8;
            this.csvResults.TabIndex = 0;
            // 
            // tabPagePrettyPrint
            // 
            this.tabPagePrettyPrint.Controls.Add(this.panel5);
            this.tabPagePrettyPrint.Location = new System.Drawing.Point(4, 22);
            this.tabPagePrettyPrint.Name = "tabPagePrettyPrint";
            this.tabPagePrettyPrint.Size = new System.Drawing.Size(632, 222);
            this.tabPagePrettyPrint.TabIndex = 5;
            this.tabPagePrettyPrint.Text = "Parsed SOQL";
            this.tabPagePrettyPrint.Click += new System.EventHandler(this.tabPagePrettyPrint_Click);
            // 
            // tabPageTSql
            // 
            this.tabPageTSql.Controls.Add(this.panel2);
            this.tabPageTSql.Location = new System.Drawing.Point(4, 22);
            this.tabPageTSql.Name = "tabPageTSql";
            this.tabPageTSql.Size = new System.Drawing.Size(632, 222);
            this.tabPageTSql.TabIndex = 1;
            this.tabPageTSql.Text = "Translated SQL";
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.translatedSql);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(632, 222);
            this.panel2.TabIndex = 2;
            // 
            // translatedSql
            // 
            this.translatedSql.BackColor = System.Drawing.SystemColors.Control;
            this.translatedSql.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translatedSql.DockPadding.Top = 2;
            this.translatedSql.EnableFolding = false;
            this.translatedSql.Encoding = ((System.Text.Encoding)(resources.GetObject("translatedSql.Encoding")));
            this.translatedSql.IndentStyle = ICSharpCode.TextEditor.Document.IndentStyle.Auto;
            this.translatedSql.Location = new System.Drawing.Point(0, 0);
            this.translatedSql.Name = "translatedSql";
            this.translatedSql.ShowEOLMarkers = true;
            this.translatedSql.ShowInvalidLines = false;
            this.translatedSql.ShowLineNumbers = false;
            this.translatedSql.ShowSpaces = true;
            this.translatedSql.ShowTabs = true;
            this.translatedSql.ShowVRuler = true;
            this.translatedSql.Size = new System.Drawing.Size(630, 220);
            this.translatedSql.TabIndent = 8;
            this.translatedSql.TabIndex = 1;
            // 
            // tabPageMessages
            // 
            this.tabPageMessages.Controls.Add(this.messagesTextBox);
            this.tabPageMessages.Location = new System.Drawing.Point(4, 22);
            this.tabPageMessages.Name = "tabPageMessages";
            this.tabPageMessages.Size = new System.Drawing.Size(768, 222);
            this.tabPageMessages.TabIndex = 3;
            this.tabPageMessages.Text = "Messages";
            // 
            // messagesTextBox
            // 
            this.messagesTextBox.AcceptsReturn = true;
            this.messagesTextBox.AcceptsTab = true;
            this.messagesTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.messagesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messagesTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.messagesTextBox.Location = new System.Drawing.Point(0, 0);
            this.messagesTextBox.Multiline = true;
            this.messagesTextBox.Name = "messagesTextBox";
            this.messagesTextBox.ReadOnly = true;
            this.messagesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.messagesTextBox.Size = new System.Drawing.Size(768, 222);
            this.messagesTextBox.TabIndex = 0;
            this.messagesTextBox.Text = "";
            // 
            // TextEditorControl1
            // 
            this.TextEditorControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextEditorControl1.DockPadding.Top = 2;
            this.TextEditorControl1.EnableFolding = false;
            this.TextEditorControl1.Encoding = ((System.Text.Encoding)(resources.GetObject("TextEditorControl1.Encoding")));
            this.TextEditorControl1.IndentStyle = ICSharpCode.TextEditor.Document.IndentStyle.Auto;
            this.TextEditorControl1.Location = new System.Drawing.Point(0, 0);
            this.TextEditorControl1.Name = "TextEditorControl1";
            this.TextEditorControl1.ShowEOLMarkers = true;
            this.TextEditorControl1.ShowInvalidLines = false;
            this.TextEditorControl1.ShowLineNumbers = false;
            this.TextEditorControl1.ShowSpaces = true;
            this.TextEditorControl1.ShowTabs = true;
            this.TextEditorControl1.ShowVRuler = true;
            this.TextEditorControl1.Size = new System.Drawing.Size(638, 122);
            this.TextEditorControl1.TabIndent = 1;
            this.TextEditorControl1.TabIndex = 0;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(0, 152);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(640, 3);
            this.splitter2.TabIndex = 0;
            this.splitter2.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.TextEditorControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(640, 124);
            this.panel1.TabIndex = 1;
            this.panel1.TabStop = true;
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.soqlPrettyPrint);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(632, 222);
            this.panel5.TabIndex = 0;
            // 
            // soqlPrettyPrint
            // 
            this.soqlPrettyPrint.BackColor = System.Drawing.SystemColors.Control;
            this.soqlPrettyPrint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.soqlPrettyPrint.DockPadding.Top = 2;
            this.soqlPrettyPrint.EnableFolding = false;
            this.soqlPrettyPrint.Encoding = ((System.Text.Encoding)(resources.GetObject("soqlPrettyPrint.Encoding")));
            this.soqlPrettyPrint.IndentStyle = ICSharpCode.TextEditor.Document.IndentStyle.Auto;
            this.soqlPrettyPrint.Location = new System.Drawing.Point(0, 0);
            this.soqlPrettyPrint.Name = "soqlPrettyPrint";
            this.soqlPrettyPrint.ShowEOLMarkers = true;
            this.soqlPrettyPrint.ShowInvalidLines = false;
            this.soqlPrettyPrint.ShowLineNumbers = false;
            this.soqlPrettyPrint.ShowSpaces = true;
            this.soqlPrettyPrint.ShowTabs = true;
            this.soqlPrettyPrint.ShowVRuler = true;
            this.soqlPrettyPrint.Size = new System.Drawing.Size(630, 220);
            this.soqlPrettyPrint.TabIndent = 8;
            this.soqlPrettyPrint.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(640, 425);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.toolBar1);
            this.Controls.Add(this.statusBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainFor_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageRecordSet.ResumeLayout(false);
            this.tabPageXml.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.tabPageCsv.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.tabPagePrettyPrint.ResumeLayout(false);
            this.tabPageTSql.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tabPageMessages.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() 
        {
            Application.Run(new MainForm());
        }

        private void menuitemFileExit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void menuitemProjectOpen_Click(object sender, System.EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {

                fd.CheckFileExists = true;
                fd.Filter = "Database XML Schema (*.xml)|*.xml|StubGen-Generated Assembly (*.dll)|*.dll|All files (*.*)|*.*";

                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    LoadProject(fd.FileName);
                }
            }
        }

        RegistryKey PreferencesRegistryKey
        {
            get
            {
                return Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Sooda\QueryAnalyzer");
            }
        }

        #region Project Support

        private string currentProject = null;
        private SchemaInfo schemaInfo = null;

        private void LoadSchemaFromXml(string fileName)
        {
            schemaInfo = SchemaManager.ReadAndValidateSchema(new XmlTextReader(fileName));
        }

        private void LoadSchemaFromAssembly(string fileName)
        {
            throw new NotImplementedException();
        }

        private void LoadProject(string fileName)
        {
            try
            {
                if (fileName.EndsWith(".xml"))
                {
                    LoadSchemaFromXml(fileName);
                }
                else if (fileName.EndsWith(".dll"))
                {
                    LoadSchemaFromAssembly(fileName);
                }
                else
                {
                    throw new NotSupportedException("Loading from this file type is not supported");
                }

                SoodaConfig.SetConfigProvider(Sooda.Config.XmlConfigProvider.FindConfigFile(Path.Combine(Path.GetDirectoryName(fileName), "sooda.config.xml")));

                currentProject = fileName;

                using (RegistryKey key = PreferencesRegistryKey)
                {
                    key.SetValue("LastWorkspace", fileName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "Unable to open project:\n\n" + fileName + "\n\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            UpdateTitleBar();
        }

        private void ReloadLastProject()
        {
            using (RegistryKey key = PreferencesRegistryKey)
            {
                string s = (string)key.GetValue("LastWorkspace", "");
                if (s != "")
                {
                    LoadProject(s);
                }
            }
        }

        #endregion

        private void UpdateTitleBar()
        {
            this.Text = "Sooda QueryAnalyzer - " + currentProject;
        }

        private void menuitemQueryRun_Click(object sender, System.EventArgs e)
        {
            RunQuery();
        }

        private void AppendCsvValue(StringBuilder sb, object v)
        {
            if (v is DBNull)
                return;

            sb.Append('"');
            sb.Append(v.ToString());
            sb.Append('"');
        }

        public void RunQuery()
        {
            try
            {
                string inputText = null; 
                if (inputText == null || inputText.Length == 0)
                    inputText = TextEditorControl1.Text;

                //translator.StripComments = true;

                DateTime translationStart = DateTime.Now;
                StringWriter sw = new StringWriter();
                StringWriter sw2 = new StringWriter();
                Sooda.Sql.SoqlToSqlConverter converter = new Sooda.Sql.SoqlToSqlConverter(sw, schemaInfo, new Sooda.Sql.SqlServerBuilder());
                Sooda.QL.SoqlPrettyPrinter prettyPrinter = new Sooda.QL.SoqlPrettyPrinter(sw2);

                Sooda.QL.SoqlQueryExpression query = Sooda.QL.SoqlParser.ParseQuery(inputText);
                converter.ConvertQuery(query);
                prettyPrinter.PrintQuery(query);
                string sqlquery = sw.ToString();
                string prettysoql = sw2.ToString();
                DateTime translationEnd = DateTime.Now;

                translatedSql.Text = sqlquery; //.Replace("\n\n", "\n").Replace("\n\n", "\n").Replace("\n\n", "\n").Replace("\n", "\r\n");
                translatedSql.Document.ReadOnly = true;
                translatedSql.Refresh();

                soqlPrettyPrint.Text = prettysoql; //.Replace("\n\n", "\n").Replace("\n\n", "\n").Replace("\n\n", "\n").Replace("\n", "\r\n");
                soqlPrettyPrint.Document.ReadOnly = true;
                soqlPrettyPrint.Refresh();

                toolBarButtonRun.Enabled = false;
                toolBar1.Update();

                DateTime t0 = DateTime.Now;

                // HACK
                DataSourceInfo dsi = schemaInfo.GetDataSourceInfo("default");
                Sooda.Sql.SqlDataSource sds = (Sooda.Sql.SqlDataSource)dsi.CreateDataSource();

                sds.Open();

                try
                {

                    IDbConnection conn = sds.Connection;
                    IDbCommand cmd = conn.CreateCommand();
                    if (!sds.DisableTransactions)
                        cmd.Transaction = sds.Transaction;
                    cmd.CommandText = sqlquery;

                    resultSet.Items.Clear();
                    resultSet.Columns.Clear();

                    IDataReader reader = cmd.ExecuteReader();
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {
                        string name = reader.GetName(i);

                        ColumnHeader ch = new ColumnHeader();
                        ch.Text = name;
                        ch.TextAlign = HorizontalAlignment.Left;
                        ch.Width = 100;
                        resultSet.Columns.Add(ch);
                    }

                    resultSet.HeaderStyle = ColumnHeaderStyle.Nonclickable;

                    StringBuilder csvText = new StringBuilder();
                    System.IO.StringWriter xmlStringWriter = new System.IO.StringWriter();
                    XmlTextWriter xmlWriter = new XmlTextWriter(xmlStringWriter);
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlWriter.WriteStartDocument(true);
                    xmlWriter.WriteStartElement("results");
                    while (reader.Read())
                    {
                        xmlWriter.WriteStartElement("item");
                        object v = reader.GetValue(0);
                        string text = (v is DBNull) ? "(null)" : v.ToString();
                        ListViewItem item = resultSet.Items.Add(text);

                        AppendCsvValue(csvText, v);
                        //if (!(v is DBNull))
                        xmlWriter.WriteAttributeString(reader.GetName(0), text);

                        for (int i = 1; i < reader.FieldCount; ++i)
                        {
                            csvText.Append(";");
                            v = reader.GetValue(i);
                            text = (v is DBNull) ? "(null)" : v.ToString();
                            item.SubItems.Add(text);
                            //if (!(v is DBNull))
                            xmlWriter.WriteAttributeString(reader.GetName(i), text);
                            AppendCsvValue(csvText, v);
                        }
                        csvText.Append("\r\n");
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                    csvResults.Text = csvText.ToString();
                    xmlResults.Text = xmlStringWriter.ToString();

                    foreach (ColumnHeader ch in resultSet.Columns)
                    {
                        ch.Width = -2;
                    }
                }
                finally
                {
                    sds.Close();
                }

                DateTime t1 = DateTime.Now;
                messagesTextBox.Text = String.Format("Got {0} row(s)\r\nQuery took {1}\r\nTranslation took {2}", resultSet.Items.Count, t1 - t0, translationEnd - translationStart);
            }
            catch (Exception e)
            {
                messagesTextBox.Text = e.ToString();
                MessageBox.Show(this, e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                toolBarButtonRun.Enabled = true;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
        
        }

        private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            if (e.Button == toolBarButtonRun)
            {
                RunQuery();
            }
            else if (e.Button == toolBarButtonNew)
            {
                TextEditorControl1.Text = "";
                TextEditorControl1.Refresh();
            }
            else if (e.Button == toolBarButtonConnection)
            {
                toolBarButtonConnection.Text = "Database: " + DateTime.Now.Ticks;
            }
        }

        private void MainFor_Load(object sender, System.EventArgs e)
        {
            statusBar1.Text = "Ready";
            TextEditorControl1.Focus();
            TextEditorControl1.Invalidate();
        }

        private void recordsetContextMenu_Popup(object sender, System.EventArgs e)
        {
        
        }

        private void contextMenuItemSelectAll_Click(object sender, System.EventArgs e)
        {
            foreach (ListViewItem lvi in resultSet.Items)
            {
                lvi.Selected = true;
            }
        }

        private void contextMenuItemCopy_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            foreach (ListViewItem lvi in resultSet.Items)
            {
                if (lvi.Selected)
                {
                    sb.Append(lvi.Text);

                    foreach (ListViewItem.ListViewSubItem lvsi in lvi.SubItems)
                    {
                        sb.Append("\t");
                        sb.Append(lvsi.Text);
                    }
                    sb.Append("\n");
                }
            }

            Clipboard.SetDataObject(sb.ToString(), true);
        }

        private void xmlResults_Load(object sender, System.EventArgs e)
        {
        
        }

        private void menuItemProjectProperties_Click(object sender, System.EventArgs e) 
        {
            using (ProjectDialog dlg = new ProjectDialog())
            {
                dlg.ShowDialog(this);
            }
        }

        private void tabPagePrettyPrint_Click(object sender, System.EventArgs e)
        {
        
        }

        class MyResourceSyntaxModeProvider : ISyntaxModeFileProvider
        {
            ArrayList syntaxModes = null;
        
            public ArrayList SyntaxModes 
            {
                get 
                {
                    return syntaxModes;
                }
            }
        
            public MyResourceSyntaxModeProvider()
            {
                Assembly assembly = typeof(MainForm).Assembly;
                Stream syntaxModeStream = assembly.GetManifestResourceStream("SoodaQuery.SyntaxModes.xml");
                if (syntaxModeStream == null) throw new ApplicationException();
                syntaxModes = SyntaxMode.GetSyntaxModes(syntaxModeStream);
            }
        
            public XmlTextReader GetSyntaxModeFile(ICSharpCode.TextEditor.Document.SyntaxMode syntaxMode)
            {
                Assembly assembly = typeof(MainForm).Assembly;
                return new XmlTextReader(assembly.GetManifestResourceStream("SoodaQuery." + syntaxMode.FileName));
            }
        }
    }
}
