using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

public partial class Form1 : Form
{
    private TextBox txtCabinetNameGiris;
    private Button btnCreateDrawer;
    private TextBox txtLinkGiris;
    private TextBox txtTitleGiris;
    private TextBox txtNoteGiris;
    private Button btnAddLink;
    private Button btnToggleTopPanel;

    private Button btnPrevDrawer;
    private Button btnNextDrawer;
    private Label lblNavigationStatus;

    private Panel pnlTopControl;
    private Panel pnlDrawerContainer;
    private List<DrawerItem> cabinetDrawers = new List<DrawerItem>();
    private DrawerItem openDrawer = null;

    private Color colorCabinetBody = Color.FromArgb(50, 58, 64);
    private Color colorCardboardBrown = Color.FromArgb(139, 105, 74);
    private Color colorCardboardBrownOpen = Color.FromArgb(166, 128, 93);
    private Color colorOldFolderBeige = Color.FromArgb(242, 227, 198);
    private Color colorFolderTabColor = Color.FromArgb(222, 202, 167);

    private Color colorNeutralHeaderBg = Color.FromArgb(38, 43, 48);
    private Color colorNeutralLabel = Color.FromArgb(210, 215, 220);
    private Color colorNeutralInputBg = Color.FromArgb(60, 66, 72);
    private Color colorNeutralBtnBg = Color.FromArgb(85, 95, 102);

    private System.Windows.Forms.Timer pushAnimationTimer;
    private System.Windows.Forms.Timer topPanelAnimationTimer;

    private bool isTopPanelOpen = true;
    private int topPanelTargetY = 0;
    private bool isProcessing = false;

    private string storageFileName = "cabinet_storage.txt";

    public class LinkRecord
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Note { get; set; }

