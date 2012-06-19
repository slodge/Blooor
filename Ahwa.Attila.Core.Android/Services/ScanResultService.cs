using System;
using System.Collections.Generic;
using System.Text;
using Ahwa.Attila.Core.Android.Interface;

namespace Ahwa.Attila.Core.Android.Services
{
    public class ScanResultService : IScanResultService
    {
        private string _result;

        public void PushResult(string result)
        {
            _result = result;
        }

        public bool TryPopResult(out string result)
        {
            result = _result;
            _result = null;
            return result != null;
        }
    }
}
