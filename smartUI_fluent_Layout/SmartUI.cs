using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D; // Bunu dosyanın en üstüne eklemeyi unutma!

namespace SmartLayoutEngine;

// --- 🌟 KURAL MOTORU (RAM Dostu, String kullanmaz) ---
public class LayoutProps
{
	public bool GrowW { get; set; }
	public bool GrowH { get; set; }
	public bool Spring { get; set; }
	public bool WrapText { get; set; }
	public int VAlign { get; set; } // 0: Top, 1: Middle, 2: Bottom
	public Control MatchWidthTarget { get; set; }
	public Control AlignRightTarget { get; set; }
	public Padding? CustomPadding { get; set; }
	public Padding? CustomMargin { get; set; }
	public int? ItemSpacing { get; set; }


	// 🌟 YENİ: Yuvarlak Köşe ve Çerçeve Kuralları
	public int? CornerRadius { get; set; }
	public Color? BorderColor { get; set; }
	public float BorderThickness { get; set; }
	public bool IsRoundedHooked { get; set; } // Olayı (Event) iki kez bağlamamak
}

public partial class SmartUI
{
	[DllImport("user32.dll")]
	private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
	private const int WM_SETREDRAW = 11;

	private Form _form;
	private float _dpiScale = 1f;
	private float _zoomFactor = 1.0f;

	private List<RowResult> _rows = new List<RowResult>();
	private List<SmartSidePanel> _sidePanels = new List<SmartSidePanel>();

	private Dictionary<Control, float> _originalFonts = new Dictionary<Control, float>();
	private Dictionary<Control, Size> _originalSizes = new Dictionary<Control, Size>();

	private Button _hamburgerBtn;
	private int _collapseThreshold = 800;
	private bool _isSidebarFlyoutOpen = false;
	private bool _isPerformingLayout = false;

	public SmartUI(Form form)
	{
		_form = form;
		using (Graphics g = form.CreateGraphics())
		{
			_dpiScale = g.DpiX / 96f;
		}

		_form.Load += (s, e) => { RefreshLayout(); RefreshLayout(); };
		_form.Resize += (s, e) => RefreshLayout();
		_form.DoubleBuffered();
	}

	public int Scale(int value) => (int)(value * _dpiScale * _zoomFactor);

	private void RegisterForZoom(Control c)
	{
		if (!_originalFonts.ContainsKey(c)) _originalFonts[c] = c.Font.Size;
		if (!(c is SmartGroup) && !_originalSizes.ContainsKey(c)) _originalSizes[c] = c.Size;
	}

	public void SetupResponsiveSidebar(Button btn, int threshold = 800)
	{
		_hamburgerBtn = btn;
		_collapseThreshold = threshold;
		_hamburgerBtn.Click += (s, e) => {
			_isSidebarFlyoutOpen = !_isSidebarFlyoutOpen;
			RefreshLayout();
		};
	}

	// --- YAPISAL METOTLAR ---
	public RowResult Row(params Control[] controls)
	{
		// ESKİDEN: Panel rowPanel = new Panel();
		// ŞİMDİ: Satırımız da artık zeki bir SmartGroup!
		SmartGroup rowPanel = new SmartGroup
		{
			LayoutOrder = controls.ToList(),
			IsVertical = false,
			BackColor = Color.Transparent,
			Margin = new Padding(0)
		};

		// Kasa küçülmesin, formun sonuna kadar uzasın diye GrowW veriyoruz
		rowPanel.GetProps().GrowW = true;
		_form.Controls.Add(rowPanel);

		foreach (var c in controls)
		{
			c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			c.Dock = DockStyle.None;
			c.Margin = new Padding(0);
			RegisterForZoom(c);
			rowPanel.Controls.Add(c);
		}

		var res = new RowResult(this) { Container = rowPanel, ControlsInRow = controls.ToList() };
		_rows.Add(res);
		if (_form.IsHandleCreated) RefreshLayout();
		return res;
	}



