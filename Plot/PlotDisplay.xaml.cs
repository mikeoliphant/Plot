using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Plot
{
    /// <summary>
    /// Interaction logic for PlotDisplay.xaml
    /// </summary>
    public partial class PlotDisplay : UserControl
    {
        PlotGenerator plot;

        public PlotDisplay()
        {
            InitializeComponent();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(PlotCanvas_DataContextChanged);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdatePlot();

            base.OnRenderSizeChanged(sizeInfo);
        }

        void PlotCanvas_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((DataContext != null) && (DataContext is PlotGenerator))
            {
                plot = (PlotGenerator)DataContext;

                UpdatePlot();
            }
        }

        private static Color[] colors = new Color[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow, Colors.Orange };

        public void UpdatePlot()
        {
            int numPoints = (int)PlotCanvas.ActualWidth;

            double xScale = (plot.EndX - plot.StartX) / (double)numPoints;

            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (PlotFunction plotFunction in plot.PlotFunctions)
            {
                for (int pos = 0; pos < numPoints; pos++)
                {
                    plot.SetX((double)pos * xScale);

                    double value = plot.GetY(plotFunction.Name);

                    if (value < minY)
                        minY = value;

                    if (value > maxY)
                        maxY = value;
                }
            }

            PlotCanvas.Children.Clear();

            int colorPos = 0;

            foreach (PlotFunction plotFunction in plot.PlotFunctions)
            {
                Path plotPath = new Path();
                plotPath.StrokeThickness = 2;
                plotPath.Stroke = new SolidColorBrush(colors[colorPos]);

                colorPos++;
                if (colorPos > colors.Length)
                    colorPos = 0;

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures = new PathFigureCollection();

                plotPath.Data = pathGeometry;
                PlotCanvas.Children.Add(plotPath);

                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = new Point(0, 0);
                pathFigure.IsClosed = false;
                pathFigure.Segments = new PathSegmentCollection();

                pathGeometry.Figures.Add(pathFigure);

                for (int pos = 0; pos < numPoints; pos++)
                {
                    double x = (double)pos * xScale;

                    plot.SetX(x);
                    double y = plot.GetY(plotFunction.Name);

                    double yScale = (y - minY) / (maxY - minY);

                    double plotY = PlotCanvas.ActualHeight - (PlotCanvas.ActualHeight * yScale);

                    LineSegment lineSegment = new LineSegment();
                    lineSegment.IsStroked = (pos != 0);
                    lineSegment.Point = new Point((double)pos, plotY);
                    pathFigure.Segments.Add(lineSegment);
                }
            }
        }
    }
}
