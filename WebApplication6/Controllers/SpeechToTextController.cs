using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;

namespace WebApplication6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeechToTextController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post(IFormFile audioFile)
        {
            try
            {
                // Azure Cognitive Services özelliklerinin ayarlanması
                var subscriptionKey = "00867fd22e0349d7b7f4677aeaaa14cb"; // Azure portalından alınan abonelik anahtarı
                var region = "westeurope";

                // Geçici dosyaya ses dosyasının kaydedilmesi (MP3 veya MP4 dosyası)
                var tempFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                // Dosyanın .wav formatına dönüştürülüp yeni dosya yolu alınması
                var audioFilePath = tempFilePath;
                if (!Path.GetExtension(audioFilePath).Equals(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    var tempWavFilePath = Path.ChangeExtension(tempFilePath, ".wav");
                    ConvertToWav(tempFilePath, tempWavFilePath);
                    audioFilePath = tempWavFilePath;
                }

                // SpeechRecognizer nesnesinin oluşturulması
                var config = SpeechConfig.FromSubscription(subscriptionKey, region);
                config.SpeechRecognitionLanguage = "tr-TR"; // Türkçe dili
                using var recognizer = new SpeechRecognizer(config, AudioConfig.FromWavFileInput(audioFilePath));

                // Tanıma işleminin başlatılması
                var result = await recognizer.RecognizeOnceAsync();

                // Sonuçların alınması
                string recognizedText;
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    recognizedText = result.Text;
                }
                else
                {
                    recognizedText = $"Hata: {result.Reason}";
                }

                // Dönüştürme işlemi tamamlandıktan sonra dosyaların serbest bırakılması veya silinmesi
                recognizer.Dispose();

                FileInfo tempFile = new FileInfo(tempFilePath);
                tempFile.Delete();

                if (!Path.GetExtension(audioFilePath).Equals(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    FileInfo wavFile = new FileInfo(audioFilePath);
                    wavFile.Delete();
                }

                // Sonucun JSON formatında döndürdürülmesi
                return Ok(new { Text = recognizedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        private void ConvertToWav(string inputPath, string outputPath)
        {
            using (var reader = new MediaFoundationReader(inputPath))
            {
                WaveFileWriter.CreateWaveFile(outputPath, reader);
            }
        }
    }
}