        public override string ToString()
        {
            return $"📄 {Title}";
        }
    }

    public class DrawerItem
    {
        public Panel DrawerWrapper { get; set; }
        public Button DrawerButton { get; set; }
        public Panel FolderPanel { get; set; }
        public ListBox LinkList { get; set; }
        public TextBox InfoTextBox { get; set; }
        public List<LinkRecord> Links { get; set; } = new List<LinkRecord>();
        public int TargetY { get; set; }
    }

    public Form1()
    {
        this.Text = "LINK DRAWER";
        this.Size = new Size(850, 750);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = colorCabinetBody;

        pushAnimationTimer = new System.Windows.Forms.Timer();
        pushAnimationTimer.Interval = 10;
        pushAnimationTimer.Tick += PushAnimationTimer_Tick;

        topPanelAnimationTimer = new System.Windows.Forms.Timer();
        topPanelAnimationTimer.Interval = 10;
        topPanelAnimationTimer.Tick += TopPanelAnimationTimer_Tick;

        BuildIronCabinetUI();
        LoadCabinetData();
    }

    private void BuildIronCabinetUI()
    {
        btnToggleTopPanel = new Button();
        btnToggleTopPanel.Text = "[ ▲ HIDE ]";
        btnToggleTopPanel.Location = new Point(690, 12);
        btnToggleTopPanel.Size = new Size(115, 30);
        btnToggleTopPanel.Font = new Font("Courier New", 9, FontStyle.Bold);
        btnToggleTopPanel.BackColor = Color.FromArgb(30, 35, 40);
        btnToggleTopPanel.ForeColor = colorNeutralLabel;
        btnToggleTopPanel.FlatStyle = FlatStyle.Flat;
        btnToggleTopPanel.Click += BtnToggleTopPanel_Click;
        this.Controls.Add(btnToggleTopPanel);

        pnlTopControl = new Panel();
        pnlTopControl.Location = new Point(0, 0);
        pnlTopControl.Size = new Size(850, 135);
        pnlTopControl.BackColor = colorNeutralHeaderBg;
        pnlTopControl.BorderStyle = BorderStyle.FixedSingle;
        this.Controls.Add(pnlTopControl);

        Label lblPanelTitle = new Label();
        lblPanelTitle.Text = "  ADD LINK / FILE";
        lblPanelTitle.Location = new Point(25, 12);
        lblPanelTitle.Size = new Size(250, 20);
        lblPanelTitle.ForeColor = colorNeutralLabel;
        lblPanelTitle.Font = new Font("Courier New", 10, FontStyle.Bold);
        pnlTopControl.Controls.Add(lblPanelTitle);

        Label lblNewCabinet = new Label();
        lblNewCabinet.Text = "FILE NAME:";
        lblNewCabinet.Location = new Point(25, 42);
        lblNewCabinet.Size = new Size(110, 20);
        lblNewCabinet.ForeColor = colorNeutralLabel;
        lblNewCabinet.Font = new Font("Courier New", 9, FontStyle.Bold);
        pnlTopControl.Controls.Add(lblNewCabinet);

        txtCabinetNameGiris = new TextBox();
        txtCabinetNameGiris.Location = new Point(140, 39);
        txtCabinetNameGiris.Size = new Size(180, 25);
        txtCabinetNameGiris.Font = new Font("Courier New", 10);
        txtCabinetNameGiris.BackColor = colorNeutralInputBg;
        txtCabinetNameGiris.ForeColor = Color.White;
        txtCabinetNameGiris.BorderStyle = BorderStyle.FixedSingle;
        pnlTopControl.Controls.Add(txtCabinetNameGiris);

        btnCreateDrawer = new Button();
        btnCreateDrawer.Text = "[ INSTALL FILE ]";
        btnCreateDrawer.Location = new Point(335, 37);
        btnCreateDrawer.Size = new Size(160, 28);
        btnCreateDrawer.Font = new Font("Courier New", 9, FontStyle.Bold);
        btnCreateDrawer.BackColor = colorNeutralBtnBg;
        btnCreateDrawer.ForeColor = Color.White;
        btnCreateDrawer.FlatStyle = FlatStyle.Flat;
        btnCreateDrawer.Click += BtnCreateDrawer_Click;
        pnlTopControl.Controls.Add(btnCreateDrawer);

        Label lblLink = new Label();
        lblLink.Text = "URL ADDRESS:";
        lblLink.Location = new Point(25, 74);
        lblLink.Size = new Size(110, 20);
        lblLink.ForeColor = colorNeutralLabel;
        lblLink.Font = new Font("Courier New", 9, FontStyle.Bold);
        pnlTopControl.Controls.Add(lblLink);

        txtLinkGiris = new TextBox();
        txtLinkGiris.Location = new Point(140, 71);
        txtLinkGiris.Size = new Size(355, 25);
        txtLinkGiris.Font = new Font("Courier New", 10);
        txtLinkGiris.BackColor = colorNeutralInputBg;
        txtLinkGiris.ForeColor = Color.White;
        txtLinkGiris.BorderStyle = BorderStyle.FixedSingle;
        pnlTopControl.Controls.Add(txtLinkGiris);

        Label lblTitle = new Label();
        lblTitle.Text = "TITLE:";
        lblTitle.Location = new Point(510, 74);
        lblTitle.Size = new Size(60, 20);
        lblTitle.ForeColor = colorNeutralLabel;
        lblTitle.Font = new Font("Courier New", 9, FontStyle.Bold);
        pnlTopControl.Controls.Add(lblTitle);

        txtTitleGiris = new TextBox();
        txtTitleGiris.Location = new Point(575, 71);
        txtTitleGiris.Size = new Size(230, 25);
        txtTitleGiris.Font = new Font("Courier New", 10);
        txtTitleGiris.BackColor = colorNeutralInputBg;
        txtTitleGiris.ForeColor = Color.White;
        txtTitleGiris.BorderStyle = BorderStyle.FixedSingle;
        pnlTopControl.Controls.Add(txtTitleGiris);

        Label lblNote = new Label();
        lblNote.Text = "FILE NOTE:";
        lblNote.Location = new Point(25, 104);
        lblNote.Size = new Size(110, 20);
        lblNote.ForeColor = colorNeutralLabel;
        lblNote.Font = new Font("Courier New", 9, FontStyle.Bold);
        pnlTopControl.Controls.Add(lblNote);

        txtNoteGiris = new TextBox();
        txtNoteGiris.Location = new Point(140, 101);
        txtNoteGiris.Size = new Size(520, 25);
        txtNoteGiris.Font = new Font("Courier New", 10);
        txtNoteGiris.BackColor = colorNeutralInputBg;
        txtNoteGiris.ForeColor = Color.White;
        txtNoteGiris.BorderStyle = BorderStyle.FixedSingle;
        pnlTopControl.Controls.Add(txtNoteGiris);

        btnAddLink = new Button();
        btnAddLink.Text = "[ ADD LINK ]";
        btnAddLink.Location = new Point(675, 99);
        btnAddLink.Size = new Size(130, 28);
        btnAddLink.Font = new Font("Courier New", 9, FontStyle.Bold);
        btnAddLink.BackColor = Color.FromArgb(70, 90, 80);
        btnAddLink.ForeColor = Color.FromArgb(240, 240, 240);
        btnAddLink.FlatStyle = FlatStyle.Flat;
        btnAddLink.Click += BtnAddLink_Click;
        pnlTopControl.Controls.Add(btnAddLink);

        pnlDrawerContainer = new Panel();
        pnlDrawerContainer.Location = new Point(25, 150);
        pnlDrawerContainer.Size = new Size(785, 495);
        pnlDrawerContainer.BackColor = Color.FromArgb(35, 40, 45);
        pnlDrawerContainer.BorderStyle = BorderStyle.Fixed3D;
        pnlDrawerContainer.AutoScroll = true;
        this.Controls.Add(pnlDrawerContainer);

        btnPrevDrawer = new Button();
        btnPrevDrawer.Text = "[ ◄ PREV ]";
        btnPrevDrawer.Location = new Point(480, 665);
        btnPrevDrawer.Size = new Size(100, 30);
        btnPrevDrawer.Font = new Font("Courier New", 9, FontStyle.Bold);
        btnPrevDrawer.BackColor = Color.FromArgb(45, 50, 55);
        btnPrevDrawer.ForeColor = Color.White;
        btnPrevDrawer.FlatStyle = FlatStyle.Flat;
        btnPrevDrawer.Click += BtnPrevDrawer_Click;
        this.Controls.Add(btnPrevDrawer);

        lblNavigationStatus = new Label();
        lblNavigationStatus.Text = "0 / 0";
        lblNavigationStatus.Location = new Point(590, 670);
        lblNavigationStatus.Size = new Size(110, 20);
        lblNavigationStatus.Font = new Font("Courier New", 10, FontStyle.Bold);
        lblNavigationStatus.ForeColor = Color.FromArgb(180, 190, 200);
        lblNavigationStatus.TextAlign = ContentAlignment.MiddleCenter;
        this.Controls.Add(lblNavigationStatus);

        btnNextDrawer = new Button();
        btnNextDrawer.Text = "[ NEXT ► ]";
        btnNextDrawer.Location = new Point(710, 665);
        btnNextDrawer.Size = new Size(100, 30);
        btnNextDrawer.Font = new Font("Courier New", 9, FontStyle.Bold);
        btnNextDrawer.BackColor = Color.FromArgb(45, 50, 55);
        btnNextDrawer.ForeColor = Color.White;
        btnNextDrawer.FlatStyle = FlatStyle.Flat;
        btnNextDrawer.Click += BtnNextDrawer_Click;
        this.Controls.Add(btnNextDrawer);

        btnToggleTopPanel.BringToFront();
    }

    private void BtnToggleTopPanel_Click(object sender, EventArgs e)
    {
        if (isTopPanelOpen)
        {
            topPanelTargetY = -140;
            isTopPanelOpen = false;
            btnToggleTopPanel.Text = "[  ADD  ]";
        }
        else
        {
            topPanelTargetY = 0;
            isTopPanelOpen = true;
            btnToggleTopPanel.Text = "[ ▲ HIDE ]";
        }
        topPanelAnimationTimer.Start();
    }

    private void TopPanelAnimationTimer_Tick(object sender, EventArgs e)
    {
        int currentY = pnlTopControl.Location.Y;
        int speed = 15;

        if (currentY > topPanelTargetY)
        {
            currentY -= speed;
            if (currentY <= topPanelTargetY) { currentY = topPanelTargetY; topPanelAnimationTimer.Stop(); }
        }
        else if (currentY < topPanelTargetY)
        {
            currentY += speed;
            if (currentY >= topPanelTargetY) { currentY = topPanelTargetY; topPanelAnimationTimer.Stop(); }
        }
        else { topPanelAnimationTimer.Stop(); }

        pnlTopControl.Location = new Point(0, currentY);

        if (isTopPanelOpen)
        {
            pnlDrawerContainer.Location = new Point(25, 150);
            pnlDrawerContainer.Size = new Size(785, 495);
        }
        else
        {
            pnlDrawerContainer.Location = new Point(25, 55);
            pnlDrawerContainer.Size = new Size(785, 590);
        }
    }

    private DrawerItem CreateNewDrawerVisual(string fullName)
    {
        int drawerIndex = cabinetDrawers.Count;

        Panel pnlWrapper = new Panel();
        pnlWrapper.Size = new Size(740, 45);
        pnlWrapper.Location = new Point(5, drawerIndex * 50);
        pnlWrapper.BackColor = Color.Transparent;

        string displayName = fullName;
        if (displayName.Length > 15) displayName = displayName.Substring(0, 12) + "...";

        // Tek bir Paint olayında hem kahverengi tabanı hem de kırmızıyı birleşik çiziyoruz!
        Panel pnlCombinedTab = new Panel();
        pnlCombinedTab.Location = new Point(0, 0);
        pnlCombinedTab.Size = new Size(60, 10);
        pnlCombinedTab.BackColor = Color.Transparent;

        pnlCombinedTab.Paint += (s, e) => {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Kırmızı Alan ve Rampası için Path Oluşturma
            using (GraphicsPath redPath = new GraphicsPath())
            {
                redPath.AddLine(2, 2, 37, 2);   // Düz üst çizgi
                redPath.AddLine(37, 2, 52, 10); // Sağa inen tatlı rampa
                redPath.AddLine(52, 10, 2, 10); // Alt taban
                redPath.CloseFigure();

                // 2. Kahverengi Çerçeve/Arka Plan Alanı için Path Oluşturma (Kırmızıyı tam sarsın)
                using (GraphicsPath brownPath = new GraphicsPath())
                {
                    brownPath.AddLine(0, 0, 37, 0);   // Kırmızının biraz üstünden başlar
                    brownPath.AddLine(37, 0, 54, 10); // Tam kırmızının eğimini takip eden paralel dış eğim
                    brownPath.AddLine(54, 10, 0, 10); // Alt birleşme çizgisi
                    brownPath.CloseFigure();

                    // Önce arkadaki kahverengi kabuğu çiziyoruz
                    using (SolidBrush brownBrush = new SolidBrush(colorCardboardBrown))
                    {
                        e.Graphics.FillPath(brownBrush, brownPath);
                    }
                }

                // Sonra öndeki kırmızı etiketi çiziyoruz
                using (SolidBrush redBrush = new SolidBrush(Color.FromArgb(165, 42, 42)))
                {
                    e.Graphics.FillPath(redBrush, redPath);
                }
            }
        };
        pnlWrapper.Controls.Add(pnlCombinedTab);

        Button btnDrawer = new Button();
        btnDrawer.Text = $"   📁 {displayName.PadRight(16)}  ═════════════════════════════════════";
        btnDrawer.TextAlign = ContentAlignment.MiddleLeft;
        btnDrawer.Font = new Font("Courier New", 11, FontStyle.Bold);
        btnDrawer.Location = new Point(0, 10);
        btnDrawer.Size = new Size(730, 35);
        btnDrawer.BackColor = colorCardboardBrown;
        btnDrawer.ForeColor = Color.FromArgb(245, 235, 215);
        btnDrawer.FlatStyle = FlatStyle.Popup;

        btnDrawer.Click += (s, e) => {
            TriggerPhysicalPush(pnlWrapper);
        };
        pnlWrapper.Controls.Add(btnDrawer);

        Panel pnlFolder = new Panel();
        pnlFolder.Size = new Size(730, 415);
        pnlFolder.Location = new Point(0, 45);
        pnlFolder.BackColor = colorOldFolderBeige;
        pnlFolder.BorderStyle = BorderStyle.FixedSingle;

        Label lblFolderTab = new Label();
        lblFolderTab.Text = $"  📄 {fullName} CONTENTS  ";
        lblFolderTab.Font = new Font("Courier New", 10, FontStyle.Bold);
        lblFolderTab.BackColor = colorFolderTabColor;
        lblFolderTab.ForeColor = Color.FromArgb(60, 45, 20);
        lblFolderTab.Size = new Size(540, 25);
        lblFolderTab.TextAlign = ContentAlignment.MiddleLeft;
        pnlFolder.Controls.Add(lblFolderTab);

        Button btnBurnDrawer = new Button();
        btnBurnDrawer.Text = "[ 🔥 BURN FILE ]";
        btnBurnDrawer.Location = new Point(540, 0);
        btnBurnDrawer.Size = new Size(190, 25);
        btnBurnDrawer.Font = new Font("Courier New", 8, FontStyle.Bold);
        btnBurnDrawer.BackColor = Color.FromArgb(180, 40, 40);
        btnBurnDrawer.ForeColor = Color.White;
        btnBurnDrawer.FlatStyle = FlatStyle.Flat;

        btnBurnDrawer.Click += (s, e) => {
            var onay = MessageBox.Show($"Are you sure you want to BURN '{fullName}'?", "CONFIRM DESTRUCTION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (onay == DialogResult.Yes)
            {
                pnlDrawerContainer.Controls.Remove(pnlWrapper);
                DrawerItem silinecek = cabinetDrawers.Find(x => x.DrawerWrapper == pnlWrapper);
                if (silinecek != null) cabinetDrawers.Remove(silinecek);

                openDrawer = null;
                AnimateCabinetLayout();
                UpdateNavigationUI();
                SaveCabinetData();
            }
        };
        pnlFolder.Controls.Add(btnBurnDrawer);

        ListBox lstLinks = new ListBox();
        lstLinks.Location = new Point(10, 40);
        lstLinks.Size = new Size(710, 210);
        lstLinks.Font = new Font("Courier New", 10, FontStyle.Bold);
        lstLinks.BackColor = Color.FromArgb(248, 242, 225);
        lstLinks.ForeColor = Color.FromArgb(40, 45, 55);
        lstLinks.BorderStyle = BorderStyle.FixedSingle;
        pnlFolder.Controls.Add(lstLinks);

        TextBox txtInfoLabel = new TextBox();
        txtInfoLabel.Multiline = true;
        txtInfoLabel.ScrollBars = ScrollBars.Vertical;
        txtInfoLabel.ReadOnly = true;
        txtInfoLabel.Location = new Point(10, 260);
        txtInfoLabel.Size = new Size(710, 65);
        txtInfoLabel.Font = new Font("Courier New", 9, FontStyle.Bold);
        txtInfoLabel.BackColor = Color.FromArgb(230, 215, 185);
        txtInfoLabel.ForeColor = Color.FromArgb(90, 70, 50);
        txtInfoLabel.BorderStyle = BorderStyle.FixedSingle;
        txtInfoLabel.Text = " NO FILE SELECTED - Select a folder record to read archived notes.";
        pnlFolder.Controls.Add(txtInfoLabel);

        lstLinks.SelectedIndexChanged += (s, e) => {
            if (lstLinks.SelectedItem is LinkRecord selectedRecord)
            {
                string noteText = string.IsNullOrEmpty(selectedRecord.Note) ? "None" : selectedRecord.Note;
                txtInfoLabel.Text = $"[URL]  {selectedRecord.Url}\r\n[NOTE] {noteText}";
            }
            else
            {
                txtInfoLabel.Text = " NO FILE SELECTED - Select a folder record to read archived notes.";
            }
        };

        int btnWidth = 230;
        Button btnLaunch = new Button();
        btnLaunch.Text = "[ 🌐 BROWSE ]";
        btnLaunch.Location = new Point(10, 370);
        btnLaunch.Size = new Size(btnWidth, 35);
        btnLaunch.Font = new Font("Courier New", 9, FontStyle.Bold);
        btnLaunch.BackColor = Color.FromArgb(45, 95, 65);
        btnLaunch.ForeColor = Color.White;
        btnLaunch.FlatStyle = FlatStyle.Flat;
        btnLaunch.Click += (s, e) => {
            if (lstLinks.SelectedItem is LinkRecord record) OpenLinkInBrowser(record.Url);
        };
        pnlFolder.Controls.Add(btnLaunch);

        Button btnLaunchAll = new Button();
        btnLaunchAll.Text = "[ 📁 OPEN BATCH ]";
        btnLaunchAll.Location = new Point(10 + btnWidth + 10, 370);
        btnLaunchAll.Size = new Size(btnWidth, 35);
        btnLaunchAll.Font = new Font("Courier New", 9, FontStyle.Bold);
        btnLaunchAll.BackColor = Color.FromArgb(45, 75, 115);
        btnLaunchAll.ForeColor = Color.FromArgb(215, 175, 105);
        btnLaunchAll.FlatStyle = FlatStyle.Flat;
        btnLaunchAll.Click += (s, e) => {
            foreach (var item in lstLinks.Items) { if (item is LinkRecord record) OpenLinkInBrowser(record.Url); }
        };
        pnlFolder.Controls.Add(btnLaunchAll);

        Button btnDelete = new Button();
        btnDelete.Text = "[ ❌ DELETE ]";
        btnDelete.Location = new Point(10 + (btnWidth * 2) + 20, 370);
        btnDelete.Size = new Size(btnWidth, 35);
        btnDelete.Font = new Font("Courier New", 9, FontStyle.Bold);
        btnDelete.BackColor = Color.FromArgb(160, 45, 45);
        btnDelete.ForeColor = Color.White;
        btnDelete.FlatStyle = FlatStyle.Flat;
        btnDelete.Click += (s, e) => {
            if (lstLinks.SelectedItem is LinkRecord record)
            {
                DrawerItem activeDrawer = cabinetDrawers.Find(x => x.LinkList == lstLinks);
                if (activeDrawer != null) activeDrawer.Links.Remove(record);
                lstLinks.Items.Remove(record);
                txtInfoLabel.Text = " NO FILE SELECTED - Select a folder record to read archived notes.";
                SaveCabinetData();
            }
        };
        pnlFolder.Controls.Add(btnDelete);

        pnlWrapper.Controls.Add(btnDrawer);
        pnlWrapper.Controls.Add(pnlFolder);
        pnlDrawerContainer.Controls.Add(pnlWrapper);

        DrawerItem newItem = new DrawerItem
        {
            DrawerWrapper = pnlWrapper,
            DrawerButton = btnDrawer,
            FolderPanel = pnlFolder,
            LinkList = lstLinks,
            InfoTextBox = txtInfoLabel,
            TargetY = drawerIndex * 50
        };
        cabinetDrawers.Add(newItem);

        AnimateCabinetLayout();
        UpdateNavigationUI();
        return newItem;
    }

    private void TriggerPhysicalPush(Panel clickedWrapper)
    {
        DrawerItem clickedItem = cabinetDrawers.Find(x => x.DrawerWrapper == clickedWrapper);

        if (openDrawer == clickedItem)
        {
            openDrawer = null;
            clickedItem.DrawerButton.BackColor = colorCardboardBrown;
        }
        else
        {
            if (openDrawer != null) openDrawer.DrawerButton.BackColor = colorCardboardBrown;
            openDrawer = clickedItem;
            clickedItem.DrawerButton.BackColor = colorCardboardBrownOpen;
        }

        AnimateCabinetLayout();
        UpdateNavigationUI();
    }

    private void AnimateCabinetLayout()
    {
        int accumulatedY = 0;

        for (int i = 0; i < cabinetDrawers.Count; i++)
        {
            var item = cabinetDrawers[i];
            item.TargetY = accumulatedY;

            if (openDrawer == item)
            {
                item.DrawerWrapper.Size = new Size(740, 460);
                accumulatedY += 465;
            }
            else
            {
                item.DrawerWrapper.Size = new Size(740, 45);
                accumulatedY += 50;
            }
        }

        pushAnimationTimer.Start();
    }

    private void PushAnimationTimer_Tick(object sender, EventArgs e)
    {
        bool allFinished = true;
        int animationSpeed = 25;

        foreach (var item in cabinetDrawers)
        {
            int curY = item.DrawerWrapper.Location.Y;
            int tarY = item.TargetY;

            if (curY != tarY)
            {
                allFinished = false;
                if (Math.Abs(curY - tarY) <= animationSpeed) { item.DrawerWrapper.Location = new Point(5, tarY); }
                else
                {
                    int direction = curY > tarY ? -1 : 1;
                    item.DrawerWrapper.Location = new Point(5, curY + (direction * animationSpeed));
                }
            }
        }

        if (allFinished)
        {
            pushAnimationTimer.Stop();

            if (openDrawer != null)
            {
                int targetScrollY = openDrawer.DrawerWrapper.Location.Y + pnlDrawerContainer.VerticalScroll.Value;
                if (targetScrollY < 0) targetScrollY = 0;
                pnlDrawerContainer.AutoScrollPosition = new Point(0, targetScrollY);
            }
        }
    }

    private void BtnPrevDrawer_Click(object sender, EventArgs e)
    {
        if (cabinetDrawers.Count == 0) return;
        int currentIndex = openDrawer != null ? cabinetDrawers.IndexOf(openDrawer) : 0;
        int prevIndex = currentIndex - 1;
        if (prevIndex >= 0) TriggerPhysicalPush(cabinetDrawers[prevIndex].DrawerWrapper);
    }

    private void BtnNextDrawer_Click(object sender, EventArgs e)
    {
        if (cabinetDrawers.Count == 0) return;
        int currentIndex = openDrawer != null ? cabinetDrawers.IndexOf(openDrawer) : -1;
        int nextIndex = currentIndex + 1;
        if (nextIndex < cabinetDrawers.Count) TriggerPhysicalPush(cabinetDrawers[nextIndex].DrawerWrapper);
    }

    private void UpdateNavigationUI()
    {
        if (cabinetDrawers.Count == 0) { lblNavigationStatus.Text = "0 / 0"; return; }
        int currentIndex = openDrawer != null ? cabinetDrawers.IndexOf(openDrawer) + 1 : 0;
        lblNavigationStatus.Text = $"{currentIndex} / {cabinetDrawers.Count}";
    }

    private void BtnCreateDrawer_Click(object sender, EventArgs e)
    {
        if (isProcessing) return;
        string name = txtCabinetNameGiris.Text.Trim().ToUpper();
        if (string.IsNullOrEmpty(name)) return;

        isProcessing = true;
        DrawerItem item = CreateNewDrawerVisual(name);

        TriggerPhysicalPush(item.DrawerWrapper);

        txtCabinetNameGiris.Clear();
        SaveCabinetData();

        System.Windows.Forms.Timer unblockTimer = new System.Windows.Forms.Timer();
        unblockTimer.Interval = 200;
        unblockTimer.Tick += (s, ev) => { isProcessing = false; unblockTimer.Stop(); unblockTimer.Dispose(); };
        unblockTimer.Start();
    }

    private void BtnAddLink_Click(object sender, EventArgs e)
    {
        string link = txtLinkGiris.Text.Trim();
        string title = txtTitleGiris.Text.Trim();
        string note = txtNoteGiris.Text.Trim();

        if (string.IsNullOrEmpty(link)) return;

        if (openDrawer == null)
        {
            MessageBox.Show("Please open a file first!", "Cabinet Closed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!link.StartsWith("http://") && !link.StartsWith("https://")) link = "https://" + link;

        if (string.IsNullOrEmpty(title)) title = link.Replace("https://", "").Replace("http://", "").Replace("www.", "");

        LinkRecord newRecord = new LinkRecord { Title = title, Url = link, Note = note };

        openDrawer.Links.Add(newRecord);
        openDrawer.LinkList.Items.Add(newRecord);

        txtLinkGiris.Clear();
        txtTitleGiris.Clear();
        txtNoteGiris.Clear();
        txtLinkGiris.Focus();
        SaveCabinetData();
    }

    private void OpenLinkInBrowser(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); } catch { }
    }

    private void SaveCabinetData()
    {
        try
        {
            List<string> linesToSave = new List<string>();
            foreach (var drawer in cabinetDrawers)
            {
                string drawerName = drawer.FolderPanel.Controls[0].Text.Replace("CONTENTS", "").Replace("📄", "").Trim();
                linesToSave.Add("DRAWER:" + drawerName);

                foreach (var item in drawer.LinkList.Items)
                {
                    if (item is LinkRecord rec)
                    {
                        linesToSave.Add($"LINKDATA:{rec.Title}||{rec.Url}||{rec.Note}");
                    }
                }
            }
            File.WriteAllLines(storageFileName, linesToSave);
        }
        catch { }
    }

    private void LoadCabinetData()
    {
        try
        {
            if (!File.Exists(storageFileName)) return;
            string[] lines = File.ReadAllLines(storageFileName);

            cabinetDrawers.Clear();
            pnlDrawerContainer.Controls.Clear();

            DrawerItem lastCreatedDrawer = null;

            foreach (string line in lines)
            {
                if (line.StartsWith("DRAWER:"))
                {
                    string drawerName = line.Replace("DRAWER:", "").Trim();
                    lastCreatedDrawer = CreateNewDrawerVisual(drawerName);
                }
                else if (line.StartsWith("LINKDATA:") && lastCreatedDrawer != null)
                {
                    string rawData = line.Replace("LINKDATA:", "").Trim();
                    string[] tokens = rawData.Split(new string[] { "||" }, StringSplitOptions.None);

                    if (tokens.Length >= 3)
                    {
                        LinkRecord rec = new LinkRecord { Title = tokens[0], Url = tokens[1], Note = tokens[2] };
                        lastCreatedDrawer.Links.Add(rec);
                        lastCreatedDrawer.LinkList.Items.Add(rec);
                    }
                }
            }
            openDrawer = null;
            AnimateCabinetLayout();
            UpdateNavigationUI();
        }
        catch { }
    }
}