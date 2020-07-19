using System;
using System.IO;
using System.Linq;

namespace RuriLib.Models
{
    /// <summary>
    /// Represents a file as a source of input data that needs to be tested against a Config by the Runner.
    /// </summary>
    public class Wordlist : Persistable<Guid>
    {
        /// <summary>The name of the Wordlist.</summary>
        public string Name { get; set; }

        /// <summary>The path where the file is stored on disk.</summary>
        public string Path { get; set; }

        /// <summary>The WordlistType as a string (since the WordlistTypes in the Environment file are editable).</summary>
        public string Type { get; set; }

        /// <summary>The purpose for which the Wordlist should be used.</summary>
        public string Purpose { get; set; }

        /// <summary>The total number of data lines of the file.</summary>
        public int Total { get; set; }

        /// <summary>If true, the Wordlist does not reside on the disk.</summary>
        public bool Temporary { get; set; }

        /// <summary>
        /// Has sub wordlist
        /// </summary>
        public bool HasSubWordlist => SubWordlists != null && SubWordlists.Count() > 0;

        /// <summary>
        /// Remove duplicate from word list
        /// </summary>
        public bool RemoveDup { get; set; }

        /// <summary>
        /// Sub word list (cookie...)
        /// </summary>
        public SubWordlist[] SubWordlists { get; set; }

        /// <summary>Needed for NoSQL deserialization.</summary>
        public Wordlist() { }

        /// <summary>
        /// Creates an instance of a Wordlist.
        /// </summary>
        /// <param name="name">The name of the Wordlist</param>
        /// <param name="path">The path to the file on disk</param>
        /// <param name="type">The WordlistType as a string</param>
        /// <param name="purpose">The purpose of the Wordlist</param>
        /// <param name="countLines">Whether to enumerate the total number of data lines in the Wordlist</param>
        /// <param name="temporary">If true, the Wordlist does not reside on the disk</param>
        /// <param name="subwordlists">Sub word list (cookie...)</param>
        public Wordlist(string name, string path, string type, string purpose,
            bool countLines = true,
            bool temporary = false,
            SubWordlist[] subwordlists = null)
        {
            Name = name;
            Path = path;
            Type = type;
            Purpose = purpose;
            Total = 0;
            Temporary = temporary;
            if (countLines)
            {
                try
                {
                    Total = File.ReadLines(path).Count();
                }
                catch { }
            }
            SubWordlists = subwordlists;
        }
    }
}
