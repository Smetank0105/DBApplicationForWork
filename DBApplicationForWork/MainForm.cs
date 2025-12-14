using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Cache;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBApplicationForWork
{
	public partial class MainForm : Form
	{
		Connector connector;

		const string connectionString = "Data Source=SMETANK\\SQLEXPRESS;Initial Catalog=BOX_3;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
		string[] panel_tp_names = new string[] { "Главная", "Отображение"};
		string[] database_tp_names = new string[] { "Картриджи", "Принтеры", "Компьютеры" };
		string[] panel_btn_names = new string[] { "Новый наряд", "Редактировать", "Изменить статус", "Печать", "Обновить", "Удалить"};
		string[] state_names = new string[] { "", "на исполнении", "на отправку в фирму", "в фирме", "готов", "списание"};
		Color[] state_colors = new Color[] { Color.Black, Color.Yellow, Color.SteelBlue, Color.DeepSkyBlue, Color.ForestGreen, Color.White};
		string[] field_names = new string[] 
		{
			"ID",
			"номер наряда",
			"дата записи",
			"номер заявки",
			"подразделение",
			"название оборудования",
			"инвенратный номер",
			"замечания",
			"дата передачи в фирму",
			"номер акта фирмы",
			"дата готовности",
			"статус"
		};
		const string font_name = "Arial";
		const int font_size = 12;

		public MainForm()
		{
			InitializeComponent();
			this.Load += new System.EventHandler(this.MainForm_Load);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			connector = new Connector(connectionString);
			initComponents();
		}

		void initComponents()
		{
			this.Size = new Size(1000, 700);

//Create TabControl "tcPanel" with TabPages "tpMain" and "tpView"
			TabControl tcPanel = new TabControl();
			tcPanel.Dock = DockStyle.Top;
			tcPanel.Height = 105;
			tcPanel.SizeMode = TabSizeMode.Fixed;
			tcPanel.ItemSize = new Size(300, 20);
			
			TabPage tpPanelMain = new TabPage(panel_tp_names[0]);
			TabPage tpPanelView = new TabPage(panel_tp_names[1]);
			tcPanel.TabPages.Add(tpPanelMain);
			tcPanel.TabPages.Add(tpPanelView);

			this.Controls.Add(tcPanel);

//Create LayoutPanel for "tpPanelMain" and "tpPanelView"
			FlowLayoutPanel flpMain = new FlowLayoutPanel();
			flpMain.Dock = DockStyle.Fill;
			flpMain.AutoScroll = true;
			flpMain.WrapContents = false;
			flpMain.FlowDirection = FlowDirection.LeftToRight;
			flpMain.Padding = new Padding(15);
			tpPanelMain.Controls.Add(flpMain);

			TableLayoutPanel tlpView = new TableLayoutPanel();
			tlpView.Dock = DockStyle.Fill;
			tlpView.AutoScroll = true;
			tlpView.Padding = new Padding(10);
			tlpView.RowCount = 2;
			tlpView.ColumnCount = field_names.Length - 1;
			tpPanelView.Controls.Add(tlpView);

//Create TabControl "tcDataBase" with TabPages "tpCartridges", "tpPrinters" and "tpComputers"
			TabControl tcDataBase = new TabControl();
			tcDataBase.Bounds = new Rectangle(0, 100, 985, 560);
			tcDataBase.Anchor = (AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right) | AnchorStyles.Bottom);
			tcDataBase.SizeMode = TabSizeMode.Fixed;
			tcDataBase.ItemSize = new Size (200, 30);
			tcDataBase.Alignment = TabAlignment.Right;

			TabPage tpCartridges = new TabPage(database_tp_names[0]);
			TabPage tpPrinters = new TabPage(database_tp_names[1]);
			TabPage tpComputers = new TabPage(database_tp_names[2]);
			tcDataBase.TabPages.Add(tpCartridges);
			tcDataBase.TabPages.Add(tpPrinters);
			tcDataBase.TabPages.Add(tpComputers);

			this.Controls.Add(tcDataBase);

//Create Buttons for "tcPanel->tpMain"
			addMainButtons(flpMain);

//Create DataGridViewWithFilter for all TabPages in "tcDataBase"
			addDataGridViewWithFilters(tcDataBase);

//Create TextBox and CheckBox for "tcPnale->tpView"
			addViewCheckBoxes(tlpView);
			addViewTextBoxes(tlpView);
		}
		void addMainButtons(FlowLayoutPanel flp)
		{
			int btnWidth = 150;
			int btnHeight = 40;

			Button[] buttonsMain = new Button[]
			{
				new Button {Text = panel_btn_names[0], Name = "btnNewOrder", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[1], Name = "btnEditFields", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[2], Name = "btnChangeStates", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[3], Name = "btnPrint", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[4], Name = "btnRefresh", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[5], Name = "btnDelete", Font = new Font(font_name, font_size)}
			};
			for (int i = 0; i < buttonsMain.Length; i++)
			{
				if(i == 5)
				{
					Panel spacer = new Panel();
					spacer.Size = new Size(Screen.PrimaryScreen.Bounds.Width - btnWidth * 7, btnHeight);
					spacer.Visible = true;
					flp.Controls.Add(spacer);
				}
				buttonsMain[i].Size = new Size(btnWidth, btnHeight);
				flp.Controls.Add(buttonsMain[i]);
			}
			buttonsMain[1].Click += new EventHandler(btnMainEditFileds_Click);
			buttonsMain[2].Click += new EventHandler(btnMainChangeStates_Click);
			buttonsMain[3].Click += new EventHandler(btnMainPrint_Click);
		}
		void addDataGridViewWithFilters(TabControl tc)
		{
			DataGridViewWithFilter[] dGrids = new DataGridViewWithFilter[]
			{
				new DataGridViewWithFilter {Name = "dgCartridges"},
				new DataGridViewWithFilter {Name = "dgPrinters"},
				new DataGridViewWithFilter {Name = "dgComputers"}
			};
			for (int i = 0; i < dGrids.Length; i++)
			{
				dGrids[i].Dock = DockStyle.Fill;
				dGrids[i].AllowUserToAddRows = false;
				dGrids[i].AllowUserToDeleteRows = false;
				dGrids[i].ReadOnly = true;
				dGrids[i].SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dGrids[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
				dGrids[i].CellContextMenuStripNeeded += new DataGridViewCellContextMenuStripNeededEventHandler(dataGridView_CellContextMenuNeeded);
				tc.TabPages[i].Controls.Add(dGrids[i]);
			}
			dGrids[0].DataSource = connector.SelectCartridgeRecords();
			for (int i = 0; i < dGrids.Length; i++)
			{ 
				for (int j = 0; j < dGrids[i].ColumnCount; j++)
					dGrids[i].Columns[j].SortMode = DataGridViewColumnSortMode.NotSortable;
				if (dGrids[i].Columns.Count > 0)dGrids[i].Columns[0].Visible = false;
			}
		}
		void addViewTextBoxes(TableLayoutPanel tlp)
		{
			int tbWidth = 160;
			int tbHeight = 20;

			TextBox[] tbViews = new TextBox[]
			{
				new TextBox {Name = "tbView_1"},
				new TextBox {Name = "tbView_2"},
				new TextBox {Name = "tbView_3"},
				new TextBox {Name = "tbView_4"},
				new TextBox {Name = "tbView_5"},
				new TextBox {Name = "tbView_6"},
				new TextBox {Name = "tbView_7"},
				new TextBox {Name = "tbView_8"},
				new TextBox {Name = "tbView_9"},
				new TextBox {Name = "tbView_10"},
				new TextBox {Name = "tbView_11"},
			};
			for (int i = 0; i < tbViews.Length; i++)
			{
				tbViews[i].Size = new Size(tbWidth, tbHeight);
				tlp.Controls.Add(tbViews[i]);
			}
		}
		void addViewCheckBoxes(TableLayoutPanel tlp)
		{
			int cbWidth = 160;
			int cbHeight = 20;

			CheckBox[] cbViews = new CheckBox[]
			{
				new CheckBox {Text = field_names[1], Name = "cbView_1"},
				new CheckBox {Text = field_names[2], Name = "cbView_2"},
				new CheckBox {Text = field_names[3], Name = "cbView_3"},
				new CheckBox {Text = field_names[4], Name = "cbView_4"},
				new CheckBox {Text = field_names[5], Name = "cbView_5"},
				new CheckBox {Text = field_names[6], Name = "cbView_6"},
				new CheckBox {Text = field_names[7], Name = "cbView_7"},
				new CheckBox {Text = field_names[8], Name = "cbView_8"},
				new CheckBox {Text = field_names[9], Name = "cbView_9"},
				new CheckBox {Text = field_names[10], Name = "cbView_10"},
				new CheckBox {Text = field_names[11], Name = "cbView_11"}
			};
			for (int i = 0;  i < cbViews.Length; i++)
			{
				cbViews[i].Size = new Size(cbWidth, cbHeight);
				cbViews[i].CheckState = CheckState.Checked;
				tlp.Controls.Add(cbViews[i]);
			}
		}

//ContextMenuStrip

		ContextMenuStrip showMainEditFieldsMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsMainEF";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			for (int i = 1; i < 11; i++)
			{
				cms.Items.Add($"Редактировать поле \"{field_names[i]}\"");
			}
			foreach (ToolStripItem item in cms.Items)
				item.Padding = new Padding(0, 2, 0, 2);
			return cms;
		}
		ContextMenuStrip showMainChangeStatesMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsMainCS";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			for(int i = 1; i < state_names.Length;  i++)
			{
				ToolStripMenuItem item = new ToolStripMenuItem(state_names[i]);
				item.BackColor = state_colors[i];
				item.Padding = new Padding(0, 2, 0, 2);
				cms.Items.Add(item);
			}
			return cms;
		}
		ContextMenuStrip showMainPrintMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsMainP";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			cms.Items.Add("Распечатать наряд");
			cms.Items.Add("Распечатать акт фирмы");
			foreach (ToolStripItem item in cms.Items)
				item.Padding = new Padding(0, 2, 0, 2);
			return cms;
		}
		ContextMenuStrip showDataBaseMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsDataBase";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			ToolStripMenuItem editMenu = new ToolStripMenuItem(panel_btn_names[1]);
			for(int i = 1; i < field_names.Length; i++)
				editMenu.DropDownItems.Add(field_names[i]);
			cms.Items.Add(editMenu);
			ToolStripMenuItem stateMenu = new ToolStripMenuItem(panel_btn_names[2]);
			for(int i = 1; i < state_names.Length; i++)
				stateMenu.DropDownItems.Add(state_names[i]);
			cms.Items.Add(stateMenu);
			ToolStripMenuItem refreshMenu = new ToolStripMenuItem(panel_btn_names[4]);
			cms.Items.Add(refreshMenu);
			foreach (ToolStripItem item in cms.Items)
				item.Padding = new Padding(0, 2, 0, 2);
			return cms;
		}

//Events

		void btnMainEditFileds_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = showMainEditFieldsMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
		void btnMainChangeStates_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = showMainChangeStatesMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
		void btnMainPrint_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = showMainPrintMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
		void dataGridView_CellContextMenuNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
		{
			if (e.RowIndex != -1 && e.ColumnIndex != -1)
			{
				if(!(sender as DataGridViewWithFilter).Rows[e.RowIndex].Selected)
				{
					(sender as DataGridViewWithFilter).ClearSelection();
					(sender as DataGridViewWithFilter).Rows[e.RowIndex].Selected = true;
				}
				showDataBaseMenuStrip().Show(Cursor.Position);
			}
		}
	}
}
