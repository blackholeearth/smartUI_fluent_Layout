public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        
        // SmartUI Başlat
        SmartUI.Init(this);

        // Satır 1: Başlık (Koyu gri arka plan, beyaz yazı)
        label1.ForeColor = Color.White;
        SmartUI.Row(label1)
               .Background(Color.FromArgb(45, 45, 48))
               .Padding(10);

        // Satır 2: Giriş Alanı (Margin ile kenarlardan boşluk)
        SmartUI.Row(label2, textBox1.GrowW())
               .Margin(left: 20, top: 10, right: 20, bottom: 0);

        // Satır 3: DataGridView (Orta alan, tüm yüksekliği sömürsün)
        SmartUI.Row(dataGridView1.GrowW().GrowH())
               .Padding(5)
               .Border(BorderStyle.None);

        // Satır 4: Butonlar (Kaydet butonu textbox'ın sağına hizalı, İptal ona uysun)
        SmartUI.Row(btnSave.AlignRight(textBox1), 
                    btnCancel.MatchWidth(btnSave))
               .Margin(0, 10, 20, 10);
    }
}