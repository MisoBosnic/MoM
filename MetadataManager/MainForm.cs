using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;
using System.Collections;

// MoM - Metadata Manager (Prototype)
// May, 2018 - Iteration 1
// ==============================================================
// Note: Not production code!!!
// ==============================================================
// Iteration 1 - Test main UI with connection to the SQL server. 
// May 2018      Display objects in the TreeView control
//               Display metadata for tables and columns
//
// Iteration 2 - Display metadata for relationships and indexes
// Jun 2018      Display metadata definition page
//               Enable adding a new metadata for a specific object (table, column,...)
//
// Iteration 3 - "New table" action (standard and lookup tables)
// July 2018     "New column" action (standard and lookup tables)
//               Publish metadata
//


namespace SMO1
{
    public partial class MainForm : Form
    {
        //-------------------------
        internal DataSet _TableMD;
        internal DataView _TablesView;
        internal DataSet _ColumnMD;
        internal DataView _ColumnsView;
        //internal DataSet _RelationshipMD;
        //----
        internal String _SelectedTableIdTable;
        internal String _SelectedTableTableName;
        //----
        internal String _SelectedColumnIdColumn;
        internal String _SelectedColumnColumnName;
        internal DataRowView[] _SelectedTableColumns;

        internal ColumnMetadata _myColumnMD;
        internal TableMetadata _myTableMD;
        internal AllTreeView _atv;
        //-------------------------------------------
        private Server _srv;

        public MainForm()
        {
            InitializeComponent();
            //
            _ColumnMD = new DataSet();
            _TableMD = new DataSet();
            _atv = new AllTreeView();
            _myTableMD = new TableMetadata();
            _myColumnMD = new ColumnMetadata();
        }

