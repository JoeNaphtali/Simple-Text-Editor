using System;
using System.Windows.Forms;

namespace Tester
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Simple_Text_Editor());
		}
	}
}
