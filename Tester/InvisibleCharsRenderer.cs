using FastColoredTextBoxNS;
using System.Collections.Generic;
using System.Drawing;

namespace Tester
{
	public class InvisibleCharsRenderer : Style
	{
		private Pen pen;

		public InvisibleCharsRenderer(Pen pen)
		{
			this.pen = pen;
		}

		public override void Draw(Graphics gr, Point position, Range range)
		{
			FastColoredTextBox tb = range.tb;
			using (Brush brush = new SolidBrush(pen.Color))
			{
				foreach (Place item in (IEnumerable<Place>)range)
				{
					char c = tb[item].c;
					if (c == ' ')
					{
						Point point = tb.PlaceToPoint(item);
						point.Offset(tb.CharWidth / 2, tb.CharHeight / 2);
						gr.DrawLine(pen, point.X, point.Y, point.X + 1, point.Y);
					}
					if (tb[item.iLine].Count - 1 == item.iChar)
					{
						Point point = tb.PlaceToPoint(item);
						point.Offset(tb.CharWidth, 0);
						gr.DrawString("¶", tb.Font, brush, point);
					}
				}
			}
		}
	}
}