	public void Gap(int size)
	{
		_rows.Add(new RowResult(this) { IsGap = true, GapSize = size });
		if (_form.IsHandleCreated) RefreshLayout();
	}

	/// <summary>
	/// expands fills the space.
	/// </summary>
	/// <returns></returns>
	public Control Spring()
	{
		var p = new Panel { Width = 0, Height = 0, BackColor = Color.Transparent, Margin = new Padding(0) };
		p.GetProps().Spring = true; // Kural motoruna bu bir "Yay" diyoruz
		return p;
	}

	public Control Space(int size)
	{
		return new Panel { Width = Scale(size), Height = Scale(size), BackColor = Color.Transparent, Margin = new Padding(0) };
	}

	public Control SidePanel(Side edge, int size, params Control[] controls)
	{
		SmartSidePanel sp = new SmartSidePanel { Edge = edge, BaseSize = size, Content = controls.ToList(), Margin = new Padding(0) };
		foreach (var c in controls)
		{
			c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			c.Margin = new Padding(0);
			sp.Controls.Add(c);
			RegisterForZoom(c);
		}
		_form.Controls.Add(sp);
		_sidePanels.Add(sp);
		RefreshLayout();
		return sp;
	}

	public Control Group(params Control[] controls) => CreateSmartGroup(false, controls);
	public Control Col(params Control[] controls) => CreateSmartGroup(true, controls);

	private Control CreateSmartGroup(bool isVertical, Control[] controls)
	{
		SmartGroup sg = new SmartGroup { LayoutOrder = controls.ToList(), IsVertical = isVertical, Margin = new Padding(0) };
		foreach (var c in controls)
		{
			c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			c.Dock = DockStyle.None;
			c.Margin = new Padding(0);
			RegisterForZoom(c);
			sg.Controls.Add(c);
		}
		Arrange(sg);
		return sg;
	}
	 
	// --- ZOM MOTORU ---
	public void SetZoom(float zoomLevel)
	{
		_zoomFactor = Math.Max(0.5f, Math.Min(3.0f, zoomLevel));
		foreach (var kvp in _originalFonts)
			kvp.Key.Font = new Font(kvp.Key.Font.FontFamily, kvp.Value * _zoomFactor, kvp.Key.Font.Style);

		foreach (var kvp in _originalSizes)
		{
			kvp.Key.Width = (int)(kvp.Value.Width * _zoomFactor);
			if (!(kvp.Key is TextBox && !((TextBox)kvp.Key).Multiline))
				kvp.Key.Height = (int)(kvp.Value.Height * _zoomFactor);
		}
		RefreshLayout();
	}
	public void ZoomIn() => SetZoom(_zoomFactor + 0.1f);
	public void ZoomOut() => SetZoom(_zoomFactor - 0.1f);
	public void ResetZoom() => SetZoom(1.0f);

	// --- DİZİLİM YARDIMCILARI ---

	private void ApplyPaddingLogic(Control c)
	{
		var props = c.GetProps();
		if (props.CustomPadding.HasValue)
		{
			var p = props.CustomPadding.Value;
			c.Padding = new Padding(Scale(p.Left), Scale(p.Top), Scale(p.Right), Scale(p.Bottom));
		}
	}

	private Padding GetScaledMargin(Control c)
	{
		var props = c.GetProps();
		if (props.CustomMargin.HasValue)
		{
			var m = props.CustomMargin.Value;
			return new Padding(Scale(m.Left), Scale(m.Top), Scale(m.Right), Scale(m.Bottom));
		}
		return new Padding(0); // Zero-Base!
	}

