using System.Windows.Forms;
using REPRODUCTOR_MUSICAL.Controllers;
using REPRODUCTOR_MUSICAL.Models;
using REPRODUCTOR_MUSICAL.Services;

namespace REPRODUCTOR_MUSICAL.Infrastructure
{
    public static class ApplicationFactory
    {
        public static Form CreateHomeForm()
        {
            var view = new FrmHome();
            var audioPlayer = CreateAudioPlayer();
            var audioAnalysis = new WavAudioAnalysisService();
            var playerState = new PlayerState();
            var controller = new HomeController(view, audioPlayer, audioAnalysis, playerState);
            controller.Initialize();

            return view;
        }

        private static IAudioPlayerService CreateAudioPlayer()
        {
            try
            {
                return new WindowsMediaAudioPlayerService();
            }
            catch
            {
                return new MciAudioPlayerService();
            }
        }
    }
}