        private void MainForm2_Load(object sender, EventArgs e)
        {
            //Connect to SQL 2012
            //_srv = default(Server);
            //_srv = new Server();

            //Connect to SQL2017
            _srv = new Server(@"miso-xps\mssqlserver01");

            // Load the images in an ImageList.?????????????????????????
            //
            ImageList myImageList = new ImageList();
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\server.ico"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\database-black.png"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\table.png"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\column.ico"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\index.ico"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\depend.ico"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\blank.ico"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\table2.jpg"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\folder01.ico"));
            myImageList.Images.Add(Image.FromFile(@"C:\Users\Predrag\Documents\SQL-MD\SMO2\SMO2\SMO1\Icons\folder.png"));

            // Assign the ImageList to the TreeView.
            treeViewSQL.ImageList = myImageList;

            // Add a main root tree node.              
            treeViewSQL.Nodes.Add(_srv.Name, _srv.Name, 0);

            //
            foreach (Database xx in _srv.Databases)
            {
                //Skip SQL system databases  
                //
                string ime = xx.Name;
                if (ime == "master" || ime == "model" || ime == "msdb" || ime == "tempdb")
                {
                    // do nothing for now!!!
                }
                else
                {
                    // add nodes to the treeViewSQL
                    //
                    this.treeViewSQL.Nodes[0].Nodes.Add(xx.Name, xx.Name, 1, 1);

                    this.treeViewSQL.Nodes[0].Nodes[xx.Name].Nodes.Add("Tables", "Tables", 9, 9);

                    this.treeViewSQL.Nodes[0].Nodes[xx.Name].Nodes["Tables"].Nodes.Add("Standard", "Standard", 9, 9);

                    this.treeViewSQL.Nodes[0].Nodes[xx.Name].Nodes["Tables"].Nodes.Add("Lookup", "Lookup", 9, 9);
                    this.treeViewSQL.Nodes[0].Nodes[xx.Name].Nodes["Tables"].Nodes.Add("Special", "Special", 9, 9);

                    this.treeViewSQL.Nodes[0].Nodes[xx.Name].Nodes.Add("Models", "Models", 9, 9);
                    this.treeViewSQL.Nodes[0].Nodes[xx.Name].Nodes["Models"].Nodes.Add("Flat", "Flat", 9, 9);
                    this.treeViewSQL.Nodes[0].Nodes[xx.Name].Nodes["Models"].Nodes.Add("Hierarchy", "Hierarchy", 9, 9);
                }
            }
            
            //
            SqlConnection con = new SqlConnection(@"Data Source=MISO-XPS\MSSQLSERVER01;Initial Catalog=TestMD;Integrated Security=True");
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            // 
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT  * from TableMD where TableType = 'data'";           
            try
            {
                // Get ColumnMD for all records to dataset _ColumnMD
                string cmdText1 = "SELECT  * from FieldMD order by idTableMD, FieldPosition";
                SqlDataAdapter adapter1 = new SqlDataAdapter(cmdText1, con);
                adapter1.Fill(_ColumnMD);

                // all columns to view
                _ColumnsView = new DataView(_ColumnMD.Tables[0], "", "idTableMD", DataViewRowState.CurrentRows);
 
                // Get TableMD all records to dataset _TableMD
                string cmdText = "SELECT  * from TableMD where TableType = 'data' order by TableName";
                SqlDataAdapter adapter = new SqlDataAdapter(cmdText, con);             
                adapter.Fill(_TableMD);

                // All tables to view
                _TablesView = new DataView(_TableMD.Tables[0], "", "TableName", DataViewRowState.CurrentRows);

                foreach (DataTable table in _TableMD.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        this.treeViewSQL.Nodes[0].Nodes["TestMD"].Nodes["Tables"].Nodes["Standard"].Nodes.Add(row.ItemArray[0].ToString(), row.ItemArray[1].ToString(), 2, 2);                      
                    }
                }
                this.treeViewSQL.ExpandAll();
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"MoM Manager");
            }
    }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            //
        }

        private void treeViewSQL_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode mySelectedNode = e.Node;
            TreeNode mySelectedNodeParent = e.Node.Parent;
            string myNodeName = mySelectedNode.Text;

            // assign main variables 
            _SelectedTableIdTable = e.Node.Name;
            _SelectedTableTableName = e.Node.Text;
            _SelectedColumnColumnName = "id" + e.Node.Text;
            //
            _atv.ShowMDforSelectedTable(this);

        }


        private void prGridTableMD_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            string sel = e.NewSelection.Label.ToString();
            if (sel == "TableDescription")
            {
                labelEditTableDescription.Visible = true;
                textBoxEditTableDescription.Visible = true;
                textBoxEditTableDescription.Text = e.NewSelection.Value.ToString();
            }
            else
            {
                labelEditTableDescription.Visible = false;
                textBoxEditTableDescription.Visible = false;
            }
        }

       

        private void textBoxEditTableDescription_TextChanged(object sender, EventArgs e)
        {
            // update prGridTableMD and read-only controls on the screen
            // 
            this._myTableMD.TableDescription = textBoxEditTableDescription.Text.ToString();
            textDescription.Text = this._myTableMD.TableDescription;
        }

        private void listBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItems.Count < 1)
            {
                return;
            }
            string mySelected = listBox2.SelectedItems[0].ToString();
            _atv.RefreshColumnMD(this, mySelected);

            // refresh columns property grid
            //
            prGridColumnsMD.SelectedObject = _myColumnMD;
            labelSelectedColumnName.Text = '(' + _myColumnMD.ColumnName.ToString() + ')';
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.splitContainer1.Visible = true;
        }

        private void buttonFind_Click(object sender, EventArgs e)
        {
           var nodes = treeViewSQL.FlattenTree()
                     .Where(n => (n.Text.ToLower()) == textBoxFind.Text.ToString().ToLower())
                     .ToList();
            if (nodes.Count() < 1)
            {
                MessageBox.Show("Node not found","MoM Manager");               
            }
            else
            {
                treeViewSQL.SelectedNode = nodes[0];
            }
        }

        private void textBoxFind_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.splitContainer1.Visible = false;
        }
    }
}

// At present is hardcoded but must be dynamic
public class TableMetadata
{
    [Description("The table unique ID"), Category("Application")]
    public string id { get; set; }
    //Attribute attr = new DescriptionAttribute("some string");
    