	// --- İÇ İÇE (NESTED) DÜZENLEYİCİ ---
	public void Arrange(SmartGroup sg)
	{
		if (sg == null) return;
		ApplyPaddingLogic(sg);

		var sgProps = sg.GetProps();
		sg.AutoSize = !sgProps.GrowW;

		int innerWidth = sg.Width - sg.Padding.Left - sg.Padding.Right;
		if (innerWidth <= 10) innerWidth = Scale(300);

		int itemSpacing = sgProps.ItemSpacing.HasValue ? Scale(sgProps.ItemSpacing.Value) : 0;

		// 🌟 YENİ: Genişlik Eşitleme (MatchWidth)
		foreach (var c in sg.LayoutOrder)
		{
			var p = c.GetProps();
			if (p.MatchWidthTarget != null) c.Width = p.MatchWidthTarget.Width;
		}

		// AŞAMA 1: İÇ ESNEKLİK HESABI
		int fixedW = 0, flexCount = 0;
		if (!sg.IsVertical)
		{
			foreach (var c in sg.LayoutOrder)
			{
				var p = c.GetProps();
				if (p.GrowW || p.Spring) flexCount++;
				else fixedW += c.Width + itemSpacing;
			}
		}
		int flexW = flexCount > 0 ? Math.Max(0, (innerWidth - fixedW) / flexCount) : innerWidth;

		// AŞAMA 2: DİZİLİM
		int currentX = sg.Padding.Left;
		int currentY = sg.Padding.Top;
		int maxWidth = 0, maxHeight = 0;

		for (int i = 0; i < sg.LayoutOrder.Count; i++)
		{
			var c = sg.LayoutOrder[i];
			ApplyPaddingLogic(c);
			var props = c.GetProps();

			int childTargetWidth = sg.IsVertical ? innerWidth : ((props.GrowW || props.Spring) ? flexW : c.Width);

			if (props.GrowW || props.Spring || sg.IsVertical)
			{
				c.Width = childTargetWidth;
				if (c is SmartGroup nested) { nested.AutoSize = false; nested.Width = childTargetWidth; }
			}

			if (c is Label lbl && props.WrapText)
			{
				lbl.AutoSize = false;
				lbl.MaximumSize = new Size(childTargetWidth, 0);
				lbl.AutoSize = true;
				Size preferred = lbl.GetPreferredSize(new Size(childTargetWidth, 0));
				lbl.Size = new Size(childTargetWidth, preferred.Height);
			}

			// REKÜRSİF: İç içe dizilim
			if (c is SmartGroup n) Arrange(n);

			c.Left = currentX;
			c.Top = currentY;

			// 🌟 YENİ: Sağa Hizalama (AlignRight)
			if (props.AlignRightTarget != null)
			{
				c.Left = props.AlignRightTarget.Right - c.Width;
			}

			int gap = (i == sg.LayoutOrder.Count - 1) ? 0 : itemSpacing;

			if (sg.IsVertical)
			{
				currentY += c.Height + gap;
				maxWidth = Math.Max(maxWidth, c.Width);
			}
			else
			{
				currentX += c.Width + gap;
				if (!props.Spring) maxHeight = Math.Max(maxHeight, c.Height); // Yaylar yüksekliği etkilemez
			}
		}

		sg.Height = (sg.IsVertical ? currentY : maxHeight + sg.Padding.Top) + sg.Padding.Bottom;
		if (!sg.IsVertical && !sgProps.GrowW) sg.Width = currentX + sg.Padding.Right;

		// AŞAMA 3: DİKEY HİZALAMA
		if (!sg.IsVertical)
		{
			foreach (var c in sg.LayoutOrder)
			{
				var p = c.GetProps();
				if (p.VAlign == 1) c.Top = sg.Padding.Top + (sg.Height - sg.Padding.Top - sg.Padding.Bottom - c.Height) / 2;
				else if (p.VAlign == 2) c.Top = sg.Height - sg.Padding.Bottom - c.Height;
			}
		}
	}

