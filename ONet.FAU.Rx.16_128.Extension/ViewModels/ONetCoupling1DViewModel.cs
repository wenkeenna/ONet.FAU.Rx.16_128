using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Shared.Enums;
using DM.Foundation.Shared.Events;
using DM.Foundation.Shared.Interfaces;
using ONet.FAU.Rx._16_128.Extension.Model;
using ONet.FAU.Rx._16_128.Extension.Services.Coupling;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.ViewModels
{
    public class ONetCoupling1DViewModel : IDialogAware
    {
        public ONetCoupling1D ToolIns { get; set; }
        /// <summary>
        /// 取消命令
        /// </summary>
        public DelegateCommand<object> CancelCommand { get; private set; }
        /// <summary>
        ///确认命令
        /// </summary>
        public DelegateCommand<object> ConfirmCommand { get; private set; }

        IEventAggregator _eventAggregator;
        IDialogService _dialogService;
        IMotionSystemService _motionSystemService;

        public event Action<IDialogResult> RequestClose;

        public List<string> AxisList { get; private set; }

        public AxisGroup[] AxisGroupEnumValues => (AxisGroup[])Enum.GetValues(typeof(AxisGroup));
        public int[] Channels { get; set; }


        public ONetCoupling1DViewModel(IMotionSystemService motionSystemService, IEventAggregator eventAggregator, IDialogService dialogService)
        {
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _motionSystemService = motionSystemService;

            ToolIns = new ONetCoupling1D();

            CancelCommand = new DelegateCommand<object>(OnCancel);
            ConfirmCommand = new DelegateCommand<object>(OnConfirm);


            AxisList = new List<string>();
            AxisList.Clear();
            AxisList.AddRange(_motionSystemService.GetAllAxisKeys());

            Channels = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        }

        private void OnConfirm(object obj)
        {
            Event_ToolToTaskList event_ToolToTaskList = new Event_ToolToTaskList();

            event_ToolToTaskList.toolEditMode = editMode;

            event_ToolToTaskList.toolbase = ToolIns;

            _eventAggregator.GetEvent<Event_ToolToTaskList>().Publish(event_ToolToTaskList);

            RequestClose.Invoke(new DialogResult(ButtonResult.OK));
        }

        private void OnCancel(object obj)
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
        }


        #region IDialogAware接口实现
        public ToolEditMode editMode { get; private set; }

        public string Title { get; set; }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            editMode = parameters.GetValue<ToolEditMode>("EditMode");
            object toolIns = parameters.GetValue<IToolBase>("ToolIns");
            if (editMode == ToolEditMode.Edit)
            {
                ToolIns = (ONetCoupling1D)toolIns;
            }
        }
        #endregion





    }
}
