using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace DBApplicationForWork
{
	public partial class UpdateRecordsForm : Form
	{
		Connector connector;
		const string connectionString = "Data Source=SMETANK\\SQLEXPRESS;Initial Catalog=BOX_3;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
		string[] names = new string[] { "", "номер наряда", "дата записи", "номер заявки", "подразделение", "название оборудования", "инвенратный номер", "замечания", "дата передачи в фирму", "номер акта фирмы", "дата готовности", "дата выдачи" };
		string[][] table_names =
		{
			new string[] { "", "", "", "RequestNumbers", "Departments", "Cartridges", "CartridgeInventorys" },
			new string[] { "", "", "", "RequestNumbers", "Departments", "Printers", "CartridgeInventorys" },
			new string[] { "", "", "", "RequestNumbers", "Departments", "Computers", "ComputerInventorys" }
		};
		private int TableIndex { get; set; }
		private int FieldIndex { get; set; }
		private List<int> Ids { get; set; }
		private string Value { get; set; }
		public UpdateRecordsForm(int table_index, int field_index, List<int> ids, string current_value)
		{
			InitializeComponent();
			this.TableIndex = table_index;
			this.FieldIndex = field_index;
			this.Ids = ids;
			this.Value = current_value;
			this.Load += new EventHandler(this.UpdateRecordsForm_Load);
		}
		void UpdateRecordsForm_Load(object sender, EventArgs e)
		{
			connector = new Connector(connectionString);
			initComponents();
		}
		void initComponents()
		{
			this.Text = $"\"{names[FieldIndex]}\"";
			this.Size = new Size(300, 120);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.StartPosition = FormStartPosition.CenterScreen;

			//Create TableLayoutPanel for this form
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.Dock = DockStyle.Fill;
			tlp.AutoSize = true;
			tlp.Padding = new Padding(10);
			this.Controls.Add(tlp);

			//Create button "Ok" and "Cancel"
			FlowLayoutPanel flp = new FlowLayoutPanel();
			flp.Dock = DockStyle.Fill;
			flp.FlowDirection = FlowDirection.RightToLeft;
			tlp.Controls.Add(flp, 0, 1);
			tlp.SetColumnSpan(flp, 2);

			Button btnCancel = new Button();
			btnCancel.Size = new Size(80, 30);
			btnCancel.Text = "Cancel";
			btnCancel .DialogResult = DialogResult.Cancel;
			flp.Controls.Add(btnCancel);

			Button btnOk = new Button();
			btnOk.Name = "btnOk";
			btnOk.Size = new Size(80, 30);
			btnOk.Text = "Ok";
			btnOk.DialogResult = DialogResult.OK;
			btnOk.Click += new EventHandler(btnOk_Click);
			flp.Controls.Add(btnOk);

			//Create edit fields control (TextBox, DateTimePicker or ComboBox)
			switch (FieldIndex)
			{
				case 1:
				case 7:
				case 9:
					TextBox txt = new TextBox();
					txt.Name = "txt";
					txt.Dock = DockStyle.Fill;
					if(!string.IsNullOrEmpty(Value)) 
						txt.Text = Value;
					if (FieldIndex != 7)
						txt.KeyPress += new KeyPressEventHandler(txtOnlyDigit_KeyPress);
					tlp.Controls.Add(txt, 0, 0);
					tlp.SetColumnSpan(txt, 2);
					break;
				case 2:
				case 8:
				case 10:
				case 11:
					DateTimePicker dtp = new DateTimePicker();
					dtp.Name = "dtp";
					dtp.Dock = DockStyle.Fill;
					dtp.CustomFormat = "yyyy-MM-dd";
					dtp.Format = DateTimePickerFormat.Custom;
					if (!string.IsNullOrEmpty(Value)) 
						dtp.Value = DateTime.Parse(Value);
					tlp.Controls.Add(dtp, 0, 0);
					tlp.SetColumnSpan(dtp, 2);
					if (FieldIndex != 2)
					{
						Button btnClear = new Button();
						btnClear.Name = "btnClear";
						btnClear.Size = new Size(80, 30);
						btnClear.Text = "Очистить";
						btnClear.DialogResult = DialogResult.OK;
						btnClear.Click += new EventHandler(btnClear_Click);
						flp.Controls.Add(btnClear);
					}
					break;
				case 3:
				case 4:
				case 5:
				case 6:
					ComboBox cbb = new ComboBox();
					cbb.Name = "cbb";
					cbb.Dock = DockStyle.Fill;
					cbb.DataSource = connector.SelectSmallTable(table_names[TableIndex][FieldIndex]);
					cbb.DisplayMember = "name";
					cbb.ValueMember = "id";
					cbb.AutoCompleteSource = AutoCompleteSource.ListItems;
					cbb.AutoCompleteMode = AutoCompleteMode.Suggest;
					//Почему-то не работает 8(
					//if (!string.IsNullOrEmpty(Value))
					//	cbb.SelectedIndex = cbb.Items.IndexOf(Value);
					tlp.Controls.Add(cbb, 0, 0);
					tlp.SetColumnSpan(cbb, 2);
					break;
				default:
					break;
			}
		}

//Events

		void btnOk_Click(object sender, EventArgs e)
		{
			int result = 0;
			switch (FieldIndex)
			{
				case 1:
				case 7:
				case 9:
					TextBox txt = this.Controls.Find("txt", true).FirstOrDefault() as TextBox;
					if(txt != null && (!string.IsNullOrEmpty(txt.Text) || FieldIndex == 7 || FieldIndex == 9))
						result = connector.UpdateRecords(TableIndex, FieldIndex, txt.Text, Ids);
					break;
				case 2:
				case 8:
				case 10:
				case 11:
					DateTimePicker dtp = this.Controls.Find("dtp", true).FirstOrDefault() as DateTimePicker;
					if(dtp != null)
						result = connector.UpdateRecords(TableIndex, FieldIndex, dtp.Text, Ids);
					break;
				case 3:
				case 4:
				case 5:
				case 6:
					ComboBox cbb = this.Controls.Find("cbb", true).FirstOrDefault() as ComboBox;
					if(cbb != null &&  !string.IsNullOrWhiteSpace(cbb.Text))
					{
						string str = string.Empty;
						if (cbb.SelectedIndex != -1)
						{
							str = cbb.SelectedValue.ToString();
						}
						else
						{
							try
							{
								str = connector.InsertOneFieldTable(table_names[TableIndex][FieldIndex], cbb.Text.Trim()).ToString();
							}
							catch (Exception ex) { MessageBox.Show(ex.Message); }
						}
						result = connector.UpdateRecords(TableIndex, FieldIndex, str, Ids);
					}
					break;
				default:
					break;
			}
			if (result == -1)
				MessageBox.Show("UpdateCartridgeRecords failed");
			else if (result != Ids.Count)
				MessageBox.Show("Error. Not all rows are updated");
		}
		void btnClear_Click(object sender, EventArgs e)
		{
			int result = connector.UpdateRecords(TableIndex, FieldIndex, "", Ids);
			if (result == -1)
				MessageBox.Show("UpdateCartridgeRecords failed");
			else if (result != Ids.Count)
				MessageBox.Show("Error. Not all rows are updated");
		}
		void txtOnlyDigit_KeyPress(object sender, KeyPressEventArgs e)
		{
			if(!Char.IsDigit(e.KeyChar))e.Handled = true;
		}
	}
}
