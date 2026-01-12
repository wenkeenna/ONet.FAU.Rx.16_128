using AvalonDock.Layout.Serialization;
using AvalonDock;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
using System.Windows.Shapes;
using ONet.FAU.Rx._16_128.ViewModels;
using System.Windows.Forms;

namespace ONet.FAU.Rx._16_128.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            #region 最大化最小化关闭
            BtnMin.Click += (s, e) => { this.WindowState = WindowState.Minimized; };

            BtnMax.Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    // 设置窗口的最大高度为无穷大
                    //this.MaxHeight = Double.PositiveInfinity;
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    // 设置窗口的最大高度
                    this.MaxHeight = Screen.PrimaryScreen.WorkingArea.Height;


                    this.WindowState = WindowState.Maximized;


                }
            };

            TitleBorder.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };
            #endregion
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {

                SaveLayout("DockingMain", DockingMain);


            }
            catch (Exception ex)
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is MainViewModel viewModel)
                {
                    if (viewModel.StartupCommand.CanExecute(null))
                    {
                        viewModel.StartupCommand.Execute(null);
                    }
                }


                // 读取配置中的布尔变量
                bool loadLayoutOnStartup = bool.Parse(ConfigurationManager.AppSettings["LoadLayoutOnStartup"]);

                // 根据配置决定是否加载布局
                if (loadLayoutOnStartup)
                {
                    CB_isLoadLayout.IsChecked = true;

                    LoadLayout("DockingMain", DockingMain);

                }
            }
            catch (Exception)
            {


            }
        }

        private void RestoreLayout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RestoreLayout("DockingMain - 副本", DockingMain);
            }
            catch (Exception)
            {


            }
        }

        private void CB_isLoadLayout_Checked(object sender, RoutedEventArgs e)
        {
            SetLoadLayoutOnStartup(true);
        }

        private void CB_isLoadLayout_Unchecked(object sender, RoutedEventArgs e)
        {
            SetLoadLayoutOnStartup(false);
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                viewModel.UserSwitchingCommand.Execute(null);
            }
        }


        #region 加载/保存布局/恢复布局
        private string AvaLonDockFilePath = "D:\\MyApp-Temp\\Layout";
        private void LoadLayout(string layoutFileName, DockingManager dockingManager)
        {
            try
            {
                if (File.Exists(System.IO.Path.Combine(AvaLonDockFilePath, layoutFileName)))
                {
                    var layoutSerializer = new XmlLayoutSerializer(dockingManager);
                    using (var stream = new StreamReader(System.IO.Path.Combine(AvaLonDockFilePath, layoutFileName)))
                    {
                        layoutSerializer.Deserialize(stream);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("加载页面布局失败，布局文件不存在");
                }
            }
            catch (Exception)
            {


            }

        }

        private void SaveLayout(string layoutFileName, DockingManager dockingManager)
        {
            try
            {
                var layoutSerializer = new XmlLayoutSerializer(dockingManager);
                using (var stream = new StreamWriter(System.IO.Path.Combine(AvaLonDockFilePath, layoutFileName)))
                {
                    layoutSerializer.Serialize(stream);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("布局保存失败--" + ex.Message);
            }
        }


        public void RestoreLayout(string layoutFileName, DockingManager dockingManager)
        {
            try
            {

                if (File.Exists(System.IO.Path.Combine(AvaLonDockFilePath, layoutFileName)))
                {
                    var layoutSerializer = new XmlLayoutSerializer(dockingManager);
                    using (var stream = new StreamReader(System.IO.Path.Combine(AvaLonDockFilePath, layoutFileName)))
                    {
                        layoutSerializer.Deserialize(stream);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("加载页面布局失败，布局文件不存在");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SetLoadLayoutOnStartup(bool value)
        {
            try
            {
                // 打开当前应用程序的配置文件
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // 检查键是否存在，如果不存在则添加，如果存在则修改
                if (config.AppSettings.Settings["LoadLayoutOnStartup"] == null)
                {
                    config.AppSettings.Settings.Add("LoadLayoutOnStartup", value.ToString().ToLower());
                }
                else
                {
                    config.AppSettings.Settings["LoadLayoutOnStartup"].Value = value.ToString().ToLower();
                }

                // 保存配置
                config.Save(ConfigurationSaveMode.Modified);

                // 刷新配置，使更改立即生效
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (ConfigurationErrorsException ex)
            {

            }
        }


        #endregion 加载/保存布局


    }
}
