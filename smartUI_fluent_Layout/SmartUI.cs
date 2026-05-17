using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SmartLayoutEngine
{
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
	}

	public class SmartUI
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
			Panel rowPanel = new Panel { BackColor = Color.Transparent, Margin = new Padding(0) };
			_form.Controls.Add(rowPanel);

			foreach (var c in controls)
			{
				c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				c.Dock = DockStyle.None;
				c.Margin = new Padding(0); // Diktatörlük Bitti, Sıfır Base!
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
			SmartGroup sg = new SmartGroup { LayoutOrder = controls, IsVertical = isVertical, Margin = new Padding(0) };
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

			int itemSpacing = sgProps.ItemSpacing.HasValue ? Scale(sgProps.ItemSpacing.Value) : 0; // Zero-Base!

			int fixedW = 0, flexCount = 0;
			if (!sg.IsVertical)
			{
				foreach (var c in sg.LayoutOrder)
				{
					if (c.GetProps().GrowW) flexCount++;
					else fixedW += c.Width + itemSpacing;
				}
			}
			int flexW = flexCount > 0 ? Math.Max(0, (innerWidth - fixedW) / flexCount) : innerWidth;

			int currentX = sg.Padding.Left;
			int currentY = sg.Padding.Top;
			int maxWidth = 0, maxHeight = 0;

			for (int i = 0; i < sg.LayoutOrder.Length; i++)
			{
				var c = sg.LayoutOrder[i];
				ApplyPaddingLogic(c);
				var props = c.GetProps();

				int childTargetWidth = sg.IsVertical ? innerWidth : (props.GrowW ? flexW : c.Width);

				if (props.GrowW || sg.IsVertical)
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

				if (c is SmartGroup n) Arrange(n);

				c.Left = currentX;
				c.Top = currentY;

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

			sg.Height = (sg.IsVertical ? currentY : maxHeight + sg.Padding.Top) + sg.Padding.Bottom;
			if (!sg.IsVertical && !sgProps.GrowW) sg.Width = currentX + sg.Padding.Right;

			// Group VAlign (Middle/Bottom)
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
			ApplyPaddingLogic(sp);
			int offset = 0; // Zero-Base!

			foreach (var c in sp.Content)
			{
				ApplyPaddingLogic(c);
				if (c is SmartGroup sg) Arrange(sg);

				Padding m = GetScaledMargin(c);

				if (vertical)
				{
					c.Location = new Point(m.Left, offset + m.Top);
					if (c.GetProps().GrowW) c.Width = sp.Width - m.Left - m.Right;
					offset += c.Height + m.Top + m.Bottom;
				}
				else
				{
					c.Top = (sp.Height - c.Height) / 2 + m.Top;
					c.Left = offset + m.Left;
					offset += c.Width + m.Left + m.Right;
				}
			}
		}

		// --- 🌟 NÜKLEER REFRESH LAYOUT MOTORU ---

		public void RefreshLayout()
		{
			if (_isPerformingLayout || _form == null || _form.IsDisposed || _form.WindowState == FormWindowState.Minimized)
				return;

			try
			{
				_isPerformingLayout = true;

				// NÜKLEER SEÇENEK: Win32 Dondurma
				if (_form.IsHandleCreated) SendMessage(_form.Handle, WM_SETREDRAW, false, 0);

				_form.SuspendLayout();
				foreach (var row in _rows) if (row.Container != null) row.Container.SuspendLayout();
				foreach (var sp in _sidePanels) sp.SuspendLayout();


				// Aşama 0: Ön Hazırlık
				foreach (var row in _rows)
					foreach (var c in row.ControlsInRow)
						if (c is SmartGroup sg) Arrange(sg);

				// Aşama 1: Responsive Sidebar
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

				// Aşama 2: Çift Geçişli Dizilim (Double-Pass)
				for (int pass = 0; pass < 2; pass++)
				{
					int totalFixedHeight = 0; // Zero-base! Ana alan ilk satırı 0'dan başlar
					int growHCount = 0;

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

					foreach (var row in _rows)
					{
						if (row.IsGap) { currentY += Scale(row.GapSize); continue; }
						if (!row.Container.Visible) continue;

						row.Container.Location = new Point(mainArea.X + Scale(row.RawLeftMargin), currentY + Scale(row.RawTopMargin));
						row.Container.Width = mainArea.Width - Scale(row.RawLeftMargin) - Scale(row.RawRightMargin);

						int fixedW = 0, flexCount = 0;
						int rowItemSpacing = Scale(row.ItemSpacing);

						foreach (var c in row.ControlsInRow)
						{
							ApplyPaddingLogic(c);
							var p = c.GetProps();
							if (p.MatchWidthTarget != null) c.Width = p.MatchWidthTarget.Width;

							if (p.GrowW || p.Spring) flexCount++;
							else fixedW += c.Width + rowItemSpacing;
						}

						int availW = row.Container.Width - row.Container.Padding.Left - row.Container.Padding.Right - fixedW;
						if (row.ControlsInRow.Count > 0) availW -= rowItemSpacing * (row.ControlsInRow.Count - 1);
						int flexW = flexCount > 0 ? Math.Max(0, availW / flexCount) : 0;

						int curX = row.Container.Padding.Left;
						int maxHInRow = 0;

						for (int i = 0; i < row.ControlsInRow.Count; i++)
						{
							var c = row.ControlsInRow[i];
							var p = c.GetProps();

							if (p.GrowW || p.Spring)
							{
								c.Width = flexW;
								if (c is SmartGroup sg) { sg.AutoSize = false; sg.Width = flexW; Arrange(sg); }
							}
							if (p.GrowH) c.Height = heightPerGrow;

							c.Left = curX;
							c.Top = row.Container.Padding.Top;

							if (p.AlignRightTarget != null) c.Left = p.AlignRightTarget.Right - c.Width;

							int gap = (i == row.ControlsInRow.Count - 1) ? 0 : rowItemSpacing;
							curX += c.Width + gap;

							if (!p.Spring) maxHInRow = Math.Max(maxHInRow, c.Height);
						}

						row.Container.Height = maxHInRow + row.Container.Padding.Top + row.Container.Padding.Bottom;

						foreach (var c in row.ControlsInRow)
						{
							var p = c.GetProps();
							if (p.VAlign == 1) c.Top = row.Container.Padding.Top + (maxHInRow - c.Height) / 2;
							else if (p.VAlign == 2) c.Top = row.Container.Height - row.Container.Padding.Bottom - c.Height;
						}

						currentY += row.Container.Height + Scale(row.RawTopMargin) + Scale(row.RawBottomMargin);
					}
				}

				if (_hamburgerBtn != null && _hamburgerBtn.Visible) _hamburgerBtn.BringToFront();
			}
			finally
			{
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
		public Control[] LayoutOrder { get; set; }
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

		// Optik Hizalama
		public static Control Nudge(this Control c, int x = 0, int y = 0) { c.Margin(x, y, 0, 0); return c; }
	}
}