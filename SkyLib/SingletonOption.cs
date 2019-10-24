using PeterHan.PLib;
using PeterHan.PLib.Options;

namespace SkyLib
{
    public class SingletonOption<T> where T: class, new()
    {
        protected static T _Instance;
        
        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = POptions.ReadSettings<T>() ?? new T();
                }

                return _Instance;
            }
            set => _Instance = value;
        }
    }
}