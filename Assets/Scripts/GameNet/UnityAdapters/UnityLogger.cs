using UnityEngine;
using GameNet.Core;

namespace GameNet.UnityAdapters
{
    /// <summary>
    /// Unity adapter for logging using Debug class.
    /// </summary>
    public class UnityLogger : IGameLogger
    {
        private readonly Object _context;

        public UnityLogger(Object context = null)
        {
            _context = context;
        }

        public void Log(string message)
        {
            if (_context != null)
            {
                Debug.Log(message, _context);
            }
            else
            {
                Debug.Log(message);
            }
        }

        public void LogError(string message)
        {
            if (_context != null)
            {
                Debug.LogError(message, _context);
            }
            else
            {
                Debug.LogError(message);
            }
        }
    }
}
