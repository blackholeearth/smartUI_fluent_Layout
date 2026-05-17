using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;


using System.Runtime.InteropServices;


namespace SmartLayoutEngine
{

	public  class SmartUI
	{

		// SmartUI sınıfının içine:
		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
		private const int WM_SETREDRAW = 11;


		private  int _currentY = 10;
		private  int _baseMargin = 8;
		private  float _dpiScale = 1f;

		// Zoom engine variables
		private  float _zoomFactor = 1.0f;
		public  int Scale(int value) => (int)(value * _dpiScale * _zoomFactor);

		

		private  Dictionary<Control, float> _originalFonts = new Dictionary<Control, float>();
		private  Dictionary<Control, Size> _originalSizes = new Dictionary<Control, Size>();

		private  Form _form;
		private  List<RowResult> _rows = new List<RowResult>();
		private  List<SmartSidePanel> _sidePanels = new List<SmartSidePanel>();

		//sidebar hanburger.
		private bool _isSidebarCollapsed = false;
		private int _collapseThreshold = 800; // Bu genişliğin altında sidebar gizlenir
		private Button _hamburgerBtn;
		private bool _isSidebarFlyoutOpen = false; // Küçük ekranda sidebar açık mı?

		// Motora hamburger butonunu ve kırılma noktasını tanıtıyoruz
		public void SetupResponsiveSidebar(Button btn, int threshold = 800)
		{
			_hamburgerBtn = btn;
			_collapseThreshold = threshold;

			_hamburgerBtn.Click += (s, e) => {
				_isSidebarFlyoutOpen = !_isSidebarFlyoutOpen;
				RefreshLayout();
			};
		}


		public SmartUI(Form form)
		{
			_form = form;
			_rows.Clear();

			using (Graphics g = form.CreateGraphics()) { 
				_dpiScale = g.DpiX / 96f; 
			}

			// ÇÖZÜM 1: Form tam yüklendiğinde (ekrana çıkarken) layout'u iki kez çalıştırır.
			// Bu, MatchWidth ve AlignRight'ın birbirini beklemesi sorununu (2 resize olayını) kökten çözer!
			_form.Load += (s, e) => {
				RefreshLayout();
				RefreshLayout();
			};

			_form.Resize += (s, e) =>
			{
				RefreshLayout();
			};

			_form.DoubleBuffered();
		}



		public  RowResult Row(params Control[] controls)
		{
			Panel rowPanel = new Panel();
			rowPanel.BackColor = Color.Transparent;
			_form.Controls.Add(rowPanel);

			foreach (var c in controls)
			{
				c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				c.Dock = DockStyle.None;

				// ZOOOM İÇİN KAYDET (Burası güncellendi)
				RegisterForZoom(c);

				rowPanel.Controls.Add(c);
			}

			//var res = new RowResult { Container = rowPanel, ControlsInRow = controls.ToList() };

			var res = new RowResult(this) { Container = rowPanel, ControlsInRow = controls.ToList() };
			_rows.Add(res);

			// Eğer form henüz yüklenmediyse sürekli hesap yapıp yorma
			if (_form.IsHandleCreated) RefreshLayout();

			return res;
		}
 
