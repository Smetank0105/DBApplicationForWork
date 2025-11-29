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
			dt.Columns.Add("Ver");
			dt.Columns.Add("Date", typeof(DateTime));
			dt.Rows.Add("1", "Ubuntu", "11.10", "13.10.2011");
			dt.Rows.Add("2", "Ubuntu LTS", "12.04", "18.10.2012");
			dt.Rows.Add("3", "Ubuntu", "12.10", "18.10.2012");
			dt.Rows.Add("4", "Ubuntu", "13.04", "25.04.2012");
			dt.Rows.Add("5", "Ubuntu", "13.10", "17.10.2013");
			dt.Rows.Add("6", "Ubuntu LTS", "14.04", "23.04.2014");
			dt.Rows.Add("7", "Ubuntu", "14.10", "23.10.2014");
			dt.Rows.Add("8", "Ubuntu", "15.04", "23.04.2015");

			DataSet ds = new DataSet();
			ds.Tables.Add(dt);

			DG.DataSource = ds.Tables[0];
		}
	}
}
