using System.Collections.Generic;

namespace DoWproReplayWatcher.Logic.Lua
{
    public class LuaObject
    {
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        public void Add<T>(string key, T value) => this.properties.Add(key, value);
        public T GetValue<T>(string key) => this.properties.ContainsKey(key) ? (T)this.properties[key] : default(T);
        public bool Contains(string key) => this.properties.ContainsKey(key);
        public bool Is<T>(string key) => this.properties[key] is T;
    }
}