		private bool _isPerformingLayout = false;
		public void RefreshLayout()
		{
			if (_isPerformingLayout || _form == null || _form.IsDisposed || _form.WindowState == FormWindowState.Minimized)
				return;

			try
			{
				_isPerformingLayout = true;

				// 🌟 NUCLEAR OPTION: Windows OS düzeyinde çizimi tamamen DURDUR
				SendMessage(_form.Handle, WM_SETREDRAW, false, 0);

				// Standart dondurma (İç mantık için yine de kalsın)
				_form.SuspendLayout();
				foreach (var row in _rows) row.Container.SuspendLayout();
				foreach (var sp in _sidePanels) sp.SuspendLayout();

				// --- ASIL MATEMATİK (Core) ---
				CalculateLayoutCore();
			}
			finally
			{
				// Standart çözme
				foreach (var row in _rows) row.Container.ResumeLayout(false);
				foreach (var sp in _sidePanels) sp.ResumeLayout(false);
				_form.ResumeLayout(false);

				// 🌟 NUCLEAR OPTION: Çizimi tekrar AÇ ve her şeyi bir kerede boya
				SendMessage(_form.Handle, WM_SETREDRAW, true, 0);

				// Formu ve tüm çocuklarını "jilet" gibi tek seferde render etmeye zorla
				_form.Refresh();

				_isPerformingLayout = false;
			}
		}
		private void CalculateLayoutCore()
		{
			// --- AŞAMA 0: ÖN HAZIRLIK ---
			foreach (var row in _rows)
				foreach (var c in row.ControlsInRow)
					if (c is SmartGroup sg) Arrange(sg);

			// --- AŞAMA 1: SIDEBAR & HAMBURGER MANTIĞI ---
			bool isNarrow = _form.ClientSize.Width < _collapseThreshold;
			SmartSidePanel leftSidebar = _sidePanels.FirstOrDefault(s => s.Edge == Side.Left);

			if (_hamburgerBtn != null)
			{
				_hamburgerBtn.Visible = isNarrow;
				if (isNarrow)
				{
					_hamburgerBtn.Parent = (_isSidebarFlyoutOpen && leftSidebar != null) ? leftSidebar : _form;
					//_hamburgerBtn.Location = new Point(Scale(10), Scale(10));
					_hamburgerBtn.Location = new Point(Scale(2), Scale(2));
					_hamburgerBtn.BringToFront();
				}
			}

			Rectangle mainArea = new Rectangle(0, 0, _form.ClientSize.Width, _form.ClientSize.Height);

			// Yan Panelleri Konumlandır
			foreach (var sp in _sidePanels)
			{
				if (sp.Edge == Side.Left)
				{
					sp.Visible = !isNarrow || _isSidebarFlyoutOpen;
					int sz = Scale(sp.BaseSize);
					if (sp.Visible)
					{
						sp.Bounds = new Rectangle(0, 0, sz, _form.ClientSize.Height);
						if (!isNarrow) { mainArea.X += sz; mainArea.Width -= sz; }
						else sp.BringToFront();
						ArrangeSideContent(sp, true);
					}
				}
				else if (sp.Edge == Side.Bottom)
				{
					if (!sp.Visible) continue;
					int sz = Scale(sp.BaseSize);
					sp.Bounds = new Rectangle(mainArea.X, _form.ClientSize.Height - sz, mainArea.Width, sz);
					mainArea.Height -= sz;
					ArrangeSideContent(sp, false);
				}
			}

			// --- AŞAMA 2: ÇİFT GEÇİŞLİ (DOUBLE-PASS) DİZİLİM ---
			for (int pass = 0; pass < 2; pass++)
			{
				int totalFixedHeight = Scale(10);
				int growHCount = 0;

				foreach (var row in _rows)
				{
					if (!row.Container.Visible || row.IsGap)
					{
						if (row.IsGap) totalFixedHeight += Scale(row.GapSize);
						continue;
					}
					bool hasGrowH = row.ControlsInRow.Any(c => (c.Tag?.ToString() ?? "").Contains("growH"));
					//totalFixedHeight += 
					//	Scale(row.RawTopMargin) + Scale(row.RawBottomMargin) + Scale(_baseMargin) 
					//	+ row.Container.Padding.Top + row.Container.Padding.Bottom;
					//// YENİ HALİ (Zorunlu baseMargin silindi):
					totalFixedHeight += 
						Scale(row.RawTopMargin) + Scale(row.RawBottomMargin) 
						+ row.Container.Padding.Top + row.Container.Padding.Bottom;

					if (hasGrowH) growHCount++;
					else
					{
						int maxH = 0;
						foreach (var c in row.ControlsInRow) if (!(c.Tag?.ToString() ?? "").Contains("spring")) maxH = Math.Max(maxH, c.Height);
						totalFixedHeight += maxH;
					}
				}

				int heightPerGrow = growHCount > 0 ? Math.Max(10, (mainArea.Height - totalFixedHeight) / growHCount) : 0;
				_currentY = mainArea.Y + Scale(10);

				foreach (var row in _rows)
				{
					if (row.IsGap) { _currentY += Scale(row.GapSize); continue; }
					if (!row.Container.Visible) continue;

					row.Container.Location = new Point(mainArea.X + Scale(row.RawLeftMargin), _currentY + Scale(row.RawTopMargin));
					row.Container.Width = mainArea.Width - Scale(row.RawLeftMargin) - Scale(row.RawRightMargin);

					int fixedW = 0, flexCount = 0;
					foreach (var c in row.ControlsInRow)
					{
						ApplyPaddingLogic(c);
						if ((c.Tag?.ToString() ?? "").Contains("growW") || (c.Tag?.ToString() ?? "").Contains("spring")) flexCount++;
						else fixedW += c.Width + Scale(row.ItemSpacing);
					}

					int availW = row.Container.Width - row.Container.Padding.Left - row.Container.Padding.Right - fixedW - Scale(_baseMargin);
					int flexW = flexCount > 0 ? Math.Max(0, availW / flexCount) : 0;

					int curX = row.Container.Padding.Left + Scale(row.ItemSpacing), maxHInRow = 0;

					foreach (var c in row.ControlsInRow)
					{
						string tag = c.Tag?.ToString() ?? "";
						if (tag.Contains("growW") || tag.Contains("spring"))
						{
							c.Width = flexW;
							if (c is SmartGroup sg) { sg.AutoSize = false; sg.Width = flexW; Arrange(sg); }
						}
						if (tag.Contains("growH")) c.Height = heightPerGrow;

						c.Left = curX; c.Top = row.Container.Padding.Top;

						if (tag.Contains("alignRight:"))
						{
							var target = _form.Controls.Find(tag.Split(';').First(x => x.StartsWith("alignRight:")).Split(':')[1], true).FirstOrDefault();
							if (target != null) c.Left = target.Right - c.Width;
						}

						curX += c.Width + Scale(row.ItemSpacing);
						if (!tag.Contains("spring")) maxHInRow = Math.Max(maxHInRow, c.Height);
					}

					row.Container.Height = maxHInRow + row.Container.Padding.Top + row.Container.Padding.Bottom;

					// VAlign (Dikey Hizalama)
					foreach (var c in row.ControlsInRow)
					{
						string vtag = c.Tag?.ToString() ?? "";
						if (vtag.Contains("vAlign:middle")) c.Top = row.Container.Padding.Top + (maxHInRow - c.Height) / 2;
						else if (vtag.Contains("vAlign:bottom")) c.Top = row.Container.Height - row.Container.Padding.Bottom - c.Height;
					}

					// ESKİ HALİ:
					// _currentY += row.Container.Height + Scale(row.RawTopMargin) + Scale(row.RawBottomMargin) + Scale(_baseMargin);
					// YENİ HALİ (Sen 0 dersen 0 kalır!):
					_currentY += row.Container.Height + Scale(row.RawTopMargin) + Scale(row.RawBottomMargin);

				}
			}

			// Z-Order Final
			if (_hamburgerBtn != null && _hamburgerBtn.Visible) _hamburgerBtn.BringToFront();
		}


