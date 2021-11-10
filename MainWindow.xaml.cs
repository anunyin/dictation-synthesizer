using System;
using System.Windows;
using System.Speech.Synthesis;
using System.Collections.Generic;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Lame;
using System.IO;
using System.Text;
using System.Linq;

namespace DictationSynthesizer
{
    public partial class MainWindow : Window
    {
        private bool speaking = false;
        private bool saving = false;

        public MainWindow()
        {
            InitializeComponent();

            var synth = new SpeechSynthesizer();
            synth.SpeakStarted += (x, x2) =>
            {
                SpeakStarted();
            };
            synth.SpeakCompleted += (x, x2) =>
            {
                SpeakCompleted();
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
            voiceComboBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            voiceComboBox.VerticalContentAlignment = VerticalAlignment.Center;

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
                    //var pauseInMinutes = 1.0 / int.Parse(wpmTextBox.Text);
                    //var words = transcriptTextBox.Text.Split(' ');
                    //var lastSpokenTime = DateTime.Now;
                    //var lastSpokenWordIndex = 0;
                    //while (speaking)
                    //{
                    //    var now = DateTime.Now;
                    //    if ((now - lastSpokenTime).TotalMinutes >= pauseInMinutes)
                    //    {
                    //        synth.SpeakAsync(words[lastSpokenWordIndex]);
                    //        lastSpokenWordIndex++;
                    //        lastSpokenTime = now;

                    //        if (words.Length < lastSpokenWordIndex + 1)
                    //            speaking = false;
                    //    }
                    //}
                }
            };

            saveButton.Click += (sender, args) =>
            {
                var dialog = new SaveFileDialog();
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                dialog.DefaultExt = "mp3";
                dialog.Filter =
                    "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
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

                    SpeakCompleted();
                }
            };

            randomizeWordsButton.Click += (sender, args) =>
            {
                var words = Tokenize(transcriptTextBox.Text);
                transcriptTextBox.Text = string.Join(" ", words.Shuffle().ToArray());
            };
        }

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

        void SpeakStarted()
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
        }

        void SpeakCompleted()
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
        }

        private List<string> Tokenize(string dictationText)
        {
            var reader = new StringReader(dictationText);
            var words = new List<string>();
            while (reader.Peek() != -1)
            {
                var c = reader.Peek();
                if (char.IsWhiteSpace((char)c))
                {
                    reader.Read();
                }
                else
                {
                    words.Add(ParseWord(reader));
                }
            }
            return words;
        }

        private string ParseWord(StringReader reader)
        {
            var w = string.Empty;
            while (
                reader.Peek() != -1 
                && !char.IsWhiteSpace((char)reader.Peek()))
            {
                w += (char)reader.Read();
            }
            return w;
        }

        private PromptBuilder BuildDictationPrompt()
        {
            var builder = new PromptBuilder();

            //var slowStyle = new PromptStyle() { Rate = PromptRate.Slow };
            //var extraSlowStyle = new PromptStyle() { Rate = PromptRate.ExtraSlow };

            var pauseInMinutes = 1.0 / int.Parse(wpmTextBox.Text);
            var pause = TimeSpan.FromMinutes(pauseInMinutes);

            var words = Tokenize(transcriptTextBox.Text); //transcriptTextBox.Text.Split(' ');
            foreach (var word in words)
            {
                var wordLowercase = word.ToLower();

                //if (ExtraSlowWords.Contains(wordLowercase))
                //    builder.StartStyle(extraSlowStyle);
                //else
                //    builder.StartStyle(slowStyle);

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
