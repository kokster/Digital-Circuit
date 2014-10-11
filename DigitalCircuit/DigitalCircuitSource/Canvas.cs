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
    class Canvas
    {
        private Panel canvas;
        private List<ItemDrawing> itemDrawings;
        private Viewport viewport;

        public Canvas(Panel canvas, Circuit circuit)
        {
            this.canvas = canvas;
            this.itemDrawings = new List<ItemDrawing>();
            viewport = new Viewport(circuit, 100, 100);
        }

        private void addItemDrawing(ItemDrawing itemDrawing)
        {
            itemDrawings.Add(itemDrawing);
            canvas.Controls.Add(itemDrawing.PictureBox);
        }

        private void removeItemDrawing(ItemDrawing itemDrawing)
        {
            itemDrawings.Remove(itemDrawing);
            canvas.Controls.Remove(itemDrawing.PictureBox);
        }

        private void removeItemDrawings(List<ItemDrawing> itemDrawings)
        {
            foreach (ItemDrawing itemDrawing in itemDrawings)
            {
                removeItemDrawing(itemDrawing);
            }
        }

        public void refresh()
        {
            refreshItems();
        }

        private bool itemIsDrawn(Item item)
        {
            foreach (ItemDrawing itemDrawing in itemDrawings)
            {
                if (itemDrawing.Item == item)
                {
                    return true;
                }
            }

            return false;
        }

        public void pan(Point vector)
        {
            viewport.changeLocationRelatively(new Point(viewport.getAbsoluteLength(vector.X), viewport.getAbsoluteLength(vector.Y)));
            refresh();
        }

        public enum PanDirection { up, down, left, right }

        public void pan(PanDirection direction)
        {
            switch (direction)
            {
                case PanDirection.up:
                    pan(new Point(0, -10));
                    break;

                case PanDirection.down:
                    pan(new Point(0, 10));
                    break;

                case PanDirection.left:
                    pan(new Point(-10, 0));
                    break;

                case PanDirection.right:
                    pan(new Point(10, 0));
                    break;
            }
        }

        private void refreshItems()
        {
            List<Item> itemsThatShouldBeDrawn = viewport.getVisibleItems();
            List<ItemDrawing> itemDrawingsThatShouldBeRemoved = new List<ItemDrawing>();
            Console.WriteLine(itemsThatShouldBeDrawn.ToString());
            // Remove drawings that should be removed
            foreach (ItemDrawing itemDrawing in itemDrawings)
            {
                // If an item exists that should not be drawn then it should be removed
                if (!itemsThatShouldBeDrawn.Contains(itemDrawing.Item))
                {
                    itemDrawingsThatShouldBeRemoved.Add(itemDrawing);
                }
                else
                {
                    itemDrawing.refresh();
                }
            }
            removeItemDrawings(itemDrawingsThatShouldBeRemoved);

            // Add drawings that should be drawn
            foreach (Item itemThatShouldBeDrawn in itemsThatShouldBeDrawn)
            {
                if (!itemIsDrawn(itemThatShouldBeDrawn))
                {
                    addItemDrawing(new ItemDrawing(itemThatShouldBeDrawn, viewport));
                }
            }
        }

        public Point getAbsoluteCoordinates(Point point)
        {
            return viewport.getAbsoluteCoordinates(point);
        }

        public void changeSize(int width, int height)
        {
            viewport.changeSize(width, height);
        }

        public void zoomOut(Point location)
        {
            viewport.zoomOut();
        }

        public void zoomIn(Point location)
        {
            viewport.zoomIn();
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
           /// decimal zoomlevel = Properties.Settings1.Default.minZoomLevel;
            ///if (viewport.zoomingLevel >= zoomlevel)
                return true;
            ///else
               /// return false;
        }

        public bool zoomInPossible()
        {
           /// decimal zoomlevel = Properties.Settings1.Default.maxZoomLevel;
            ///if (viewport.zoomingLevel <= zoomlevel)
           /// {
                return true;
           /// }
           /// else
            ///    return false;
        }

    }
}
