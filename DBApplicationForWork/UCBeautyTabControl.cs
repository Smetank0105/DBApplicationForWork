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
    public partial class UCBeautyTabControl : UserControl
    {
        private Color panelBackColor = Color.WhiteSmoke;
        private Color btnSelectedBackColor = Color.SteelBlue;
        private Color btnSelectedForeColor = Color.WhiteSmoke;
        private Color btnUnselectedBackColor = Color.LightSteelBlue;
        private Color btnUnselectedForeColor = Color.White;
        private DockStyle btnDockStyle = DockStyle.Left;
        private DockStyle panelBtnDockStyle = DockStyle.Top;
        private DockStyle panelContentDockStyle = DockStyle.Fill;
        private int panelBtnWidth = 120;
        private int panelBtnHeight = 30;
        private int tabBtnWidth = 120;
        private int tabBtnHeight = 30;
        private int selectedIndex = -1;

        private Panel panelBtns;
        private Panel panelContent;
        private List<BeautyButton> tabBtns;
        private List<Control> tabPages;

        public Color PanelBackColor { get => panelBackColor; set => panelBackColor = value; }
        public Color BtnSelectedBackColor { get => btnSelectedBackColor; set => btnSelectedBackColor = value; }
        public Color BtnSelectedForeColor { get => btnSelectedForeColor; set => btnSelectedForeColor = value; }
        public Color BtnUnselectedBackColor { get => btnUnselectedBackColor; set => btnUnselectedBackColor = value; }
        public Color BtnUnselectedForeColor { get => btnUnselectedForeColor; set => btnUnselectedForeColor = value; }
        public DockStyle BtnDockStyle { get => btnDockStyle; set { btnDockStyle = value; foreach (var btn in tabBtns) btn.Dock = btnDockStyle; } }
        public DockStyle PanelBtnDockStyle { get => panelBtnDockStyle; set { panelBtnDockStyle = value; panelBtns.Dock = panelBtnDockStyle; } }
        public DockStyle PanelContentDockStyle { get => panelContentDockStyle; set { panelContentDockStyle = value; panelContent.Dock = panelContentDockStyle; } }
        public int PanelBtnWidth { get => panelBtnWidth; set { panelBtnWidth = value; panelBtns.Width = panelBtnWidth; } }
        public int PanelBtnHeight { get => panelBtnHeight; set { panelBtnHeight = value; panelBtns.Height = panelBtnHeight; } }
        public int TabBtnWidth { get => tabBtnWidth; set { tabBtnWidth = value; foreach (var btn in tabBtns) btn.Width = tabBtnWidth; } }
        public int TabBtnHeight { get => tabBtnHeight; set { tabBtnHeight = value; foreach (var btn in tabBtns) btn.Height = tabBtnHeight; } }

        public event EventHandler SelectedIndexChanged;

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value != selectedIndex && value > -1 && value < tabPages.Count)
                {
                    selectedIndex = value;
                    ShowTab(selectedIndex);
                    OnSelectedIndexChanged(EventArgs.Empty);
                }
            }
        }


        public UCBeautyTabControl()
        {
            InitializeComponent();
            InitUCBeautyTabControl();
        }

        private void InitUCBeautyTabControl()
        {
            tabBtns = new List<BeautyButton>();
            tabPages = new List<Control>();

            panelBtns = new Panel
            {
                Height = PanelBtnHeight,
                Dock = PanelBtnDockStyle,
                BackColor = panelBackColor
            };

            panelContent = new Panel()
            {
                Dock=PanelContentDockStyle,
                BackColor = panelBackColor
            };

            this.Controls.Add(panelBtns);
            this.Controls.Add(panelContent);
        }

        public void AddTab(string tabName, Control content)
        {
            int tabIndex = tabPages.Count();

            BeautyButton btn = new BeautyButton
            {
                Text = tabName,
                Dock = BtnDockStyle,
                Width = TabBtnWidth,
                Tag = tabIndex,
                BackColor = BtnUnselectedBackColor,
                ForeColor = BtnUnselectedForeColor
        };
            btn.Click += TabBtn_Click;

            tabBtns.Add(btn);
            tabPages.Add(content);

            panelBtns.Controls.Add(btn);
            panelContent.Controls.Add(content);

            content.Dock = DockStyle.Fill;
            content.Visible = false;

            if (tabIndex == 0)
                SelectedIndex=0;
        }

        private void TabBtn_Click(object sender, EventArgs e)
        {
            BeautyButton btn = sender as BeautyButton;
            if(btn != null)
            {
                SelectedIndex = (int)btn.Tag;
            }
        }

        public void ShowTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= tabPages.Count)
                return;

            foreach (var page in tabPages)
                page.Visible = false;

            tabPages[tabIndex].Visible = true;

            UpdateBtnAppearance();
        }

        private void UpdateBtnAppearance()
        {
            foreach (var btn in tabBtns)
            {
                if ((int)btn.Tag == SelectedIndex)
                {
                    btn.BackColor = BtnSelectedBackColor;
                    btn.ForeColor = BtnSelectedForeColor;
                }
                else
                {
                    btn.BackColor = BtnUnselectedBackColor;
                    btn.ForeColor = BtnUnselectedForeColor;
                }
            }
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }
    }
}
