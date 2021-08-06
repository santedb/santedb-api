using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Interface which provides localization functions
    /// </summary>
    public interface ILocalizationService : IServiceImplementation
    {

        /// <summary>
        /// Get the specified string in the current locale
        /// </summary>
        String GetString(String stringKey);

        /// <summary>
        /// Get the specified <paramref name="stringKey"/> in <paramref name="locale"/>
        /// </summary>
        String GetString(String locale, String stringKey);

        /// <summary>
        /// Format a <paramref name="stringKey"/> with <paramref name="parameters"/>
        /// </summary>
        String FormatString(String stringKey, params object[] parameters);

        /// <summary>
        /// Format a <paramref name="stringKey"/> from <paramref name="locale"/> with <paramref name="parameters"/>
        /// </summary>
        String FormatString(String locale, String stringKey, params object[] parameters);

        /// <summary>
        /// Get all strings in the specified locale
        /// </summary>
        /// <param name="locale"></param>
        /// <returns></returns>
        KeyValuePair<String,String>[] GetStrings(String locale);

        /// <summary>
        /// Reload string definitions
        /// </summary>
        void Reload();

    }
}
