﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Constants.CSharpGrammar
{
    public class Keywords
    {
        public const string Abstract = "abstract";
        public const string As = "as";
        public const string Base = "base";
        public const string Bool = "bool";
        public const string Break = "break";
        public const string Byte = "byte";
        public const string Case = "case";
        public const string Catch = "catch";
        public const string Char = "char";
        public const string Checked = "checked";
        public const string Class = "class";
        public const string Const = "const";
        public const string Continue = "continue";
        public const string Decimal = "decimal";
        public const string Default = "default";
        public const string Delegate = "delegate";
        public const string Do = "do";
        public const string Double = "double";
        public const string Else = "else";
        public const string Enum = "enum";
        public const string Event = "event";
        public const string Explicit = "explicit";
        public const string Extern = "extern";
        public const string False = "false";
        public const string Finally = "finally";
        public const string Fixed = "fixed";
        public const string Float = "float";
        public const string For = "for";
        public const string Foreach = "foreach";
        public const string Goto = "goto";
        public const string If = "if";
        public const string Implicit = "implicit";
        public const string In = "in";
        public const string Int = "int";
        public const string Interface = "interface";
        public const string Internal = "internal";
        public const string Is = "is";
        public const string Lock = "lock";
        public const string Long = "long";
        public const string Namespace = "namespace";
        public const string New = "new";
        public const string Null = "null";
        public const string Object = "object";
        public const string Operator = "operator";
        public const string Out = "out";
        public const string Override = "override";
        public const string Params = "params";
        public const string Private = "private";
        public const string Protected = "protected";
        public const string Public = "public";
        public const string Readonly = "readonly";
        public const string Ref = "ref";
        public const string Return = "return";
        public const string Sbyte = "sbyte";
        public const string Sealed = "sealed";
        public const string Short = "short";
        public const string Sizeof = "sizeof";
        public const string Stackalloc = "stackalloc";
        public const string Static = "static";
        public const string String = "string";
        public const string Struct = "struct";
        public const string Switch = "switch";
        public const string This = "this";
        public const string Throw = "throw";
        public const string True = "true";
        public const string Try = "try";
        public const string Typeof = "typeof";
        public const string Uint = "uint";
        public const string Ulong = "ulong";
        public const string Unchecked = "unchecked";
        public const string Unsafe = "unsafe";
        public const string Ushort = "ushort";
        public const string Using = "using";
        public const string Virtual = "virtual";
        public const string Void = "void";
        public const string Volatile = "volatile";
        public const string While = "while";
    }

    public class ContextualKeywords
    {
        public const string Add = "add";
        public const string Alias = "alias";
        public const string Ascending = "ascending";
        public const string Async = "async";
        public const string Await = "await";
        public const string By = "by";
        public const string Descending = "descending";
        public const string Dynamic = "dynamic";
        public const string Equals_ = "equals";
        public const string From = "from";
        public const string Get = "get";
        public const string Global = "global";
        public const string Group = "group";
        public const string Into = "into";
        public const string Join = "join";
        public const string Let = "let";
        public const string Nameof = "nameof";
        public const string On = "on";
        public const string Orderby = "orderby";
        public const string Partial = "partial";
        public const string Remove = "remove";
        public const string Select = "select";
        public const string Set = "set";
        public const string Unmanaged = "unmanaged";
        public const string Value = "value";
        public const string Var = "var";
        public const string When = "when";
        public const string Where = "where";
        public const string Yield = "yield";
    }

    public class Modifiers
    {
        public static HashSet<string> Interface { get; } = new HashSet<string>()
        {
            Keywords.New,
            Keywords.Public,
            Keywords.Protected,
            Keywords.Internal,
            Keywords.Private,
            Keywords.Unsafe
        };

        public static HashSet<string> Class { get; } = new HashSet<string>()
        {
            Keywords.New,
            Keywords.Public,
            Keywords.Protected,
            Keywords.Internal,
            Keywords.Private,
            Keywords.Abstract,
            Keywords.Sealed,
            Keywords.Static,
            Keywords.Unsafe
        };

        public static HashSet<string> Field { get; } = new HashSet<string>()
        {
            Keywords.New,
            Keywords.Public,
            Keywords.Protected,
            Keywords.Internal,
            Keywords.Private,
            Keywords.Static,
            Keywords.Readonly,
            Keywords.Volatile,
            Keywords.Unsafe
        };

        public static HashSet<string> Method { get; } = new HashSet<string>()
        {
            Keywords.New,
            Keywords.Public,
            Keywords.Protected,
            Keywords.Internal,
            Keywords.Private,
            Keywords.Static,
            Keywords.Virtual,
            Keywords.Sealed,
            Keywords.Override,
            Keywords.Abstract,
            Keywords.Extern,
            ContextualKeywords.Async,
            Keywords.Unsafe
        };

        public static HashSet<string> InterfaceMethod { get; } = new HashSet<string>()
        {
            Keywords.New
        };

        public static HashSet<string> Property { get; } = new HashSet<string>()
        {
            Keywords.New,
            Keywords.Public,
            Keywords.Protected,
            Keywords.Internal,
            Keywords.Private,
            Keywords.Static,
            Keywords.Virtual,
            Keywords.Sealed,
            Keywords.Override,
            Keywords.Abstract,
            Keywords.Extern,
            Keywords.Unsafe
        };

        public static HashSet<string> InterfaceProperty { get; } = new HashSet<string>()
        {
            Keywords.New
        };

        public static HashSet<string> Constructor { get; } = new HashSet<string>()
        {
            Keywords.Public,
            Keywords.Protected,
            Keywords.Internal,
            Keywords.Private,
            Keywords.Extern,
            Keywords.Unsafe
        };

    }
}
