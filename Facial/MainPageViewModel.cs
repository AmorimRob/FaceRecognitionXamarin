using System;
using System.Linq;
using System.Windows.Input;
using MvvmHelpers;
using Plugin.Media;
using Xamarin.Forms;

namespace Facial
{
    public class MainPageViewModel : BaseViewModel
    {
        public ICommand TirarFotoCommand { get; set; }
        public ICommand IdentificarEmoçãoCommand { get; set; }

        ImageSource foto;
        public ImageSource Foto
        {
            get => foto;
            set => SetProperty(ref foto, value);
        }

        private string Arquivo { get; set; }

        string emoção;
        public string Emoção
        {
            get => emoção;
            set => SetProperty(ref emoção, value);
        }


        public MainPageViewModel()
        {
            TirarFotoCommand = new Command(async () =>
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    Console.Write(@"Camera indisponível");
                    return;
                }

                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "Exemplo",
                    Name = "teste.jpg"
                });

                if (file == null)
                    return;

                Arquivo = file.Path;

                Foto = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    file.Dispose();
                    return stream;
                });
            });

            IdentificarEmoçãoCommand = new Command(async () =>
            {
                var resultado = await new FaceRecognitionService().MakeAnalysisRequest(Arquivo);
                Emoção = resultado.FirstOrDefault().FaceAttributes.CurrentEmotion();
               await App.Current.MainPage.DisplayAlert("Emoção da foto", Emoção, "OK");
            });
        }
    }
}
