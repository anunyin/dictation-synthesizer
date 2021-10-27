using System;
using System.Windows;
using System.Speech.Synthesis;
using System.Collections.Generic;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Lame;
using System.IO;

namespace DictationSynthesizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            bool speaking = false;
            bool saving = false;

            var synth = new SpeechSynthesizer();
            synth.SpeakStarted += (x, x2) =>
            {
                voiceComboBox.IsEnabled = false;

                if (speaking)
                {
                    speakButton.Content = "Stop speaking";
                    saveButton.IsEnabled = false;
                }
                if (saving)
                {
                    saveButton.Content = "Saving...";
                    speakButton.IsEnabled = false;
                }
            };
            synth.SpeakCompleted += (x, x2) =>
            {
                voiceComboBox.IsEnabled = true;

                if (speaking)
                {
                    speaking = false;
                    speakButton.Content = "Speak";
                    saveButton.IsEnabled = true;
                }
                if (saving)
                {
                    saving = false;
                    saveButton.Content = "Save";
                    speakButton.IsEnabled = true;
                }
            };

            // init voiceComboBox
            foreach (var voice in synth.GetInstalledVoices())
            {
                voiceComboBox.Items.Add(voice.VoiceInfo.Name);
            }
            voiceComboBox.SelectionChanged += (sender, args) =>
            {
                synth.SelectVoice(voiceComboBox.SelectedItem as string);
            };
            voiceComboBox.SelectedIndex = 0;

            // init wpmTextBox
            wpmTextBox.Text = "40";

            // init transcriptTextBox
            transcriptTextBox.TextWrapping = TextWrapping.Wrap;

            // buttons
            speakButton.Click += (sender, args) =>
            {
                if (speaking)
                    synth.SpeakAsyncCancelAll();
                else
                {
                    speaking = true;
                    synth.SetOutputToDefaultAudioDevice();
                    synth.SpeakAsync(BuildDictationPrompt());
                }
            };

            saveButton.Click += (sender, args) =>
            {
                var dialog = new SaveFileDialog();
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                dialog.FileName = "dictation";

                if (dialog.ShowDialog() == true)
                {
                    saving = true;

                    var stream = new MemoryStream();
                    synth.SetOutputToWaveStream(stream);
                    synth.Speak(BuildDictationPrompt());
                    stream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new WaveFileReader(stream))
                    using (var writer = new LameMP3FileWriter(dialog.FileName, reader.WaveFormat, LAMEPreset.VBR_90))
                        reader.CopyTo(writer);
                }
            };
        }

        //private Dictionary<string, string> AliasWords = new Dictionary<string, string>
        //{
        //    { "an", "anne" },
        //    { "and", "anned" },
        //    { "the", "thee" },
        //};

        private Dictionary<string, string> PronunciationWords = new Dictionary<string, string>
        {
            { "i", "aɪ" },
            { "a", "eɪ" },
            { "an", "æn" },
            { "and", "ænd" },
            { "the", "ði" },
        };

        //private List<string> ExtraSlowWords = new List<string>
        //{
        //    "i", "on", "in", "a", "an", "and",
        //};

        private PromptBuilder BuildDictationPrompt()
        {
            var builder = new PromptBuilder();

            //var slowStyle = new PromptStyle() { Rate = PromptRate.Slow };
            //var extraSlowStyle = new PromptStyle() { Rate = PromptRate.ExtraSlow };

            var pauseInSeconds = 1.0 / int.Parse(wpmTextBox.Text);
            var pause = TimeSpan.FromMinutes(pauseInSeconds);

            var words = transcriptTextBox.Text.Split(' ');
            foreach (var word in words)
            {
                var wordLowercase = word.ToLower();

                //if (ExtraSlowWords.Contains(wordLowercase))
                //    builder.StartStyle(extraSlowStyle);
                //else
                //    builder.StartStyle(slowStyle);

                //if (AliasWords.ContainsKey(wordLowercase))
                //    builder.AppendTextWithAlias(word, AliasWords[wordLowercase]);
                if (PronunciationWords.ContainsKey(wordLowercase))
                    builder.AppendTextWithPronunciation(word, PronunciationWords[wordLowercase]);
                else
                    builder.AppendText(word);
                
                //builder.EndStyle();

                if (word == string.Empty)
                { 
                    continue;
                }

                builder.AppendBreak(pause);
            }

            return builder;
        }
    }
}
