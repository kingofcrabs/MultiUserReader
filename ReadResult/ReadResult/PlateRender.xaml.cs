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
        public PlateRender(Window parent)
        {
            InitializeComponent();
            this.Loaded += PlateRender_Loaded;
            parent.SizeChanged += parent_SizeChanged;
        }

        void parent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (microPlate == null)
                return;
            microPlate.BoundingSize = new Size(myCanvas.ActualWidth, myCanvas.ActualHeight);
        }

        void PlateRender_Loaded(object sender, RoutedEventArgs e)
        {
            microPlate = new MicroPlate(this.ActualWidth, this.ActualHeight);
            myCanvas.Children.Add(microPlate);
        }
    }
}
