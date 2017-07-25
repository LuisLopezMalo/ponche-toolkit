using System;
using NLog;

namespace PoncheToolkit.Builder
{
    /// <summary>
    /// Log the events taking configuration from NLog.config file
    /// </summary>
    public class Logger
    {
        private NLog.Logger logger;

        #region Properties
        /// <summary>
        /// Get or set if the object has been initialized.
        /// </summary>
        public bool IsInitialized { get; set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="currentClass"></param>
        public Logger(Type currentClass)
        {
            logger = LogManager.GetLogger(currentClass.FullName);
            IsInitialized = true;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Log any text only if the compiler is set to Debug.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        public void Debug(string text)
        {
#if DEBUG
            logger.Log(LogLevel.Debug, text);
#endif
        }

        /// <summary>
        /// Log any text with the given values to replace the format strings only if the compiler is set to Debug.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        /// <param name="values">Values to be set in runtime for string.format text.</param>
        public void Debug(string text, params object[] values)
        {
#if DEBUG
            logger.Log(LogLevel.Debug, text, values);
#endif
        }

        /// <summary>
        /// Log any text with the given values to replace the format strings only if the logger level is Info.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        public void Info(string text)
        {
            logger.Log(LogLevel.Info, text);
        }

        /// <summary>
        /// Log any text with the given values to replace the format strings only if the logger level is Info.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        /// <param name="values">Values to be set in runtime for string.format text.</param>
        public void Info(string text, params object[] values)
        {
            logger.Log(LogLevel.Info, text, values);
        }

        /// <summary>
        /// Log any text with the given values to replace the format strings only if the logger level is Warning.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        public void Warning(string text)
        {
            logger.Log(LogLevel.Warn, text);
        }

        /// <summary>
        /// Log any text with the given values to replace the format strings only if the logger level is Warning.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        /// <param name="values">Values to be set in runtime for string.format text.</param>
        public void Warning(string text, params object[] values)
        {
            logger.Log(LogLevel.Warn, text, values);
        }

        /// <summary>
        /// Log any text with the given values to replace the format strings only if the logger level is Warning.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        /// <param name="ex"></param>
        public void Warning(string text, Exception ex)
        {
            logger.Log(LogLevel.Warn, ex, text);
        }

        /// <summary>
        /// Log any text with the error tag.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        public void Error(string text)
        {
            logger.Log(LogLevel.Error, text);
        }

        /// <summary>
        /// Log any text with the error tag.
        /// </summary>
        /// <param name="text">Text to be logged.</param>
        /// <param name="values"></param>
        public void Error(string text, params object[] values)
        {
            logger.Log(LogLevel.Error, text, values);
        }

        /// <summary>
        /// Log any text with the given values to replace the format strings only if the logger level is Warning.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ex"></param>
        public void Error(string text, Exception ex)
        {
            logger.Log(LogLevel.Error, ex, text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ex"></param>
        /// <param name="values"></param>
        public void Error(string text, Exception ex, params object[] values)
        {
            logger.Log(LogLevel.Error, ex, text, values);
        }

        #endregion
    }
}
