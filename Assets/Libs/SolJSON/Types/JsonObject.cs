using System;
using System.Collections.Generic;
using System.Text;

/* 
 * JSON 관련 타입 구현은 RFC4627 ( http://www.ietf.org/rfc/rfc4627.txt ) 를 참조하였다.
 * 다른 타입은 다 같고 Object의 경우 C# Object와의 혼동을 피하기 위해 Dictonary로 구현하였다.
 */
namespace SolJSON.Types
{
    public abstract class JsonObject
    {
        public enum TYPE
        {
            ARRAY = 0 ,
            DICTONARY = 1,
            NUMBER = 2,
            BOOL = 3,
            STRING = 4,
            NULL = 5,
        };

        public abstract TYPE Type
        {
            get;
        }

        public JsonArray AsArray
        {
            get
            {
                return this as JsonArray;
            }
        }

        public JsonBool AsBool
        {
            get
            {
                return this as JsonBool;
            }
        }

        public JsonDictonary AsDictonary
        {
            get
            {
                return this as JsonDictonary;
            }
        }

        public JsonNumber AsNumber
        {
            get
            {
                return this as JsonNumber;
            }
        }

        public JsonString AsString
        {
            get
            {
                return this as JsonString;
            }
        }

        public JsonNull AsNull
        {
            get
            {
                return this as JsonNull;
            }
        }
        
        protected bool IsPrettyPrint
        {
            get;
            set;
        }

        public abstract override string ToString();

        public string ToString(int indent)
        {
            return ToString(indent, 1);
        }

        public abstract string ToString(int indent, int depth);

        protected void Tab(StringBuilder sb, int indent, int depth)
        {
            for (int i = 0; i < indent * depth; ++i)
                sb.Append(" ");
        }

        protected void NewLine(StringBuilder sb)
        {
            if (IsPrettyPrint)
                sb.Append("\r\n");
        }

    }
}
