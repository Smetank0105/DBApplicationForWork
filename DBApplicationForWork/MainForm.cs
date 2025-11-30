using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBApplicationForWork
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.Size = new Size(480, 480);
			DataGridViewWithFilter DG = new DataGridViewWithFilter();

			DG.Bounds = new Rectangle(10, 10, 445, 420);
			DG.Anchor = ((AnchorStyles)(AnchorStyles.Top | AnchorStyles.Left)| AnchorStyles.Right|AnchorStyles.Bottom);
			DG.AllowUserToAddRows = false;

			this.Controls.Add(DG);
			DataTable dt = new DataTable();
			dt.Columns.Add("Number", typeof(int));
			dt.Columns.Add("Name");
			dt.Columns.Add("Date", typeof(DateTime));
			dt.Columns.Add("Ver");
			dt.Rows.Add("1", "Ubuntu", "13.10.2011", "11.10");
			dt.Rows.Add("2", "Ubuntu LTS", "18.10.2012", "12.04");
			dt.Rows.Add("3", "Ubuntu", "18.10.2012", "12.10");
			dt.Rows.Add("4", "Ubuntu", "25.04.2012", "13.04");
			dt.Rows.Add("5", "Ubuntu", "17.10.2013", "13.10");
			dt.Rows.Add("6", "Ubuntu LTS", "23.04.2014", "14.04");
			dt.Rows.Add("7", "Ubuntu", "23.10.2014", "14.10");
			dt.Rows.Add("8", "Ubuntu", "23.04.2015", "15.04");

			DataSet ds = new DataSet();
			ds.Tables.Add(dt);

			DG.DataSource = ds.Tables[0];
		}
	}
}
