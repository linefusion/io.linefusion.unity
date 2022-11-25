using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;

namespace Linefusion.SourceGenerator.Model;

public enum TypeKind
{
    [Description("unknown")]
    Unknown,

    [Description("class")]
    Class,

    [Description("interface")]
    Interface,
    
    [Description("enum")]
    Enum,

    [Description("struct")]
    Struct,
    
    [Description("delegate")]
    Delegate
}

public class Type
{
    public virtual TypeKind Kind => TypeKind.Unknown;

    public string Name => GetName(false);
    public string FullName => GetName(true);
    public string Namespace => _type.Namespace;

    public bool IsByRef => _type.IsByRef;
    public bool IsPointer => _type.IsPointer;
    public bool IsArray => _type.IsArray;
    public bool IsValueType => _type.IsValueType;
    public bool IsGenericParameter => _type.IsGenericParameter;
    public bool IsGenericType => _type.IsGenericType;
    public bool IsGenericTypeDefinition => _type.IsGenericTypeDefinition;
    public bool IsConstructedGenericType => _type.IsConstructedGenericType;
    public bool IsPublic => _type.IsPublic;
    public bool IsPrimitive => _type.IsPrimitive;
    public bool IsAbstract => _type.IsAbstract;
    public bool IsSealed => _type.IsSealed;
    public bool IsNestedPublic => _type.IsNestedPublic;
    public bool IsNestedPrivate => _type.IsNestedPrivate;
    public bool IsNestedFamily => _type.IsNestedFamily;
    public bool IsNestedAssembly => _type.IsNestedAssembly;
    public bool IsNestedFamANDAssem => _type.IsNestedFamANDAssem;
    public bool IsNestedFamORAssem => _type.IsNestedFamORAssem;
    public bool IsAutoLayout => _type.IsAutoLayout;
    public bool IsLayoutSequential => _type.IsLayoutSequential;
    public bool IsExplicitLayout => _type.IsExplicitLayout;

    protected readonly System.Type _type;

    protected Type? _base;
    public Type? Base => _base ??= _type.BaseType != null ? Make(_type.BaseType) : null;
    
    protected Type[]? _interfaces;
    public Type[] Interfaces => _interfaces ??= _type.GetInterfaces().Select(Make).OrderBy(iface => iface.FullName).ToArray();

    private Method[]? _methods;
    public Method[] Methods => _methods ??= _type.GetMethods().Select(method => new Method(method)).ToArray();

    private Constructor[]? _constructors;
    public Constructor[] Constructors => _constructors ??= _type.GetConstructors().Select(constructor => new Constructor(constructor)).OrderBy(constructor => constructor.Parameters.Length).ToArray();

    private Attribute[]? _attributes;
    public Attribute[] Attributes => _attributes ??= _type.GetCustomAttributesData().Select(attr => new Attribute(attr)).ToArray();

    public Type(System.Type type) => _type = type;

    private static Dictionary<System.Type, Type> _cache = new();

    public static Type Make(System.Type type)
    {
        if (_cache.ContainsKey(type))
        {
            return _cache[type];
        }
        
        if (type.IsClass) return _cache[type] = new Class(type);
        else if (type.IsInterface) return _cache[type] = new Interface(type);
        else if (type.IsEnum) return _cache[type] = new Enum(type);
        else if (type.IsValueType && !type.IsPrimitive) return _cache[type] = new Struct(type);
        else return _cache[type] = new Type(type);
    }

    public override string ToString()
    {
        return this.GetName(true);
    }

    private string GetName(bool full)
    {
        return _type.FullName switch {
            "System.Void" => "void",
            "System.Boolean" => "bool",
            "System.Byte" => "byte",
            "System.SByte" => "sbyte",
            "System.Char" => "char",
            "System.Decimal" => "decimal",
            "System.Double" => "double",
            "System.Single" => "float",
            "System.Int32" => "int",
            "System.UInt32" => "uint",
            "System.Int64" => "long",
            "System.UInt64" => "ulong",
            "System.Object" => "object",
            "System.Int16" => "short",
            "System.UInt16" => "ushort",
            "System.String" => "string",
            _ => full ? _type.FullName : _type.Name
        };
    }
}

public class Attribute
{
    private readonly System.Reflection.CustomAttributeData _data;
    public string Alias => Name.EndsWith("Attribute") ? Name.Substring(0, Name.Length - 9) : Name;
    public string Name => _data.AttributeType.Name;
    public string FullName => _data.AttributeType.FullName;
    public string Namespace => _data.AttributeType.Namespace;

    public Attribute(System.Reflection.CustomAttributeData data) => _data = data;
}

public class Parameter
{
    private readonly System.Reflection.ParameterInfo _parameter;

    private Attribute[]? _attributes;
    public Attribute[] Attributes => _attributes ??= _parameter.GetCustomAttributesData().Select(attr => new Attribute(attr)).ToArray();

    public string Name => _parameter.Name;

    private Type? _type;
    public Type Type => _type ??= Type.Make(_parameter.ParameterType);

