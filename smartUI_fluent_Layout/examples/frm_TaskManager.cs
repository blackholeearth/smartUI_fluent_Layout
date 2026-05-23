using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SmartLayoutEngine;

namespace smartUI_fluent_Layout.examples
{
	public partial class frm_TaskManager : Form
	{
		private SmartUI ui;
		private SmartSidePanel leftSidePanel;
		private Panel _sidebarSpacer;
		private bool _isSidebarExpanded = false; // Resim 1 gibi kapalı başlar

		public frm_TaskManager()
		{
			this.Text = "Görev Yöneticisi";
			this.Size = new Size(1000, 700);
			this.MinimumSize = new Size(800, 500);
			this.BackColor = Color.FromArgb(243, 243, 243); // Win11 Açık Tema Arka Planı

			ui = new SmartUI(this);

			// --- 1. SİLİNMEYEN SIDEBAR TANIMLAMASI ---
			leftSidePanel = (SmartSidePanel)ui.SidePanel(Side.Left, 60, new Control[] { });
			leftSidePanel.BackColor = Color.FromArgb(249, 249, 249); // Sidebar rengi

			// İlk yüklemede sidebar elemanlarını doldur
			PopulateSidebar(leftSidePanel);

			// Form yeniden boyutlandığında alttaki Ayarlar butonunun konumunu korumak için spacer yüksekliğini güncelle
			this.Resize += (s, e) => {
				if (_sidebarSpacer != null)
				{
					int itemsHeight = _isSidebarExpanded ? 460 : 410;
					_sidebarSpacer.Height = Math.Max(10, this.ClientSize.Height - itemsHeight);
				}
			};

			// --- 2. ÜST ARAMA VE AKSİYON SATIRI ---
			TextBox txtSearch = new TextBox
			{
				BorderStyle = BorderStyle.None,
				Font = new Font("Segoe UI", 9.5f),
				Width = 260,
				BackColor = Color.White
			};
			//.Placeholder("Aramak için bir ad, yayımcı veya PID girin...");

			Panel searchContainer = new Panel
			{
				Width = 290,
				Height = 32,
				BackColor = Color.White
			};

			searchContainer.Rounded(6, Color.FromArgb(218, 218, 218));

			txtSearch.Location = new Point(10, 7);
			searchContainer.Controls.Add(txtSearch);

			Button btnNewTask = CreateHeaderButton("\uE710", "Yeni görevi çalıştır");
			Button btnEndTask = CreateHeaderButton("\uE711", "Görevi sonlandır");
			Button btnEcoMode = CreateHeaderButton("\uE70E", "Verimlilik modu");
			Button btnMore = CreateHeaderButton("\uE712", ""); // Üç nokta

			var headerRow = ui.Row(
				searchContainer.VAlignMiddle(),
				ui.Spring(), // Aksiyon butonlarını en sağa yaslamak için flexbox spacer
				btnNewTask.VAlignMiddle(),
				ui.Space(6),
				btnEndTask.VAlignMiddle(),
				ui.Space(6),
				btnEcoMode.VAlignMiddle(),
				ui.Space(6),
				btnMore.VAlignMiddle()
			)
			.Margin(16, 12, 16, 6);

			// --- 3. TABLO BAŞLIĞI (COLUMN HEADERS) ---
			var gridHeader = ui.Row(
				new Label { Text = "Ad", Font = new Font("Segoe UI Semibold", 9), AutoSize = true },
				new Label { Text = "% CPU", Font = new Font("Segoe UI Semibold", 9), AutoSize = true, TextAlign = ContentAlignment.MiddleCenter },
				new Label { Text = "% Bellek", Font = new Font("Segoe UI Semibold", 9), AutoSize = true, TextAlign = ContentAlignment.MiddleCenter },
				new Label { Text = "% Disk", Font = new Font("Segoe UI Semibold", 9), AutoSize = true, TextAlign = ContentAlignment.MiddleCenter },
				new Label { Text = "% Ağ", Font = new Font("Segoe UI Semibold", 9), AutoSize = true, TextAlign = ContentAlignment.MiddleCenter }
			)
			.Padding(12, 6, 12, 6)
			.Margin(16, 4, 16, 2)
			.BackColor(Color.FromArgb(249, 249, 249))
			.Rounded(4, Color.FromArgb(230, 230, 230));

			// --- 4. GÖREV LİSTESİ SATIRLARI (MOCK DATA) ---
			// CPU, Bellek ve Disk yoğunluk oranlarına göre hücreler otomatik Windows 11 mavisiyle boyanır.
			ProcessRow("zerotier-one_64.exe", "", "%0", "4,3 MB", "0 MB/sn", "0 MB/sn", 0.0, 0.02, 0.0);
			ProcessRow("Başlat (2)", "", "%0", "4,3 MB", "0 MB/sn", "0 MB/sn", 0.0, 0.02, 0.0);
			ProcessRow("Firefox (27)", "", "%0", "1.675,9 MB", "0,1 MB/sn", "0 MB/sn", 0.0, 0.91, 0.01); // %91 Bellek koyu mavi!
			ProcessRow("GitHubDesktop.exe (4)", "Eco", "%0,1", "141,2 MB", "0,2 MB/sn", "0 MB/sn", 0.01, 0.15, 0.02); // Eco yaprak ikonlu
			ProcessRow("Sublime Text (4)", "", "%0", "11,5 MB", "0 MB/sn", "0 MB/sn", 0.0, 0.05, 0.0);
			ProcessRow("System", "", "%1,4", "0,1 MB", "5,6 MB/sn", "0 MB/sn", 0.05, 0.0, 0.55); // %5.6 Disk hafif mavi
			ProcessRow("Paint (2)", "", "%0", "143,9 MB", "0,1 MB/sn", "0 MB/sn", 0.0, 0.15, 0.01);
			ProcessRow("Everything", "", "%0", "60,5 MB", "0 MB/sn", "0 MB/sn", 0.0, 0.08, 0.0);
		}

