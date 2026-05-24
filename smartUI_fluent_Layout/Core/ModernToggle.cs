using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;



using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;



using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class ModernToggle : CheckBox
{
	private readonly int _baseWidth = 40;
	private readonly int _baseHeight = 20;

	public ModernToggle()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
		BackColor = Color.Transparent;
		Cursor = Cursors.Hand;
		AutoSize = true;
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		float scale = DeviceDpi / 96f;
		int tW = (int)(_baseWidth * scale);
		int tH = (int)(_baseHeight * scale);

		int textWidth = 0;
		if (!string.IsNullOrEmpty(Text))
		{
			textWidth = TextRenderer.MeasureText(Text, Font).Width + (int)(10 * scale);
		}

		// 🌟 BANANA FIX: Sağdan kesilmesin diye +2 piksel "nefes payı" (Breathing Room) ekledik
		return new Size(textWidth + tW + 2, Math.Max(tH, Font.Height + (int)(4 * scale)));
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

		// 🌟 ESKİ: Color parentBg = Parent?.BackColor ?? Color.White;
		// 🌟 YENİ: Zinciri yukarı doğru tarayıp gerçek rengi alıyoruz.
		Color parentBg = GetRealParentBackColor();
		e.Graphics.Clear(parentBg);

		float scale = DeviceDpi / 96f;
		int tW = (int)(_baseWidth * scale);
		int tH = (int)(_baseHeight * scale);

		int textWidth = 0;
		if (!string.IsNullOrEmpty(Text))
		{
			textWidth = TextRenderer.MeasureText(Text, Font).Width;
			int textY = (Height - Font.Height) / 2;
			TextRenderer.DrawText(e.Graphics, Text, Font, new Point(0, textY), ForeColor);
		}

		// 🌟 BANANA FIX (İÇERİ ÇEKME): Kapsülü 1 piksel daraltarak çiziyoruz ki 
		// çizgi kalınlığı (Pen Thickness) kontrolün dışına taşıp kesilmesin!
		int pillX = textWidth + (string.IsNullOrEmpty(Text) ? 0 : (int)(10 * scale));
		int pillY = (Height - tH) / 2;

		Rectangle pillRect = new Rectangle(pillX + 1, pillY + 1, tW - 2, tH - 2);
		int d = pillRect.Height; // Kavis çapı

		Color onColor = Color.FromArgb(0, 103, 192);
		Color offBorder = Color.FromArgb(135, 135, 135);
		Color offThumb = Color.FromArgb(87, 87, 87);

		using (GraphicsPath path = GetRoundedRect(pillRect, d))
		{
			if (Checked)
			{
				using (SolidBrush br = new SolidBrush(onColor)) e.Graphics.FillPath(br, path);
			}
			else
			{
				using (SolidBrush br = new SolidBrush(parentBg)) e.Graphics.FillPath(br, path);
				using (Pen pen = new Pen(offBorder, 1.5f * scale)) e.Graphics.DrawPath(pen, path);
			}
		}

		// --- YUVARLAK TUŞ (THUMB) ---
		int thumbSize = Checked ? (int)(12 * scale) : (int)(10 * scale);
		int thumbOffset = (tH - thumbSize) / 2;

		// Tuşun yerini de yeni daraltılmış kasaya (pillRect) göre ayarlıyoruz
		int thumbX = Checked ? (pillRect.Right - thumbSize - thumbOffset + 1) : (pillRect.Left + thumbOffset);
		int thumbY = pillY + thumbOffset;

		Color thumbColor = Checked ? Color.White : offThumb;
		using (SolidBrush thumbBr = new SolidBrush(thumbColor))
		{
			e.Graphics.FillEllipse(thumbBr, thumbX, thumbY, thumbSize, thumbSize);
		}
	}

	// Çap (diameter) değerini parametre aldık ki tam otursun
	private GraphicsPath GetRoundedRect(Rectangle bounds, int diameter)
	{
		GraphicsPath path = new GraphicsPath();
		path.AddArc(bounds.X, bounds.Y, diameter, diameter, 90, 180);
		path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 180);
		path.CloseFigure();
		return path;
	}

	// 🌟 BANANA FIX: Şeffaf parent zincirini kırıp gerçek katı rengi bulan metot
	private Color GetRealParentBackColor_old()
	{
		Control p = Parent;
		while (p != null && (p.BackColor == Color.Transparent || p.BackColor.A == 0))
		{
			p = p.Parent;
		}
		return p?.BackColor ?? SystemColors.Control;
	}

	/// <summary>
	/// max 10 level up. or else default to SystemColors.Control
	/// </summary>
	/// <returns></returns>
	private Color GetRealParentBackColor()
	{
		Control p = Parent;
		int depth = 0;

		// Maksimum 10 seviye yukarı çık, daha fazlasına izin verme
		while (p != null && (p.BackColor == Color.Transparent || p.BackColor.A == 0) && depth < 10)
		{
			p = p.Parent;
			depth++;
		}

		// Eğer 10 seviyede hala katı renk bulamadıysak varsayılan sistem rengini çek
		return p?.BackColor ?? SystemColors.Control;
	}
}



public class ModernToggle_v2 : CheckBox
{
	// Temel boyutlar (100% DPI için). Motor bunu 125% veya 150% için otomatik çarpacak.
	private readonly int _baseWidth = 40;
	private readonly int _baseHeight = 20;

	public ModernToggle_v2()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

