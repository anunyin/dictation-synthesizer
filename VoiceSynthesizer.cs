namespace DictationSynthesizer
{
    public interface IVoiceSynthesizer
    {
        //event SpeakStarted;
        //event SpeakCompleted;
        void Speak();
        void SpeakAsync();
        void SpeakToFile();
    }
}
