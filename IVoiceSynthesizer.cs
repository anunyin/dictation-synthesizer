using System;
using System.Speech.Synthesis;

namespace DictationSynthesizer
{
    public interface IVoiceSynthesizer
    {
        event EventHandler SpeakStarted;
        event EventHandler SpeakCompleted;

        void Speak(PromptBuilder b);
        void SpeakAsync(PromptBuilder b);
        void SpeakToFile(PromptBuilder b, string path);
    }
}
