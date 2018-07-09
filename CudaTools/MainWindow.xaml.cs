﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace CudaTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {     
        MainWindow_ViewModel m_vm; 

        int m_x = 0;
        int m_y = 0;
  
        Stopwatch m_sw;

        int[] m_dummyData;
        int m_dummyDataSize;

        long m_duration;
        
        public MainWindow()
        {
            InitializeComponent();

            m_vm = new MainWindow_ViewModel();
            DataContext = m_vm;

            ChartArray.Init(8, 12, 1, 2, 10000);  // NOTE: allocate for expected max number of points
                                                   // ~ 120 Megabytes of GPU memory required for every 10,000 points

            m_duration = 0;

            m_dummyDataSize = 15000;
            m_dummyData = new int[m_dummyDataSize];

            for(int i = 0; i<m_dummyDataSize; i++)
            {                
                double angle = ((double)i) / 80.0;                
                m_dummyData[i] = (int)((100.0+(double)i) * Math.Sin(angle));
            }    
            
        }


        private void BackgroundTask(int count, int delay)
        {  
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StartPB.IsEnabled = false;
                ResetPB.IsEnabled = false;
            }), DispatcherPriority.Background);
            

            int num = 0;
            long[] intervals = new long[count];
            long last = 0;

            TimeSpan delayTime = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond / 2);

            int[] x = new int[ChartArray.NumRows() * ChartArray.NumCols()];
            int[] y1 = new int[ChartArray.NumRows() * ChartArray.NumCols()];

            int[] y2 = new int[ChartArray.NumRows() * ChartArray.NumCols()];
            int[] y3 = new int[ChartArray.NumRows() * ChartArray.NumCols()];
            int[] y4 = new int[ChartArray.NumRows() * ChartArray.NumCols()];

            //IntPtr ptr = ChartArray.GetImagePtr(0);
            //ptr = ChartArray.GetImagePtr(1);
            //ptr = ChartArray.GetImagePtr(2);
            //ptr = ChartArray.GetImagePtr(3);

            

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (num<count)
            {
                while (sw.ElapsedMilliseconds - last < delay)
                {
                    Thread.Sleep(delayTime);
                }
                intervals[num] = sw.ElapsedMilliseconds - last;
                last = sw.ElapsedMilliseconds;
                num++;

                for (int i = 0; i < ChartArray.NumRows() * ChartArray.NumCols(); i++)
                {
                    x[i] = m_x;
                    y1[i] = m_dummyData[num+i]; // m_y;
                    y2[i] = 2*m_dummyData[num+i+50];
                    y3[i] = 3*m_dummyData[num+i+75];
                    y4[i] = 4*m_dummyData[num+i+200];
                }
                ChartArray.AppendData(x, y1, 0);
                ChartArray.AppendData(x, y2, 1);
                ChartArray.AppendData(x, y3, 2);
                ChartArray.AppendData(x, y4, 3);

                

                m_x++;
                m_y = m_x;
            }

   
            m_duration = sw.ElapsedMilliseconds;
            
            //Debug.Print("total time = " + sw.ElapsedMilliseconds.ToString() + "   for  " + count.ToString() + "  Points");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                InfoText.Text = m_duration.ToString() + "/" + ChartArray.m_totalPoints.ToString();
                ResetPB.IsEnabled = true;
            }), DispatcherPriority.Background);
        }

      
        private void QuitPB_Click(object sender, RoutedEventArgs e)
        {          
            Close();
        }

        private void StartPB_Click(object sender, RoutedEventArgs e)
        {            
            m_sw = new Stopwatch();
            m_sw.Start();

            Task task = Task.Run(() => BackgroundTask(1000, 2));            
        }

        private void ResetPB_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChartArray.Reset();
                StartPB.IsEnabled = true;
                m_x = 0;
            }), DispatcherPriority.Background);

        }

        private void TestPB_Click(object sender, RoutedEventArgs e)
        {
            //Task.Run(() => TestRoutine());

            ChartArray.Refresh();
            //ChartArray.Resize();

            //int w = 0, h = 0;
            //ChartArray.GetBestBitmapSize(ref w, ref h);

            InfoText.Text = m_duration.ToString() + "/" + ChartArray.m_totalPoints.ToString();
        }


        private void TestRoutine()
        {
            int loopCount = 0;
            while (loopCount < 100)
            {
                Task task = Task.Run(() => BackgroundTask(1000, 5));
                task.Wait();

                ResetPB_Click(null, null);

                loopCount++;
            }
        }
    }








    public class MainWindow_ViewModel : INotifyPropertyChanged
    {
        private int _width;
        public int width
        {
            get { return _width; }
            set { _width = value; OnPropertyChanged(new PropertyChangedEventArgs("width")); }
        }

        private int _height;
        public int height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged(new PropertyChangedEventArgs("height")); }
        }

        private WriteableBitmap _bitmap;
        public WriteableBitmap bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; OnPropertyChanged(new PropertyChangedEventArgs("bitmap")); }
        }

        public void SetBitmap(byte[] imageData, int newWidth, int newHeight)
        {
            if (newWidth != width || newHeight != height)
            {
                width = newWidth;
                height = newHeight;
                bitmap = BitmapFactory.New(width, height);
            }

            Int32Rect imageRect = new Int32Rect(0, 0, width, height);

            try
            {
                bitmap.Lock();
                bitmap.WritePixels(imageRect, imageData, width * 4, 0);
                bitmap.Unlock();
            }
            catch(Exception ex)
            {
                string errMsg = ex.Message;
            }
        }


        public byte[] SynthesizeImage(int width, int height)
        {
            byte[] data = new byte[width * height * 4];

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int ndx = (r * width * 4) + (c * 4);
                    data[ndx + 0] = 0;      // blue
                    data[ndx + 1] = 0;    // green
                    data[ndx + 2] = 0;      // red
                    data[ndx + 3] = 255;    // alpha
                }
            }

            return data;
        }


        public MainWindow_ViewModel()
        {
            width = 0;
            height = 0;

            int newWidth = 1200;
            int newHeight = 800;

            byte[] img = SynthesizeImage(newWidth, newHeight);

            SetBitmap(img, newWidth, newHeight);
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