		public Control Group(params Control[] controls) => CreateSmartGroup(false, controls);

		// İçine koyulan kontrolleri DİKEYDE (Alt Alta) paketleyen Akıllı Sütun
		public Control Col(params Control[] controls) => CreateSmartGroup(true, controls);

		private Control CreateSmartGroup(bool isVertical, Control[] controls)
		{
			SmartGroup sg = new SmartGroup { LayoutOrder = controls, IsVertical = isVertical };
			sg.DoubleBuffered();

			foreach (var c in controls)
			{
				c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				RegisterForZoom(c);
				sg.Controls.Add(c);
			}
			Arrange(sg); // Tek bir merkezi düzenleyici çağırıyoruz
			return sg;
		}
		public void Arrange(SmartGroup sg)
		{
			if (sg == null) return;
			ApplyPaddingLogic(sg);

			// GrowW olan gruplar AutoSize'ı kapatmalı ki genişliği Row motoru yönetebilsin
			bool isGrow = (sg.Tag?.ToString() ?? "").Contains("growW");
			sg.AutoSize = !isGrow;

			// Grubun İÇ genişliği (Kullanılabilir net alan)
			int innerWidth = sg.Width - sg.Padding.Left - sg.Padding.Right;
			if (innerWidth <= 10) innerWidth = Scale(300); // Başlangıç varsayılanı

			// --- 🌟 YENİ: SPACING (İç Boşluk) OKUYUCU ---
			//int itemSpacing = Scale(_baseMargin / 2); // Varsayılan boşluk
			// zero-based  explicit - no ghost gap:
			int itemSpacing = 0; // 🌟 Sen demezsen araya zerre boşluk girmez!
			string sgTag = sg.Tag?.ToString() ?? "";
			if (sgTag.Contains("spacing:"))
			{
				string val = sgTag.Split(';').First(x => x.StartsWith("spacing:")).Split(':')[1];
				itemSpacing = Scale(int.Parse(val)); // Dışarıdan verilen değeri Scale et
			}

			// --- AŞAMA 1: İÇ ESNEKLİK HESABI (Sadece Yatay Gruplar İçin) ---
			int fixedW = 0, flexCount = 0;
			if (!sg.IsVertical)
			{
				foreach (var c in sg.LayoutOrder)
				{
					if ((c.Tag?.ToString() ?? "").Contains("growW")) flexCount++;
					else fixedW += c.Width + itemSpacing; // Boşluğu yeni itemSpacing'e göre ekle
				}
			}
			int flexW = flexCount > 0 ? Math.Max(0, (innerWidth - fixedW) / flexCount) : innerWidth;

			// --- AŞAMA 2: DİZİLİM ---
			int currentX = sg.Padding.Left;
			int currentY = sg.Padding.Top;
			int maxWidth = 0, maxHeight = 0;

			for (int i = 0; i < sg.LayoutOrder.Length; i++)
			{
				var c = sg.LayoutOrder[i];
				ApplyPaddingLogic(c);
				string tag = c.Tag?.ToString() ?? "";

				// ÇOCUK KONTROLÜN GENİŞLİĞİNİ BELİRLE (Zincirleme Genişlik)
				int childTargetWidth = sg.IsVertical ? innerWidth : (tag.Contains("growW") ? flexW : c.Width);

				if (tag.Contains("growW") || sg.IsVertical)
				{
					c.Width = childTargetWidth;
					if (c is SmartGroup nested)
					{
						nested.AutoSize = false;
						nested.Width = childTargetWidth;
					}
				}

				// 🌟 WRAP LOGIC: Metni yeni genişliğe göre sınırla ve aşağı kaydır
				if (c is Label lbl && tag.Contains("wrap"))
				{
					lbl.AutoSize = false; // Önce kapat ki boyutu biz ezebilelim
					lbl.MaximumSize = new Size(childTargetWidth, 0);
					lbl.AutoSize = true;  // Hesaplama için aç

					// WinForms'u Layout Refresh yapmaya zorlayan o sihirli "dürtme"
					Size preferred = lbl.GetPreferredSize(new Size(childTargetWidth, 0));
					lbl.Size = new Size(childTargetWidth, preferred.Height);
				}

				// REKÜRSİF: İç içe ne varsa şimdi diz (Genişlikler belirlendi!)
				if (c is SmartGroup n) Arrange(n);

				c.Left = currentX;
				c.Top = currentY;

				// 🌟 ARTIK YENİ BOŞLUK KURALI (itemSpacing) GEÇERLİ
				int gap = (i == sg.LayoutOrder.Length - 1) ? 0 : itemSpacing;

				if (sg.IsVertical)
				{
					currentY += c.Height + gap;
					maxWidth = Math.Max(maxWidth, c.Width);
				}
				else
				{
					currentX += c.Width + gap;
					maxHeight = Math.Max(maxHeight, c.Height);
				}
			}

			// --- AŞAMA 3: KASA KAPATMA ---
			sg.Height = (sg.IsVertical ? currentY : maxHeight + sg.Padding.Top) + sg.Padding.Bottom;
			if (!sg.IsVertical && !isGrow) sg.Width = currentX + sg.Padding.Right;

			// --- AŞAMA 4: DİKEY HİZALAMA (Sadece Yatay Gruplar İçin) ---
			if (!sg.IsVertical)
			{
				foreach (var c in sg.LayoutOrder)
				{
					string tag = c.Tag?.ToString() ?? "";
					if (tag.Contains("vAlign:middle"))
						c.Top = sg.Padding.Top + (sg.Height - sg.Padding.Top - sg.Padding.Bottom - c.Height) / 2;
					else if (tag.Contains("vAlign:bottom"))
						c.Top = sg.Height - sg.Padding.Bottom - c.Height;
				}
			}
		}

