using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReadResult
{
    /// <summary>
    /// Interaction logic for PlateRender.xaml
    /// </summary>
    public partial class PlateRender : UserControl
    {
        MicroPlate microPlate;
        public PlateRender()
        {
            InitializeComponent();
            this.Loaded += PlateRender_Loaded;
            this.SizeChanged += PlateRender_SizeChanged;
        }

        void PlateRender_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            microPlate.BoundingSize = new Size(myCanvas.ActualWidth, myCanvas.ActualHeight);
        }

        void PlateRender_Loaded(object sender, RoutedEventArgs e)
        {
            microPlate = new MicroPlate(this.ActualWidth, this.ActualHeight);
            myCanvas.Children.Add(microPlate);
        }
    }
}