    [Description("The table name in database"), Category("SQL")]
    public string TableName { get; set; }

    [Description("The table caption on UI"), Category("Appearance")]
    public string TableCaption { get; set; }

    [Description("The table description"), Category("Application")]
    public string TableDescription { get; set; }

    [Description("Is the table visible on UI"), Category("Appearance")]
    public Boolean IsVisible { get; set; }

    [Description("Set to true if you allow users to change the table"), Category("Appearance")]
    public bool IsCustomizable { get; set; }

    [Description("The date the table has been created"), Category("Application")]
    public DateTime CreatedDate { get; set; }

    [Description("Set to true if table is auditable"), Category("Application")]
    public bool IsAuditable { get; set; }

    [Description("Set to true if the table is importable"), Category("Appearance")]
    public bool IsImportable { get; set; }

    [Description("Set to true if the table is user entity"), Category("Application")]
    public bool IsUserEntity { get; set; }

    [Description("Set to true if the table is localazible"), Category("Appearance")]
    public bool IsLocalizable { get; set; }

    [Description("Set to true if the table is user localizable"), Category("Application")]
    public bool IsUserLocalizable { get; set; }

    [Description("Set to true if the table is available for mobile"), Category("Application")]
    public bool IsAvailableForMobile { get; set; }

    [Description("The extended table name"), Category("Application")]
    public string ExtendedTableName { get; set; }
}

// At present is hardcoded but must be dynamic
public class ColumnMetadata
{
    [Description("Column unique ID"), Category("SQL")]
    public string id { get; set; }

    [Description("The name of the column"), Category("SQL")]
    public string ColumnName { get; set; }

    [Description("The SQL type of the column"), Category("SQL")]
    public string ColumnType { get; set; }

    [Description("The position of the column"), Category("SQL")]
    public string ColumnPosition { get; set; }

    [Description("The size of the column"), Category("SQL")]
    public int ColumnSize { get; set; }

    [Description("The column decimal points"), Category("SQL")]
    public int ColumnDP { get; set; }

    [Description("Is the column nulable"), Category("SQL")]
    public bool IsNulable { get; set; }

    [Description("The column default value"), Category("SQL")]
    public string DefaultValue { get; set; }

    [Description("Is the column computed"), Category("SQL")]
    public bool IsComputed { get; set; }

    [Description("The computation function"), Category("SQL")]
    public string ComputationFunction { get; set; }

    [Description("The caption of the column"), Category("Appearance")]
    public string ColumnCaption { get; set; }

    [Description("The description of the column"), Category("Appearance")]
    public string ColumnDescription { get; set; }

    [Description("Is the column visible on UI"), Category("Appearance")]
    public Boolean IsVisible { get; set; }

    [Description("Left range (validation Left/Right range)"), Category("Validation")]
    public string LeftRange { get; set; }

    [Description("Right range (validation Left/Right range)"), Category("Validation")]
    public string RightRange { get; set; }

    [Description("Maximum data lenght in characters"), Category("Validation")]
    public int MaxLength { get; set; }

    [Description("The validation expression"), Category("Validation")]
    public string ValidationExpression { get; set; }

    [Description("Test date)"), Category("Appearance")]
    public DateTime MyDate { get; set; }

    [Description("The display format"), Category("Validation")]
    public string DisplayFormat { get; set; }

    [Description("Is the column read only"), Category("Misc")]
    public bool IsReadOnly { get; set; }

    [Description("Is the column searchable"), Category("Misc")]
    public bool IsSearchable { get; set; }

    [Description("Is the column a user defined column"), Category("Misc")]
    public bool IsUserColumn { get; set; }

    [Description("Allow full text search"), Category("Misc")]
    public bool AllowFullTextSearch { get; set; }


    [Description("Is the column importable"), Category("Validation")]
    public string IsImportable { get; set; }

    [Description("Allow mass update"), Category("Validation")]
    public bool AllowMassUpdate { get; set; }
}