		// --- SIDEBAR İÇERİĞİNİ OLUŞTURAN METOT ---
		private void PopulateSidebar(SmartSidePanel sp)
		{
			// 1. Hamburger Menü Butonu
			Button btnHamburger = new Button
			{
				Text = "\uE700",
				Font = new Font("Segoe Fluent Icons", 11),
				Width = 36,
				Height = 36,
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.Transparent
			};
			if (btnHamburger.Font.Name != "Segoe Fluent Icons") btnHamburger.Font = new Font("Segoe MDL2 Assets", 11);
			btnHamburger.FlatAppearance.BorderSize = 0;
			btnHamburger.Rounded(4);
			btnHamburger.HoverBackColor(Color.FromArgb(230, 230, 230));

			// Hamburger tıklama olayı
			btnHamburger.Click += (s, e) => {
				_isSidebarExpanded = !_isSidebarExpanded;
				sp.BaseSize = _isSidebarExpanded ? 230 : 60; // Genişliği esnet

				// Sidebar'ı temizle ve yeni duruma göre tekrar çiz
				sp.Controls.Clear();
				sp.Content.Clear();
				PopulateSidebar(sp);

				ui.RefreshLayout();
			};

			sp.Controls.Add(btnHamburger);
			sp.Content.Add(btnHamburger);
			sp.Content.Add(ui.Space(8));
			sp.Controls.Add(ui.Space(8));

			// Menü Elemanları Tanımlaması (Simge, Metin, Seçili mi?)
			var menuItems = new List<(string Icon, string Text, bool IsSelected)> {
				("\uE990", "İşlemler", true),
				("\uE9D9", "Performans", false),
				("\uE81C", "Uygulama geçmişi", false),
				("\uE7B3", "Başlangıç uygulamaları", false),
				("\uE716", "Kullanıcılar", false),
				("\uE14C", "Ayrıntılar", false),
				("\uE713", "Hizmetler", false)
			};

			foreach (var item in menuItems)
			{
				var sidebarBtn = CreateSidebarItem(item.Icon, item.Text, item.IsSelected, _isSidebarExpanded);
				sp.Controls.Add(sidebarBtn);
				sp.Content.Add(sidebarBtn);
			}

			// Settings'i en alta itmek için dinamik boşluk paneli
			int currentItemsHeight = _isSidebarExpanded ? 460 : 410;
			_sidebarSpacer = new Panel
			{
				Width = 10,
				Height = Math.Max(10, this.ClientSize.Height - currentItemsHeight),
				BackColor = Color.Transparent
			};
			sp.Controls.Add(_sidebarSpacer);
			sp.Content.Add(_sidebarSpacer);

			// En Alttaki Ayarlar Butonu
			var settingsBtn = CreateSidebarItem("\uE713", "Ayarlar", false, _isSidebarExpanded);
			sp.Controls.Add(settingsBtn);
			sp.Content.Add(settingsBtn);
		}