	private void ArrangeSideContent(SmartSidePanel sp, bool vertical)
	{
		ApplyPaddingLogic(sp); // Sidebar'ın kendi padding'ini bas (20px)

		// 🌟 BAŞLANGIÇ NOKTASI: Artık 0 değil, Sidebar'ın üst/sol padding'inden başlıyoruz!
		int offset = vertical ? sp.Padding.Top : sp.Padding.Left;

		foreach (var c in sp.Content)
		{
			ApplyPaddingLogic(c);
			if (c is SmartGroup sg) Arrange(sg);

			Padding m = GetScaledMargin(c);

			if (vertical) // Sol/Sağ Sidebar
			{
				// 🌟 KONUM: Sidebar Padding'i + Kontrolün Margin'i
				c.Location = new Point(sp.Padding.Left + m.Left, offset + m.Top);

				// 🌟 GENİŞLİK SIKIŞTIRMASI: Sidebar'ın sağ/sol padding'ini de hesaptan düş!
				if (c.GetProps().GrowW)
					c.Width = sp.Width - sp.Padding.Left - sp.Padding.Right - m.Left - m.Right;

				// Bir sonraki eleman için Y eksenini kaydır
				offset += c.Height + m.Top + m.Bottom;
			}
			else // Alt Panel (Bottom Bar)
			{
				// Dikeyde tam ortalama ama Padding.Top ve Bottom'ı hesaba katarak
				int availH = sp.Height - sp.Padding.Top - sp.Padding.Bottom;
				c.Top = sp.Padding.Top + (availH - c.Height) / 2 + m.Top;

				c.Left = offset + m.Left;
				offset += c.Width + m.Left + m.Right;
			}
		}
	}
	// --- 🌟 NÜKLEER REFRESH LAYOUT MOTORU ---
	public void RefreshLayout()
	{
		// 1. GÜVENLİK KONTROLÜ
		if (_isPerformingLayout || _form == null || _form.IsDisposed || _form.WindowState == FormWindowState.Minimized)
			return;

		try
		{
			_isPerformingLayout = true;

			// 🌟 SESSİZ OPERASYON: Win32 ile ekranı dondur
			if (_form.IsHandleCreated) SendMessage(_form.Handle, WM_SETREDRAW, false, 0);

			_form.SuspendLayout();
			foreach (var row in _rows) if (row.Container != null) row.Container.SuspendLayout();
			foreach (var sp in _sidePanels) sp.SuspendLayout();

			RefreshLayoutCore();
		}
		finally
		{
			// 🌟 EKRANI SERBEST BIRAK VE ÇİZ
			foreach (var row in _rows) if (row.Container != null) row.Container.ResumeLayout(false);
			foreach (var sp in _sidePanels) sp.ResumeLayout(false);
			_form.ResumeLayout(false);

			if (_form.IsHandleCreated)
			{
				SendMessage(_form.Handle, WM_SETREDRAW, true, 0);
				_form.Refresh();
			}
			_isPerformingLayout = false;
		}
	}

