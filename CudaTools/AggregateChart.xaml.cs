using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace CudaTools
{
    /// <summary>
    /// Interaction logic for AggregateChart.xaml
    /// </summary>
    public partial class AggregateChart : UserControl
    {
        AggregateChart_ViewModel m_vm;

        public AggregateChart()
        {
            InitializeComponent();
        }


        public void Init(int rows, int cols)
        {
            m_vm = new AggregateChart_ViewModel(rows, cols);

            DataContext = m_vm;

            double[] x = new double[rows * cols];
            double[] y = new double[rows * cols];
   
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < rows * cols; i++)
                {
                    x[i] = j;
                    y[i] = j % 10;
                }
                m_vm.AddRawPoint(x, y);
            }
        }


    }




    public class AggregateChart_ViewModel : INotifyPropertyChanged
    {
        private int _rows;
        public int rows
        {
            get { return _rows; }
            set { _rows = value; OnPropertyChanged(new PropertyChangedEventArgs("rows")); }
        }

        private int _cols;
        public int cols
        {
            get { return _cols; }
            set { _cols = value; OnPropertyChanged(new PropertyChangedEventArgs("cols")); }
        }


        private SeriesCollection _rawSeries;
        public SeriesCollection rawSeries
        {
            get { return _rawSeries; }
            set { _rawSeries = value; OnPropertyChanged(new PropertyChangedEventArgs("rawSeries")); }
        }


        public void AddRawPoint(double[] x, double[] y)
        {
            for(int r = 0; r<rows; r++)
                for(int c = 0; c<cols; c++)
                {
                    int ndx = (r * cols) + c;
                    rawSeries[ndx].Values.Add(new ObservablePoint(x[ndx], y[ndx]));                    
                }
        }

        public AggregateChart_ViewModel(int Rows, int Cols)
        {
            rows = Rows;
            cols = Cols;

            var XYmapper = Mappers.Xy<ObservablePoint>() //in this case value is of type <ObservablePoint>
                       .X(value => value.X) //use the X property as X
                       .Y(value => value.Y); //use the Y property as Y

            rawSeries = new SeriesCollection(XYmapper);

            for(int i = 0; i < rows*cols; i++)
            {
                LineSeries ls = new LineSeries();
                ls.DataLabels = false;
                ls.Fill = System.Windows.Media.Brushes.Transparent;
                ls.PointGeometry = Geometry.Empty;
                ChartValues<ObservablePoint> cv = new ChartValues<ObservablePoint>();
                ls.Values = cv;  
                rawSeries.Add(ls);
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }




}