		public  Control SidePanel(Side edge, int size, params Control[] controls)
		{
			SmartSidePanel sp = new SmartSidePanel
			{
				Edge = edge,
				BaseSize = size,
				Content = controls.ToList()
			};

			sp.DoubleBuffered();

			foreach (var c in controls)
			{
				c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				c.Dock = DockStyle.None;
				sp.Controls.Add(c);
				RegisterForZoom(c); // Zoom sistemine dahil et
			}

			_form.Controls.Add(sp);
			_sidePanels.Add(sp);

			RefreshLayout();
			return sp;
		}
		// SidePanel içindeki kontrolleri dizen yardımcı
		private void ArrangeSideContent(SmartSidePanel sp, bool vertical)
		{
			ApplyPaddingLogic(sp);
			int offset = 0; // 🌟 Eskiden Scale(10) idi, SIFIRLADIK!

			foreach (var c in sp.Content)
			{
				ApplyPaddingLogic(c);
				if (c is SmartGroup sg) Arrange(sg);

				Padding m = GetScaledMargin(c);

				if (vertical)
				{
					c.Location = new Point(m.Left, offset + m.Top);
					if ((c.Tag?.ToString() ?? "").Contains("growW")) c.Width = sp.Width - m.Left - m.Right;

					// 🌟 Eskiden burada + Scale(2) vardı, SIFIRLADIK!
					offset += c.Height + m.Top + m.Bottom;
				}
				else
				{
					c.Top = (sp.Height - c.Height) / 2 + m.Top;
					c.Left = offset + m.Left;
					// 🌟 Eskiden burada + Scale(10) vardı, SIFIRLADIK!
					offset += c.Width + m.Left + m.Right;
				}
			}
		}