	private void RefreshLayoutCore()
	{
		// --- AŞAMA 1: RESPONSIVE SIDEBAR & HAMBURGER ---
		bool isNarrow = _form.ClientSize.Width < _collapseThreshold;
		SmartSidePanel leftSidebar = _sidePanels.FirstOrDefault(s => s.Edge == Side.Left);

		if (_hamburgerBtn != null)
		{
			_hamburgerBtn.Visible = isNarrow;
			if (isNarrow)
			{
				_hamburgerBtn.Parent = (_isSidebarFlyoutOpen && leftSidebar != null) ? leftSidebar : _form;
				_hamburgerBtn.Location = new Point(Scale(10), Scale(10));
				_hamburgerBtn.BringToFront();
			}
		}

		Rectangle mainArea = new Rectangle(0, 0, _form.ClientSize.Width, _form.ClientSize.Height);

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
			int totalFixedHeight = 0;
			int growHCount = 0;

			// Dikey boşluk hesaplama (GrowH)
			foreach (var row in _rows)
			{
				if (!row.Container.Visible || row.IsGap)
				{
					if (row.IsGap) totalFixedHeight += Scale(row.GapSize);
					continue;
				}

				bool hasGrowH = row.ControlsInRow.Any(c => c.GetProps().GrowH);
				totalFixedHeight += Scale(row.RawTopMargin) + Scale(row.RawBottomMargin) + row.Container.Padding.Top + row.Container.Padding.Bottom;

				if (hasGrowH) growHCount++;
				else
				{
					int maxH = 0;
					foreach (var c in row.ControlsInRow) if (!c.GetProps().Spring) maxH = Math.Max(maxH, c.Height);
					totalFixedHeight += maxH;
				}
			}

			int heightPerGrow = growHCount > 0 ? Math.Max(10, (mainArea.Height - totalFixedHeight) / growHCount) : 0;
			int currentY = mainArea.Y;

			// --- AŞAMA 3: SATIRLARI DİZ (JİLET VERSİYON) ---
			foreach (var row in _rows)
			{
				if (row.IsGap) { currentY += Scale(row.GapSize); continue; }
				if (!row.Container.Visible) continue;

				// 🌟 Satırımız artık bir SmartGroup!
				SmartGroup sgRow = (SmartGroup)row.Container;

				// 1. Satırın dış kasasını (Genişlik ve Konum) belirle
				sgRow.Location = new Point(mainArea.X + Scale(row.RawLeftMargin), currentY + Scale(row.RawTopMargin));
				sgRow.Width = mainArea.Width - Scale(row.RawLeftMargin) - Scale(row.RawRightMargin);

				// 2. Satır içindeki GrowH elemanlarının yüksekliğini set et
				foreach (var c in row.ControlsInRow)
				{
					if (c.GetProps().GrowH) c.Height = heightPerGrow;
				}

				// 3. EFSANE HAMLE: Tüm esneklik, VAlign, Hizalama hesabını Arrange yapsın!
				Arrange(sgRow);

				// 4. Sonraki satıra geç (Yüksekliği Arrange metodu buldu)
				currentY += sgRow.Height + Scale(row.RawTopMargin) + Scale(row.RawBottomMargin);
			}
		}

		// Z-ORDER FİNAL
		if (_hamburgerBtn != null && _hamburgerBtn.Visible) _hamburgerBtn.BringToFront();
	}


}

