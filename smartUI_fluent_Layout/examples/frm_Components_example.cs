using SmartLayoutEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smartUI_fluent_Layout.examples
{
	public partial class frm_Components_example : Form
	{
		SmartUI ui;

		public frm_Components_example()
		{
			InitializeComponent();


			ui = new SmartUI(this);
			SetupFluentPlaygroundPage();

		}


		private void SetupFluentPlaygroundPage()
		{
			// --- 1. SEKMELİ GEÇİŞ (FluentTabControl_v1) ---
			// Altından mavi çizgi kayan modern sekme seçici
			string[] tabs = { "Genel Ayarlar", "Sistem Performansı", "Kullanıcı Profili" };
			Control tabControl = ui.FluentTabControl_v1(tabs, (index) => {
				// Tab geçiş aksiyonu (Tip-Güvenli delege)
				Console.WriteLine($"Seçilen Sekme: {tabs[index]}");
			});

			// --- 2. AKILLI ARAMA KUTUSU (FluentSearchBox_v1) ---
			// Solunda büyüteç ikonu hazır gelen arama kutusu
			Control searchBox = ui.FluentSearchBox_v1("Ayarlarda ara...", width: 280);

			// Tab geçişini ve arama kutusunu yan yana modern bir satırda topluyoruz
			ui.Row(
				tabControl.VAlignMiddle(),
				ui.Spring(), // Arama kutusunu en sağa iter
				searchBox.VAlignMiddle()
			)
			.Margin(16, 12, 16, 8);


			// --- 3. ALERTMESSAGE & DURUM ROZETLERİ SATIRI ---
			// Dönüş hızı 60 FPS'e sabitlenmiş yükleme halkası ve renkli hap rozetleri
			Control loadingRing = ui.FluentLoadingRing_v1(size: 20);

			// Bilgi rozetleri (Info Badge)
			Control activeBadge = ui.FluentInfoBadge_v1("Sistem Aktif", Color.FromArgb(223, 246, 221), Color.FromArgb(15, 123, 15)); // Açık yeşil badge
			Control updateBadge = ui.FluentInfoBadge_v1("v1.2.0 Güncellemesi", Color.FromArgb(254, 240, 219), Color.FromArgb(159, 107, 12)); // Açık sarı badge

			ui.Row(
				loadingRing.VAlignMiddle(),
				ui.Space(8),
				new Label { Text = "Sistem Durumu taranıyor...", Font = new Font("Segoe UI", 9), AutoSize = true }.VAlignMiddle(),
				ui.Spring(),
				activeBadge.VAlignMiddle(),
				ui.Space(8),
				updateBadge.VAlignMiddle()
			)
			.Margin(16, 4, 16, 4)
			;


			// --- 4. SEÇENEK KARTLARININ HAZIRLANMASI (FluentCard_v1) ---

			// Kart A için ComboBox (Tema Seçimi) - (FluentCard_v1 ile güncellendi!)
			string[] themes = { "Sistem Varsayılanı (Açık)", "Koyu Tema (Dark Mode)", "Yüksek Karşıtlık" };
			Control themeDropdown = ui.FluentComboBox_v1(themes, (selectedIndex) => {
				Console.WriteLine($"Seçilen Tema: {themes[selectedIndex]}");
			});
			Control cardTheme = 
				ui.FluentCard_v1("\uE7F4", "Görsel Tema", "Uygulamanın genel renk ve pencere temasını belirleyin.", themeDropdown);

			// Kart B için CheckBox (Otomatik Güncelleme) - (FluentCard_v1 ile güncellendi!)
			Control autoUpdateCheck = 
				ui.FluentCheckBox_v1("Güncelleştirmeleri otomatik denetle", isChecked: true, (isChecked) => {
				Console.WriteLine($"Otomatik Güncelleme Durumu: {isChecked}");
			});
			Control cardUpdate = 
				ui.FluentCard_v1("\uE895", "Yazılım Güncelleştirmeleri", "Arka planda yeni sürümleri otomatik kontrol et.", autoUpdateCheck);

			// Kart C için TextBox (Kullanıcı Adı) - (FluentCard_v1 ile güncellendi!)
			Control txtUsername = ui.FluentTextBox_v1("Adınızı yazın", width: 180);
			Control cardUsername = ui.FluentCard_v1("\uE77B", "Kullanıcı Bilgileri", "Görev yöneticisi üzerinde görüntülenecek profil adınızı girin.", txtUsername);

			// Kart D için ProgressBar (Depolama Göstergesi) - (FluentCard_v1 ile güncellendi!)
			Control progressBar = ui.FluentProgressBar_v1(initialValue: 65f); // Doluluk %65
			Control cardProgress = ui.FluentCard_v1("\uE9D9", "Sürücü Depolama Alanı", "Doluluk oranı: %65. Sürücü sağlığı kararlı durumda.", progressBar);


			// --- 5. WINDOWS 11 GRUP KAPSAYICISI (FluentCardGroup_v1) ---
			// Artık kartları doğrudan veriyoruz, arka planda hayalet satır birikmiyor!
			Control settingsCardGroup = ui.FluentCardGroup_v1(
				cardTheme,
				cardUpdate,
				cardUsername,
				cardProgress
			);

			// Grubu sayfaya ekliyoruz
			ui.Row(settingsCardGroup.GrowW())
			  .Margin(0);
		}

		private void frm_Components_example_Load(object sender, EventArgs e)
		{
		}

















	}
}
