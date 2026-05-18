using SmartLayoutEngine;

namespace smartUI_fluent_Layout
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			// SmartUI Başlat
			var ui = new SmartUI(this);

			label1.ForeColor = Color.White;

			ui.Row(
				// 1. KUTU: Açık Gri (İçinde Etiket Var)
				ui.Group(label1)
					   .BackColor(Color.FromArgb(113,96,232))
					   .Padding(18)
					   ,

				ui.Spring(), /* ara yay. sağa it */

				// 2. KUTU: Koyu Gri (İçinde Butonlar Var)
				ui.Group(btnZoomIn, btnZoomOut, btnReset)
					   .BackColor(Color.FromArgb(46, 46, 46))
					   .Padding(10)
			)
			.BackColor(Color.FromArgb(31, 31, 31))
			.VAlignMiddle()
			;


			// Button Click Events
			btnZoomIn.Click += (s, e) => ui.ZoomIn();
			btnZoomOut.Click += (s, e) => ui.ZoomOut();
			btnReset.Click += (s, e) => ui.ResetZoom();


			//SmartUI.Row(SmartUI.Spring(), btnZoomIn, btnZoomOut, btnReset)
			//	 .Background(Color.FromArgb(31,31,31))
			//	 .Padding(10);

			//// Satır 1: Başlık (Koyu gri arka plan, beyaz yazı)
			//label1.ForeColor = Color.White;
			//SmartUI.Row(label1)
			//	   .Background(Color.FromArgb(46, 46, 46))
			//	   .Padding(10);

			// Satır 2: Giriş Alanı (Margin ile kenarlardan boşluk)
			ui.Row(label2, textBox1.GrowW())
				   .Padding(5);

			// Satır 3: DataGridView (Orta alan, tüm yüksekliği sömürsün)
			ui.Row(dataGridView1.GrowW().GrowH())
				   .Padding(5)
				   //.Border(BorderStyle.None)
				   ;

			// Satır 4: Butonlar (Kaydet butonu textbox'ın sağına hizalı, İptal ona uysun)
			ui.Row(btnSave.AlignRight(textBox1), btnCancel.MatchWidth(btnSave))
				   .Padding(5);



		}
 


		private void Form1_Load(object sender, EventArgs e)
		{
		
		}


	}
}
