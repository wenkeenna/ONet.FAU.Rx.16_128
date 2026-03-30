using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Shared.Enums;
using DM.Foundation.Shared.Events;
using DM.Foundation.Shared.Interfaces;
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
    public class ONetLensVisionCorrectionViewModel : IDialogAware
    {

        public ONetLensVisionCorrection ToolIns { get; set; }
        /// <summary>
        /// 取消命令
        /// </summary>
        public DelegateCommand<object> CancelCommand { get; private set; }
        /// <summary>
        ///确认命令
        /// </summary>
        public DelegateCommand<object> ConfirmCommand { get; private set; }

        public DelegateCommand<object> BindingDataCommand { get; private set; }

        private string BindingCommandPara { get; set; }

        IEventAggregator _eventAggregator;
        IDialogService _dialogService;
        IMotionSystemService _motionSystemService;

        public event Action<IDialogResult> RequestClose;

        public ONetLensVisionCorrectionViewModel(IMotionSystemService motionSystemService, IEventAggregator eventAggregator, IDialogService dialogService)
        {
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _motionSystemService = motionSystemService;

            ToolIns = new ONetLensVisionCorrection();

            CancelCommand = new DelegateCommand<object>(OnCancel);
            ConfirmCommand = new DelegateCommand<object>(OnConfirm);


            BindingDataCommand = new DelegateCommand<object>(OnBindingDataCommand);
        }
        private void OnBindingDataCommand(object obj)
        {

            BindingCommandPara = obj.ToString();

            IDialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("UserDefined", ToolIns.Parameter.UserDefined);
            _dialogService.ShowDialog("DataBindingView", dialogParameters, OnDataBindingCallback);
        }
        private void OnDataBindingCallback(IDialogResult result)
        {
            try
            {
                var name = result.Parameters.GetValue<string>("Name");
                var parentName = result.Parameters.GetValue<string>("ParentName");

                if (BindingCommandPara == "Str_PIC_A_X")
                {
                    ToolIns.Str_PIC_A_X = $"{parentName}.{name}";
                }
                if (BindingCommandPara == "Str_PIC_A_Y")
                {
                    ToolIns.Str_PIC_A_Y = $"{parentName}.{name}";
                }
                if (BindingCommandPara == "Str_PIC_A_Angle")
                {
                    ToolIns.Str_PIC_A_Angle = $"{parentName}.{name}";
                }


                if (BindingCommandPara == "Str_PIC_B_X")
                {
                    ToolIns.Str_PIC_B_X = $"{parentName}.{name}";
                }
                if (BindingCommandPara == "Str_PIC_B_Y")
                {
                    ToolIns.Str_PIC_B_Y = $"{parentName}.{name}";
                }
                if (BindingCommandPara == "Str_PIC_B_Angle")
                {
                    ToolIns.Str_PIC_B_Angle = $"{parentName}.{name}";
                }


                if (BindingCommandPara == "OffSet_PIC_A_X")
                {
                    ToolIns.OffSet_PIC_A_X = $"{parentName}.{name}";
                }
                if (BindingCommandPara == "OffSet_PIC_A_Y")
                {
                    ToolIns.OffSet_PIC_A_Y = $"{parentName}.{name}";
                }

                if (BindingCommandPara == "OffSet_PIC_B_X")
                {
                    ToolIns.OffSet_PIC_B_X = $"{parentName}.{name}";
                }
                if (BindingCommandPara == "OffSet_PIC_B_Y")
                {
                    ToolIns.OffSet_PIC_B_Y = $"{parentName}.{name}";
                }



                if (BindingCommandPara == "PICA_GX")
                {
                    ToolIns.LeftPara.X_Str = $"{parentName}.{name}";
                }
                if (BindingCommandPara == "PICA_GY")
                {
                    ToolIns.LeftPara.Y_Str = $"{parentName}.{name}";
                }


                if (BindingCommandPara == "PICB_GX")
                {
                    ToolIns.RightPara.X_Str = $"{parentName}.{name}";
                }
                if (BindingCommandPara == "PICB_GY")
                {
                    ToolIns.RightPara.Y_Str = $"{parentName}.{name}";
                }

            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish(ex.ToString());
            }


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
                ToolIns = (ONetLensVisionCorrection)toolIns;
            }
        }
        #endregion




    }
}
