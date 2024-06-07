using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;

namespace WebApplication6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextToSpeechController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] TextToSpeechRequest request)
        {
            try
            {
                // Azure Cognitive Services özelliklerinin ayarlanması
                var subscriptionKey = "00867fd22e0349d7b7f4677aeaaa14cb"; // Azure portalından alınan abonelik anahtarı
                var region = "westeurope"; 

                // TextToSpeech nesnesinin oluşturulması
                var config = SpeechConfig.FromSubscription(subscriptionKey, region);
                config.SpeechSynthesisLanguage = "tr-TR";
                using var synthesizer = new SpeechSynthesizer(config);

                // Metnin sese dönüştürülmesi
                using var stream = new MemoryStream();
                var result = await synthesizer.SpeakTextAsync(request.Text);

                // Sonucun dönüştürülmesi
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    // MemoryStream içeriğinin geri döndürülmesi
                    return File(result.AudioData, "audio/mpeg", "output.mp3");
                }
                else
                {
                    return BadRequest(new { Error = result.Reason.ToString() });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }

    public class TextToSpeechRequest
    {
        public string Text { get; set; }
    }
}