		BackColor = Color.Transparent;
		Cursor = Cursors.Hand;
		AutoSize = true; // Artık kendi boyutunu metne göre ayarlayabilir
	}

	// 🌟 DPI'a ve Metne göre kontrolün ne kadar yer kaplayacağını hesaplar
	public override Size GetPreferredSize(Size proposedSize)
	{
		float scale = DeviceDpi / 96f;
		int tW = (int)(_baseWidth * scale);
		int tH = (int)(_baseHeight * scale);

		int textWidth = 0;
		if (!string.IsNullOrEmpty(Text))
		{
			// Metin + Araya 10px boşluk
			textWidth = TextRenderer.MeasureText(Text, Font).Width + (int)(10 * scale);
		}

		return new Size(textWidth + tW, Math.Max(tH, Font.Height + (int)(4 * scale)));
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

		// Saydamlık bug'ını önlemek için arka plan temizliği
		Color parentBg = Parent?.BackColor ?? Color.White;
		e.Graphics.Clear(parentBg);

		float scale = DeviceDpi / 96f;
		int tW = (int)(_baseWidth * scale);
		int tH = (int)(_baseHeight * scale);

		// --- 1. METNİ SOLA ÇİZ ---
		int textWidth = 0;
		if (!string.IsNullOrEmpty(Text))
		{
			textWidth = TextRenderer.MeasureText(Text, Font).Width;
			// Dikeyde tam ortala
			int textY = (Height - Font.Height) / 2;
			TextRenderer.DrawText(e.Graphics, Text, Font, new Point(0, textY), ForeColor);
		}

		// --- 2. TOGGLE KASASINI ÇİZ ---
		int pillX = textWidth + (string.IsNullOrEmpty(Text) ? 0 : (int)(10 * scale));
		int pillY = (Height - tH) / 2;
		Rectangle pillRect = new Rectangle(pillX, pillY, tW, tH);

		// Win11 Renkleri
		Color onColor = Color.FromArgb(0, 103, 192);   // Win11 Mavisi
		Color offBorder = Color.FromArgb(135, 135, 135); // Kapalıyken gri çerçeve
		Color offThumb = Color.FromArgb(87, 87, 87);     // Kapalıyken koyu gri tuş

		using (GraphicsPath path = GetRoundedRect(pillRect, tH))
		{
			if (Checked)
			{
				using (SolidBrush br = new SolidBrush(onColor)) e.Graphics.FillPath(br, path);
			}
			else
			{
				using (SolidBrush br = new SolidBrush(parentBg)) e.Graphics.FillPath(br, path);
				using (Pen pen = new Pen(offBorder, 1.5f * scale)) e.Graphics.DrawPath(pen, path);
			}
		}

		// --- 3. YUVARLAK TUŞU (THUMB) ÇİZ ---
		// Win11'de tuş kapalıyken biraz daha küçüktür
		int thumbSize = Checked ? (int)(12 * scale) : (int)(10 * scale);
		int thumbOffset = (tH - thumbSize) / 2;

		int thumbX = Checked ? pillX + tW - thumbSize - thumbOffset : pillX + thumbOffset;
		int thumbY = pillY + thumbOffset;

		Color thumbColor = Checked ? Color.White : offThumb;
		using (SolidBrush thumbBr = new SolidBrush(thumbColor))
		{
			e.Graphics.FillEllipse(thumbBr, thumbX, thumbY, thumbSize, thumbSize);
		}
	}

	private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
	{
		GraphicsPath path = new GraphicsPath();
		path.AddArc(bounds.X, bounds.Y, radius, radius, 90, 180);
		path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 180);
		path.CloseFigure();
		return path;
	}
}





public class ModernToggle_v1 : CheckBox
{
	public ModernToggle_v1()
	{
		// 🌟 Saydamlık desteğini açıyoruz!
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

		this.BackColor = Color.Transparent; // Arka plan şeffaf
		this.AutoSize = false;
		this.Size = new Size(44, 22);
		this.Cursor = Cursors.Hand;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

		// 🌟 BANANA FIX: Siyah köşeleri engellemek için arkadaki GERÇEK (Solid) rengi bul!
		Color parentSolidColor = Color.White; // Varsayılan
		Control p = this.Parent;
		while (p != null)
		{
			// Eğer saydam değilse (Alpha = 255) gerçek rengi bulduk demektir
			if (p.BackColor != Color.Transparent && p.BackColor.A == 255)
			{
				parentSolidColor = p.BackColor;
				break;
			}
			p = p.Parent;
		}

		// Önce o siyahlaşan zemini, bulduğumuz gerçek renkle tertemiz bir boya
		e.Graphics.Clear(parentSolidColor);

		// --- TOGGLE KASASI ÇİZİMİ ---
		Color toggleColor = this.Checked ? Color.FromArgb(0, 103, 192) : Color.FromArgb(200, 200, 200);

		using (GraphicsPath path = new GraphicsPath())
		{
			int d = this.Height - 1;
			path.AddArc(0, 0, d, d, 90, 180);
			path.AddArc(this.Width - d - 1, 0, d, d, 270, 180);
			path.CloseFigure();

			using (SolidBrush brush = new SolidBrush(toggleColor))
				e.Graphics.FillPath(brush, path);
		}

		// --- YUVARLAK DÜĞME (THUMB) ÇİZİMİ ---
		int thumbSize = this.Height - 6;
		int thumbX = this.Checked ? this.Width - thumbSize - 3 : 3;

		using (SolidBrush thumbBrush = new SolidBrush(Color.White))
			e.Graphics.FillEllipse(thumbBrush, thumbX, 3, thumbSize, thumbSize);
	}
}