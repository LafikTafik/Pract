using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace NAMI.Helpers
{
    class RoundedButton : Button
    {
        private int _cornerRadius = 20;

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; this.Invalidate(); }
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);
            using (GraphicsPath path = new GraphicsPath())
            {
                int arcWidth = CornerRadius * 2;
                path.AddArc(bounds.X, bounds.Y, arcWidth, arcWidth, 180, 90); // верхний левый угол
                path.AddArc(bounds.Right - arcWidth, bounds.Y, arcWidth, arcWidth, 270, 90); // верхний правый
                path.AddArc(bounds.Right - arcWidth, bounds.Bottom - arcWidth, arcWidth, arcWidth, 0, 90); // нижний правый
                path.AddArc(bounds.X, bounds.Bottom - arcWidth, arcWidth, arcWidth, 90, 90); // нижний левый
                path.CloseAllFigures();

                this.Region = new Region(path);
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.WordEllipsis;
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, bounds, this.ForeColor, flags);
            }
        }
    }
}
