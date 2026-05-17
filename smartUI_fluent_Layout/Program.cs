namespace smartUI_fluent_Layout
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

			//var f3 = new Form3(); f3.Show();
			//var f1 = new Form1(); f1.Show();
			//var f2 = new Form2(); f2.Show();

			Application.Run(new Form3());


			

		}
    }
}