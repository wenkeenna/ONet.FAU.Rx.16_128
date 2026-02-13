using DM.Foundation.DataBinding.Interfaces;
using ONet.FAU.Rx._16_128.Extension.Common;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ONet.FAU.Rx._16_128.Extension.ViewModels
{
    public class OpticalModuleViewModel : BindableBase, IDestructible
    {
       
        private Brush _displayColor = Brushes.Gray;

        private IEventAggregator _eventAggregator = null;
 
        private IDataBindingContext _dataBinding;

        private CancellationTokenSource _loopCts;

        private int _selectedGroupIndex;
      //  private readonly ChannelCurrentViewModel[] _channels;
        private bool _isGroup0Selected = true;
        private bool _isGroup1Selected;
        private bool _isGroup2Selected;
        private bool _isGroup3Selected;
        private bool _isGroup4Selected;
        private bool _isGroup5Selected;
        private bool _isGroup6Selected;
        private bool _isGroup7Selected;


        public OpticalModuleViewModel(IEventAggregator eventAggregator, IContainerProvider containerProvider, IDataBindingContext dataBinding) 
        {
                
        }

        public void Destroy()
        {
           
        }



    }
}
