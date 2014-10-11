using DigitalCircuit.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DigitalCircuitSource
{
    public class ItemDrawing
    {
        private Viewport viewport;

        private Item item;
        public Item Item
        {
            get
            {
                return item;
            }
        }

        private PictureBox pictureBox;
        public PictureBox PictureBox
        {
            get
            {
                return this.pictureBox;
            }
        }

        public ItemDrawing(Item item, Viewport viewport)
        {
            this.item = item;
            this.viewport = viewport;
            pictureBox = new PictureBox();
            refresh();
        }

        public void refresh()
        {
            pictureBox.Width = viewport.getPixelLength(item.Width);
            pictureBox.Height = viewport.getPixelLength(item.Height);
            pictureBox.BackColor = Color.Blue;
            pictureBox.Location = viewport.getPixelLocation(item.Location);
        }
    }
}
