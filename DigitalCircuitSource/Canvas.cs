using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DigitalCircuit.Library;
using System.Drawing;

namespace DigitalCircuitSource
{
    public class Canvas
    {
        private Panel panelCanvas;

        private List<ItemDrawing> itemDrawings;
        private List<ConnectionDrawing> connectionDrawings;

        private Viewport viewport;
        public MainForm mainForm;
        private Point lastMousePoint;
        public bool mouseOnCanvas = false;
        Rectangle itemPreviewRectangle;
        SolidBrush itemPreviewBrush = new SolidBrush(Color.LightGray);

        public Canvas(Panel panelCanvas, MainForm mainForm)
        {
            this.panelCanvas = panelCanvas;
            this.itemDrawings = new List<ItemDrawing>();
            this.connectionDrawings = new List<ConnectionDrawing>();
            this.mainForm = mainForm;
            this.viewport = mainForm.circuitManager.circuit.Viewport;
            this.panelCanvas.MouseUp += new MouseEventHandler(mouseUp);
            this.panelCanvas.MouseWheel += new MouseEventHandler(mouseWheel);
            this.panelCanvas.Resize += new EventHandler(resize);
            this.panelCanvas.MouseMove += new MouseEventHandler(mouseMove);
            this.panelCanvas.Paint += new PaintEventHandler(paint);
            this.panelCanvas.DragOver += dragOver;
            changeSize(panelCanvas.Width, panelCanvas.Height);

            foreach (Item item in mainForm.circuitManager.circuit.Items)
            {
                ItemDrawing itemDrawing = new ItemDrawing(item, viewport, mainForm);
                itemDrawings.Add(itemDrawing);
            }

            this.lastMousePoint = Point.Empty;

            this.itemPreviewRectangle = new Rectangle(0, 0, viewport.getPixelLength(100), viewport.getPixelLength(100));
        }

        void dragOver(object sender, DragEventArgs e)
        {
            mouseMove(sender, new MouseEventArgs(MouseButtons.None, 0, e.X, e.Y, 0));
        }

        private void addItemDrawing(ItemDrawing itemDrawing)
        {
            itemDrawings.Add(itemDrawing);
        }

        public void removeItemDrawing(ItemDrawing itemDrawing)
        {
            itemDrawings.Remove(itemDrawing);
            //List<ConnectionDrawing> associatedConnectionDrawings = getConnectionDrawings(itemDrawing);
            panelCanvas.Controls.Remove(itemDrawing.PictureBox);
            removeConnectionDrawings(connectionDrawings);
        }

        public List<ConnectionDrawing> getConnectionDrawings(ItemDrawing itemDrawing)
        {
            return connectionDrawings;
        }

        private void removeItemDrawings(List<ItemDrawing> itemDrawings)
        {
            foreach (ItemDrawing itemDrawing in itemDrawings)
            {
                removeItemDrawing(itemDrawing);
            }
            removeConnectionDrawings(connectionDrawings);
        }

        private void addConnectionDrawing(ConnectionDrawing connectionDrawing)
        {
            connectionDrawings.Add(connectionDrawing);
        }

        public void removeConnectionDrawing(ConnectionDrawing connectionDrawing)
        {
            connectionDrawings.Remove(connectionDrawing);
        }

        private void removeConnectionDrawings(List<ConnectionDrawing> connectionDrawings)
        {
            connectionDrawings.RemoveRange(0, connectionDrawings.Count);
        }

        public void refresh()
        {
            refreshItems();
            refreshConnections();
            panelCanvas.Invalidate();
        }

        public void pan(Point vector)
        {
            viewport.changeLocationRelatively(new Point(viewport.getAbsoluteLength(vector.X), viewport.getAbsoluteLength(vector.Y)));
            refresh();
        }

        private void refreshItems()
        {
            List<Item> itemsThatShouldBeDrawn = viewport.getVisibleItems();
            List<ItemDrawing> itemDrawingsThatShouldBeRemoved = new List<ItemDrawing>();

            foreach (Item itemThatShouldBeDrawn in itemsThatShouldBeDrawn)
            {

                ItemDrawing itemDrawing = getItemDrawing(itemThatShouldBeDrawn);
                if (itemDrawing == null)
                {
                    itemDrawing = new ItemDrawing(itemThatShouldBeDrawn, viewport, mainForm);
                    itemDrawings.Add(itemDrawing);
                }

                if (itemDrawing.IsDrawn)
                {
                    itemDrawing.refresh();
                }
                else
                {
                    panelCanvas.Controls.Add(itemDrawing.PictureBox);
                    panelCanvas.BringToFront();
                    itemDrawing.IsDrawn = true;
                }
            }

            foreach (ItemDrawing itemDrawing in itemDrawings)
            {
                if (itemDrawing.IsDrawn && !itemsThatShouldBeDrawn.Contains(itemDrawing.Item))
                {
                    panelCanvas.Controls.Remove(itemDrawing.PictureBox);
                    itemDrawing.IsDrawn = false;
                }
            }
        }

