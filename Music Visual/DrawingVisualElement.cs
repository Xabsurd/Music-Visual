using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Music_Visual
{
    public class DrawingVisualElement: FrameworkElement
    {
        public DrawingVisual drawingVisual = new DrawingVisual();
        public DrawingVisualElement()
        {
            this.AddVisualChild(drawingVisual);
        }
        // EllipseAndRectangle instance is our only visual child
        protected override Visual GetVisualChild(int index)
        {
            return drawingVisual;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }
    }
}