public class AllTreeView  
{
    internal void ShowMDforSelectedTable(SMO1.MainForm myForm)
    {
        int rowIndex = myForm._TablesView.Find(myForm._SelectedTableTableName);
        if (rowIndex == -1)
            Console.WriteLine("No match found.");
        else
        {
            myForm._myTableMD.id = myForm._TablesView[rowIndex]["idTableMD"].ToString();
            myForm._myTableMD.TableName = myForm._TablesView[rowIndex]["TableName"].ToString();
            myForm._myTableMD.TableCaption = myForm._TablesView[rowIndex]["Caption"].ToString();
            myForm._myTableMD.TableDescription = myForm._TablesView[rowIndex]["TableDesc"].ToString();
            myForm._myTableMD.IsVisible = GetBoolean(myForm._TablesView[rowIndex]["IsVisible"].ToString());
 
            // Update fields on the form
            myForm.labelPanelTitle.Text = myForm._myTableMD.TableName.ToString();
            myForm.textName.Text = myForm._myTableMD.TableName.ToString();
            myForm.textCaption.Text = myForm._myTableMD.TableCaption.ToString();
            myForm.textType.Text = "data";
            myForm.textDescription.Text = myForm._myTableMD.TableDescription.ToString();

            myForm.prGridTableMD.SelectedObject = myForm._myTableMD;
            // Refresh columns
            ShowColumnsMDforSelectedTable(myForm);
        }
    }

 
    internal void ShowColumnsMDforSelectedTable(SMO1.MainForm myForm)
    {
        DataRowView[] columnsRows = myForm._ColumnsView.FindRows(new object[] { myForm._SelectedTableIdTable });
        myForm._SelectedTableColumns = columnsRows;

        if (columnsRows.Length == 0)
            Console.WriteLine("No match found.");
        else
        {
            myForm.listBox2.Items.Clear();
            foreach (DataRowView myDRV in columnsRows)
            {
                // populate the column list box
                myForm.listBox2.Items.Add(myDRV["FieldName"].ToString());
            }
            // refresh columns property grid
            myForm._atv.RefreshColumnMD(myForm, myForm._SelectedColumnColumnName);
            myForm.prGridColumnsMD.SelectedObject = myForm._myColumnMD;

            // refresh labels on the form
            myForm.labelSelectedColumnName.Text = '('+ myForm._myColumnMD.ColumnName + ')';
            myForm.labelNoOfColumns.Text = '(' + columnsRows.Length.ToString() + ')';
        }
    }

    // 
    internal void RefreshColumnMD(SMO1.MainForm myForm, string mySelected)
    {
        foreach (DataRowView myDRV in myForm._SelectedTableColumns)
        {
            if (myDRV.Row.ItemArray[2].ToString() == mySelected)
            {
                myForm._myColumnMD.id = myDRV.Row.ItemArray[0].ToString();
                myForm._myColumnMD.ColumnName = myDRV.Row.ItemArray[2].ToString();
                myForm._myColumnMD.ColumnCaption = myDRV.Row.ItemArray[4].ToString();
                myForm._myColumnMD.ColumnDescription = myDRV.Row.ItemArray[3].ToString();
                myForm._myColumnMD.IsVisible = myForm._atv.GetBoolean(myDRV.Row.ItemArray[2].ToString());
                myForm._myColumnMD.ColumnType = myDRV.Row.ItemArray[6].ToString();
                myForm._myColumnMD.ColumnPosition = myDRV.Row.ItemArray[5].ToString();
                return;               
            }
        }
    }

    internal Boolean GetBoolean(String stringValue)
    {
        if (stringValue == "false")
        {
            return false;
        }
        else
        {
            return true;
        }
    }


}

public static class SOExtension
{
    public static IEnumerable<TreeNode> FlattenTree(this TreeView tv)
    {
        return FlattenTree(tv.Nodes);
    }

    public static IEnumerable<TreeNode> FlattenTree(this TreeNodeCollection coll)
    {
        return coll.Cast<TreeNode>()
                    .Concat(coll.Cast<TreeNode>()
                                .SelectMany(x => FlattenTree(x.Nodes)));
    }
}