        private void refreshConnections()
        {
            List<Connection> connectionsThatShouldBeDrawn = viewport.getVisibleConnections();
            List<ConnectionDrawing> connectionsThatShouldBeRemoved = new List<ConnectionDrawing>();

            Console.WriteLine(connectionsThatShouldBeDrawn.Count.ToString());
            foreach (Connection connectionThatShouldBeDrawn in connectionsThatShouldBeDrawn)
            {
                ConnectionDrawing connectionDrawing = getConnectionDrawing(connectionThatShouldBeDrawn);

                if (connectionDrawing == null)
                {
                    connectionDrawing = new ConnectionDrawing(connectionThatShouldBeDrawn, viewport, mainForm);
                    connectionDrawings.Add(connectionDrawing);
                }

                connectionDrawing.IsVisible = true;
            }

            foreach (ConnectionDrawing connectionDrawing in connectionDrawings)
            {
                if (connectionDrawing.IsVisible && !connectionsThatShouldBeDrawn.Contains(connectionDrawing.Connection))
                {
                    connectionsThatShouldBeRemoved.Add(connectionDrawing);
                    //connectionDrawing.IsVisible = false;
                }
                else
                {
                    connectionDrawing.refresh();
                }
            }

            foreach (ConnectionDrawing connectionThatShouldBeRemoved in connectionsThatShouldBeRemoved)
            {
                connectionThatShouldBeRemoved.Disconnect();
                connectionDrawings.Remove(connectionThatShouldBeRemoved);
            }
        }

        private ItemDrawing getItemDrawing(Item item)
        {
            foreach (ItemDrawing itemDrawing in itemDrawings)
            {
                if (itemDrawing.Item == item)
                {
                    return itemDrawing;
                }
            }

            return null;
                }

        public PortDrawing getPortDrawing(Port port)
        {
            foreach (ItemDrawing itemDrawing in itemDrawings)
            {
                PortDrawing portDrawing = itemDrawing.getPortDrawing(port);
                if (portDrawing != null)
                {
                    return portDrawing;
                }
            }

            return null;
            }

        private ConnectionDrawing getConnectionDrawing(Connection connection)
        {
            foreach (ConnectionDrawing connectionDrawing in connectionDrawings)
            {
                if (connectionDrawing.Connection == connection)
                {
                    return connectionDrawing;
                }
            }

            return null;
        }

        public void changeSize(int width, int height)
        {
            viewport.changeSize(width, height);
        }

        public void zoomOut(Point location)
        {
            viewport.zoomOut();
            itemPreviewRectangle.Width = viewport.getPixelLength(100);
            itemPreviewRectangle.Height = viewport.getPixelLength(100);
        }

        public void zoomIn(Point location)
        {
            viewport.zoomIn();
            itemPreviewRectangle.Width = viewport.getPixelLength(100);
            itemPreviewRectangle.Height = viewport.getPixelLength(100);
        }

        public void zoomOut()
        {
            viewport.zoomOut();
        }

        public void zoomIn()
        {
            if (zoomInPossible())
            {
                viewport.zoomIn();
            }
        }

        public bool zoomOutPossible()
        {
            decimal zoomlevel = Properties.Settings1.Default.minZoomLevel;
            if (viewport.zoomingLevel > zoomlevel) {
                return true;
            }
            else {
               return false;
            }
        }

        public bool zoomInPossible()
        {
            decimal zoomlevel = Properties.Settings1.Default.maxZoomLevel;
            if (viewport.zoomingLevel < zoomlevel)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void mouseUp(object sender, MouseEventArgs e)
        {
            if (mainForm.nodeBeingDragged != null)
            {
                Item item = null;
                switch (mainForm.nodeBeingDragged.Name)
                {
                    case "nodeAndGate":
                        item = new AndGate();
                        break;

                    case "nodeOrGate":
                        item = new OrGate();
                        break;

                    case "nodeNotGate":
                        item = new NotGate();
                        break;

                    case "nodeNorGate":
                        item = new NorGate();
                        break;

                    case "nodeXorGate":
                        item = new XorGate();
                        break;

                    case "nodeXnorGate":
                        item = new XnorGate();
                        break;

                    case "nodeNandGate":
                        item = new NandGate();
                        break;

                    case "nodeSwitch":
                        item = new Switch();
                        break;

                    case "nodeLamp":
                        item = new Lamp();
                        break;
                }
                if (item != null)
                {
                    Point absoluteCoordinates = viewport.getAbsoluteCoordinates(new Point(e.X, e.Y));
                    item.Location = absoluteCoordinates;
                    if (mainForm.circuitManager.addItem(item))
                    {
                        mainForm.unsavedChangesMade = true;
                    }

                    mainForm.nodeBeingDragged = null;
                    mainForm.refresh();
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                if (selectedConnectionDrawing != null)
                {
                    selectedConnectionDrawing.Disconnect();
                    mainForm.circuitManager.deleteConnection(selectedConnectionDrawing.Connection);
                    mainForm.refresh();
                }
            }
        }

        public void mouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (zoomOutPossible())
                {
                    zoomOut(new Point(e.X, e.Y));
                    mainForm.refresh();
                }
            }
            else
            {
                if (zoomInPossible())
                {
                    zoomIn(new Point(e.X, e.Y));
                    mainForm.refresh();
                }
            }
        }

