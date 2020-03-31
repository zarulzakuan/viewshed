using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.Data;
using Color = System.Drawing.Color;
using System.Threading;
using System.Windows;
using Esri.ArcGISRuntime.UI.Controls;

namespace arcgis3dtest1
{
    class Map2D
    {

        private const string _viewshedUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/ESRI_Elevation_World/GPServer/Viewshed";
        
        private GraphicsOverlay overlay;
        private GraphicsOverlay _resultOverlay;
        private GraphicsOverlay _inputOverlay;

        // Event publisher for location
        private Position _positionPublisher;
        private Graphic graphicWithSymbol = new Graphic();

        // Constructor
        public Map2D()
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

        public void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Create initial map location and reuse the location for graphic
            MapPoint centralLocation = new MapPoint(_positionPublisher.getX(), _positionPublisher.getY(), SpatialReferences.Wgs84);
           
             Viewpoint initialViewpoint = new Viewpoint(centralLocation, 7500);

            // Set initial viewpoint
            myMap.InitialViewpoint = initialViewpoint;

            // Provide used Map to the MapView
            MainWindow.View.Map2D.Map = myMap;

            // Create overlay to where graphics are shown
            overlay = new GraphicsOverlay();

            // Add created overlay to the MapView
            MainWindow.View.Map2D.GraphicsOverlays.Add(overlay);

            // Create a simple marker symbol
            SimpleMarkerSymbol simpleSymbol = new SimpleMarkerSymbol()
            {
                Color = Color.Red,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Circle
            };

            // Add a new graphic with a central point that was created earlier
            graphicWithSymbol = new Graphic(centralLocation, simpleSymbol);
            overlay.Graphics.Add(graphicWithSymbol);

            // Hook into the tapped event
            MainWindow.View.Map2D.GeoViewTapped += OnMapViewTapped;

            //VShed(centralLocation);
            CreateOverlays();

        }

        private async void VShed(MapPoint MP)
        {

            try
            {
                // Execute the geoprocessing task using the user click location
                //await CalculateViewshed(MP);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }

        }

        private void MoveMarker(double X, double Y)
        {
            var newPoint = new MapPoint(X, Y, SpatialReferences.Wgs84);
            graphicWithSymbol.Geometry = newPoint;
        }

        private async Task CalculateViewshed(MapPoint location)
        {
            // This function will define a new geoprocessing task that performs a custom viewshed analysis based upon a
            // user click on the map and then display the results back as a polygon fill graphics overlay. If there
            // is a problem with the execution of the geoprocessing task an error message will be displayed

            // Create new geoprocessing task using the url defined in the member variables section
            GeoprocessingTask myViewshedTask = await GeoprocessingTask.CreateAsync(new Uri(_viewshedUrl));

            // Create a new feature collection table based upon point geometries using the current map view spatial reference
            FeatureCollectionTable myInputFeatures = new FeatureCollectionTable(new List<Field>(), GeometryType.Point, MainWindow.View.Map2D.SpatialReference);

            // Create a new feature from the feature collection table. It will not have a coordinate location (x,y) yet
            Feature myInputFeature = myInputFeatures.CreateFeature();

            // Assign a physical location to the new point feature based upon where the user clicked in the map view
            myInputFeature.Geometry = location;

            // Add the new feature with (x,y) location to the feature collection table
            await myInputFeatures.AddFeatureAsync(myInputFeature);

            // Create the parameters that are passed to the used geoprocessing task
            GeoprocessingParameters myViewshedParameters =
                new GeoprocessingParameters(GeoprocessingExecutionType.SynchronousExecute)
                {

                    // Request the output features to use the same SpatialReference as the map view
                    OutputSpatialReference = MainWindow.View.Map2D.SpatialReference
                };

            // Add an input location to the geoprocessing parameters
            myViewshedParameters.Inputs.Add("Input_Observation_Point", new GeoprocessingFeatures(myInputFeatures));

            // Create the job that handles the communication between the application and the geoprocessing task
            GeoprocessingJob myViewshedJob = myViewshedTask.CreateJob(myViewshedParameters);

            try
            {
                // Execute analysis and wait for the results
                GeoprocessingResult myAnalysisResult = await myViewshedJob.GetResultAsync();

                // Get the results from the outputs
                GeoprocessingFeatures myViewshedResultFeatures = (GeoprocessingFeatures)myAnalysisResult.Outputs["Viewshed_Result"];

                // Add all the results as a graphics to the map
                IFeatureSet myViewshedAreas = myViewshedResultFeatures.Features;
                foreach (Feature myFeature in myViewshedAreas)
                {

                    _resultOverlay.Graphics.Add(new Graphic(myFeature.Geometry));
                }
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem
                if (myViewshedJob.Status == JobStatus.Failed && myViewshedJob.Error != null)
                    MessageBox.Show("Executing geoprocessing failed. " + myViewshedJob.Error.Message, "Geoprocessing error");
                else
                    MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
            finally
            {
                // Indicate that the geoprocessing is not running
                //SetBusy(false);
            }
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {


            _inputOverlay.Graphics.Clear();
            _resultOverlay.Graphics.Clear();

            // Get the tapped point

            MapPoint geometry = new MapPoint(e.Location.X, e.Location.Y, e.Location.SpatialReference);


            Console.WriteLine(geometry.ToString());
            // Create a marker graphic where the user clicked on the map and add it to the existing graphics overlay
            Graphic myInputGraphic = new Graphic(geometry);
            _inputOverlay.Graphics.Add(myInputGraphic);

            // Normalize the geometry if wrap-around is enabled
            //    This is necessary because of how wrapped-around map coordinates are handled by Runtime
            //    Without this step, the task may fail because wrapped-around coordinates are out of bounds.
            if (MainWindow.View.Map2D.IsWrapAroundEnabled) { geometry = (MapPoint)GeometryEngine.NormalizeCentralMeridian(geometry); }

            try
            {
                // Execute the geoprocessing task using the user click location
               await CalculateViewshed(geometry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void CreateOverlays()
        {
            // This function will create the overlays that show the user clicked location and the results of the
            // viewshed analysis. Note: the overlays will not be populated with any graphics at this point

            // Create renderer for input graphic. Set the size and color properties for the simple renderer
            SimpleRenderer myInputRenderer = new SimpleRenderer()
            {
                Symbol = new SimpleMarkerSymbol()
                {
                    Size = 15,
                    Color = Color.Blue,
                    Style = SimpleMarkerSymbolStyle.Circle
                }
            };

            // Create overlay to where input graphic is shown
            _inputOverlay = new GraphicsOverlay()
            {
                Renderer = myInputRenderer
            };

            // Create fill renderer for output of the viewshed analysis. Set the color property of the simple renderer
            SimpleRenderer myResultRenderer = new SimpleRenderer()
            {
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(100, 226, 119, 40)
                }
            };

            // Create overlay to where viewshed analysis graphic is shown
            _resultOverlay = new GraphicsOverlay()
            {
                Renderer = myResultRenderer
            };

            // Add the created overlays to the MapView
            MainWindow.View.Map2D.GraphicsOverlays.Add(_resultOverlay);
            MainWindow.View.Map2D.GraphicsOverlays.Add(_inputOverlay);
        }

    }
}
