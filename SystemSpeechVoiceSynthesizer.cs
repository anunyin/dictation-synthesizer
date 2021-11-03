using System.Speech.Synthesis;

namespace DictationSynthesizer
{
    public class SystemSpeechVoiceSynthesizer : IVoiceSynthesizer
    {
        private SpeechSynthesizer synth;

        void IVoiceSynthesizer.Speak()
        {
            
        }

        void IVoiceSynthesizer.SpeakAsync()
        {
            
        }

        void IVoiceSynthesizer.SpeakToFile()
        {
            throw new System.NotImplementedException();
        }
    }
}