        private void resize(object sender, EventArgs e)
        {
            changeSize(panelCanvas.Width, panelCanvas.Height);
        }

        public void itemMouseEntered()
        {
            if (selectedConnectionDrawing != null)
            {
                selectedConnectionDrawing.lineHovered = false;
            }
            panelCanvas.Invalidate();
        }

        private ConnectionDrawing selectedConnectionDrawing;

        public void mouseMove(object sender, MouseEventArgs e)
        {
            if (mainForm.middleMouseDown && mainForm.toolPanSelected || mainForm.leftMouseDown && mainForm.toolPanSelected)
            {
                if (lastMousePoint != Point.Empty)
                {
                    pan(new Point(lastMousePoint.X - e.X, lastMousePoint.Y - e.Y));
                }
                lastMousePoint = new Point(e.X, e.Y);
                panelCanvas.Invalidate();
            }
            else
            {
                lastMousePoint = Point.Empty;
            }

            if (mainForm.nodeBeingDragged != null)
            {
                itemPreviewRectangle.X = e.X;
                itemPreviewRectangle.Y = e.Y;

                Point absoluteCoordinates = viewport.getAbsoluteCoordinates(new Point(e.X, e.Y));
                Rectangle absolutePreviewRectangle = new Rectangle(absoluteCoordinates.X, absoluteCoordinates.Y, 100, 100);
                if (viewport.checkCollision(absolutePreviewRectangle))
                {
                    itemPreviewBrush.Color = Color.FromArgb(128, 255, 0, 0);
                }
                else
                {
                    itemPreviewBrush.Color = Color.FromArgb(32, 127, 127, 127);
                }
                panelCanvas.Invalidate();
            }

            if (!(sender is ItemDrawing))
            {
                ConnectionDrawing newClosestConnectionDrawing = null;
                int? closestConnectionDrawingDistance = null;

                foreach (ConnectionDrawing connectionDrawing in connectionDrawings)
                {
                    int distance = connectionDrawing.distanceToPoint(new Point(e.X, e.Y));

                    if (closestConnectionDrawingDistance == null || distance < closestConnectionDrawingDistance)
                    {
                        closestConnectionDrawingDistance = distance;
                        newClosestConnectionDrawing = connectionDrawing;
                    }
                }

                int threshold = viewport.getPixelLength(18);
                bool refresh = false;

                if (newClosestConnectionDrawing != null && closestConnectionDrawingDistance < threshold)
                {
                    if (!newClosestConnectionDrawing.lineHovered)
                    {
                        refresh = true;

                        if (selectedConnectionDrawing != null && selectedConnectionDrawing != newClosestConnectionDrawing)
                        {
                            selectedConnectionDrawing.lineHovered = false;
                        }
                        newClosestConnectionDrawing.lineHovered = true;

                        selectedConnectionDrawing = newClosestConnectionDrawing;
                    }
                }
                else
                {
                    if (selectedConnectionDrawing != null && selectedConnectionDrawing.lineHovered)
                    {
                        refresh = true;
                        selectedConnectionDrawing.lineHovered = false;
                        selectedConnectionDrawing = null;
                    }
                }

                if (refresh)
                {
                    if (newClosestConnectionDrawing != null)
                    {
                        newClosestConnectionDrawing.refresh();
                    }
                    panelCanvas.Refresh();
                }
            }
        }

        public ConnectionDrawing getConnectionDrawing(Port port)
        {
            foreach (ConnectionDrawing connectionDrawing in connectionDrawings)
            {
                if (connectionDrawing.Connection.inputPort == port || connectionDrawing.Connection.outputPort == port)
                {
                    return connectionDrawing;
                }
            }

            return null;
        }

        private void paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                
                if (mainForm.nodeBeingDragged != null && mouseOnCanvas)
                {
                    g.FillRectangle(itemPreviewBrush, itemPreviewRectangle);
                }

                foreach (ConnectionDrawing connectionDrawing in connectionDrawings)
                {
                    if (connectionDrawing.IsVisible)
                    {
                        Point[] line = connectionDrawing.getLine();
                        float penWidth = viewport.getPixelLength(5);
                        Pen pen = new Pen(connectionDrawing.getColor(), penWidth);
                        g.DrawLine(pen, line[0], line[1]);
                    }
                    
                }

                g.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("bug found: " + ex.Message);
            }
        }

        public void Destruct()
        {
            this.panelCanvas.MouseUp -= mouseUp;
            this.panelCanvas.MouseWheel -= mouseWheel;
            this.panelCanvas.Resize -= resize;
            this.panelCanvas.MouseMove -= mouseMove;
            this.panelCanvas.Paint -= paint;
            this.panelCanvas.DragOver -= dragOver;
        }

    
    }
}
