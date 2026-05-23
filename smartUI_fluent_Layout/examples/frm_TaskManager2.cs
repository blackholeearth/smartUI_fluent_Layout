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
	// 🌟 Sınıf ismi ve yapısı istediğiniz gibi partial class olarak güncellendi.
	public partial class frm_TaskManager2 : Form
	{
		private SmartUI ui;
		private SmartSidePanel leftSidePanel;
		private Panel _sidebarSpacer;
		private bool _isSidebarExpanded = false; // Kapalı (Resim 1) başlar

		// 🌟 Kullanıcının kolon boyutlarını sürükleyebileceği modern Win11 tablosu
		private DataGridView dgvProcesses;

		public frm_TaskManager2()
		{
			this.Text = "Görev Yöneticisi";
			this.Size = new Size(1000, 700);
			this.MinimumSize = new Size(800, 500);
			this.BackColor = Color.FromArgb(243, 243, 243); // Win11 Açık Tema Arka Planı

			ui = new SmartUI(this);

			// --- 1. SIDEBAR KURULUMU (Hamburger Toggles) ---
			leftSidePanel = (SmartSidePanel)ui.SidePanel(Side.Left, 60, new Control[] { });
			leftSidePanel.BackColor = Color.FromArgb(249, 249, 249);
			PopulateSidebar(leftSidePanel);

			//// Alttaki Ayarlar butonunun form boyutuna göre konumunu koruması için
			//this.Resize += (s, e) => {
			//	if (_sidebarSpacer != null)
			//	{
			//		int itemsHeight = _isSidebarExpanded ? 460 : 410;
			//		_sidebarSpacer.Height = Math.Max(10, this.ClientSize.Height - itemsHeight);
			//	}
			//};

			// --- 2. ÜST ARAMA VE AKSİYON ALANI ---
			TextBox txtSearch = new TextBox
			{
				BorderStyle = BorderStyle.None,
				Font = new Font("Segoe UI", 9.5f),
				Width = 260,
				BackColor = Color.White
			};
			// Eklediğiniz managed placeholder desteğiyle arama metnini bağlıyoruz
			//txtSearch.Placeholder("Aramak için bir ad, yayımcı veya PID girin...");

			Panel searchContainer = new Panel()
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
			Button btnMore = CreateHeaderButton("\uE712", "");

			var headerRow = ui.Row(
				searchContainer.VAlignMiddle(),
				ui.Spring(), // Geri kalan elemanları en sağa iter
				btnNewTask.VAlignMiddle(),
				ui.Space(6),
				btnEndTask.VAlignMiddle(),
				ui.Space(6),
				btnEcoMode.VAlignMiddle(),
				ui.Space(6),
				btnMore.VAlignMiddle()
			)
			.Margin(16, 12, 16, 6);

			// --- 3. AKILLI TABLONUN (GRID) BAĞLANMASI ---
			SetupProcessGrid();

			// Tabloyu SmartUI Row içine gömüp GrowH ve GrowW veriyoruz.
			// Böylece form büyüdükçe tablo da otomatik esneyecektir.
			ui.Row(dgvProcesses.GrowW().GrowH())
			  .Margin(16, 4, 16, 16);

			// Verileri yükle
			LoadMockData();
		}

		// --- MODERN WIN11 DATA GRID VIEW TASARIMI ---
		private void SetupProcessGrid()
		{
			dgvProcesses = new DataGridView
			{
				BackgroundColor = Color.White,
				BorderStyle = BorderStyle.None,
				CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
				GridColor = Color.FromArgb(240, 240, 240),
				RowHeadersVisible = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				MultiSelect = false,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				AllowUserToOrderColumns = true, // Sürükleyerek kolon yerini değiştirebilme
				AllowUserToResizeRows = false,
				RowTemplate = { Height = 36 },
				EnableHeadersVisualStyles = false,
				ColumnHeadersHeight = 34,
				ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
			};

			// 🌟 BANANA HACK: Çift arabelleğe almayı (Double Buffering) açıyoruz ki sürüklerken veya kaydırırken hiç titremesin!
			dgvProcesses.DoubleBuffered();

			// Kolon Başlığı Stilleri (Windows 11 Tarzı Hafif Gri)
			dgvProcesses.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);
			dgvProcesses.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(32, 32, 32);
			dgvProcesses.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f);
			dgvProcesses.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(249, 249, 249);
			dgvProcesses.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

			// Hücre Stilleri
			dgvProcesses.DefaultCellStyle.BackColor = Color.White;
			dgvProcesses.DefaultCellStyle.ForeColor = Color.FromArgb(32, 32, 32);
			dgvProcesses.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
			dgvProcesses.DefaultCellStyle.SelectionBackColor = Color.FromArgb(234, 244, 252);
			dgvProcesses.DefaultCellStyle.SelectionForeColor = Color.Black;

			// Kolonları el yordamıyla ve resizable (boyutlandırılabilir) olarak ekliyoruz:
			var colName = new DataGridViewTextBoxColumn { Name = "Ad", HeaderText = "Ad", Width = 280, MinimumWidth = 100 };
			var colStatus = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Durum", Width = 90, MinimumWidth = 50 };
			var colCpu = new DataGridViewTextBoxColumn { Name = "CPU", HeaderText = "% CPU", Width = 100, MinimumWidth = 60 };
			var colMem = new DataGridViewTextBoxColumn { Name = "Mem", HeaderText = "% Bellek", Width = 110, MinimumWidth = 60 };
			var colDisk = new DataGridViewTextBoxColumn { Name = "Disk", HeaderText = "% Disk", Width = 110, MinimumWidth = 60 };
			var colNet = new DataGridViewTextBoxColumn { Name = "Net", HeaderText = "% Ağ", Width = 100, MinimumWidth = 60 };

			// Verileri hizala
			colCpu.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			colMem.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			colDisk.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			colNet.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

			dgvProcesses.Columns.AddRange(colName, colStatus, colCpu, colMem, colDisk, colNet);

			// 🌟 Canlı Kaynak Boyama Olayı (GDI+ Custom Paint)
			dgvProcesses.CellPainting += DgvProcesses_CellPainting;
		}

		// --- GÖREV YÖNETİCİSİ RENK YOĞUNLUĞU VE ÖZEL ÇİZİM SİSTEMİ ---
		private void DgvProcesses_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex < 0) return; // Başlık satırını boyama

			// Sadece CPU, Bellek, Disk ve Ağ kolonlarını tonda boyayacağız (2, 3, 4, 5. kolonlar)
			if (e.ColumnIndex >= 2 && e.ColumnIndex <= 5)
			{
				string cellValue = e.Value?.ToString() ?? "";
				double intensity = CalculateIntensity(e.ColumnIndex, cellValue);

				if (intensity > 0)
				{
					e.Handled = true; // WinForms varsayılan çizimini durdur

					Color baseBlue = Color.FromArgb(0, 120, 212); // Windows 11 Mavisi
					Color cellColor = Color.FromArgb((int)(intensity * 210 + 25), baseBlue); // Yoğunluğa göre şeffaflık tonu

					// Arka planı boya
					using (SolidBrush bgBrush = new SolidBrush(cellColor))
					{
						e.Graphics.FillRectangle(bgBrush, e.CellBounds);
					}

					// Altındaki ve yanındaki grid çizgilerini çiz
					using (Pen gridPen = new Pen(dgvProcesses.GridColor))
					{
						e.Graphics.DrawLine(gridPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
						e.Graphics.DrawLine(gridPen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);
					}

					// Yazıyı çiz
					Color textColor = intensity > 0.4 ? Color.White : Color.Black; // Çok koyu tonlarda yazı beyaza döner
					TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
					TextRenderer.DrawText(e.Graphics, cellValue, e.CellStyle.Font, e.CellBounds, textColor, flags);
				}
			}
			// Eco sütununda yeşil yaprak simgesini basma
			else if (e.ColumnIndex == 1)
			{
				string cellValue = e.Value?.ToString() ?? "";
				if (cellValue == "Eco")
				{
					e.Handled = true;
					e.PaintBackground(e.CellBounds, true);

					string leafIcon = "\uE70E";
					Font iconFont = new Font("Segoe Fluent Icons", 9.5f);
					if (iconFont.Name != "Segoe Fluent Icons") iconFont = new Font("Segoe MDL2 Assets", 9.5f);

					TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
					TextRenderer.DrawText(e.Graphics, leafIcon, iconFont, e.CellBounds, Color.FromArgb(0, 138, 0), flags);
				}
			}
		}

		// --- DEĞERE GÖRE TENSION (KOYULUK) ORANI HESAPLAYICI ---
		private double CalculateIntensity(int colIdx, string val)
		{
			if (string.IsNullOrEmpty(val)) return 0;
			try
			{
				string cleanVal = val.Replace("%", "").Replace("MB/sn", "").Replace("MB", "").Trim();
				if (double.TryParse(cleanVal, out double num))
				{
					if (colIdx == 2) // CPU için: %100'e göre ölçekle
					{
						if (num == 0) return 0;
						return Math.Min(1.0, num / 100.0 + 0.05);
					}
					else if (colIdx == 3) // Bellek için: 1500MB üzerini koyu yapalım
					{
						if (num > 1500) return 0.85; // Firefox gibi
						if (num > 100) return 0.25;
						if (num > 10) return 0.08;
						return 0.02;
					}
					else if (colIdx == 4) // Disk için: 5 MB/sn üzerini koyulaştır
					{
						if (num > 5) return 0.60; // System gibi
						if (num > 0) return 0.15;
					}
				}
			}
			catch { }
			return 0;
		}

		// --- VERİ DOLDURUCU (MOCK DATA) ---
		private void LoadMockData()
		{
			dgvProcesses.Rows.Add("zerotier-one_64.exe", "", "%0", "4,3 MB", "0 MB/sn", "0 MB/sn");
			dgvProcesses.Rows.Add("Başlat (2)", "", "%0", "4,3 MB", "0 MB/sn", "0 MB/sn");
			dgvProcesses.Rows.Add("Firefox (27)", "", "%0,5", "1.675,9 MB", "0,1 MB/sn", "0 MB/sn");
			dgvProcesses.Rows.Add("GitHubDesktop.exe (4)", "Eco", "%0,1", "141,2 MB", "0,2 MB/sn", "0 MB/sn");
			dgvProcesses.Rows.Add("Sublime Text (4)", "", "%0", "11,5 MB", "0 MB/sn", "0 MB/sn");
			dgvProcesses.Rows.Add("System", "", "%1,4", "0,1 MB", "5,6 MB/sn", "0 MB/sn");
			dgvProcesses.Rows.Add("Paint (2)", "", "%0", "143,9 MB", "0,1 MB/sn", "0 MB/sn");
			dgvProcesses.Rows.Add("Everything", "", "%0", "60,5 MB", "0 MB/sn", "0 MB/sn");
		}

		// --- SIDEBAR POPULATE METODU ---
		public static SmartSidePanel Add( SmartSidePanel sp, Control c)
		{
			if (sp.Content == null) sp.Content = new List<Control>();

			sp.Controls.Add(c);
			sp.Content.Add(c);
			return sp;
		}

		private void PopulateSidebar(SmartSidePanel sp)
		{
			// 1. Hamburger Butonu
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

			sp.BaseSize = _isSidebarExpanded ? 230 : 48;
			btnHamburger.Click += (s, e) => {
				_isSidebarExpanded = !_isSidebarExpanded;

				// Genişlik Windows 11 standartlarına göre 48px'e çekildi:
				sp.BaseSize = _isSidebarExpanded ? 230 : 48;
				//sp.BaseSize = _isSidebarExpanded ? 230 : 60;
				sp.Controls.Clear();
				sp.Content.Clear();
				PopulateSidebar(sp);

				ui.RefreshLayout();
			};

			// Yeni AddContent metodunu kullanıyoruz
			sp.AddContent(btnHamburger);
			sp.AddContent(ui.Space(8));

			// Menü Elemanları
			var menuItems = new List<(string Icon, string Text, bool IsSelected)> {
				("\uE990", "İşlemler", true),
				("\uE9D9", "Performans", false),
				("\uE81C", "Uygulama geçmişi", false),
				(SegoeMDL2Icons.SpeedHigh, "Başlangıç uygulamaları", false),
				("\uE716", "Kullanıcılar", false),
				("\uE14C", "Ayrıntılar", false),
				(SegoeMDL2Icons.Puzzle, "Hizmetler", false)
			};

			foreach (var item in menuItems)
			{
				// 🌟 Kendi v1 metodumuzu tüm parametreleriyle evrensel olarak çağırıyoruz
				var sidebarBtn = ui.SidebarItem_v2(item.Icon, item.Text, item.IsSelected, _isSidebarExpanded, showIndicator: true);
				sp.AddContent(sidebarBtn);
			}

			// 🌟 Pürüzsüz dikey yay sistemi
			sp.AddContent(ui.Spring());

			// En alttaki Ayarlar butonu (Gösterge istemediğimiz için showIndicator: false)
			var settingsBtn = ui.SidebarItem_v2("\uE713", "Ayarlar", false, _isSidebarExpanded, showIndicator: false);
			sp.AddContent(settingsBtn);
		}
 
		private Control CreateSidebarItem(string icon, string text, bool isSelected, bool isExpanded)
		{
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
				Visible = isExpanded
			};

			var group = ui.Group(
				indicator/*.Nudge(0, 4)*/,
				ui.Space(8),
				ico.VAlignMiddle().BackColor(Color.LightCoral),
			ui.Space(12).Visible(isExpanded),
			lbl.VAlignMiddle().Visible(isExpanded)

			)
			.GrowW()
			.Padding(0,10,0,10)
			.Margin(4, 1, 4, 1)
			.Rounded(4)
			.BackColor(Color.OrangeRed);

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


	}
}