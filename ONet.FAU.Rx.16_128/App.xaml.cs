using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.DataBinding.Services;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Logging.LogManager;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Shared.Interfaces;
using DM.Foundation.Shared.Models;
using DM.InstrumentKit.Services;
using DM.TrayMap.Models;
using DM.TrayMap.Services;
using DM.UserManagement.Models;
using DM.UserManagement.Services;
using DM.Vision.Interfaces;
using DM.Vision.Services;
using DryIoc;
using ONet.FAU.Rx._16_128.Extension.Services;
using ONet.FAU.Rx._16_128.Initialization;
using ONet.FAU.Rx._16_128.ViewModels;
using ONet.FAU.Rx._16_128.Views;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ONet.FAU.Rx._16_128
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var eventAggregator = containerRegistry.GetContainer().Resolve<IEventAggregator>();

            string basePath = "D:\\MyApp";

            var globalConfigService = new GlobalConfigService(/*传参*/);
            globalConfigService.SetConfigPath("TaskFlow", $"{basePath}\\Solutions");//解决方案路径
            globalConfigService.SetConfigPath("NLogLogger", $"{basePath}\\\\Config\\NLog\\NLogConfig.config");//日志配置文件路径
            globalConfigService.SetConfigPath("DevicePara", $"{basePath}\\Config\\DeviceParameter\\DeviceParameter.json");//设备参数文件路径
            globalConfigService.SetConfigPath("LicensePath", $"{basePath}\\Config\\License");//软件授权文件路径

            globalConfigService.SetConfigPath("ToolBaseDll", "DM.ToolBase.dll");
            globalConfigService.SetConfigPath("ToolBaseDllEx", "ONet.FAU.Rx.16_128.Extension.dll");//

            globalConfigService.SetConfigPath("ControlCard", $"{basePath}\\Config");//控制卡配置文件
            globalConfigService.SetConfigPath("VisionMaster", $"{basePath}\\VisionMaster");//海康图像处理解决方案

            containerRegistry.RegisterInstance<IGlobalConfigService>(globalConfigService);//配置文件路径注入

            containerRegistry.RegisterSingleton<ILogger>(() => new NLogLogger(globalConfigService.GetConfigPath("NLogLogger")));//日志实例注入

            var logger = containerRegistry.GetContainer().Resolve<ILogger>();//从容器获取日志实例

            containerRegistry.RegisterSingleton<AD8622Helper>(() => new AD8622Helper());//三轴压力传感器注入

            containerRegistry.RegisterSingleton<UPS81BHelper>(() => new UPS81BHelper(logger));//UV灯注入

            containerRegistry.RegisterSingleton<DC65LightSourceHelper>(() => new DC65LightSourceHelper(logger));//视觉光源控制器注入


            TrayGridService trayGridService = new TrayGridService(eventAggregator);
            trayGridService.AddTray(
                new TrayConfig
                {
                    Id = "Tray1",
                    Rows = 4,
                    Columns = 4,
                    CellWidth = 15.0,
                    CellHeight = 15.0,
                    OriginX = 0,
                    OriginY = 0
                });

            trayGridService.AddTray(
                new TrayConfig
                {
                    Id = "Tray2",
                    Rows = 4,
                    Columns = 4,
                    CellWidth = 15.0,
                    CellHeight = 15.0,
                    OriginX = 0,
                    OriginY = 0
                });

            containerRegistry.RegisterInstance<ITrayGridService>(trayGridService);//托盘注入

            containerRegistry.RegisterSingleton<UserService>(() => new UserService("D:\\MyApp-Temp\\Config\\Users.db"));//用户数据库文件路径

            //var motionSystemService = MotionSystemInitializer.Initialize(eventAggregator);//初始化轴实例-在打样机台测试

            var motionSystemService = MotionSystemInitializer.Initialize(eventAggregator, logger);//初始化轴实例-在传承机台测试

            containerRegistry.RegisterInstance<IMotionSystemService>(motionSystemService);//轴实例注入

            var maynuoM8811 = new MaynuoM8811Helper();
            containerRegistry.RegisterInstance<MaynuoM8811Helper>(maynuoM8811);//源表实例注入

            containerRegistry.RegisterSingleton<CalibrationServices>();//标定转换服务注入

            //var lightmodule800G = new LightModule800GHelper(eventAggregator);
            //containerRegistry.RegisterInstance<LightModule800GHelper>(lightmodule800G);//光模块服务注入

            containerRegistry.RegisterSingleton<IParameterInitializer, ParameterInitializer>();//参数模板注入

            containerRegistry.RegisterSingleton<IRuntimeContext>(() => new RuntimeContext());//运行过程中数据

            var dataBindingContext = new DataBindingContext();
            containerRegistry.RegisterInstance<IDataBindingContext>(dataBindingContext);//单例注册数据绑定容器


            var calib = containerRegistry.GetContainer().Resolve<CalibrationServices>();
            var visionProcess = new VisionProcess(logger, eventAggregator, globalConfigService.GetConfigPath("VisionMaster"), "VisionSolution.sol", dataBindingContext, calib);
            containerRegistry.RegisterInstance<IVisionProcess>(visionProcess);//注册图像处理实例




            var runtimeContext = containerRegistry.GetContainer().Resolve<IRuntimeContext>();

            var ad8622 = containerRegistry.GetContainer().Resolve<AD8622Helper>();

            var ups81b = containerRegistry.GetContainer().Resolve<UPS81BHelper>();

            var traygrid = containerRegistry.GetContainer().Resolve<ITrayGridService>();
            //var calib = containerRegistry.GetContainer().Resolve<CalibrationServices>();
            var dc65 = containerRegistry.GetContainer().Resolve<DC65LightSourceHelper>();


            var m8811 = containerRegistry.GetContainer().Resolve<MaynuoM8811Helper>();

            ToolExecutionContext toolExecutionContext = new ToolExecutionContext();
            toolExecutionContext.Set("DataBindingContext", dataBindingContext); //将数据绑定容器添加到工具执行上下文
            toolExecutionContext.Set("IMotionSystemService", motionSystemService);//获取控制卡相关实例，包括轴实例、输入、输出
            toolExecutionContext.Set("IVisionProcess", visionProcess);//添加图像处理实例到工具执行参数
            toolExecutionContext.Set("IRuntimeContext", runtimeContext);//实时运行数据
            toolExecutionContext.Set("AD8622Helper", ad8622);//压力传感器
            toolExecutionContext.Set("UPS81BHelper", ups81b);//UV灯
            toolExecutionContext.Set("ILogger", logger);//日志
            toolExecutionContext.Set("ITrayGridService", traygrid);//托盘
            toolExecutionContext.Set("CalibrationServices", calib);//标定类
            toolExecutionContext.Set("DC65LightSourceHelper", dc65);//光源
            //toolExecutionContext.Set("LightModule800GHelper", lightmodule);//光模块
            toolExecutionContext.Set("MaynuoM8811Helper", m8811);//电源

            containerRegistry.RegisterInstance<ToolExecutionContext>(toolExecutionContext);//注册工具执行参数，包括视觉处理，仪表，轴控等实例
        }



        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<DM.DataVisualization.DataVisualization_Module>();//数据展示模块

            moduleCatalog.AddModule<DM.TaskFlow.TaskFlow_Module>();//任务流程模块

            moduleCatalog.AddModule<DM.ToolBase.ToolBase_Module>();//基础工具模块

            moduleCatalog.AddModule<DM.Foundation.Logging.Logging_Module>();//日志模块

            moduleCatalog.AddModule<DM.Foundation.Parameters.Parameters_Module>();//参数模块

            moduleCatalog.AddModule<DM.Foundation.DataBinding.DataBinding_Module>();//数据绑定模块

            moduleCatalog.AddModule<DM.Vision.Vision_Module>();//视觉模块

            //  moduleCatalog.AddModule<DM.AnalogCamLib.AnalogCam_Module>();//模拟信号相机模块

            //moduleCatalog.AddModule<DM.Foundation.UserPermission.PermissionModule>();//用户权限模块


            moduleCatalog.AddModule<DM.UserManagement.UserManagement_Module>();


            moduleCatalog.AddModule<DM.LoginModule.Login_Module>();


            moduleCatalog.AddModule<DM.ManualMotionControl.ManualMotion_Module>();//手动轴控制模块

            moduleCatalog.AddModule<DM.InstrumentKit.InstrumentKit_Module>();//仪表模块

            moduleCatalog.AddModule<DM.TrayMap.TrayModule>();//上料托盘模块


        }


        #region 登录窗口
        private static System.Threading.Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new System.Threading.Mutex(true, "OnlyRun_DaMu");
            if (mutex.WaitOne(0, false))
            {
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show("程序已经在运行！", "提示");
                this.Shutdown();
            }
        }


        IDialogParameters parameters { get; set; }
        IDialogService dialog;
        protected override void OnInitialized()
        {
            try
            {
                base.OnInitialized();
                var regionManager = Container.Resolve<IRegionManager>();
                //regionManager.RegisterViewWithRegion("Region_ProductionInfoView", typeof(ProductionInfoView));

                dialog = Container.Resolve<IDialogService>();

                dialog.ShowDialog("LoginView", parameters, Callback);
            }
            catch (Exception)
            {


            }
        }


        private void Callback(IDialogResult result)
        {
            if (result.Result == ButtonResult.OK)
            {

                var loggedInUser = result.Parameters.GetValue<User>("LoggedInUser");

                var runtimecontext = Container.Resolve<IRuntimeContext>();

                runtimecontext.CurrentUser = loggedInUser.UserName;

                // 获取主窗口的 ViewModel 并传递用户
                var mainWindow = (MainView)Current.MainWindow;
                var mainViewModel = (MainViewModel)mainWindow.DataContext;
                //给主窗体传值
                base.OnInitialized();
                //return;
            }
            else
            {
                Environment.Exit(0);
            }
        }
        #endregion
    }

}
