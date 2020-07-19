using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using AngleSharp.Text;
using RuriLib;
using RuriLib.ViewModels;
using Tesseract;

namespace SilverBullet.Tesseract
{
    public class OcrEngine
    {
        private List<Engine> Engines = new List<Engine>();
        private readonly object createEngineLock = new object();
        private readonly object createEngineDisposedLock = new object();
        private readonly object removeEngineLock = new object();
        /// <summary>
        /// Get or create tesseract engine
        /// </summary>
        /// <param name="data">bot data</param>
        /// <param name="lang">language</param>
        /// <param name="engineMode">engine mode</param>
        /// <returns>Tesseract engine</returns>
        public TesseractEngine GetOrCreateEngine(BotData data, string lang,
            EngineMode engineMode)
        {
            if (data.BotsAmount > Engines.Count)
            {
                lock (createEngineLock)
                {
                    for (var b = 0; b <= data.BotsAmount - Engines.Count; b++)
                    {
                        Engines.Add(CreateOcrEngine(data, lang, engineMode));
                    }
                }
            }
            lock (removeEngineLock)
            {
                while (Engines.Count > data.BotsAmount)
                {
                    var index = Engines.Count - 1;
                    try { Engines[index].Tesseract.Dispose(); } catch { }
                    try { Engines.RemoveAt(index); } catch { }
                }
            }
            if (Engines.Any(e => e.Tesseract.IsDisposed))
            {
                lock (createEngineDisposedLock)
                {
                    try
                    {
                        var disposedEngines = Engines.Where(e => e.Tesseract.IsDisposed)
                            .ToList();
                        if (disposedEngines?.Count > 0)
                        {
                            for (var e = 0; e < disposedEngines.Count; e++)
                            {
                                var index = Engines.IndexOf(disposedEngines[e]);
                                Engines.Remove(disposedEngines[e]);
                                Engines.Insert(index, CreateOcrEngine(data, lang, engineMode));
                            }
                        }
                    }
                    catch { }
                }
            }
            return Engines[data.BotNumber - 1].Tesseract;
        }

        private Engine CreateOcrEngine(BotData data, string lang, EngineMode engineMode)
        {
            if (!Directory.Exists(@".\tessdata"))
            {
                if (data != null)
                {
                    data.Status = BotStatus.ERROR;
                    data.Log(new LogEntry("tessdata not found!", Colors.Red));
                }
                throw new DirectoryNotFoundException("tessdata not found!");
            }

            if (!File.Exists($@".\tessdata\{lang}.traineddata"))
            {
                if (data != null)
                {
                    data.Status = BotStatus.ERROR;
                    data.Log(new LogEntry($"Language '{lang}' not found!", Colors.Red));
                }
                throw new FileNotFoundException($"Language '{lang}' not found!");
            }

            var engine = new Engine(
                new TesseractEngine(@".\tessdata", lang, engineMode),
                  lang, engineMode);
            SetVariable(data, engine.Tesseract);

            return engine;
        }

        /// <summary>
        /// Dispose tesseract engine
        /// </summary>
        /// <returns></returns>
        private bool Dispose(Engine engine)
        {
            try { engine.Tesseract?.Dispose(); if (engine.Tesseract != null) return engine.Tesseract.IsDisposed; return false; } catch { if (engine.Tesseract != null) return engine.Tesseract.IsDisposed; return false; }
        }

        /// <summary>
        /// Stop all engines
        /// </summary>
        public void StopEngines()
        {
            try
            {
                Engines.ForEach(e =>
                {
                    try { e.Tesseract.Stopped = true; } catch { }
                });
            }
            catch { }
        }

        /// <summary>
        /// Dispose and remove tesseract engines
        /// </summary>
        public void DisposeEngines()
        {
            try
            {
                var i = 0;
                var d = 0;
                while (Engines.Count > 0)
                {
                    var e = Engines[i];
                    if (!e.Tesseract.CanDispose)
                    {
                        if (d >= 5)
                            goto dispose;
                        d++;
                        Thread.Sleep(50);
                        continue;
                    }
dispose:
                    try { Dispose(e); } catch { }
                    try { Engines.RemoveAt(0); } catch { }
                    i++;
                }
            }
            catch { }
        }

        static private void SetVariable(BotData data,
              TesseractEngine tesseract)
        {
            var varCount = data.GlobalSettings.Ocr.VariableList.Count;
            if (varCount > 0)
            {
                for (var i = 0; i < varCount; i++)
                {
                    try
                    {
                        var tesseractVariable = data.GlobalSettings.Ocr.VariableList[i];
                        var name = tesseractVariable.Name;
                        var value = tesseractVariable.Value;
                        var valType = tesseractVariable.ValueType;
                        switch (valType)
                        {
                            case VariableValueType.String:
                                tesseract.SetVariable(name, value);
                                break;
                            case VariableValueType.Integer:
                                tesseract.SetVariable(name, int.Parse(value));
                                break;
                            case VariableValueType.Double:
                                tesseract.SetVariable(name, double.Parse(value));
                                break;
                            case VariableValueType.Boolean:
                                tesseract.SetVariable(name, value.ToBoolean());
                                break;
                        }
                    }
                    catch { }
                }
            }
        }

        static bool Equals<T>(List<T> a, List<T> b)
        {
            if (a == null) return b == null;
            if (b == null || a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!object.Equals(a[i], b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        ~OcrEngine()
        {
            DisposeEngines();
        }
    }
}
