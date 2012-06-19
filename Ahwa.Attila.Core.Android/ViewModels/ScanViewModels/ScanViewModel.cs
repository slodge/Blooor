using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ahwa.Attila.Core.Android.Interface;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ahwa.Attila.Core.Android.ViewModels.BaseViewModels;
using Ahwa.Attila.Core.Android.Models;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ViewModels;

namespace Ahwa.Attila.Core.Android.ViewModels.ScanViewModels
{
    public class ScanViewModel
        : MvxViewModel
        , IMvxServiceConsumer<IScanResultService>
    {
        public ScanViewModel()
            : base()
        {            
        }

        private bool _scanFound;

        public void Scan(string text)
        {
            if (_scanFound)
            {
                return;
            }

            _scanFound = true;
            this.GetService<IScanResultService>().PushResult(text);
            this.RequestClose(this);
        }
    }
}