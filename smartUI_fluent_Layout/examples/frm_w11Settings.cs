using SmartLayoutEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace smartUI_fluent_Layout
{
	public partial class frm_w11Settings : Form
	{
		// Sidebar Kontrolleri
		private Label lblUser, lblEmail, lblHome, lblSystem, lblBluetooth, lblNetwork;
		private PictureBox imgProfile;
		private TextBox txtSearch;
		// İçerik Başlıkları
		private Label lblBreadcrumb, lblPageTitle, lblSectionBrightness, lblSectionScale;
		// Kart 1: Parlaklık
		private Label icoBrightness, lblBrightnessTitle, lblBrightnessDesc;
		private TrackBar trackBrightness;
		// Kart 2: Gece Işığı
		private Label icoNight, lblNightTitle, lblNightDesc;
		private Button btnToggleNight;
		// Kart 3: HDR
		private Label icoHDR, lblHDRTitle, lblHDRDesc, icoArrowHDR;
		// Kart 4: Ölçek
		private ComboBox cmbScale;


		private SmartUI ui;
		 
		public frm_w11Settings()
		{
			InitializeComponent();

			var sw = new Stopwatch();
			sw.Start();

			BuildUI__W11ModernSettings();

			sw.Stop();

			Text += " -- BuildUI__W11ModernSettings: " + sw.ElapsedMilliseconds+" ms";
		}


		private void BuildUI__W11ModernSettings()
		{
			// Windows 11 Light Theme colors
			Color winBg = Color.FromArgb(243, 243, 243); // Main Bg
			Color winSidebar = Color.FromArgb(238, 238, 238); // Sidebar Bg
			Color winCard = Color.White; // Setting card Bg
			Color winText = Color.FromArgb(32, 32, 32);
			Color winSubText = Color.Gray;
			Color BorderColor = Color.FromArgb(229, 229, 229);

			this.Font = new Font("Segoe UI Variable Display", 10); // Windows 11 Fontu
			this.BackColor = winBg;
			this.Text = "Ayarlar";
			this.Size = new Size(1100, 700);

			// --- 1. create UI Controls in code (or just drag drop em into designer ) ---
			InitControls();

			// --- 2. let SMARTUI layout the controls  relative style.( the ART ) ---
			ui = new SmartUI(this);

			// 1. Create Menu Button.
			Button myBurger = new() {
				Text = SegoeMDL2Icons.HamburgerMenu,  
				Font = SegoeMDL2Icons._Font12F, 
				FlatStyle = FlatStyle.Flat, Size = new Size(40, 40)
			};
			Flat_NoBorder(myBurger);
			this.Controls.Add(myBurger); // add it directly to form. 

			// 2. tell SmartUI to use this button to open close SideBar .
			ui.SetupResponsiveSidebar(myBurger, 850);


			// LEFT SIDEBAR
			ui.SidePanel(Side.Left, 280,
				ui.Group( 
					imgProfile, 
					ui.Col(lblUser, lblEmail).VAlignMiddle()
				).VAlignMiddle().Padding(20),

				txtSearch.GrowW(),
				ui.Space(10),
				ui.CreateSidebarItem_v1(SegoeMDL2Icons.Home, "Giriş"),
				ui.CreateSidebarItem_v1(SegoeMDL2Icons.System, "Sistem", isSelected: true).BackColor(winCard),
				ui.CreateSidebarItem_v1(SegoeMDL2Icons.Bluetooth, "Bluetooth ve cihazlar"),
				ui.CreateSidebarItem_v1(SegoeMDL2Icons.Globe, "Ağ ve internet")  /*  "\uE774" */
			).BackColor(winSidebar)
			.Padding(16,0,0,0);



			// RİGHT Content
			ui.Row(lblPageTitle).Margin(30 - 10, 0, 0, 10);

			// BÖLÜM 1: Brightness
			//ui.Row(lblSectionBrightness).Margin(30, 10, 0, 10);
			ui.SmartUI_SectionHeader_v1("Parlaklık ve renk");

			ui.SmartUI_CardView_v1( icoBrightness, lblBrightnessTitle, lblBrightnessDesc, trackBrightness);
			ui.SmartUI_CardView_v1( icoNight,      lblNightTitle, lblNightDesc, btnToggleNight );
			ui.SmartUI_CardView_v1( icoHDR,        lblHDRTitle, lblHDRDesc, icoArrowHDR );

			// BÖLÜM 2: Scale
			//ui.Row(lblSectionScale).Margin(30, 10 + 16, 0, 10);
			ui.SmartUI_SectionHeader_v1("Ölçek ve düzen");

			// Scaling Card
			var cmbScale = new ComboBox { Width = 150 }; cmbScale.Items.Add("125% (Rcommended)"); cmbScale.SelectedIndex = 0;
			ui.SmartUI_CardView_v1( SegoeMDL2Icons.ResizeMouseMedium , "Ölçek", "Metin, uygulama ve diğer öğelerin boyutunu değiştir", cmbScale );

			ui.SmartUI_Divider_v1(); 
			ui.SmartUI_AlertBox_v1( SegoeMDL2Icons.Info, "Windows Update güncellemeleri duraklatıldı.", Color.LightBlue, Color.DarkBlue);
			

		}

		private static void Flat_NoBorder( Button myBurger)
		{
			myBurger.FlatStyle = FlatStyle.Flat;
			myBurger.FlatAppearance.BorderSize = 0;
		}

		private void InitControls()
		{
			// 1. EKRANI KOMPLE TEMİZLE (Eskileri çöpe at)
			this.Controls.Clear();

			// Fontlar
			Font mainFont = new Font("Segoe UI Variable Display", 10);
			Font boldFont = new Font("Segoe UI Variable Display", 10, FontStyle.Bold);
			Font iconFont = new Font("Segoe MDL2 Assets", 12);

			// Profil
			imgProfile = new PictureBox { 
				Size = new Size(60, 60), 
				BackColor = Color.LightGray,
			};
			System.Drawing.Drawing2D.GraphicsPath gp = new ();
			gp.AddEllipse(0, 0, 60, 60); 
			imgProfile.Region = new Region(gp); 
			

			lblUser = new Label { Text = "Some DEV", Font = boldFont, AutoSize = true };
			lblEmail = new Label { Text = "somedev@mail.com", Font = new Font(mainFont.FontFamily, 8), ForeColor = Color.Gray, AutoSize = true };
 

			// Sidebar
			txtSearch = new TextBox { Text = "Bir ayar bulun", Width = 240, BackColor = Color.White, Font = mainFont };
			lblHome = new Label(); lblSystem = new Label(); lblBluetooth = new Label(); lblNetwork = new Label();

			// Başlıklar
			//lblBreadcrumb = new Label { Text = "Sistem  >  Ekran", ForeColor = Color.Gray, Font = mainFont, AutoSize = true };
			lblPageTitle = new Label { Text = "Sistem  >  Ekran", Font = new Font("Segoe UI Variable Display", 22, FontStyle.Bold), AutoSize = true };
			lblSectionBrightness = new Label { Text = "Parlaklık ve renk", Font = boldFont, AutoSize = true };
			lblSectionScale = new Label { Text = "Ölçek ve düzen", Font = boldFont, AutoSize = true };

			// Kart 1: Parlaklık
			icoBrightness = new Label { Text = "", Font = iconFont, AutoSize = true };
			lblBrightnessTitle = new Label { Text = "Parlaklık", Font = boldFont, AutoSize = true };
			lblBrightnessDesc = new Label { Text = "Yerleşik ekranın parlaklığını ayarla", ForeColor = Color.Gray, AutoSize = true };
			trackBrightness = new TrackBar { Width = 200, Minimum = 0, Maximum = 100, Value = 70 , TickStyle = TickStyle.None,
				Height = 22,
				AutoSize = false,
			};
			

			// Kart 2: Gece Işığı
			icoNight = new Label { Text = "", Font = iconFont, AutoSize = true };
			lblNightTitle = new Label { Text = "Gece ışığı", Font = boldFont, AutoSize = true };
			lblNightDesc = new Label { Text = "Mavi ışığı engellemeye yardımcı olması için daha sıcak renkler kullanın", ForeColor = Color.Gray, AutoSize = true };
			btnToggleNight = new Button { Text = "Kapalı", Width = 80, FlatStyle = FlatStyle.System };

			// Kart 3: HDR
			icoHDR = new Label { Text = "", Font = iconFont, AutoSize = true };
			lblHDRTitle = new Label { Text = "HDR", Font = boldFont, AutoSize = true };
			lblHDRDesc = new Label { Text = "HDR hakkında daha fazla bilgi", ForeColor = Color.DarkBlue, AutoSize = true };
			icoArrowHDR = new Label { Text = "", Font = iconFont, AutoSize = true };

			
		}


		////  --- Composite Controls . Reusable. i did this for Example .
		////   you can do it to .. share it here. if its general purpose.
		//public RowResult SmartUI_CardView_v1(Label lbl_icon, Label lbl_title, Label lbl_desc, Control Control_atRightSide)
		//{
		//	return
		//		ui.Row
		//		(
		//			ui.Group
		//			(
		//				lbl_icon.Padding(0, 0, 10, 0).VAlignMiddle().BackColor(Color.Transparent),
		//				ui.Col(
		//					lbl_title
		//						/*.BackColor(Color.Orange)*/,
		//					lbl_desc.WrapText()
		//					//.BackColor(Color.Green)
		//					).GrowW().Padding(0)
		//			).VAlignMiddle().Padding(0).GrowW(),
		//			ui.Space(12),
		//			Control_atRightSide.VAlignMiddle()
		//		)
		//		.BackColor(Color.White).Padding(18).Margin(30, 0, 30, 4)
		//		.Rounded(8, Color.FromArgb(229, 229, 229))
		//		 ;
		//}

		///// <summary>
		///// 
		///// </summary>
		///// <param name="iconCode">Segoe MDL2 Assets -  PUA icon codes</param>
		///// <param name="lbl_title"></param>
		///// <param name="lbl_desc"></param>
		///// <param name="Control_atRightSide"></param>
		///// <returns></returns>
		//public RowResult SmartUI_CardView_v1(string iconCode, string title, string desc, Control Control_atRightSide)
		//{
		//	// Fontlar
		//	Font mainFont = new Font("Segoe UI Variable Display", 10);
		//	Font boldFont = new Font("Segoe UI Variable Display", 10, FontStyle.Bold);
		//	Font iconFont = new Font("Segoe MDL2 Assets", 12);

		//	var lbl_icon = new Label { Text = iconCode, Font = iconFont, AutoSize = true };
		//	var lbl_title = new Label { Text = title, Font = boldFont, AutoSize = true };
		//	var lbl_desc = new Label { Text = desc, ForeColor = Color.Gray, AutoSize = true };


		//	return SmartUI_CardView_v1(lbl_icon, lbl_title, lbl_desc, Control_atRightSide);

		//	//return
		//	//	ui.Row
		//	//	(
		//	//		ui.Group
		//	//		(
		//	//			lbl_icon.Padding(0, 0, 10, 0).VAlignMiddle().BackColor(Color.Transparent),
		//	//			ui.Col(lbl_title, lbl_desc.WrapText()).GrowW().Padding(0)
		//	//		).VAlignMiddle().Padding(0).GrowW(),
		//	//		ui.Space(12),
		//	//		Control_atRightSide.VAlignMiddle()
		//	//	)
		//	//	.BackColor(Color.White).Padding(12).Margin(30, 0, 30, 4);
		//}

		//// Sidebar öğesi oluşturmak için yardımcı (Tekrardan kaçınmak usta işidir)
		//private Control CreateSidebarItem_v1(string iconCode, string text, bool isSelected = false)
		//{
		//	Label ico = new Label
		//	{
		//		Text = iconCode,
		//		// Windows 11 ise "Segoe Fluent Icons", Windows 10 ise "Segoe MDL2 Assets"
		//		Font = new Font("Segoe Fluent Icons", 12),
		//		AutoSize = true,
		//		BackColor = Color.Transparent
		//	};

		//	// Eğer font hala yüklenmiyorsa Windows'un yedeğine (MD2) düşelim
		//	if (ico.Font.Name != "Segoe Fluent Icons")
		//		ico.Font = new Font("Segoe MDL2 Assets", 12);

		//	Label lbl = new Label
		//	{
		//		Text = text,
		//		Font = new Font("Segoe UI Variable Display", 10, isSelected ? FontStyle.Bold : FontStyle.Regular),
		//		AutoSize = true,
		//		BackColor = Color.Transparent
		//	};


		//	var group = ui.Group(ico, lbl)
		//		 .GrowW()
		//		 .VAlignMiddle()
		//		 .Padding(10, 12, 10, 12)
		//		 .Margin(0);

		//	if (isSelected) group.BackColor(Color.White);


		//	// 🌟 HOVER MANTIĞINI BURADA TANIMLIYORUZ (Fonksiyon olarak)
		//	Action turnOnHover = () => {
		//		if (!isSelected) group.BackColor = Color.FromArgb(230, 230, 230);
		//	};

		//	Action turnOffHover = () => {
		//		if (!isSelected) group.BackColor = Color.Transparent;
		//	};

		//	// 1. Grubun kendi olayları
		//	group.MouseEnter += (s, e) => turnOnHover();
		//	group.MouseLeave += (s, e) => turnOffHover();

		//	// 2. TIKLAMA: Çocukların (İkon/Yazı) olaylarını Gruba yönlendir
		//	foreach (Control child in group.Controls)
		//	{
		//		child.MouseEnter += (s, e) => turnOnHover();
		//		child.MouseLeave += (s, e) => turnOffHover();

		//		// Opsiyonel: Çocuklara tıklandığında grubun Click eventini tetiklemek istersen:
		//		// child.Click += (s, e) => group_Click(group, e); 
		//	}

		//	return group;

		//}



		private void Form3_Load(object sender, EventArgs e)
		{

		}











	}
}