		private void ApplyPaddingLogic(Control c)
		{
			string tag = c.Tag?.ToString() ?? "";
			if (tag.Contains("customPad:"))
			{
				// "customPad:10,5,10,5" gibi olan değeri parçala
				string val = tag.Split(';').First(x => x.StartsWith("customPad:")).Split(':')[1];
				string[] parts = val.Split(',');

				if (parts.Length == 1)
				{
					// Tek rakam ise her yöne aynı Scale uygula
					int all = int.Parse(parts[0]);
					c.Padding = new Padding(Scale(all));
				}
				else if (parts.Length == 4)
				{
					// 4 rakam ise L-T-R-B sırasıyla Scale uygula
					c.Padding = new Padding(
						Scale(int.Parse(parts[0])),
						Scale(int.Parse(parts[1])),
						Scale(int.Parse(parts[2])),
						Scale(int.Parse(parts[3]))
					);
				}
			}
		}

		//--zoom function
		public  void SetZoom(float zoomLevel)
		{
			// Zoom sınırları (Çok küçülmesin veya devasa olmasın)
			_zoomFactor = Math.Max(0.5f, Math.Min(3.0f, zoomLevel));

			// 1. Fontları Büyüt
			foreach (var kvp in _originalFonts)
			{
				Control c = kvp.Key;
				c.Font = new Font(c.Font.FontFamily, kvp.Value * _zoomFactor, c.Font.Style);
			}

			// 2. Kasaları (Boyutları) Büyüt
			foreach (var kvp in _originalSizes)
			{
				Control c = kvp.Key;

				// Genişliği Zoom ile çarp (GrowW olanları RefreshLayout zaten ezecek, sorun yok)
				c.Width = (int)(kvp.Value.Width * _zoomFactor);

				// WinForms'ta tek satırlı TextBox'ların yüksekliği Font'a bağlıdır, onlara dokunmuyoruz.
				// Ama buton, label, panel gibi kontrollerin yüksekliğini Zoom ile çarpıyoruz!
				if (!(c is TextBox && !((TextBox)c).Multiline))
				{
					c.Height = (int)(kvp.Value.Height * _zoomFactor);
				}
			}

			// 3. Her şey büyüdüğüne göre, yeni boyutlara göre düzeni tekrar hesapla
			RefreshLayout();
		}
		public  void ZoomIn() => SetZoom(_zoomFactor + 0.1f);
		public  void ZoomOut() => SetZoom(_zoomFactor - 0.1f);
		public  void ResetZoom() => SetZoom(1.0f);
		private  void RegisterForZoom(Control c)
		{
			// Orijinal Font'u kaydet
			if (!_originalFonts.ContainsKey(c))
				_originalFonts[c] = c.Font.Size;

			// Orijinal Kasayı (Size) kaydet (Ama SmartGroup hariç, o dinamiktir!)
			if (!(c is SmartGroup))
			{
				if (!_originalSizes.ContainsKey(c))
					_originalSizes[c] = c.Size;
			}
		}


