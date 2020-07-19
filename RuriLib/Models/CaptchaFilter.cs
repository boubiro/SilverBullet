using System;
using System.Reflection;

namespace RuriLib.Models
{
    /// <summary>
    /// Image filter
    /// </summary>
    public class CaptchaFilter
    {
        /// <summary>
        /// Filter method
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// Filter parameter
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// Filter parameter type
        /// </summary>
        public Type ParameterType { get; set; }

        /// <summary>
        /// Filter name
        /// </summary>
        public string Name { get; set; }

    }
}
