using System;
using System.Windows;
using System.Speech.Synthesis;
using System.Collections.Generic;
using Microsoft.Win32;

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
                dialog.FileName = "dictation.wav";

                if (dialog.ShowDialog() == true)
                {
                    saving = true;
                    synth.SetOutputToWaveFile(dialog.FileName);
                    synth.SpeakAsync(BuildDictationPrompt());
                }
            };
            //synth.SelectVoice("Microsoft Zira Desktop");

            //var pause = TimeSpan.FromSeconds(1);
            //var builder = new PromptBuilder();
            //var slowRateStyle = new PromptStyle() { Rate = PromptRate.ExtraSlow };

            //builder.StartSentence();
            //builder.StartStyle(slowRateStyle);
            //builder.AppendText("The");
            //builder.EndStyle();
            //builder.AppendText("cat");
            //builder.AppendBreak(pause);
            //builder.AppendText("killed");
            //builder.AppendBreak(pause);
            //builder.StartStyle(slowRateStyle);
            //builder.AppendText("The");
            //builder.EndStyle();
            //builder.AppendText("mouse.");
            //builder.EndSentence();

            //builder.AppendBreak(TimeSpan.FromSeconds(1.5));

            //builder.StartSentence();
            //builder.AppendText("I");
            //builder.AppendBreak(pause);
            //builder.AppendText("will be");
            //builder.AppendBreak(pause);
            //builder.AppendText("fighting");
            //builder.AppendBreak(pause);
            //builder.AppendText("you.");
            //builder.EndSentence();

            //synth.SpeakAsync(builder);
        }

        private Dictionary<string, string> AliasWords = new Dictionary<string, string>
        {
            { "an", "anne" },
            { "and", "anned" },
        };

        private List<string> ExtraSlowWords = new List<string>
        {
            "i", "a", "on", "in"
        };

        private PromptBuilder BuildDictationPrompt()
        {
            var builder = new PromptBuilder();

            var slowStyle = new PromptStyle() { Rate = PromptRate.Slow };
            var extraSlowStyle = new PromptStyle() { Rate = PromptRate.ExtraSlow };

            var pauseInSeconds = 60.0 / int.Parse(wpmTextBox.Text);
            var pause = TimeSpan.FromSeconds(pauseInSeconds);

            var words = transcriptTextBox.Text.Split(' ');
            foreach (var word in words)
            {
                var wordLowercase = word.ToLower();

                if (ExtraSlowWords.Contains(wordLowercase))
                    builder.StartStyle(extraSlowStyle);
                else
                    builder.StartStyle(slowStyle);

                if (AliasWords.ContainsKey(wordLowercase))
                    builder.AppendTextWithAlias(word, AliasWords[wordLowercase]);
                else
                    builder.AppendText(word);

                builder.EndStyle();

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
