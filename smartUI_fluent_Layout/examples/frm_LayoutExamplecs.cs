using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using global::SmartLayoutEngine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace smartUI_fluent_Layout.examples
{
	public partial class frm_LayoutExample : Form
	{
		private SmartUI ui;

		public frm_LayoutExample()
		{
			this.Text = "SmartUI Hiyerarşik Düzen Testi";
			this.Size = new Size(1100, 750);
			this.MinimumSize = new Size(800, 600);
			this.BackColor = Color.FromArgb(243, 243, 243); // Windows 11 açık arka plan

			ui = new SmartUI(this);
			SetupLayoutTest();
		}

		private void SetupLayoutTest()
		{
			// 🌟 LEVEL 4: EN İÇ KATMAN (Yatay Grup) - Pastel Yeşil
			// Padding: 12px, Margin: Yok. İçinde gerçek buton ve yeni seçim kutusu barındırır.
			Control innermostGroup = ui.Group(
				new Label { Text = "Lvl 4: En İç Yatay Grup", Font = new Font("Segoe UI Semibold", 9), AutoSize = true }.VAlignMiddle(),
				ui.Space(12),
				ui.FluentButton_v1("Buton A", false, () => MessageBox.Show("Buton A Tıklandı!")).VAlignMiddle(),
				ui.Space(8),
				ui.FluentCheckBox_v1("Check A", true, (state) => Console.WriteLine($"Check A: {state}")).VAlignMiddle()
			)
			.BackColor(Color.FromArgb(220, 245, 220)) // Pastel Yeşil
			.Padding(12)
			.Rounded(6, Color.FromArgb(180, 220, 180));

			// 🌟 LEVEL 3: ORTA DİKEY KATMAN (Dikey Sütun) - Pastel Sarı
			// Padding: 12px. Level 4 Yatay Grubu kendi içine alıp dikeyde dizer.
			Control midCol = ui.Col(
				new Label { Text = "Lvl 3: Sarı Sütun (Dikey)", Font = new Font("Segoe UI Semibold", 9), AutoSize = true },
				ui.Space(8),
				innermostGroup.GrowW(), // En iç grubu buraya yerleştirip yatayda genişletiyoruz
				ui.Space(8),
				new Label { Text = "Sarı Sütunun Alt Açıklama Yazısı", ForeColor = Color.FromArgb(120, 120, 120), AutoSize = true }
			)
			.BackColor(Color.FromArgb(255, 255, 210)) // Pastel Sarı
			.Padding(12)
			.Rounded(6, Color.FromArgb(230, 230, 180));

			// 🌟 LEVEL 2: ORTA YATAY KATMAN (Yatay Grup) - Pastel Mavi
			// Padding: 12px. Level 3 Dikey Sütunu içine yerleştirir.
			Control midGroup = ui.Group(
				new Label { Text = "Lvl 2: Mavi Grup (Yatay)", Font = new Font("Segoe UI Semibold", 9), AutoSize = true }.VAlignMiddle(),
				ui.Space(12),
				midCol.GrowW() // Sarı dikey sütunu yatayda buraya yerleştirip genişletiyoruz
			)
			.BackColor(Color.FromArgb(230, 240, 255)) // Pastel Mavi
			.Padding(12)
			.Rounded(6, Color.FromArgb(190, 210, 240));

			// 🌟 LEVEL 1: DIŞ DİKEY KATMAN (Dikey Sütun) - Pastel Kırmızı
			// Padding: 12px. Level 2 Yatay Grubu ve dış butonu içine yerleştirir.
			Control outerCol = ui.Col(
				new Label { Text = "Lvl 1: Kırmızı Sütun (Dikey Başlık)", Font = new Font("Segoe UI Bold", 10), AutoSize = true },
				ui.Space(10),
				midGroup.GrowW(), // Mavi grubu buraya gömüyoruz
				ui.Space(10),
				ui.FluentButton_v1("Dış Buton", true, () => MessageBox.Show("Dış Buton Tıklandı!")).VAlignMiddle()
			)
			.BackColor(Color.FromArgb(255, 230, 230)) // Pastel Kırmızı
			.Padding(12)
			.Rounded(8, Color.FromArgb(230, 190, 190));

			// 🌟 KÖK SEVİYE SATIR (Formun En Dışı: Row) - Koyu Gri Arka Plan
			// Padding: 12px, Margin: 20px (Tüm kenarlardan 20px içeri çekilir)
			ui.Row(outerCol.GrowW())
			  .BackColor(Color.FromArgb(230, 230, 230)) // Kök Satır Rengi (Koyu Gri)
			  .Padding(12)
			  .Margin(20);
		}
	}
}