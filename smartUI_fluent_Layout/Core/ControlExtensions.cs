using System.Reflection;
namespace SmartLayoutEngine
{
	public static class ControlExtensions
	{
	

		public static void DoubleBuffered(this Control control)
		{
			// Use reflection to set the protected DoubleBuffered property
			typeof(Control).InvokeMember("DoubleBuffered",
				BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
				null, control, new object[] { true });
		}
	}
}