// --- composite Kontrols ---
public partial class SmartUI
{
	//  --- Composite Controls . Reusable. i did this for Example .
	//   you can do it to .. share it here. if its general purpose.
	public RowResult SmartUI_CardView_v1(Label lbl_icon, Label lbl_title, Label lbl_desc, Control Control_atRightSide)
	{
		return
			this.Row
			(
				this.Group
				(
					lbl_icon.Padding(0, 0, 10, 0).VAlignMiddle().BackColor(Color.Transparent),
					this.Col(
						lbl_title
							/*.BackColor(Color.Orange)*/,
						lbl_desc.WrapText()
						//.BackColor(Color.Green)
						).GrowW().Padding(0)
				).VAlignMiddle().Padding(0).GrowW(),
				this.Space(12),
				Control_atRightSide.VAlignMiddle()
			)
			.BackColor(Color.White).Padding(18).Margin(30, 0, 30, 4)
			.Rounded(8, Color.FromArgb(229, 229, 229))
			 ;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="iconCode">Segoe MDL2 Assets -  PUA icon codes</param>
	/// <param name="lbl_title"></param>
	/// <param name="lbl_desc"></param>
	/// <param name="Control_atRightSide"></param>
	/// <returns></returns>
	public RowResult SmartUI_CardView_v1(string iconCode, string title, string desc, Control Control_atRightSide)
	{
		// Fontlar
		Font mainFont = new Font("Segoe UI Variable Display", 10);
		Font boldFont = new Font("Segoe UI Variable Display", 10, FontStyle.Bold);
		Font iconFont = new Font("Segoe MDL2 Assets", 12);

		var lbl_icon = new Label { Text = iconCode, Font = iconFont, AutoSize = true };
		var lbl_title = new Label { Text = title, Font = boldFont, AutoSize = true };
		var lbl_desc = new Label { Text = desc, ForeColor = Color.Gray, AutoSize = true };


		return SmartUI_CardView_v1(lbl_icon, lbl_title, lbl_desc, Control_atRightSide);

		//return
		//	ui.Row
		//	(
		//		ui.Group
		//		(
		//			lbl_icon.Padding(0, 0, 10, 0).VAlignMiddle().BackColor(Color.Transparent),
		//			ui.Col(lbl_title, lbl_desc.WrapText()).GrowW().Padding(0)
		//		).VAlignMiddle().Padding(0).GrowW(),
		//		ui.Space(12),
		//		Control_atRightSide.VAlignMiddle()
		//	)
		//	.BackColor(Color.White).Padding(12).Margin(30, 0, 30, 4);
	}

	// Sidebar öğesi oluşturmak için yardımcı (Tekrardan kaçınmak usta işidir)
	public Control CreateSidebarItem_v1(string iconCode, string text, bool isSelected = false)
	{
		Label ico = new Label
		{
			Text = iconCode,
			// Windows 11 ise "Segoe Fluent Icons", Windows 10 ise "Segoe MDL2 Assets"
			Font = new Font("Segoe Fluent Icons", 12),
			AutoSize = true,
			BackColor = Color.Transparent
		};

		// Eğer font hala yüklenmiyorsa Windows'un yedeğine (MD2) düşelim
		if (ico.Font.Name != "Segoe Fluent Icons")
			ico.Font = new Font("Segoe MDL2 Assets", 12);

		Label lbl = new Label
		{
			Text = text,
			Font = new Font("Segoe UI Variable Display", 10, isSelected ? FontStyle.Bold : FontStyle.Regular),
			AutoSize = true,
			BackColor = Color.Transparent
		};


		var group = this.Group(ico, lbl)
			 .GrowW()
			 .VAlignMiddle()
			 .Padding(10, 12, 10, 12)
			 .Margin(0);

		if (isSelected) group.BackColor(Color.White);


		// 🌟 HOVER MANTIĞINI BURADA TANIMLIYORUZ (Fonksiyon olarak)
		Action turnOnHover = () => {
			if (!isSelected) group.BackColor = Color.FromArgb(230, 230, 230);
		};

		Action turnOffHover = () => {
			if (!isSelected) group.BackColor = Color.Transparent;
		};

		// 1. Grubun kendi olayları
		group.MouseEnter += (s, e) => turnOnHover();
		group.MouseLeave += (s, e) => turnOffHover();

		// 2. TIKLAMA: Çocukların (İkon/Yazı) olaylarını Gruba yönlendir
		foreach (Control child in group.Controls)
		{
			child.MouseEnter += (s, e) => turnOnHover();
			child.MouseLeave += (s, e) => turnOffHover();

			// Opsiyonel: Çocuklara tıklandığında grubun Click eventini tetiklemek istersen:
			// child.Click += (s, e) => group_Click(group, e); 
		}

		return group;

	}
}


// --- YARDIMCI SINIFLAR ---
public enum Side { Left, Right, Bottom }

public class SmartSidePanel : Panel
{
	public Side Edge { get; set; }
	public int BaseSize { get; set; }
	public List<Control> Content { get; set; }
	public SmartSidePanel() { BackColor = Color.Transparent; Margin = new Padding(0); }
}

public class SmartGroup : Panel
{
	public List<Control> LayoutOrder { get; set; }
	public bool IsVertical { get; set; }
	public SmartGroup() { BackColor = Color.Transparent; Margin = new Padding(0); }
}

public class RowResult
{
	public Panel Container { get; set; }
	public List<Control> ControlsInRow { get; set; }
	public bool IsGap { get; set; }
	public int GapSize { get; set; }

