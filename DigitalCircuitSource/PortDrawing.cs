using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalCircuit.Library;
using System.Windows.Forms;
using System.Drawing;

namespace DigitalCircuitSource
{
    public class PortDrawing
    {
        private Viewport viewport;
        public Port port { get; private set; }
        public Point location { get; private set; }
        public bool hovered { get; private set; }
        public bool connectionHovered { get; set; }
        private PictureBox pictureBox;
        public PictureBox PictureBox
        {
            get
            {
                return this.pictureBox;
            }
        }

        private MainForm mainForm;

        public PortDrawing(Port port, Point location, Viewport viewport, MainForm mainForm)
        {
            this.port = port;
            this.location = location;
            this.viewport = viewport;
            this.hovered = false;
            this.mainForm = mainForm;
            this.port.PoweredChanged += new Port.PoweredChangedHandler(PowerChanged);
            initializePictureBox();
        }

        public void initializePictureBox()
        {
            pictureBox = new PictureBox();
            ((Control)pictureBox).AllowDrop = true;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.BackColor = Color.Transparent;
            pictureBox.Image = global::DigitalCircuitSource.Properties.Resources.port_unpowered;
            pictureBox.MouseDown += new MouseEventHandler(mouseDown);
            pictureBox.MouseEnter += new EventHandler(mouseEnter);
            pictureBox.MouseLeave += new EventHandler(mouseLeave);
            pictureBox.DragDrop += dragDrop;
            pictureBox.DragEnter += dragEnter;
            refresh();
        }

        void dragEnter(object sender, DragEventArgs e)
        {
            if (mainForm.connectionPossible(this.port))
            {
                e.Effect = DragDropEffects.Link;
            }
        }

        void dragDrop(object sender, DragEventArgs e)
        {
            mainForm.portUp(this);
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!port.isUsed || !port.isInput)
                {
                    mainForm.portDown(this);
                    pictureBox.DoDragDrop("", DragDropEffects.Link);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                ConnectionDrawing connectionDrawing = mainForm.canvas.getConnectionDrawing(this.port);
                if (connectionDrawing != null)
                {
                    connectionDrawing.Disconnect();
                }
                mainForm.circuitManager.deleteConnectionsForPort(this.port);
                mainForm.refresh();
            }
        }
        private void mouseEnter(object sender, EventArgs e)
        {
            hovered = true;
            mainForm.canvas.refresh();
            refresh();
        }

        private void mouseLeave(object sender, EventArgs e)
        {
            hovered = false;
            mainForm.canvas.refresh();
            refresh();
        }

        public void refresh()
        {
            pictureBox.Width = viewport.getPixelLength(15);
            pictureBox.Height = viewport.getPixelLength(15);
            pictureBox.Location = new Point(viewport.getPixelLength(location.X), viewport.getPixelLength(location.Y));

            if (hovered || connectionHovered)
            {
                if (port.isUsed)
                {
                    pictureBox.Image = global::DigitalCircuitSource.Properties.Resources.port_hover_delete;
                }
                else
                {
                    pictureBox.Image = global::DigitalCircuitSource.Properties.Resources.port_hover_add;
                }
            }
            else if (port.Powered == true)
            {
                pictureBox.Image = global::DigitalCircuitSource.Properties.Resources.port_powered;
            }
            else if (!port.isUsed)
            {
                pictureBox.Image = global::DigitalCircuitSource.Properties.Resources.port_unused;
            }
            else
            {
                pictureBox.Image = global::DigitalCircuitSource.Properties.Resources.port_unpowered;
            }
        }

        private void PowerChanged(object sender)
        {
            refresh();
        }
    }
}
