using System;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Color = System.Drawing.Color;

namespace arcgis3dtest1
{
    class Map3D
    {
        // URL for a service to use as an elevation source.
        private readonly Uri _elevationSourceUrl = new Uri(
            "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // URL for the scene layer.
        private readonly Uri _serviceUri = new Uri(
            "https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Portland/SceneServer");

        private GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
        private Graphic graphic = new Graphic();

        // Event publisher for location
        private Position _positionPublisher;

        // Constructor
        public Map3D()
        {
            // create new event publisher
            _positionPublisher = new Position();

        }

        public async void SubscribeAndStart()
        {

            //subscribe to _positionPublisher event publisher (MoveMarker method will be the handler of the event)
            _positionPublisher._PositionChangedHandler += MoveMarker;
            await Task.Run(() =>
            {
                // Start the event publisher
                _positionPublisher.Start();
            });
        }

        public async void Initialize()
        {
            // Create a new scene with basemap
            var myScene = new Scene(Basemap.CreateImageryWithLabels());

            // create an elevation source
            ArcGISTiledElevationSource elevationSrc = new ArcGISTiledElevationSource(_elevationSourceUrl);
            myScene.BaseSurface.ElevationSources.Add(elevationSrc);

            // create additional layer
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_serviceUri);
            myScene.OperationalLayers.Add(sceneLayer);

            // Set the surface placement mode for the overlay.
            graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;

            // create a red "X" marker symbol
            SimpleMarkerSceneSymbol car = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Cube, Color.Red, 20, 20, 20, SceneSymbolAnchorPosition.Bottom);

            // create a new graphic; assign the point and the symbol in the constructor


            try
            {
                // Load the layer.
                await sceneLayer.LoadAsync();

                // Get the center of the scene layer.
               MapPoint center = (MapPoint)GeometryEngine.Project(sceneLayer.FullExtent.GetCenter(), SpatialReferences.Wgs84);
                Console.WriteLine(center.ToString());
                var newPoint = new MapPoint(_positionPublisher.getX(), _positionPublisher.getY(), 0, SpatialReferences.Wgs84);

                graphic.Geometry = newPoint;
                graphic.Symbol = car;

                graphicsOverlay.Graphics.Add(graphic);
                MainWindow.View.Map3D.GraphicsOverlays.Add(graphicsOverlay);

                // Assign the Scene to the SceneView.
                MainWindow.View.Map3D.Scene = myScene;

                ////  CAMERA ANGLE ////
                // Create a camera with coordinates showing layer data.
                // Camera camera = new Camera(center.Y, center.X, 225, 220, 80, 0);
                // Set view point of scene view using camera.
                //await Map3D.SetViewpointCameraAsync(camera);

                // create an OrbitGeoElementCameraController, pass in the target graphic and initial camera distance
                var orbitGraphicController = new OrbitGeoElementCameraController(graphic, 1000);
                MainWindow.View.Map3D.CameraController = orbitGraphicController;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }

            
        }

        private void MoveMarker(double X, double Y)
        {
            var newPoint = new MapPoint(X, Y);
            Console.WriteLine(newPoint.ToString());
            graphic.Geometry = newPoint;
        }

    }
}
