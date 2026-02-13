using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Licensing;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Parameters.Services;
using DM.Foundation.Shared.Enums;
using DM.Foundation.Shared.Events;
using DM.Foundation.Shared.Interfaces;
using DM.ManualMotionControl.Delegates;
using DM.UserManagement.Models;
using ONet.FAU.Rx._16_128.Initialization;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ONet.FAU.Rx._16_128.ViewModels
{
    public class MainViewModel : BindableBase
    {
        /// <summary>
        /// 软件启动命令
        /// </summary>
        public DelegateCommand<object> StartupCommand { get; private set; }

        public DelegateCommand<object> AppCloseCommand { get; private set; }

        public DelegateCommand<object> MenuCommand { get; private set; }

        private bool islicensevalid;
        public bool IsLicenseValid
        {
            get { return islicensevalid; }
            set { islicensevalid = value; RaisePropertyChanged(); }
        }

        public DelegateCommand<object> UserSwitchingCommand { get; private set; }

        public DelegateCommand<object> CalibrationToolCommand { get; private set; }

        private readonly IEventAggregator _eventAggregator;

        private IDialogService _dialogService;
        private ILogger _logger;
        private readonly IContainerProvider _container;
        private IDataBindingContext _dataBinding;
        private IMotionSystemService _motionSystem;
        private IParameterInitializer _initializer;
        private IGlobalConfigService _configService;
        public IRuntimeContext _runtimeContext { get; set; }


        public MainViewModel(IEventAggregator eventAggregator,
                           IMotionSystemService motionSystem,
                           IDialogService dialogService,
                           IDataBindingContext dataBinding,
                           IParameterInitializer initializer,
                           IGlobalConfigService configService,
                           IContainerProvider container,
                           IRuntimeContext runtimeContext,
                           ILogger logger)
        {
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _motionSystem = motionSystem;
            _initializer = initializer;
            _dataBinding = dataBinding;
            _configService = configService;
            _logger = logger;

            _container = container;
            _runtimeContext = runtimeContext;



            StartupCommand = new DelegateCommand<object>(OnStartup);
            AppCloseCommand = new DelegateCommand<object>(OnAppCloseCommandAsync);

            MenuCommand = new DelegateCommand<object>(OnMenuCommand);

            UserSwitchingCommand = new DelegateCommand<object>(OnUserSwitchingCommand);
            CalibrationToolCommand = new DelegateCommand<object>(OnCalibrationToolCommand);

            DM.DataVisualization.Views.PlotPanelView.ChartCount = 2;
        }

        private void OnCalibrationToolCommand(object obj)
        {
            _dialogService.Show("CalibrationView", null, null);
        }

        private void OnUserSwitchingCommand(object obj)
        {
            _dialogService.ShowDialog("UserSwitchingView", null, OnUserSwitchCallBack);
        }

        private void OnUserSwitchCallBack(IDialogResult result)
        {
            if (result.Result == ButtonResult.OK)
            {
                var userinfo = result.Parameters.GetValue<User>("LoggedInUser");
                _runtimeContext.CurrentUser = userinfo.UserName;
            }
        }

        private void OnMenuCommand(object obj)
        {
            switch (obj.ToString())
            {
                case "设备参数":
                    DialogParameters para = new DialogParameters();
                    para.Add("ParameterCategory", ParameterCategory.Device);
                    _dialogService.ShowDialog("ParameterView", para, null);
                    break;
                case "产品参数":
                    DialogParameters para1 = new DialogParameters();
                    para1.Add("ParameterCategory", ParameterCategory.Product);
                    _dialogService.ShowDialog("ParameterView", para1, null);
                    break;
                case "数据绑定":
                    DialogParameters para2 = new DialogParameters();
                    para2.Add("ParameterCategory", ParameterCategory.Product);
                    _dialogService.ShowDialog("DataBindingView");
                    break;
                case "用户管理":
                    _dialogService.ShowDialog("UserManagementView");
                    break;


            }
        }

        private async void OnAppCloseCommandAsync(object obj)
        {
            // 显示一个包含“确定”按钮的消息框
            MessageBoxResult result = System.Windows.MessageBox.Show("确定要退出吗？", "确认", MessageBoxButton.OKCancel);


            SaveOpenedProjectName();


            // 判断用户点击了“确定”按钮
            if (result == MessageBoxResult.OK)
            {
                var releasables = _container.Resolve<IEnumerable<IAppResourceReleasable>>();
                foreach (var service in releasables)
                {
                    try
                    {
                        await service.ReleaseAsync();
                    }
                    catch (Exception ex)
                    {
                        // 记录日志或忽略
                    }
                }


                await Task.Delay(1000);
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void OnStartup(object obj)
        {
            try
            {
                var licensePath = _configService.GetConfigPath("LicensePath");//获取设备参数文件路径

                LicenseManager.EnsureHardwareIdExists(licensePath);//检查是否有设备信息文件

                IsLicenseValid = LicenseManager.ValidateLicense(licensePath, _eventAggregator);//验证license
                //IsLicenseValid = true;
                if (!IsLicenseValid)
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("授权验证失败，请联系供应商。");
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<Event_Message>().Publish(ex.Message);
            }



            try
            {
                //创建手动轴控制布局
                ManualMotionDelegates.CreateAxisGroups = (onGroupActivated, onSyncModeSet) =>
                {
                    return MotionSystemInitializer.InitialAxisGroupView(_motionSystem, _eventAggregator, onGroupActivated, onSyncModeSet);
                };

                var temple = _initializer.InitDeviceParameters();//加载设备参数模板

                var path = _configService.GetConfigPath("DevicePara");//获取设备参数文件路径

                var Parameters = ParameterStorageService.LoadAndMerge(path, temple);//合并本地参数和模板参数

                //加载进数据绑定容器
                foreach (var parameter in Parameters)
                {
                    if (parameter.IsRoot)
                    {
                        foreach (var childPara in parameter.Children)
                        {
                            _dataBinding.Set(parameter.Name, childPara.Name, childPara);
                        }

                    }
                }


                string lastProject = ConfigurationManager.AppSettings["LastOpenedProject"];//从运行根目录的App.config中获取上次加载项目名称
                //temple = _initializer.InitProductParameters();//获取产品参数模板
                path = _configService.GetConfigPath("TaskFlow");//获取流程文件目录，产品参数跟随流程文件创建
                Parameters = ParameterStorageService.Load(path, lastProject);//合并本地产品参数和模板参数

                //加载进数据绑定容器
                foreach (var parameter in Parameters)
                {
                    if (parameter.IsRoot)
                    {
                        foreach (var childPara in parameter.Children)
                        {
                            _dataBinding.Set(parameter.Name, childPara.Name, childPara);
                        }
                    }
                }
                _eventAggregator.GetEvent<AppStartUpEvent>().Publish($"TaskFlow:{lastProject}");//发布软件加载完成事件
            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"{ex.Message}");
            }
        }


        /// <summary>
        /// 保存加载项目名称
        /// </summary>
        private void SaveOpenedProjectName()
        {
            try
            {
                // 打开当前应用程序的配置文件
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // 检查键是否存在，如果不存在则添加，如果存在则修改
                if (config.AppSettings.Settings["LastOpenedProject"] == null)
                {
                    config.AppSettings.Settings.Add("LastOpenedProject", _runtimeContext.CurrentRecipeName);
                }
                else
                {
                    config.AppSettings.Settings["LastOpenedProject"].Value = _runtimeContext.CurrentRecipeName;
                }

                // 保存配置
                config.Save(ConfigurationSaveMode.Modified);

                // 刷新配置，使更改立即生效
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {


            }

        }

    }
}
