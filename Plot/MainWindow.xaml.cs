using System;
using System.Windows;
using System.Windows.Controls;

namespace Plot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlotGenerator plot;

        public PlotGenerator Plot
        {
            get { return plot; }
        }

        public MainWindow()
        {
            plot = new PlotGenerator();
            plot.Compile();

            double x = Math.Sin(1.0);

            this.DataContext = this;

            InitializeComponent();

            StartValueTextBox.DataContext = plot.StartValueFunction;
            EndValueTextBox.DataContext = plot.EndValueFunction;

            AddFunctionButton.Click += new RoutedEventHandler(AddFunctionButton_Click);
            UpdateButton.Click += new RoutedEventHandler(UpdateButton_Click);
        }

        void AddFunctionButton_Click(object sender, RoutedEventArgs e)
        {
            plot.AddFunction("0");
        }

        void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                plot.Compile();

                PlotDisplay.UpdatePlot();
            }
            catch (Exception ex)
            {
                ErrorDialog dialog = new ErrorDialog("Expression Compilation Error", ex.ToString())
                {
                    Owner = Window.GetWindow(this)
                };

                dialog.ShowDialog();
            }
        }

        private void Delete_Function_Button_Click(object sender, RoutedEventArgs e)
        {
            PlotFunction function = (PlotFunction)((Button)sender).DataContext;

            plot.PlotFunctions.Remove(function);
        }
    }
}
