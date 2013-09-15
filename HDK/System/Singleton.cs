using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class Singleton<T> where T : new()
    {
        private static Lazy<T> m_Instance;

        public static bool IsValueCreated { get { return IsAvailable && m_Instance.IsValueCreated; } }

        public static bool IsAvailable { get { return m_Instance != null; } }

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new Lazy<T>(() => new T());

                return m_Instance.Value;
            }
        }
    }
}
