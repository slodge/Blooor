using System;
using Ahwa.Attila.Core.Android.Models;
using Ahwa.Attila.Core.Android.ViewModels.BaseViewModels;
using Ahwa.Attila.Core.Android.ViewModels.ScanViewModels;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.Platform.Tasks;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using System.IO;
using Ahwa.Attila.Core.Android.Interface;

namespace Ahwa.Attila.Core.Android.ViewModels.ProductViewModels
{
    public abstract class BaseEditProductViewModel<T>: BaseEditObjectViewModel<Product>
        , IMvxServiceConsumer<IMvxZXingBarCodeReaderTask>
        , IMvxServiceConsumer<IMvxPictureChooserTask>
        , IMvxServiceConsumer<IScanResultService>
        where T : Product, new()
    {
        private const int IMAGE_SIZE = 640;
        private const int IMAGE_QUALITY = 70;

        private Category _SelectedCategory;

        public Category SelectedCategory
        {
            get { return DataStore.GetObject<Category>(Model.CategoryID); }
            set { Model.CategoryID = value.ID; FirePropertyChanged(() => SelectedCategory); }
        }

        public IObservableCollection<Category> Categories
        {
            get
            {
                var categories = DataStore.GetModelContainer<Category>();
                return categories;
            }
        }

        protected BaseEditProductViewModel(string id):base(id)
        {                        
        }

        public IMvxCommand ReadBarCodeCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    try
                    {
                        this.RequestNavigate<ScanViewModel>();
                        //this.GetService<IMvxZXingBarCodeReaderTask>().ReadBarCode(ProcessBarCode);
                    }
                    catch (Exception exception)
                    {
                        TraceHelper.Trace("There is a problem updating product data. Error description: {0}", exception.ToString());
                    }
                });
            }
        }

        public IMvxCommand TakePictureCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    this.GetService<IMvxPictureChooserTask>().TakePicture(IMAGE_SIZE, IMAGE_QUALITY, SaveImage, ImageAssumeCancelled);
                });
            }
        }   
        
        public IMvxCommand BrowsePictureCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    this.GetService<IMvxPictureChooserTask>().ChoosePictureFromLibrary(IMAGE_SIZE, IMAGE_QUALITY, SaveImage, ImageAssumeCancelled);
                });
            }
        }

        public IMvxCommand SelectCategoryCommand
        {
            get
            {
                //return new MvxRelayCommand<Category>((category) =>
                //{
                //    Model.CategoryID = category.ID;
                //});
                throw new NotImplementedException();
            }
        }
     
        public void UpdateViewModelIfScanResultAvailable()
        {
            var service = this.GetService<IScanResultService>();
            string barCodeValue;
            if (service.TryPopResult(out barCodeValue))
            {
                Model.BarCode = barCodeValue;
            }
        }

        private void SaveImage(Stream image)
        {
            var oldImage = Model.ImagePath;            
            Model.ImagePath = DataStore.SaveImage(Model.ID, image);
            DataStore.DeleteImage(oldImage);            
        }

        private void ImageAssumeCancelled()
        {
        }
    }
}