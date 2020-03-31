using System;
using System.Windows;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Color = System.Drawing.Color;
using System.Windows.Input;

namespace arcgis3dtest1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow View; // exposing this class to public
        private Map2D _Map2D;
        private Map3D _Map3D;
 
        public MainWindow()
        {

            InitializeComponent();
            View = this;

            // Create maps objects and initilize them
            _Map2D = new Map2D();
            _Map3D = new Map3D();
            _Map2D.Initialize();
            _Map3D.Initialize();

            _Map2D.SubscribeAndStart();
            _Map3D.SubscribeAndStart();
        }


    }
}
