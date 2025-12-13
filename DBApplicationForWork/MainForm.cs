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
		string[] panel_tp_names = new string[] { "Главная", "Отображение"};
		string[] database_tp_names = new string[] { "Картриджи", "Принтеры", "Компьютеры" };
		string[] panel_btn_names = new string[] { "Новый наряд", "Редактировать", "Изменить статус", "Печать"};
		string[] state_names = new string[] { "на исполнении", "на отправку в фирму", "в фирме", "готов", "списание"};
		Color[] state_colors = new Color[] { Color.Yellow, Color.SteelBlue, Color.DeepSkyBlue, Color.ForestGreen, Color.White};
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
			"передача в фирму",
			"номер акта фирмы",
			"дата готовности",
			"статус"
		};
		const string font_name = "Arial";
		const int font_size = 12;
		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			initComponents();
		}

		void initComponents()
		{
			this.Size = new Size(1000, 700);
			Control ctrl;

			//Create TabControl "tcPanel" with TabPages "tpMain" and "tpView"
			TabControl tcPanel = new TabControl();
			tcPanel.Dock = DockStyle.Top;
			tcPanel.Height = 100;
			tcPanel.SizeMode = TabSizeMode.Fixed;
			tcPanel.ItemSize = new Size(300, 20);
			
			TabPage tpPanelMain = new TabPage(panel_tp_names[0]);
			TabPage tpPanelView = new TabPage(panel_tp_names[1]);
			tcPanel.TabPages.Add(tpPanelMain);
			tcPanel.TabPages.Add(tpPanelView);

			this.Controls.Add(tcPanel);

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
			addMainButtons(tpPanelMain);

			//Create DataGridViewWithFilter for all TabPages in "tcDataBase"
			addDataGridViewWithFilters(tcDataBase);

			//Create TextBox and CheckBox for "tcPnale->tpView"
			addViewTextBoxes(tpPanelView);
			addViewCheckBoxes(tpPanelView);

			//Bound ContextMenuStrip for "tpMain"
			ctrl = Controls.Find("btnEditFields", true).FirstOrDefault();
			(ctrl as Button).Click += new EventHandler(btnMainEditFileds_Click);

			ctrl = Controls.Find("btnChangeStates", true).FirstOrDefault();
			(ctrl as Button).Click += new EventHandler(btnMainChangeStates_Click);

			ctrl = Controls.Find("btnPrint", true).FirstOrDefault();
			(ctrl as Button).Click += new EventHandler(btnMainPrint_Click);

			//test table
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
			Control dgCartridges = Controls.Find("dgCartridges", true).FirstOrDefault();
			(dgCartridges as DataGridViewWithFilter).DataSource = ds.Tables[0];
		}
		void addMainButtons(TabPage tp)
		{
			int btnWidth = 150;
			int btnHeight = 40;
			int btnSpacing = 30;
			int btnStartX = 15;
			int btnStartY = 15;

			Button[] buttonsMain = new Button[]
			{
				new Button {Text = panel_btn_names[0], Name = "btnNewOrder", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[1], Name = "btnEditFields", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[2], Name = "btnChangeStates", Font = new Font(font_name, font_size)},
				new Button {Text = panel_btn_names[3], Name = "btnPrint", Font = new Font(font_name, font_size)}
			};
			for (int i = 0; i < buttonsMain.Length; i++)
			{
				buttonsMain[i].Size = new Size(btnWidth, btnHeight);
				buttonsMain[i].Location = new Point(btnStartX + i * (btnWidth + btnSpacing), btnStartY);
				tp.Controls.Add(buttonsMain[i]);
			}
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
				tc.TabPages[i].Controls.Add(dGrids[i]);
			}
		}
		void addViewTextBoxes(TabPage tp)
		{
			int tbWidth = 140;
			int tbHeight = 20;
			int tbSpacing = 20;
			int tbStartX = 15;
			int tbStartY = 45;

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
				tbViews[i].Location = new Point(tbStartX + i * (tbWidth + tbSpacing), tbStartY);
				tp.Controls.Add(tbViews[i]);
			}
		}
		void addViewCheckBoxes(TabPage tp)
		{
			int cbWidth = 140;
			int cbHeight = 20;
			int cbSpacing = 20;
			int cbStartX = 15;
			int cbStartY = 15;

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
				cbViews[i].Location = new Point(cbStartX + i * (cbWidth + cbSpacing), cbStartY);
				cbViews[i].CheckState = CheckState.Checked;
				tp.Controls.Add(cbViews[i]);
			}
		}
		ContextMenuStrip ShowMainEditFieldsMenuStrip()
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
		ContextMenuStrip ShowMainChangeStatesMenuStrip()
		{
			ContextMenuStrip cms = new ContextMenuStrip();
			cms.Name = "cmsMainCS";
			cms.Font = new Font(font_name, font_size);
			cms.ShowImageMargin = false;
			cms.ShowCheckMargin = false;
			for(int i = 0; i < state_names.Length;  i++)
			{
				ToolStripMenuItem item = new ToolStripMenuItem(state_names[i]);
				item.BackColor = state_colors[i];
				item.Padding = new Padding(0, 2, 0, 2);
				cms.Items.Add(item);
			}
			return cms;
		}
		ContextMenuStrip ShowMainPrintMenuStrip()
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

		//			Events

		void btnMainEditFileds_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = ShowMainEditFieldsMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
		void btnMainChangeStates_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = ShowMainChangeStatesMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
		void btnMainPrint_Click(object sender, EventArgs e)
		{
			ContextMenuStrip cms = ShowMainPrintMenuStrip();
			Button btn = (sender as Button);
			cms.Show(btn, new Point(0, btn.Height));
		}
	}
}
