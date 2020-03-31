using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;

namespace arcgis3dtest1
{
    class Position
    {

        private double Position_X;
        private double Position_Y;
        // declare delegate 
        public delegate void PositionChangedHandler(double X, double Y);

        //declare event of method pointer
        public event PositionChangedHandler _PositionChangedHandler;

        public Position()
        {
            // initial position
            //Position_X = -226773;
            //Position_Y = 6550477;
            
            //Position_X = -122.672686996571;
            //Position_Y = 45.5161186317961;

            Position_Y = 45.3790902612337;
            Position_X = 6.84905317262762;
            // X = -122.672686996571, Y = 45.5161186317961
        }

        public void Start()
        {
            var i = Position_X;
            while (true)
            {
                Position_X += 0.0001;
                Position_Y += 0.0001;
                System.Threading.Thread.Sleep(1000);
                _PositionChangedHandler?.Invoke(Position_X, Position_Y); //invoke the method that we have referenced to pointer for every 3 seconds

            }
        }

        public double getX()
        {
            return Position_X;
        }

        public double getY()
        {
            return Position_Y;
        }
    }
}