	// Zero-Base (Sıfır Diktatörlük)
	public int RawTopMargin = 0;
	public int RawBottomMargin = 0;
	public int RawLeftMargin = 0;
	public int RawRightMargin = 0;
	public int ItemSpacing = 0;

	private SmartUI _parent;

	public RowResult(SmartUI parent) { _parent = parent; }
	public RowResult BackColor(Color c) { if (Container != null) Container.BackColor = c; return this; }
	public RowResult Padding(int all) { if (Container != null) Container.Padding = new Padding(_parent.Scale(all)); _parent.RefreshLayout(); return this; }
	public RowResult Padding(int l, int t, int r, int b) { if (Container != null) Container.Padding = new Padding(_parent.Scale(l), _parent.Scale(t), _parent.Scale(r), _parent.Scale(b)); _parent.RefreshLayout(); return this; }
	public RowResult Margin(int all) { RawLeftMargin = RawTopMargin = RawRightMargin = RawBottomMargin = all; _parent.RefreshLayout(); return this; }
	public RowResult Margin(int l, int t, int r, int b) { RawLeftMargin = l; RawTopMargin = t; RawRightMargin = r; RawBottomMargin = b; _parent.RefreshLayout(); return this; }
	public RowResult Spacing(int space) { ItemSpacing = space; _parent.RefreshLayout(); return this; }
	public RowResult Visible(bool v) { if (Container != null) Container.Visible = v; _parent.RefreshLayout(); return this; }

	public RowResult VAlignMiddle()
	{
		if (ControlsInRow != null) foreach (var c in ControlsInRow) c.GetProps().VAlign = 1;
		_parent.RefreshLayout();
		return this;
	}

	public RowResult Rounded(int radius, Color? borderColor = null, float thickness = 1.5f)
	{
		if (Container != null) Container.Rounded(radius, borderColor, thickness);
		return this;
	}
	 
}

// --- FLUENT API EXTENSIONLARI (String çöplüğü yok, RAM dostu) ---
public static class UIExtensions
{
	internal static ConditionalWeakTable<Control, LayoutProps> _rules = new ConditionalWeakTable<Control, LayoutProps>();
	internal static LayoutProps GetProps(this Control c) => _rules.GetOrCreateValue(c);

	public static Control GrowW(this Control c) { c.GetProps().GrowW = true; return c; }
	public static Control GrowH(this Control c) { c.GetProps().GrowH = true; return c; }
	public static Control WrapText(this Control c) { c.GetProps().WrapText = true; return c; }
	public static Control Spring(this Control c) { c.GetProps().Spring = true; return c; }
	public static Control MatchWidth(this Control c, Control target) { c.GetProps().MatchWidthTarget = target; return c; }
	public static Control AlignRight(this Control c, Control target) { c.GetProps().AlignRightTarget = target; return c; }

	public static Control VAlignMiddle(this Control c)
	{
		c.GetProps().VAlign = 1;
		if (c is SmartGroup sg) foreach (var child in sg.LayoutOrder) child.GetProps().VAlign = 1;
		return c;
	}
	public static Control VAlignBottom(this Control c)
	{
		c.GetProps().VAlign = 2;
		if (c is SmartGroup sg) foreach (var child in sg.LayoutOrder) child.GetProps().VAlign = 2;
		return c;
	}

	public static Control Pad(this Control c, int all) { c.GetProps().CustomPadding = new Padding(all); return c; }
	public static Control Pad(this Control c, int l, int t, int r, int b) { c.GetProps().CustomPadding = new Padding(l, t, r, b); return c; }
	public static Control Padding(this Control c, int all) => c.Pad(all);
	public static Control Padding(this Control c, int l, int t, int r, int b) => c.Pad(l, t, r, b);

