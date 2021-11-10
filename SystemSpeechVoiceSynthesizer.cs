using NAudio.Lame;
using NAudio.Wave;
using System;
using System.IO;
using System.Speech.Synthesis;

namespace DictationSynthesizer
{
    public class SystemSpeechVoiceSynthesizer : IVoiceSynthesizer
    {
        public event EventHandler SpeakStarted;
        public event EventHandler SpeakCompleted;

        private SpeechSynthesizer synth;

        public SystemSpeechVoiceSynthesizer()
        {
            synth = new SpeechSynthesizer();
            synth.SpeakStarted += (o, s) => SpeakStarted?.Invoke(this, s);
            synth.SpeakCompleted += (o, s) => SpeakCompleted?.Invoke(this, s);
        }

        void IVoiceSynthesizer.Speak(PromptBuilder b)
        {
            synth.SetOutputToDefaultAudioDevice();
            synth.Speak(b);
        }

        void IVoiceSynthesizer.SpeakAsync(PromptBuilder b)
        {
            synth.SetOutputToDefaultAudioDevice();
            synth.SpeakAsync(b);
        }

        void IVoiceSynthesizer.SpeakToFile(PromptBuilder b, string path)
        {
            var stream = new MemoryStream();
            synth.SetOutputToWaveStream(stream);
            synth.Speak(b);
            stream.Seek(0, SeekOrigin.Begin);

            // TODO: Abstract stream writing to its own class.
            using (var reader = new WaveFileReader(stream))
            using (var writer = new LameMP3FileWriter(path, reader.WaveFormat, LAMEPreset.VBR_90))
                reader.CopyTo(writer);

            SpeakCompleted.Invoke(this, new EventArgs());
        }
    }
}
