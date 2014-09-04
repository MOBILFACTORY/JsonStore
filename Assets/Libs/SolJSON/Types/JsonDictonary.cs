using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SolJSON.Types
{
    public class JsonDictonary : JsonObject, IEnumerable<KeyValuePair<string, JsonObject>>
    {
        protected Dictionary<string, JsonObject> _dict;

        public JsonDictonary()
        {
            _dict = new Dictionary<string, JsonObject>();
        }

        public void Add(string key, JsonObject value)
        {
            _dict.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public int Count
        {
            get
            {
                return _dict.Count;
            }
        }

        public bool Contains(string key)
        {
            return _dict.ContainsKey(key);
        }

        public JsonObject this[string key]
        {
            get
            {
                return _dict[key];
            }
            set
            {
                _dict[key] = value;
            }
        }


        public override TYPE Type
        {
            get
            {
                return TYPE.DICTONARY;
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, JsonObject>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public override string ToString(int indent, int depth)
        {
            this.IsPrettyPrint = indent > 0;

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            NewLine(sb);

            bool first = true;

            foreach (var value in _dict)
            {
                if (first == true)
                {
                    first = false;
                }
                else
                {
                    sb.Append(",");
                    NewLine(sb);
                }

                Tab(sb, indent, depth);
                sb.Append('"');
                sb.Append(JsonString.Escape(value.Key));
                sb.Append('"');
                sb.Append(": ");
                sb.Append(value.Value.ToString(indent, depth + 1));
            }
            NewLine(sb);
            Tab(sb, indent, depth - 1);
            sb.Append("}");

            return sb.ToString();
        }
    }
}