	public static Control Margin(this Control c, int all) { c.GetProps().CustomMargin = new Padding(all); return c; }
	public static Control Margin(this Control c, int l, int t, int r, int b) { c.GetProps().CustomMargin = new Padding(l, t, r, b); return c; }

	public static Control Spacing(this Control c, int space) { c.GetProps().ItemSpacing = space; return c; }
	public static Control BackColor(this Control c, Color color) { c.BackColor = color; return c; }

	

	// Herhangi bir kontrolü yuvarlatır ve isteğe bağlı renkli çerçeve çizer
	public static Control Rounded(this Control c, int radius, Color? borderColor = null, float borderThickness = 1.5f)
	{
		var props = c.GetProps();
		props.CornerRadius = radius;
		props.BorderColor = borderColor;
		props.BorderThickness = borderThickness;

		// "Flat" (Düz) tasarım yapıyoruz ki WinForms'un o eski 3D sınırları gitsin
		if (c is Panel p) p.BorderStyle = BorderStyle.None;
		if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0; }

		// Eğer daha önce çizim olayına bağlanmadıysak bağlanalım
		if (!props.IsRoundedHooked)
		{
			props.IsRoundedHooked = true;

			// 1. KESME İŞLEMİ (Region): Kontrol boyut değiştirdikçe köşelerini kırp
			c.Resize += (s, e) => {
				Control ctrl = (Control)s;
				var p_props = ctrl.GetProps();
				if (p_props.CornerRadius.HasValue)
				{
					// Yüksek DPI ekranlarda köşe kıvrımı da düzgün görünsün diye Dpi ile çarpıyoruz
					int r = (int)(p_props.CornerRadius.Value * (ctrl.DeviceDpi / 96f));
					using (GraphicsPath path = GetRoundedPath(ctrl.ClientRectangle, r))
					{
						ctrl.Region = new Region(path); // Fazlalıkları makasla kes!
					}
				}
			};

			// 2. ÇERÇEVE ÇİZİMİ (Paint): Yuvarlatılmış bölgenin kenarına çizgi çek
			c.Paint += (s, e) => {
				Control ctrl = (Control)s;
				var p_props = ctrl.GetProps();

				if (p_props.CornerRadius.HasValue && p_props.BorderColor.HasValue)
				{
					e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; // Tırtıkları yok et (Jilet modu)

					int r = (int)(p_props.CornerRadius.Value * (ctrl.DeviceDpi / 96f));

					// Kenarlığı dışarı taşırmamak için alanı 1 piksel daraltıyoruz
					Rectangle rect = ctrl.ClientRectangle;
					rect.Width -= 1; rect.Height -= 1;

					using (GraphicsPath path = GetRoundedPath(rect, r))
					using (Pen pen = new Pen(p_props.BorderColor.Value, p_props.BorderThickness))
					{
						e.Graphics.DrawPath(pen, path);
					}
				}
			};
		}

		// İlk kesme ve çizim işlemini hemen tetikle
		c.Invalidate();
		return c;
	}
	private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
	{
		GraphicsPath path = new GraphicsPath();
		if (radius <= 0)
		{
			path.AddRectangle(rect);
			return path;
		}
		int d = radius * 2;
		// WinForms'un çizim sapmalarını (1px kayma) önlemek için -1 yapıyoruz
		Rectangle arc = new Rectangle(rect.X, rect.Y, d, d);
		path.AddArc(arc, 180, 90); // Sol Üst
		arc.X = rect.Right - d - 1;
		path.AddArc(arc, 270, 90); // Sağ Üst
		arc.Y = rect.Bottom - d - 1;
		path.AddArc(arc, 0, 90);   // Sağ Alt
		arc.X = rect.Left;
		path.AddArc(arc, 90, 90);  // Sol Alt
		path.CloseFigure();
		return path;
	}

}