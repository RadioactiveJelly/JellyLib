using System.Collections.Generic;

namespace JellyLib.Utilities
{
    /// <summary>
    /// A container for data. Primarily used to add custom data that can be read in RS for easier cross script/mutator data communication.
    /// </summary>
    public class CustomData
    {
        private bool _isImmutable = false;
        
        private readonly Dictionary<string, float> _floats = new();
        private readonly Dictionary<string, int> _ints = new();
        private readonly Dictionary<string, bool> _booleans = new();
        private readonly Dictionary<string, string> _strings = new();
        
        public void SetImmutable() => _isImmutable = true;

        public void SetFloat(string key, float value)
        {
            if (_isImmutable) return;
            
            _floats[key] = value;
        }
        public void SetInt(string key, int value)
        {
            if (_isImmutable) return;
            
            _ints[key] = value;
        }
        public void SetBoolean(string key, bool value)
        {
            if (_isImmutable) return;
            
            _booleans[key] = value;
        }
        public void SetString(string key, string value)
        {
            if (_isImmutable) return;
            
            _strings[key] = value;
        }
        
        public float GetFloat(string key) => _floats.TryGetValue(key, out var value) ? value : 0;
        public int GetInt(string key) => _ints.TryGetValue(key, out var value) ? value : 0;
        public bool GetBoolean(string key) => _booleans.TryGetValue(key, out var value) && value;
        public string GetString(string key) => _strings.TryGetValue(key, out var value) ? value : "";
        
        public bool HasFloat(string key) => _floats.ContainsKey(key);
        public bool HasInt(string key) => _ints.ContainsKey(key);
        public bool HasBoolean(string key) => _booleans.ContainsKey(key);
        public bool HasString(string key) => _strings.ContainsKey(key);
    }
}
