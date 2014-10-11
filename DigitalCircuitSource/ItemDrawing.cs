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
        private MainForm mainForm;
        private List<PortDrawing> portDrawings;

        public bool IsDrawn
        {
            get;
            set;
        }

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

        private Rectangle subDrawingAbsoluteBoundingBox = Rectangle.Empty;
        private Rectangle subDrawingPixelBoundingBox
        {
            get
            {
                if (subDrawingAbsoluteBoundingBox == Rectangle.Empty)
                {
                    return Rectangle.Empty;
                }

                int x = viewport.getPixelLength(subDrawingAbsoluteBoundingBox.X);
                int y = viewport.getPixelLength(subDrawingAbsoluteBoundingBox.Y);
                int width = viewport.getPixelLength(subDrawingAbsoluteBoundingBox.Width);
                int height = viewport.getPixelLength(subDrawingAbsoluteBoundingBox.Height);

                return new Rectangle(x, y, width, height);
            }
        }

        public ItemDrawing(Item item, Viewport viewport, MainForm mainForm)
        {
            this.item = item;
            this.IsDrawn = false;
            this.mainForm = mainForm;
            this.viewport = viewport;
            setEventHandlers();
            initializePictureBox();
            createPortDrawings();
            refresh();
        }

        public void setEventHandlers()
        {
            Item.OutputChanged += Item_OutputChanged;
        }

        void Item_OutputChanged(object sender, bool? newValue)
        {
            setPictureboxImage();
        }

        public void initializePictureBox()
        {
            pictureBox = new PictureBox();
            pictureBox.MouseMove += new MouseEventHandler(itemDrawing_MouseMove);
            pictureBox.MouseEnter += new EventHandler(itemDrawing_MouseEnter);
            pictureBox.MouseWheel += new MouseEventHandler(itemDrawing_MouseWheel);
            pictureBox.MouseDown += new MouseEventHandler(itemDrawing_Click);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            setPictureboxImage();
        }

        private void setPictureboxImage()
        {
            Bitmap image;

            if(item.getOutput() == null) {
               image = new Bitmap(global::DigitalCircuitSource.Properties.Resources.item_null);
            }
            else {
               image = new Bitmap(global::DigitalCircuitSource.Properties.Resources.item); 
            }

            Graphics g = Graphics.FromImage(image);
            Image overlay = null;
            if (item is OrGate)
            {
                overlay = global::DigitalCircuitSource.Properties.Resources.item_or;
            }
            else if (item is AndGate)
            {
                overlay = global::DigitalCircuitSource.Properties.Resources.item_and;
            }
            else if (item is NotGate)
            {
                overlay = global::DigitalCircuitSource.Properties.Resources.item_not;
            }
            else if (item is NorGate)
            {
                overlay = global::DigitalCircuitSource.Properties.Resources.item_nor;
            }
            else if (item is XorGate)
            {
                overlay = global::DigitalCircuitSource.Properties.Resources.item_xor;
            }
            else if (item is XnorGate)
            {
                overlay = global::DigitalCircuitSource.Properties.Resources.item_xnor;
            }
            else if (item is NandGate)
            {
                overlay = global::DigitalCircuitSource.Properties.Resources.item_nand;
            }
            else if (item is Lamp)
            {
                if (item.getOutput() == true)
                {
                    
                    overlay = global::DigitalCircuitSource.Properties.Resources.item_lamp_on;
                }
                else
                {
                    overlay = global::DigitalCircuitSource.Properties.Resources.item_lamp_off;
                }
            }
            else if (item is Switch)
            {
                if (item.getOutput() == true)
                {
                    overlay = global::DigitalCircuitSource.Properties.Resources.item_switch_on;
                }
                else
                {
                    overlay = global::DigitalCircuitSource.Properties.Resources.item_switch_off;
                }
            }
            if(overlay != null) {
                int x = 17;
                if (item.Inputs.Count() == 0)
                {
                    x -= 12;
                }
                else if (item.Outputs.Count() == 0)
                {
                    x += 12;
                }
                subDrawingAbsoluteBoundingBox = new Rectangle(x, 0, 66, 100);
                g.DrawImage(overlay, subDrawingAbsoluteBoundingBox);
            }
            pictureBox.Image = image;
            g.Dispose();
        }

        private void createPortDrawings()
        {
            portDrawings = new List<PortDrawing>();

            int inputCount = item.Inputs.Count();
            if (inputCount > 0)
            {
                if (inputCount == 1)
                {
                    portDrawings.Add(new PortDrawing(item.Inputs[0], new Point(3, 40), viewport, mainForm));
                }
                else if (inputCount == 2)
                {
                    portDrawings.Add(new PortDrawing(item.Inputs[0], new Point(3, 5), viewport, mainForm));
                    portDrawings.Add(new PortDrawing(item.Inputs[1], new Point(3, 72), viewport, mainForm));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            int outputCount = item.Outputs.Count();
            if (outputCount > 0)
            {
                if (outputCount == 1)
                {
                    portDrawings.Add(new PortDrawing(item.Outputs[0], new Point(81, 40), viewport, mainForm));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            foreach (PortDrawing portDrawing in portDrawings)
            {
                pictureBox.Controls.Add(portDrawing.PictureBox);
            }
        }

        private void itemDrawing_MouseEnter(object sender, EventArgs e)
        {
            mainForm.canvas.itemMouseEntered();
            pictureBox.Focus();
        }


        private void itemDrawing_MouseMove(object sender, MouseEventArgs e)
        {
            int newX = ((Control)sender).Location.X + e.X;
            int newY = ((Control)sender).Location.Y + e.Y;
            //object newSender = ((Control)sender).Parent;
            MouseEventArgs newE = new MouseEventArgs(e.Button, e.Clicks, newX, newY, e.Delta);

            mainForm.canvas.mouseMove(this, newE);
        }

        private void itemDrawing_MouseWheel(object sender, MouseEventArgs e) {
            int newX = ((Control)sender).Location.X + e.X;
            int newY = ((Control)sender).Location.Y + e.Y;
            object newSender = ((Control)sender).Parent;
            MouseEventArgs newE = new MouseEventArgs(e.Button, e.Clicks, newX, newY, e.Delta);

            mainForm.canvas.mouseWheel(sender, newE);
        }

        public void itemDrawing_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (item is IToggleable)
                {
                    if (subDrawingPixelBoundingBox != Rectangle.Empty)
                    {
                        if (!subDrawingPixelBoundingBox.Contains(new Point(e.X, e.Y)))
                        {
                            return;
                        }
                    }
                    mainForm.circuitManager.toggle((IToggleable)item);
                    mainForm.refresh();
                    mainForm.unsavedChangesMade = true;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                mainForm.circuitManager.deleteItem(item);
                mainForm.canvas.removeItemDrawing(this);
                mainForm.refresh();
                mainForm.unsavedChangesMade = true;
            }
        }

        public void refresh()
        {
            pictureBox.Width = viewport.getPixelLength(item.Width);
            pictureBox.Height = viewport.getPixelLength(item.Height);
            pictureBox.Location = viewport.getPixelLocation(item.Location);

            foreach (PortDrawing portDrawing in portDrawings)
            {
                portDrawing.refresh();
            }
        }

        public PortDrawing getPortDrawing(Port port)
        {
            foreach (PortDrawing portDrawing in portDrawings)
            {
                if (portDrawing.port == port)
                {
                    return portDrawing;
                }
            }

            return null;
        }
    }
}
