using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Extreme.Net;
using RuriLib.Functions.Requests;
using RuriLib.LS;

namespace RuriLib
{
    /// <summary>
    /// Speech recognition
    /// </summary>
    public class BlockSpeechToText : BlockBase
    {
        #region Variables

        private string variableName = "";

        /// <summary>The name of the output variable where the OCR response will be stored.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool isCapture = false;

        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        private string url = "";

        /// <summary>The URL of the image.</summary>
        public string Url
        {
            get { return url; }
            set
            {
                url = value.Replace("\n", "");
                OnPropertyChanged();
            }
        }

        private string lang = "en-US";
        public string Lang { get { return lang; } set { lang = value; OnPropertyChanged(); } }

        #endregion end Variables

        /// <summary>
        /// Creates a speech to text block.
        /// </summary>
        public BlockSpeechToText()
        {
            Label = "SPEECHTOTEXT";
        }

        /// <inheritdoc/>
        public override void Process(BotData data)
        {
            base.Process(data);
            //InsertVariable(data, IsCapture, Run(data), VariableName);
        }

        /// <inheritdoc/>
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer.Label(Label)
                .Token("SPEECHTOTEXT")
                .Token(Lang)
                .Literal(Url);

            if (!writer.CheckDefault(VariableName, nameof(VariableName)))
            {
                writer.Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(VariableName)
                    .Indent();
            }

            return writer.ToString();
        }

        /// <inheritdoc/>
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            Lang = LineParser.ParseToken(ref input, TokenType.Parameter, false);
            Url = LineParser.ParseLiteral(ref input, nameof(Url));

            return this;
        }

        //public string Run(BotData data)
        //{
        //    var inputs = ReplaceValues(Url, data);

        //    var request = new Request();

        //    // Setup
        //    request.Setup(data.GlobalSettings, maxRedirects: data.ConfigSettings.MaxRedirects);

        //    data.Log(new LogEntry($"Calling URL: {inputs}", Colors.MediumTurquoise));

        //    request.SetStandardContent(string.Empty, "application/x-www-form-urlencoded", HttpMethod.GET, false, GetLogBuffer(data));

        //    // Set proxy
        //    if (data.UseProxies)
        //    {
        //        request.SetProxy(data.Proxy);
        //    }

        //    //// Set headers
        //    //data.Log(new LogEntry("Sent Headers:", Colors.DarkTurquoise));
        //    //var headers = CustomHeaders.Select(h =>
        //    //       new KeyValuePair<string, string>(ReplaceValues(h.Key, data), ReplaceValues(h.Value, data))
        //    //    ).ToDictionary(h => h.Key, h => h.Value);
        //    //request.SetHeaders(headers, log: GetLogBuffer(data));

        //    // Set cookies
        //    data.Log(new LogEntry("Sent Cookies:", Colors.MediumTurquoise));
        //    request.SetCookies(data.Cookies, GetLogBuffer(data));

        //    // End the request part
        //    data.LogNewLine();

        //    // Perform the request
        //    try
        //    {
        //        (data.Address, data.ResponseCode, data.ResponseHeaders, data.Cookies) = request
        //            .Perform(inputs, HttpMethod.GET, GetLogBuffer(data), true);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (data.ConfigSettings.IgnoreResponseErrors)
        //        {
        //            data.Log(new LogEntry(ex.Message, Colors.Tomato));
        //            data.ResponseSource = ex.Message;
        //            return null;
        //        }
        //        throw;
        //    }

        //    data.ResponseSource = request.SaveString(true, data.ResponseHeaders, GetLogBuffer(data));
        //    request.GetMemoryStream();

        //}

        //private List<LogEntry> GetLogBuffer(BotData data) =>
        //data.GlobalSettings.General.EnableBotLog || data.IsDebug ? data.LogBuffer : null;

        //#region locals

        ///// <summary>
        ///// the engine
        ///// </summary>
        //SpeechRecognitionEngine speechRecognitionEngine = null;

        ///// <summary>
        ///// list of predefined commands
        ///// </summary>
        //List<Word> words = new List<Word>();

        //#endregion


        //#region internal functions and methods

        ///// <summary>
        ///// Creates the speech engine.
        ///// </summary>
        ///// <param name="preferredCulture">The preferred culture.</param>
        ///// <returns></returns>
        //private SpeechRecognitionEngine CreateSpeechEngine(string preferredCulture)
        //{
        //    RecognizerInfo config = SpeechRecognitionEngine.InstalledRecognizers()
        //        .FirstOrDefault(e => e.Culture.Name == preferredCulture);

        //    speechRecognitionEngine = new SpeechRecognitionEngine(config);

        //    return speechRecognitionEngine;
        //}


        ///// <summary>
        ///// represents a word
        ///// </summary>
        //public class Word
        //{
        //    public Word() { }
        //    public string Text { get; set; }
        //    public string AttachedText { get; set; }
        //    public bool IsShellCommand { get; set; }
        //}
    }
}