		// --- SIDEBAR ELEMANI GENERATORU ---
		private Control CreateSidebarItem(string icon, string text, bool isSelected, bool isExpanded)
		{
			// Seçili elemanın solundaki küçük mavi dikey bar
			Panel indicator = new Panel
			{
				Width = 3,
				Height = 16,
				BackColor = isSelected ? Color.FromArgb(0, 103, 192) : Color.Transparent
			};
			indicator.Rounded(1);

			Label ico = new Label
			{
				Text = icon,
				Font = new Font("Segoe Fluent Icons", 11),
				AutoSize = true,
				BackColor = Color.Transparent,
				ForeColor = Color.FromArgb(32, 32, 32)
			};
			if (ico.Font.Name != "Segoe Fluent Icons") ico.Font = new Font("Segoe MDL2 Assets", 11);

			Label lbl = new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 9f, isSelected ? FontStyle.Bold : FontStyle.Regular),
				AutoSize = true,
				BackColor = Color.Transparent,
				ForeColor = Color.FromArgb(32, 32, 32),
				Visible = isExpanded // Sidebar kapalıyken metin gizlenir!
			};

			var group = ui.Group(
				indicator, // Mavi barı görsel olarak simgeyle dikeyde ortala
				ui.Space(8),
				ico.VAlignMiddle(),
				ui.Space(12),
				lbl.VAlignMiddle()
			)
			.GrowW()
			.Padding(2, 6, 2, 6)
			.Margin(4, 1, 4, 1)
			.Rounded(4);

			if (isSelected)
			{
				group.BackColor(Color.FromArgb(234, 234, 234));
			}
			else
			{
				group.HoverBackColor(Color.FromArgb(243, 243, 243));
			}

			return group;
		}

		// --- ÜST AKSİYON BUTONU YAPICI ---
		private Button CreateHeaderButton(string icon, string text)
		{
			Button btn = new Button
			{
				Text = string.IsNullOrEmpty(text) ? icon : $"{icon}  {text}",
				Font = new Font("Segoe Fluent Icons", 9f),
				Height = 32,
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.White,
				ForeColor = Color.FromArgb(32, 32, 32),
				AutoSize = true
			};
			if (btn.Font.Name != "Segoe Fluent Icons") btn.Font = new Font("Segoe MDL2 Assets", 9f);

			btn.FlatAppearance.BorderSize = 0;
			btn.Rounded(6, Color.FromArgb(218, 218, 218));
			btn.HoverBackColor(Color.FromArgb(245, 245, 245));

			return btn;
		}

		// --- TABLO SATIR OLUŞTURUCU (MOCK PROCESS ROW) ---
		private void ProcessRow(string name, string status, string cpu, string memory, string disk, string network, double cpuLoad, double memLoad, double diskLoad)
		{
			Label lblName = new Label { Text = name, Font = new Font("Segoe UI", 9), AutoSize = true };

			// Eğer Eco mod aktifse yeşil yaprak ikonu göster
			Label lblStatus = new Label
			{
				Text = status == "Eco" ? "\uE70E" : "",
				Font = new Font("Segoe Fluent Icons", 9.5f),
				ForeColor = Color.FromArgb(0, 138, 0),
				AutoSize = true
			};
			if (lblStatus.Font.Name != "Segoe Fluent Icons") lblStatus.Font = new Font("Segoe MDL2 Assets", 9.5f);

			Control cellCpu = CreateResourceCell(cpu, cpuLoad);
			Control cellMem = CreateResourceCell(memory, memLoad);
			Control cellDisk = CreateResourceCell(disk, diskLoad);
			Control cellNet = CreateResourceCell(network, 0.0); // Network genelde sıfır

			ui.Row(
				ui.Group(lblName, ui.Space(6), lblStatus.VAlignMiddle()).VAlignMiddle(),
				cellCpu,
				cellMem,
				cellDisk,
				cellNet
			)
			.Padding(12, 4, 12, 4)
			.Margin(16, 1, 16, 1)
			.BackColor(Color.White)
			.Rounded(4, Color.FromArgb(240, 240, 240));
		}

		// --- KAYNAK YOĞUNLUĞUNA GÖRE BOYALIK HÜCRE ÜRETİCİ ---
		private Control CreateResourceCell(string text, double intensity)
		{
			Color baseBlue = Color.FromArgb(0, 103, 192); // Win11 Accent Blue

			Label lbl = new Label
			{
				Text = text,
				Font = new Font("Segoe UI", 9, intensity > 0.4 ? FontStyle.Bold : FontStyle.Regular),
				ForeColor = intensity > 0.4 ? Color.White : Color.Black,
				TextAlign = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Fill,
				BackColor = Color.Transparent
			};

			Panel cell = new Panel
			{
				Height = 24,
				// Yoğunluğa göre saydamlığı (alfa değerini) ayarlayarak mavinin tonunu ekiyoruz
				BackColor = intensity > 0
					? Color.FromArgb((int)(intensity * 230 + 25), baseBlue)
					: Color.Transparent
			};
			cell.Controls.Add(lbl);
			return cell;
		}
	}
}