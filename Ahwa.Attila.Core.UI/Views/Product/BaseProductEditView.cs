
using Ahwa.Attila.Core.Android.ViewModels.BaseViewModels;
using Ahwa.Attila.Core.Android.ViewModels.ProductViewModels;
using Android.Views;

namespace Ahwa.Attila.UI.Android.Views.Product
{
    public class BaseProductEditView<TViewModel> :
        BaseView<TViewModel>
        where TViewModel : BaseEditProductViewModel<Attila.Core.Android.Models.Product>
    {
        private const int save_menu = Resource.Id.save_product;
        private const int content = Resource.Layout.ProductEditView;

        private readonly int _whichMenu;

        public BaseProductEditView(int whichMenu)
        {
            _whichMenu = whichMenu;
        }

        protected override void OnViewModelSet()
        {
            SetContentView(content);            
        }

        protected override void OnCreate(global::Android.OS.Bundle bundle)
        {
            base.OnCreate(bundle);

            ViewModel.UpdateViewModelIfScanResultAvailable();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(_whichMenu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case save_menu:
                    ViewModel.SaveCommand.Execute();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}