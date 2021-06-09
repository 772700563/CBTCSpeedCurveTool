using CBTCSpeedCurveTool.Classes;
using CBTCSpeedCurveTool.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Separator = LiveCharts.Wpf.Separator;

namespace CBTCSpeedCurveTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region <����>
        private string authorText;
        public string AuthorText
        {
            get { return authorText; }
            set { authorText = value; RaisePropertyChanged(); }
        }

        private SeriesCollection _CBTCSeries;
        public SeriesCollection CBTCSeries
        {
            get { return _CBTCSeries; }
            set { _CBTCSeries = value; RaisePropertyChanged(); }
        }

        private AxesCollection _AxisXCollection;
        public AxesCollection AxisXCollection
        {
            get { return _AxisXCollection; }
            set { _AxisXCollection = value; RaisePropertyChanged(); }
        }

        private AxesCollection _AxisYCollection;
        public AxesCollection AxisYCollection
        {
            get { return _AxisYCollection; }
            set { _AxisYCollection = value; RaisePropertyChanged(); }
        }

        private int AxisFontSize = 15;

        private SolidColorBrush AxisBrush = new SolidColorBrush(Colors.Black);
        #endregion

        #region <����>
        public RelayCommand SetParametersCM { get; set; }
        public RelayCommand UpdateCanvasCM { get; set; }
        #endregion

        public MainViewModel()
        {
            Initproperties();
            InitCommands();
        }


        #region <����>
        private void Initproperties()
        {
            AuthorText = "���� by Pu Fan (Beijing Jiaotong University)";

            CBTCSeries = new SeriesCollection();

            //���������
            AxisXCollection = new AxesCollection()
            {
                new Axis()
                {
                    Title = "Distance (m)",
                    FontSize=AxisFontSize,
                    Foreground=AxisBrush,
                    Separator=new Separator()
                    {
                        Stroke=new SolidColorBrush(Colors.LightGray),
                    }
                }
            };
            AxisYCollection = new AxesCollection()
            {
                new Axis()
                {
                    Title = "Velocity (km/h)",                 
                    FontSize=AxisFontSize,
                    Foreground=AxisBrush,
                    Separator=new Separator()
                    {
                        Stroke=new SolidColorBrush(Colors.LightGray),
                    }
                }
            };

            //������������
            XYPoint.Mapping();
        }

        private void InitCommands()
        {
            SetParametersCM = new RelayCommand(SetParametersFunc);
            UpdateCanvasCM = new RelayCommand(UpdateCanvasFunc);
        }

        private void SetParametersFunc()
        {
            SetParamView view = new SetParamView();
            view.ShowDialog();
        }

        private void UpdateCanvasFunc()
        {
            try
            {
                //Ŀ�����
                double target_distance = GlobalParams.ParamList[0].RealParamValue;
                //���ǣ�����ٶ�
                double max_traction_acc = GlobalParams.ParamList[1].RealParamValue;
                //�����ƶ���С���ٶ�
                double min_em_braking_dec = GlobalParams.ParamList[2].RealParamValue;
                //����ٶ�
                double max_veloc = GlobalParams.ParamList[3].RealParamValue;
                //�����м��ٶ�
                double max_coast_acc = GlobalParams.ParamList[4].RealParamValue;
                //ATP��Ӧʱ��
                double atp_respon_t = GlobalParams.ParamList[5].RealParamValue;
                //ǣ���г�ʱ��
                double trac_cancle_t = GlobalParams.ParamList[6].RealParamValue;
                //�����ƶ�����ʱ��
                double em_brakong_launch_t = GlobalParams.ParamList[7].RealParamValue;
                //�����ƶ���������ʱ��
                double em_brakong_launch_extra_t = GlobalParams.ParamList[8].RealParamValue;
                //��ʱ����
                double temp_speed_restr = GlobalParams.ParamList[9].RealParamValue;
                //��ʱ�������
                double temp_speed_restr_start_point = GlobalParams.ParamList[10].RealParamValue;
                //��ʱ�����յ�
                double temp_speed_restr_end_point = GlobalParams.ParamList[11].RealParamValue;

                GlobalParams.DistanceStep = target_distance / GlobalParams.PointsCount;
                //����x�᷶Χ
                AxisXCollection[0].SetRange(0, target_distance);
                //����y�᷶Χ
                AxisYCollection[0].SetRange(0, max_veloc);

                CBTCSeries = new SeriesCollection();

                //��������ٶ�����
                var max_speed_line = new LineSeries()
                {
                    Title = "����ٶ�",
                    Fill = new SolidColorBrush(Colors.Transparent),
                    LineSmoothness = 0,
                };
                var max_speed_points = new ChartValues<XYPoint>()
                {
                    new XYPoint(0,max_veloc),
                    new XYPoint(target_distance,max_veloc)
                };
                max_speed_line.Values = max_speed_points;
                CBTCSeries.Add(max_speed_line);

                //������ʱ��������
                var temp_speed_color = Colors.Gray;
                var temp_speed_brush = new SolidColorBrush(Color.FromArgb(70, temp_speed_color.R, temp_speed_color.G, temp_speed_color.B));
                var temp_speed_restr_line = new LineSeries()
                {
                    Title = "��ʱ����",
                    Stroke=temp_speed_brush,
                    Fill=temp_speed_brush,
                    LineSmoothness = 0,
                };
                var temp_speed_restr_points = new ChartValues<XYPoint>()
                {
                    new XYPoint(temp_speed_restr_start_point,temp_speed_restr),
                    new XYPoint(temp_speed_restr_end_point,temp_speed_restr)
                };
                temp_speed_restr_line.Values = temp_speed_restr_points;
                CBTCSeries.Add(temp_speed_restr_line);

                //����GEBR����
                var GEBR_line = new LineSeries()
                {
                    Title= "GEBR����",
                    Fill = new SolidColorBrush(Colors.Transparent),
                    LineSmoothness = 0,
                };
                var GEBR_points = new ChartValues<XYPoint>();
                double cur_y = 0;
                double cur_x = target_distance;
                while (GEBR_points.Count <= GlobalParams.PointsCount)
                {
                    //�趨�ٶȲ�������ʱ����
                    if (cur_x >= temp_speed_restr_start_point && cur_x <= temp_speed_restr_end_point)
                    {
                        cur_y = Math.Min(cur_y, temp_speed_restr / 3.6);
                    }

                    //�趨�ٶȲ��������ֵ
                    cur_y = Math.Min(cur_y, max_veloc / 3.6);
                    GEBR_points.Add(new XYPoint(cur_x, cur_y * 3.6));
                    double next_cur_x = cur_x - GlobalParams.DistanceStep;
                    double next_cur_y = Math.Pow(2 * min_em_braking_dec * GlobalParams.DistanceStep + Math.Pow(cur_y, 2), 0.5);
                    cur_x = next_cur_x;
                    cur_y = next_cur_y;
                }
                GEBR_line.Values = GEBR_points;
                CBTCSeries.Add(GEBR_line);

                //����������
                var ATP_points = new ChartValues<XYPoint>();
                var conn_line_color = Colors.Black;
                var conn_line_brush = new SolidColorBrush(Color.FromArgb(70, conn_line_color.R, conn_line_color.G, conn_line_color.B));
                bool should_break = false;
                foreach (var point in GEBR_points)
                {
                    //�������ݵ�
                    var conn_points = new ChartValues<XYPoint>();
                    //�����ٶȺ;���
                    //d��
                    double yd = point.Y / 3.6;
                    double xd = point.X;
                    //c��
                    double yc = Math.Max(0, yd - em_brakong_launch_extra_t * max_coast_acc);
                    double xc = xd - (Math.Pow(yd, 2) - Math.Pow(yc, 2)) / (2 * max_coast_acc);
                    //b��
                    double yb = Math.Max(0, yc - em_brakong_launch_t * max_coast_acc);
                    double xb = xc - (Math.Pow(yc, 2) - Math.Pow(yb, 2)) / (2 * max_coast_acc);
                    //a��
                    double ya = Math.Max(0, yb - trac_cancle_t * max_traction_acc);
                    double xa = xb - (Math.Pow(yb, 2) - Math.Pow(ya, 2)) / (2 * max_traction_acc);
                    //0��
                    double y0 = Math.Max(0, ya - atp_respon_t * max_traction_acc);
                    double x0 = xa - (Math.Pow(ya, 2) - Math.Pow(y0, 2)) / (2 * max_traction_acc);

                    //С��0�򳬳���ͼ��Χ��ֱ������
                    if (x0 < 0 && should_break)
                    {
                        break;
                    }
                    else
                    {
                        var conn_line = new LineSeries()
                        {
                            Title = null,
                            Fill = new SolidColorBrush(Colors.Transparent),
                            Stroke = conn_line_brush,
                            LineSmoothness = 0,
                            StrokeDashArray = new DoubleCollection(2),
                            PointGeometrySize = 5,                           
                        };
                        conn_line.SetValue(Panel.ZIndexProperty, 100);
                        var conn_line_points = new ChartValues<XYPoint>()
                        {
                            new XYPoint(x0,y0*3.6),
                            new XYPoint(xa,ya*3.6),
                            new XYPoint(xb,yb*3.6),
                            new XYPoint(xc,yc*3.6),
                            new XYPoint(xd,yd*3.6)
                        };
                        conn_line.Values = conn_line_points;
                        CBTCSeries.Add(conn_line);

                        //���ATP��������
                        ATP_points.Add(new XYPoint(x0, y0 * 3.6));

                        if (x0 < 0)
                        {
                            should_break = true;
                        }
                    }                   
                }

                //����ATP����
                var atp_line_color = Colors.Green;
                var atp_line_brush = new SolidColorBrush(Color.FromArgb(100, atp_line_color.R, atp_line_color.G, atp_line_color.B));
                var atp_line = new LineSeries()
                {
                    Title = "ATP��������",
                    Fill = atp_line_brush,
                    Stroke = atp_line_brush,
                    LineSmoothness = 1,
                };
                atp_line.Values = ATP_points;
                CBTCSeries.Add(atp_line);
            }
            catch
            {
                MessageBox.Show("δ���ò�����", "����", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }            
        }
        #endregion
    }
}