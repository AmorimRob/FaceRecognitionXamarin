using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;
using MvvmHelpers;
using Plugin.Media;
using Xamarin.Forms;

namespace Facial
{
    public class MainPageViewModel : BaseViewModel
    {
        HubConnection hubConnection;

        public ICommand TirarFotoCommand { get; set; }
        public ICommand IdentificarIdadeCommand { get; set; }

        ImageSource foto;
        public ImageSource Foto
        {
            get => foto;
            set => SetProperty(ref foto, value);
        }

        bool _possuiResultado;
        public bool PossuiResultado
        {
            get => _possuiResultado;
            set => SetProperty(ref _possuiResultado, value);
        }

        bool _buscandoIdade;
        public bool BuscandoIdade
        {
            get => _buscandoIdade;
            set => SetProperty(ref _buscandoIdade, value);
        }

        private string Arquivo { get; set; }

        public MainPageViewModel()
        {
            BuscandoIdade = false;
            PossuiResultado = false;

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

            IdentificarIdadeCommand = new Command(async () =>
            {
                BuscandoIdade = true;
                PossuiResultado = false;

                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("EnviarIdade", "Identificando...");
                var resultado = await new FaceRecognitionService().MakeAnalysisRequest(Arquivo);
                var idade = resultado.FirstOrDefault().FaceAttributes.Idade;
                await hubConnection.InvokeAsync("EnviarIdade", idade);

                BuscandoIdade = false;
                PossuiResultado = true;

                await hubConnection.StopAsync();
            });

            hubConnection = new HubConnectionBuilder()
            .WithUrl($"https://suaidadeserver.azurewebsites.net/faceHub")
            .Build();
        }
    }
}
