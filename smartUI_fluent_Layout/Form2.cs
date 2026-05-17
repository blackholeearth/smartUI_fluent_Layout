using SmartLayoutEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace smartUI_fluent_Layout
{
	public partial class Form2 : Form
	{
		public Form2()
		{
			InitializeComponent();

			// SmartUI Başlat
			var ui = new SmartUI(this);

			// 1. SOL SIDEBAR (Visual Studio gibi)
			ui.SidePanel(Side.Left, 200,
				btnMenu1, btnMenu2, btnMenu3,
				ui.Group(lblStatus).Padding(5).BackColor(Color.DarkSlateBlue)
			);

			// 2. ALT BİLGİ PANELİ (Taskbar / Statusbar gibi)
			ui.SidePanel(Side.Bottom, 40,
				lblVersion, ui.Spring(), btnHelp
			).BackColor(Color.FromArgb(20, 20, 20));


			// 3. ANA İÇERİK (Eski kodun aynen duruyor, motor boş alanı otomatik bulacak!)
			ui.Row(
				ui.Group(label1).BackColor(Color.FromArgb(113, 96, 232)).Padding(18),
				ui.Spring(),
				ui.Group(btnZoomIn, btnZoomOut, btnReset).BackColor(Color.FromArgb(46, 46, 46)).Padding(10)
			)
				.BackColor(Color.FromArgb(31, 31, 31))
				.VAlignMiddle();

			ui.Row(label2, textBox1.GrowW()).Padding(5);

			// BURASI KRİTİK: GrowH artık Alt Paneli fark edip tam üstünde duracak!
			ui.Row(dataGridView1.GrowW().GrowH()).Padding(5);

			ui.Row(btnSave.AlignRight(textBox1), btnCancel.MatchWidth(btnSave)).Padding(5);

			// Button Click Events
			btnZoomIn.Click += (s, e) => ui.ZoomIn();
			btnZoomOut.Click += (s, e) => ui.ZoomOut();
			btnReset.Click += (s, e) => ui.ResetZoom();
		}

		private void Form2_Load(object sender, EventArgs e)
		{

		}
	}
}
