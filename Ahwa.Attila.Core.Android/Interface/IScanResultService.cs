namespace Ahwa.Attila.Core.Android.Interface
{
    public interface IScanResultService
    {
        void PushResult(string result);
        bool TryPopResult(out string result);
    }
}