    public bool IsNullable => _parameter.ParameterType.IsGenericType && _parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>);
    public bool IsOptional => _parameter.IsOptional;
    public bool IsOut => _parameter.IsOut;
    public bool IsIn => _parameter.IsIn;
    public bool IsParams => _parameter.IsDefined(typeof(ParamArrayAttribute), false);
    public bool IsRetval => _parameter.IsRetval;
    public bool IsByRef => _parameter.ParameterType.IsByRef;

    public Parameter(System.Reflection.ParameterInfo parameter) => _parameter = parameter;
}

public class Method
{
    protected readonly System.Reflection.MethodInfo _method;
    
    public string Name => _method.Name;

    private Attribute[]? _attributes;
    public Attribute[] Attributes => _attributes ??= _method.GetCustomAttributesData().Select(attr => new Attribute(attr)).ToArray();

    public bool IsStatic => _method.IsStatic;
    public bool IsVirtual => _method.IsVirtual;
    public bool IsAbstract => _method.IsAbstract;
    public bool IsPublic => _method.IsPublic;
    public bool IsPrivate => _method.IsPrivate;
    public bool IsProtected => _method.IsFamily;
    public bool IsInternal => _method.IsAssembly;
    public bool IsProtectedInternal => _method.IsFamilyOrAssembly;
    public bool IsProtectedPrivate => _method.IsFamilyAndAssembly;
    public bool IsConstructor => _method.IsConstructor;
    public bool IsFinal => _method.IsFinal;
    public bool IsGenericMethod => _method.IsGenericMethod;
    public bool IsGenericMethodDefinition => _method.IsGenericMethodDefinition;

    private Type? _returnType;
    public Type ReturnType => _returnType ??= _method.ReturnType != null ? Type.Make(_method.ReturnType) : Type.Make(typeof(void));

    public int ParameterCount => _method.GetParameters().Length;
    public int GenericParameterCount => _method.GetGenericArguments().Length;

    public Parameter[] Parameters => _method.GetParameters().Select(param => new Parameter(param)).ToArray();

    public Method(System.Reflection.MethodInfo method) => _method = method;

    public override string ToString()
    {
        return _method.ToString();
    }
}

public class Constructor
{
    protected readonly System.Reflection.ConstructorInfo _constructor;

    public string Name => _constructor.Name;

    private Attribute[]? _attributes;
    public Attribute[] Attributes => _attributes ??= _constructor.GetCustomAttributesData().Select(attr => new Attribute(attr)).ToArray();

    public bool IsPublic => _constructor.IsPublic;
    public bool IsPrivate => _constructor.IsPrivate;
    public bool IsProtected => _constructor.IsFamily;
    public bool IsInternal => _constructor.IsAssembly;
    public bool IsProtectedInternal => _constructor.IsFamilyOrAssembly;
    public bool IsProtectedPrivate => _constructor.IsFamilyAndAssembly;

    public int ParameterCount => _constructor.GetParameters().Length;
    public int GenericParameterCount => _constructor.GetGenericArguments().Length;

    public Parameter[] Parameters => _constructor.GetParameters().Select(param => new Parameter(param)).ToArray();

    public Constructor(System.Reflection.ConstructorInfo constructor) => _constructor = constructor;

    public override string ToString()
    {
        return _constructor.ToString();
    }
}

public class Class : Type
{
    public override TypeKind Kind => TypeKind.Class;
    
    public Class(System.Type type)
        : base(type)
    {
    }
}

public class Struct : Class
{
    public override TypeKind Kind => TypeKind.Struct;

    public Struct(System.Type type)
        : base(type)
    {
    }
}

public class Interface : Type
{
    public override TypeKind Kind => TypeKind.Interface;

    public Interface(System.Type type)
        : base(type)
    {
    }
}

public class Enum : Type
{
    public override TypeKind Kind => TypeKind.Enum;

    public Enum(System.Type type)
        : base(type)
    {
    }
}

public class Assembly
{
    private readonly System.Reflection.Assembly _assembly;
    
    private Type[]? _types;
    public Type[] Types => _types ??= _assembly.GetTypes().Select(Type.Make).OrderBy(type => type.FullName).ToArray();
    
    public string Name => _assembly.FullName;
    public string FullName => _assembly.FullName;
    
    private Type[]? _classes;
    public Type[] Classes => _classes ??= Types.Where(type => type.Kind == TypeKind.Class).OrderBy(type => type.FullName).ToArray();
    
    private Type[]? _structs;
    public Type[] Structs => _structs ??= Types.Where(type => type.Kind == TypeKind.Struct).OrderBy(type => type.FullName).ToArray();
    
    private Type[]? _enums;
    public Type[] Enums => _enums ??= Types.Where(type => type.Kind == TypeKind.Enum).OrderBy(type => type.FullName).ToArray();
    
    private Type[]? _interfaces;
    public Type[] Interfaces => _interfaces ??= Types.Where(type => type.Kind == TypeKind.Interface).OrderBy(type => type.FullName).ToArray();

    public Assembly(System.Reflection.Assembly assembly) => _assembly = assembly;
}
