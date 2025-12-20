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
	public partial class InsertRecordsForm : Form
	{
		Connector connector;
		const string connectionString = "Data Source=SMETANK\\SQLEXPRESS;Initial Catalog=BOX_3;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

		private List<short> cartridges;
		private List<int> inventorys;
		public InsertRecordsForm()
		{
			InitializeComponent();
			this.Load += new System.EventHandler(this.MainForm_Load);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			connector = new Connector(connectionString);
			cartridges = new List<short>();
			inventorys = new List<int>();
			initComponents();
		}
		void initComponents()
		{
//Fixed size for this form
			this.Size = new Size(400, 540);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.StartPosition = FormStartPosition.CenterScreen;

//Create TableLayoutPanel for this form
			TableLayoutPanel tlpPanel = new TableLayoutPanel();
			tlpPanel.Dock = DockStyle.Fill;
			tlpPanel.Padding = new Padding(10);
			tlpPanel.AutoSize = true;
			tlpPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
			this.Controls.Add(tlpPanel);

//Create GroupBox 1 and 2
			GroupBox gb1 = new GroupBox();
			gb1.Dock = DockStyle.Fill;
			tlpPanel.Controls.Add(gb1, 0, 0);
			//tlpPanel.SetColumnSpan(gb1, 2);

			GroupBox gb2 = new GroupBox();
			gb2.Dock = DockStyle.Fill;
			tlpPanel.Controls.Add(gb2, 0, 1);
			//tlpPanel.SetColumnSpan(gb2, 2);

			//Create TableLayoutPanel for GroupBox 1 and 2
			TableLayoutPanel tlpBaseInfo = new TableLayoutPanel();
			tlpBaseInfo.Dock = DockStyle.Fill;
			tlpBaseInfo.AutoSize = true;
			tlpBaseInfo.ColumnCount = 2;
			gb1.Controls.Add(tlpBaseInfo);

			TableLayoutPanel tlpDetails = new TableLayoutPanel();
			tlpDetails.Dock = DockStyle.Fill;
			tlpDetails.AutoSize = true;
			tlpDetails.ColumnCount = 2;
			gb2.Controls.Add(tlpDetails);

//Create fields
//Fill tlpVaseInfo
			//Create feild "order_number"
			Label lblOrderNumber = new Label();
			lblOrderNumber.Dock = DockStyle.Fill;
			lblOrderNumber.Text = "Номер наряда:";
			tlpBaseInfo.Controls.Add(lblOrderNumber, 0, 0);

			TextBox txtOrderNumber = new TextBox();
			txtOrderNumber.Name = "txtOrderNumber";
			txtOrderNumber.Dock = DockStyle.Fill;
			txtOrderNumber.Text = (connector.GetLastOrderNumber() + 1).ToString();
			txtOrderNumber.KeyPress += new KeyPressEventHandler(textBoxOnlyDigit_KeyPress);
			tlpBaseInfo.Controls.Add(txtOrderNumber, 1, 0);

			//Create field "recording_date"
			Label lblRecordingDate = new Label();
			lblRecordingDate.Dock = DockStyle.Fill;
			lblRecordingDate.Text = "Дата наряда:";
			tlpBaseInfo.Controls.Add(lblRecordingDate, 0, 1);

			DateTimePicker dtpRecordingDate = new DateTimePicker();
			dtpRecordingDate.Name = "dtpRecordingDate";
			dtpRecordingDate.Dock = DockStyle.Fill;
			dtpRecordingDate.CustomFormat = "yyyy-MM-dd";
			dtpRecordingDate.Format = DateTimePickerFormat.Custom;
			tlpBaseInfo.Controls.Add(dtpRecordingDate, 1, 1);

			//Create field "request_number"
			Label lblRequestNumber = new Label();
			lblRequestNumber.Dock = DockStyle.Fill;
			lblRequestNumber.Text = "Номер заявки:";
			tlpBaseInfo.Controls.Add(lblRequestNumber, 0, 2);

			ComboBox cbbRequestNumber = new ComboBox();
			cbbRequestNumber.Name = "cbbRequestNumber";
			cbbRequestNumber.Dock = DockStyle.Fill;
			cbbRequestNumber.DataSource = connector.SelectSmallTable("RequestNumbers");
			cbbRequestNumber.DisplayMember = "name";
			cbbRequestNumber.ValueMember = "id";
			cbbRequestNumber.AutoCompleteSource = AutoCompleteSource.ListItems;
			cbbRequestNumber.AutoCompleteMode = AutoCompleteMode.Suggest;
			tlpBaseInfo.Controls.Add(cbbRequestNumber, 1, 2);

			//Create field "department"
			Label lblbDepartment = new Label();
			lblbDepartment.Dock = DockStyle.Fill;
			lblbDepartment.Text = "Отделение:";
			tlpBaseInfo.Controls.Add(lblbDepartment, 0, 3);

			ComboBox cbbDepartment = new ComboBox();
			cbbDepartment.Name = "cbbDepartment";
			cbbDepartment.Dock = DockStyle.Fill;
			cbbDepartment.DataSource = connector.SelectSmallTable("Departments");
			cbbDepartment.DisplayMember = "name";
			cbbDepartment.ValueMember = "id";
			cbbDepartment.AutoCompleteSource = AutoCompleteSource.ListItems;
			cbbDepartment.AutoCompleteMode = AutoCompleteMode.Suggest;
			tlpBaseInfo.Controls.Add(cbbDepartment, 1, 3);

//Fill tlpDetails
			//Create field "cartringe"
			Label lblCartridge = new Label();
			lblCartridge.Dock = DockStyle.Fill;
			lblCartridge.Text = "Наименование:";
			tlpDetails.Controls.Add(lblCartridge, 0, 0);

			ComboBox cbbCartridge = new ComboBox();
			cbbCartridge.Name = "cbbCartridge";
			cbbCartridge.Dock = DockStyle.Fill;
			cbbCartridge.DataSource = connector.SelectSmallTable("Cartridges");
			cbbCartridge.DisplayMember = "name";
			cbbCartridge.ValueMember = "id";
			cbbCartridge.AutoCompleteSource = AutoCompleteSource.ListItems;
			cbbCartridge.AutoCompleteMode = AutoCompleteMode.Suggest;
			tlpDetails.Controls.Add(cbbCartridge, 1, 0);

			//Create field "inventory"
			Label lblInventory = new Label();
			lblInventory.Dock = DockStyle.Fill;
			lblInventory.Text = "Инвентарный:";
			tlpDetails .Controls.Add(lblInventory, 0, 1);

			ComboBox cbbInventory = new ComboBox();
			cbbInventory.Name = "cbbInventory";
			cbbInventory.Dock = DockStyle.Fill;
			cbbInventory.DataSource = connector.SelectSmallTable("CartridgeInventorys");
			cbbInventory.DisplayMember = "name";
			cbbInventory.ValueMember = "id";
			cbbInventory.AutoCompleteSource = AutoCompleteSource.ListItems;
			cbbInventory .AutoCompleteMode = AutoCompleteMode.Suggest;
			tlpDetails.Controls.Add(cbbInventory, 1, 1);

			//Create button "add to list"
			Button btnAddToList = new Button();
			btnAddToList.Name = "btnAddToList";
			btnAddToList.Dock = DockStyle.Fill;
			btnAddToList.Text = "Добавить";
			btnAddToList.Click += new EventHandler(btnAddToList_Click);
			tlpDetails.Controls.Add(btnAddToList, 0, 2);
			tlpDetails.SetColumnSpan(btnAddToList, 2);
			
			//Create DataGridView for "Cartridge" and "Invetory" views
			DataGridView dgvDetails = new DataGridView();
			dgvDetails.Name = "dgvDetails";
			dgvDetails.Dock = DockStyle.Fill;
			dgvDetails.AllowUserToAddRows = false;
			dgvDetails.AllowUserToDeleteRows = false;
			dgvDetails.ReadOnly = true;
			dgvDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvDetails.Columns.Add("Cartridges", "Наименование");
			dgvDetails.Columns.Add("CartridgeInventorys", "Инвентарный");
			tlpDetails.Controls.Add(dgvDetails, 0, 3);
			tlpDetails.SetColumnSpan(dgvDetails , 2);

			//Create buttons "Clear", "Ok" and "Cancel"
			Button btnClear = new Button();
			btnClear.Name = "btnClear";
			btnClear.Dock = DockStyle.Fill;
			btnClear.Text = "Очистить";
			btnClear.Click += new EventHandler(btnClear_Click);
			tlpDetails.Controls.Add(btnClear, 0, 4);

			Button btnOk = new Button();
			btnOk.Name = "btnOk";
			btnOk.Size = new Size(80, 30);
			btnOk.DialogResult = DialogResult.OK;
			btnOk.Text = "OK";
			btnOk.Click += new EventHandler(btnOk_Click);

			Button btnCancel = new Button();
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(80, 30);
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.Text = "Cancel";

			FlowLayoutPanel flpOkCancel = new FlowLayoutPanel();
			flpOkCancel.Dock = DockStyle.Fill;
			flpOkCancel.FlowDirection = FlowDirection.RightToLeft;
			flpOkCancel.Controls.Add(btnCancel);
			flpOkCancel.Controls.Add(btnOk);
			tlpDetails.Controls.Add(flpOkCancel, 1, 5);
		}

//Events
		void btnAddToList_Click(object sender, EventArgs e)
		{
			ComboBox cbCart = this.Controls.Find("cbbCartridge", true).FirstOrDefault() as ComboBox;
			ComboBox cbInv = this.Controls.Find("cbbInventory", true).FirstOrDefault() as ComboBox;
			DataGridView dgv = this.Controls.Find("dgvDetails", true).FirstOrDefault() as DataGridView;

			DataGridViewRow newRow = new DataGridViewRow();
			newRow.Cells.Add(new DataGridViewTextBoxCell());
			newRow.Cells.Add(new DataGridViewTextBoxCell());

			if(cbCart != null && cbInv != null)
			{
				if(!string.IsNullOrWhiteSpace(cbCart.Text) && !string.IsNullOrWhiteSpace(cbInv.Text))
				{
					if(cbCart.SelectedIndex != -1)
					{
						cartridges.Add(Convert.ToInt16(cbCart.SelectedValue));
						newRow.Cells[0].Value = cbCart.Text;
					}
					else
					{
						try
						{
							int id = connector.InsertOneFieldTable("Cartridges", cbCart.Text.Trim());
							cartridges.Add((short)id);
							newRow.Cells[0].Value = cbCart.Text.Trim();
							cbCart.DataSource = connector.SelectSmallTable("Cartridges");
							cbCart.DisplayMember = "name";
							cbCart.ValueMember = "id";
						}
						catch (Exception ex) { MessageBox.Show(ex.Message); }
					}

					if (cbInv.SelectedIndex != -1)
					{
						inventorys.Add(Convert.ToInt32(cbInv.SelectedValue));
						newRow.Cells[1].Value = cbInv.Text;
					}
					else
					{
						try
						{
							int id = connector.InsertOneFieldTable("CartridgeInventorys", cbInv.Text.Trim());
							inventorys.Add(id);
							newRow.Cells[1].Value = cbInv.Text.Trim();
							cbInv.DataSource = connector.SelectSmallTable("CartridgeInventorys");
							cbInv.DisplayMember = "name";
							cbInv.ValueMember = "id";
						}
						catch (Exception ex) { MessageBox.Show(ex.Message); }
					}
					if (newRow.Cells[0] != null && newRow.Cells[1] != null)
						dgv.Rows.Add(newRow);
				}
			}
		}
		void btnClear_Click(object sender, EventArgs e)
		{
			DataGridView dgv = this.Controls.Find("dgvDetails", true).FirstOrDefault() as DataGridView;

			if (dgv != null)
			{
				cartridges.Clear();
				inventorys.Clear();
				dgv.Rows.Clear();
			}
		}
		void btnOk_Click(object sender, EventArgs e)
		{
			TextBox txtOrederNUmber = this.Controls.Find("txtOrderNumber", true).FirstOrDefault() as TextBox;
			DateTimePicker dtpRecordingDate = this.Controls.Find("dtpRecordingDate", true).FirstOrDefault() as DateTimePicker;
			ComboBox cbbRequestNumber = this.Controls.Find("cbbRequestNumber", true).FirstOrDefault() as ComboBox;
			ComboBox cbbDepartment = this.Controls.Find("cbbDepartment", true).FirstOrDefault() as ComboBox;

			int request_number = 0;
			short department = 0;

			if (txtOrederNUmber != null && dtpRecordingDate != null && cbbRequestNumber != null && cbbDepartment != null && cartridges.Count > 0)
			{
				if (!string.IsNullOrWhiteSpace(cbbRequestNumber.Text) && !string.IsNullOrWhiteSpace(cbbDepartment.Text))
				{
					if (cbbRequestNumber.SelectedIndex != -1)
					{
						request_number = Convert.ToInt32(cbbRequestNumber.SelectedValue);
					}
					else
					{
						try
						{
							request_number = connector.InsertOneFieldTable("RequestNumbers", cbbRequestNumber.Text.Trim());
						}
						catch (Exception ex) { MessageBox.Show(ex.Message); }
					}
					if (cbbDepartment.SelectedIndex != -1)
					{
						department = Convert.ToInt16(cbbDepartment.SelectedValue);
					}
					else
					{
						try
						{
							department = (short)connector.InsertOneFieldTable("Departments", cbbDepartment.Text.Trim());
						}
						catch (Exception ex) { MessageBox.Show(ex.Message); }
					}
				}
			int result = connector.InsertCartridgeRecords
									(
									Convert.ToInt32(txtOrederNUmber.Text),
									dtpRecordingDate.Value.ToString("yyyy-MM-dd"),
									request_number,
									department,
									cartridges,
									inventorys
									);
			if (result == -1)
				MessageBox.Show("InsertCartridgeRecords failed");
			else if (result != cartridges.Count)
				MessageBox.Show("Error. Not all rows are inserted");
			}
		}
		void textBoxOnlyDigit_KeyPress(object sender, KeyPressEventArgs e)
		{
			if(!Char.IsDigit(e.KeyChar))e.Handled = true;
		}
	}
}