		// Görünmez bir panel oluşturur. Tek amacı boşlukları sömürmektir.
		public  Control Spring() { return new Panel { Tag = "spring", Width = 0, Height = 0, BackColor = Color.Transparent }; }

		// SidePanel veya Group içinde kullanılacak "boşluk" kontrolü
		public Control Space(int size)
		{
			return new Panel
			{
				Width = Scale(size),
				Height = Scale(size),
				BackColor = Color.Transparent,
				Margin = new Padding(0)
			};
		}
		// Bu metot  "hayalet bir satır" oluşturur.
		public void Gap(int size)
		{
			// Hafızaya "Burada şu kadar piksellik bir boşluk var" diye not düşüyoruz
			var res = new RowResult(this)
			{
				IsGap = true,
				GapSize = size
			};
			_rows.Add(res);

			if (_form.IsHandleCreated) RefreshLayout();
		}


		private Padding GetScaledMargin(Control c)
		{
			string tag = c.Tag?.ToString() ?? "";
			if (tag.Contains("rawMargin:"))
			{
				string val = tag.Split(';').First(x => x.StartsWith("rawMargin:")).Split(':')[1];
				string[] p = val.Split(',');
				if (p.Length == 4)
					return new Padding(Scale(int.Parse(p[0])), Scale(int.Parse(p[1])), Scale(int.Parse(p[2])), Scale(int.Parse(p[3])));
			}
			// Eğer özel margin yoksa, standart baseMargin'i her yöne uygula
			return new Padding(Scale(_baseMargin));
		}
	}

	public class RowResult
	{
		public Panel Container { get; set; }
		public List<Control> ControlsInRow { get; set; }
		public bool IsGap { get; set; } // Bu bir boşluk mu?
		public int GapSize { get; set; } // Boşluk miktarı

		// 🌟 BANANA FIX 1: Varsayılan margin değerlerini burada belirliyoruz.
		public int RawTopMargin = 0;
		public int RawBottomMargin = 0;
		public int RawLeftMargin = 0;
		public int RawRightMargin = 0;

		// Satırın içindeki kontrollerin arasındaki boşluk (Varsayılan 0)
		public int ItemSpacing = 0;

		

		private SmartUI _parent;
		public RowResult(SmartUI parent) { _parent = parent; }


		public RowResult BackColor(Color color) { Container.BackColor = color; return this; }
		public RowResult Border(BorderStyle s = BorderStyle.FixedSingle) { Container.BorderStyle = s; return this; }

		/// <summary>
		/// space between child controls
		/// </summary>
		/// <param name="space"></param>
		/// <returns></returns>
		public RowResult Spacing(int space)
		{
			ItemSpacing = space;
			if (_parent != null) { /* Winforms refresh */ } // veya direkt RefreshLayout çağır
			return this;
		}

		public RowResult Padding(int all)
		{
			Container.Padding = new Padding(_parent.Scale(all));
			if (_form()?.IsHandleCreated == true) _parent.RefreshLayout();
			return this;
		}
		public RowResult Padding(int l, int t, int r, int b)
		{
			Container.Padding = new Padding(_parent.Scale(l), _parent.Scale(t), _parent.Scale(r), _parent.Scale(b));
			if (_form()?.IsHandleCreated == true) _parent.RefreshLayout();
			return this;
		}

		public RowResult Margin(int all)
		{
			RawLeftMargin = RawTopMargin = RawRightMargin = RawBottomMargin = all;
			if (_form()?.IsHandleCreated == true) _parent.RefreshLayout();
			return this;
		}
		public RowResult Margin(int l, int t, int r, int b)
		{
			RawLeftMargin = l; RawTopMargin = t; RawRightMargin = r; RawBottomMargin = b;
			if (_form()?.IsHandleCreated == true) _parent.RefreshLayout();
			return this;
		}

