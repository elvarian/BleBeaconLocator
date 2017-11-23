using BleBeaconDBLib;
using BleBeaconServer.DbEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BleLocationUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Map map = null;
        List<BleBeacon> beacons = new List<BleBeacon>();
        List<BleNode> nodes = new List<BleNode>();
        List<BleDistance> distances = new List<BleDistance>();

        static int metersToPixels = 100;
        static int extraBorder = 25;

        private void Form1_Load(object sender, EventArgs e)
        {
            using (BleDBContext db = new BleDBContext())
            {
                map = db.Maps.First();
                foreach(BleNode node in db.BleNodes)
                {
                    nodes.Add(node);
                }
                foreach(BleBeacon beacon in db.BleBeacons)
                {
                    beacons.Add(beacon);
                    beaconsComboBox.Items.Add(beacon);
                }
            }
            
            if (map != null)
            {
                Bitmap bitmap = drawBaseImage();

                //pictureBox1.Size = new Size(width+50, height+50);

                pictureBox1.Image = bitmap;
            }

            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            using (BleDBContext db = new BleDBContext())
            {
                Bitmap bitmap = drawBaseImage();
                Graphics g = Graphics.FromImage(bitmap);

                foreach (BleBeacon beacon in db.BleBeacons)
                {
                    List<BleLastLocation> locations = db.LastLocations.Where(l => l.BleBeaconsId == beacon.BleBeaconsId).ToList();
                    BleLastLocation location = locations.OrderByDescending(l => l.Date).FirstOrDefault();

                    if (location != null)
                    {
                        drawCircle(g, Pens.Black, Brushes.Black, (float) coordinateWithOffset(location.X), (float) coordinateWithOffset(location.Y), 3);
                        //g.DrawEllipse(Pens.Black, (float)((location.X * 100 + 24) - 3), (float)((location.Y * 100 + 24) - 3), 3 + 3, 3 + 3);
                        //g.FillEllipse(Brushes.Black, (float)((location.X * 100 + 24) - 3), (float)((location.Y * 100 + 24) - 3), 3 + 3, 3 + 3);
                        
                        g.DrawString(beacon.Name, DefaultFont, Brushes.Black, (float)(location.X * metersToPixels + (extraBorder +9)), (float)(location.Y * metersToPixels + (extraBorder - 11)));
                    }
                }
                pictureBox1.Image = bitmap;
            }
        }

        private Bitmap drawBaseImage()
        {
            using (BleDBContext db = new BleDBContext())
            {
                map = db.Maps.First();
                nodes.Clear();
                foreach (BleNode node in db.BleNodes)
                {
                    nodes.Add(node);
                }
                distances.Clear();
                foreach(BleDistance distance in db.Distances)
                {
                    distances.Add(distance);
                }
            }

            if (map != null)
            {
                int width = (int)(map.Width * metersToPixels);
                int height = (int)(map.Height * metersToPixels);
                Bitmap bitmap = new Bitmap(width + extraBorder*2, height + extraBorder*2);
                Graphics g = Graphics.FromImage(bitmap);
                Rectangle rect = new Rectangle(extraBorder -1, extraBorder -1, width, height);
                g.DrawRectangle(Pens.Black, rect);
                g.FillRectangle(Brushes.White, rect);

                if (nodes != null)
                {
                    foreach (BleNode node in nodes)
                    {
                        drawCircle(g, Pens.Red, Brushes.Red, (float)coordinateWithOffset(node.X), (float)coordinateWithOffset(node.Y), 3);
                        
                        if(beaconsComboBox.SelectedItem != null && beaconsComboBox.SelectedItem is BleBeacon)
                        {
                            //string name = (string) beaconsComboBox.SelectedItem;
                            //beacons.FindLast(b => b.Name == name);
                            BleBeacon beacon = (BleBeacon)beaconsComboBox.SelectedItem;
                            List<BleDistance> matches = distances.Where(d => d.BleBeaconsId == beacon.BleBeaconsId && d.BleNodesId == node.BleNodesId).ToList();
                            if(matches != null && matches.Count == 1)
                            {
                                drawCircle(g, Pens.Blue, Brushes.Transparent, (float)coordinateWithOffset(node.X), (float)coordinateWithOffset(node.Y), (float)coordinateWithOffset(matches[0].Distance));
                            }
                        }

                        //g.DrawEllipse(Pens.Red, (float)((node.X * 100 + 24) - 3), (float)((node.Y * 100 + 24) - 3), 3 + 3, 3 + 3);
                        //g.FillEllipse(Brushes.Red, (float)((node.X * 100 + 24) - 3), (float)((node.Y * 100 + 24) - 3), 3 + 3, 3 + 3);
                    }
                }

                return bitmap;
            }

            return null;
        }

        private double coordinateWithOffset(double coordinate)
        {
            return coordinate * metersToPixels + (extraBorder -1);
        }

        private void drawCircle(Graphics g, Pen pen, Brush brush, float x, float y, float radius)
        {
            g.DrawEllipse(pen, x - radius, y - radius, radius + radius, radius + radius);
            g.FillEllipse(brush, x - radius, y - radius, radius + radius, radius + radius);
        }
    }
}
