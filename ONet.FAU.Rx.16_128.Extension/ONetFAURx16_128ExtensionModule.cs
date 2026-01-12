using ONet.FAU.Rx._16_128.Extension.ViewModels;
using ONet.FAU.Rx._16_128.Extension.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension
{
    public class ONetFAURx16_128ExtensionModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ONetCoupling1DView, ONetCoupling1DViewModel>("ONetCoupling1DView");
        }
    }
}