		public RowResult Visible(bool v)
		{
			Container.Visible = v;
			if (_form()?.IsHandleCreated == true) _parent.RefreshLayout();
			return this;
		}


		public  RowResult VAlignMiddle()
		{
			foreach (var c in ControlsInRow) 
				c.VAlignMiddle();
			_parent.RefreshLayout();
			return this;
		}

		public  RowResult VAlignBottom()
		{
			foreach (var c in ControlsInRow) 
				c.VAlignBottom();
			_parent.RefreshLayout();
			return this;
		}

		// C# null check helper for internal use
		private Form _form() => Container?.FindForm();
	}

	// Standart panelden tek farkı: İçine hangi sırayla kontrol eklendiğini unutmaz!
	public class SmartGroup : Panel
	{
		public Control[] LayoutOrder { get; set; }
		public bool IsVertical { get; set; }

		public SmartGroup()
		{
			BackColor = Color.Transparent;
		}
	}

	public enum Side { Left, Right, Bottom }

	public class SmartSidePanel : Panel
	{
		public Side Edge { get; set; }
		public int BaseSize { get; set; } // Orijinal genişlik/yükseklik (Zoom için)
		public List<Control> Content { get; set; }

		public SmartSidePanel() { BackColor = Color.FromArgb(40, 40, 40); }
	}

	public static class UIExtensions
	{
		private static void AddTag(Control c, string rule)
		{
			string current = c.Tag?.ToString() ?? "";
			if (!current.Contains(rule)) c.Tag = current + (current == "" ? "" : ";") + rule;
		}

		public static Control GrowW(this Control c) { AddTag(c, "growW"); return c; }
		public static Control GrowH(this Control c) { AddTag(c, "growH"); return c; }
		public static Control AlignRight(this Control c, Control target) { AddTag(c, "alignRight:" + target.Name); return c; }
		public static Control MatchWidth(this Control c, Control target) { AddTag(c, "matchWidth:" + target.Name); return c; }


		// Dikey (Vertical) Hizalama Seçenekleri
		public static Control VAlignMiddle(this Control c) { AddTag(c, "vAlign:middle"); return c; }
		public static Control VAlignBottom(this Control c) { AddTag(c, "vAlign:bottom"); return c; }

		// UI Extensions içine ekle
		public static Control WrapText(this Control c)
		{
			string t = c.Tag?.ToString() ?? "";
			if (!t.Contains("wrap")) c.Tag = t + (t == "" ? "" : ";") + "wrap";
			return c;
		}

		//----non layout things
		public static Control BackColor(this Control c, Color color) { c.BackColor = color; return c; }

		public static Control Margin(this Control c, int l, int t, int r, int b)
		{
			// Değerleri ham (raw) olarak saklıyoruz, motor Scale edecek.
			string tag = c.Tag?.ToString() ?? "";
			string rule = $"rawMargin:{l},{t},{r},{b}";
			if (!tag.Contains("rawMargin:"))
				c.Tag = tag + (tag == "" ? "" : ";") + rule;
			return c;
		}

		// Kolaylık olsun diye tek rakam versiyonu
		public static Control Margin(this Control c, int all) => c.Margin(all, all, all, all);

		// Tek rakam: Her yöne aynı (All)
		public static Control Padding(this Control c, int l, int t, int r, int b)
		{
			AddTag(c, $"customPad:{l},{t},{r},{b}");
			return c;
		}

		public static Control Padding(this Control c, int all)
		{
			AddTag(c, "customPad:" + all);
			return c;
		}


		/// <summary>
		/// Kutunun (Group veya Col) içindeki elemanların arasındaki boşluğu (Gap) ayarlar.
		/// <para></para> Padding gibi ama elemaların arası boşluk için
		/// </summary>
		public static Control Spacing(this Control c, int space)
		{
			string t = c.Tag?.ToString() ?? "";
			string rule = "spacing:" + space;
			if (!t.Contains("spacing:"))
				c.Tag = t + (t == "" ? "" : ";") + rule;
			return c;
		}

	}
}