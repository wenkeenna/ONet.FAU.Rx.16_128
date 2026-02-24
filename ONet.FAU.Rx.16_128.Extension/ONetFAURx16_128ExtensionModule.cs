using DM.Foundation.Shared.Interfaces;
using ONet.FAU.Rx._16_128.Extension.ViewModels;
using ONet.FAU.Rx._16_128.Extension.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
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
            var regionManager = containerProvider.Resolve<IRegionManager>();


            regionManager.RegisterViewWithRegion("Region_ONetMaynuoM8811View", typeof(MaynuoM8811View));
            regionManager.RegisterViewWithRegion("Region_LD9204SView", typeof(LD9204SView));
            regionManager.RegisterViewWithRegion("Region_LD9204S_B_View", typeof(LD9204S_B_View));
            regionManager.RegisterViewWithRegion("Region_GolightOSMWD41310View", typeof(GolightOSMWD41310View));
            regionManager.RegisterViewWithRegion("Region_GolightOSMWD41310View_B", typeof(GolightOSMWD41310_B_View));


            regionManager.RegisterViewWithRegion("OpticalModuleView", typeof(OpticalModuleView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
          

            containerRegistry.RegisterDialog<ONetCoupling1DView, ONetCoupling1DViewModel>("ONetCoupling1DView");
        }
    }
}
