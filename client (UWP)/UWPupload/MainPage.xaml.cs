using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPupload
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CancellationTokenSource cts;
        private string uploadURL = "http://localhost:3000/upload"; //nodejs server
        public MainPage()
        {
            this.InitializeComponent();
            cts = new CancellationTokenSource();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            { Uri uri = new Uri(uploadURL);
                UploadSingleFile(uri, file);

            }


        }

        private async void UploadSingleFile(Uri uri, Windows.Storage.StorageFile file)
        {
            var httpClientUpload = new HttpClient();


            if (file == null)
            {
                cts = new CancellationTokenSource();
  
                return;
            }

          
            Stream stream = new MemoryStream();
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                fileStream.AsStream().CopyTo(stream);
            }

            try
            {
                IProgress<HttpProgress> progress = new Progress<HttpProgress>(UploadProgressHandler);
                HttpStreamContent streamContent = new HttpStreamContent(await file.OpenReadAsync());
                streamContent.Headers.ContentDisposition = new HttpContentDispositionHeaderValue("attachment")
                {
                    Name = "\"" + file.Name + "\"",
                    FileName = "\"" + file.Name + "\"",
                    
                };


                HttpMultipartFormDataContent form = new HttpMultipartFormDataContent { { streamContent, "file", file.Name } };
                       HttpResponseMessage responseUpload = await httpClientUpload.PostAsync(uri, form).AsTask(cts.Token, progress);
            }
            catch (TaskCanceledException)
            {

                return;
            }
            catch (Exception ex)
            {

                return;
            }
        }

        private void UploadProgressHandler(HttpProgress progress)
        {
            ulong totalBytesToSend = 0;
            if (progress.TotalBytesToSend.HasValue)
            {
                //Total to send
                totalBytesToSend = progress.TotalBytesToSend.Value;
            }
            else
            {

            }
            ulong totalBytesToReceive = 0;
            if (progress.TotalBytesToReceive.HasValue)
            {
                totalBytesToReceive = progress.TotalBytesToReceive.Value;
              
            }
            

            double requestProgress = 0;
            if (progress.Stage == HttpProgressStage.SendingContent && totalBytesToSend > 0)
            {
                requestProgress = progress.BytesSent * 100 / totalBytesToSend;
            }
            else if (progress.Stage == HttpProgressStage.ReceivingContent)
            {
                // Start with 50 percent, request content was already sent.
                requestProgress += 50;

                if (totalBytesToReceive > 0)
                {
                    requestProgress += progress.BytesReceived * 100 / totalBytesToReceive;
                }
            }
            else
            {
                return;
            }

           // var progress = requestProgress;


        }


    }
}
