using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Linefusion.Generators.Extensions;
using Newtonsoft.Json;
using NTypewriter.CodeModel;
using NTypewriter.Runtime;
using SQLitePCL;
using UnityEngine;

namespace Linefusion.Generators.Editor.CodeModel
{
    public static class ReflectionExtensions
    {
        public static bool IsAssignableTo<Base>(this Type type)
        {
            return typeof(Base).IsAssignableFrom(type);
        }

        public static bool IsAssignableTo(this Type type, Type target)
        {
            return target.IsAssignableFrom(type);
        }

        public static bool IsAnonymous<T>(this T type)
            where T : Type
        {
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType
                && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && type.Attributes.HasFlag(TypeAttributes.NotPublic);
        }

        public static bool IsCollection<T>(this T type)
            where T : Type
        {
            return type.GetInterfaces()
                .Union(new T[] { type })
                .Any(type =>
                    type != typeof(string)
                    && (
                        type == typeof(System.Collections.ICollection)
                        || type.IsAssignableTo(typeof(System.Collections.ICollection))
                        || (
                            type.IsGenericType
                            && type.GetGenericTypeDefinition() == typeof(ICollection<>)
                        )
                    )
                );
        }

        public static bool IsStruct<T>(this T type)
            where T : Type
        {
            return type.IsValueType && !type.IsEnum && !type.IsPrimitive;
        }

        public static bool IsAttribute<T>(this T type)
            where T : Type
        {
            return type.BaseType.IsAssignableTo<Attribute>();
        }

        public static bool IsEnumerable<T>(this T type)
            where T : Type
        {
            return type.GetInterfaces()
                .Union(new T[] { type })
                .Any(type =>
                    type != typeof(string)
                    && (
                        type == typeof(System.Collections.IEnumerable)
                        || type.IsAssignableTo(typeof(System.Collections.IEnumerable))
                        || (
                            type.IsGenericType
                            && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        )
                    )
                );
        }

        public static bool IsDelegate<T>(this T type)
            where T : Type
        {
            return type.IsSubclassOf(typeof(Delegate));
        }

        public static bool IsDynamic<T>(this T type)
            where T : Type
        {
            return type.IsAssignableTo<IDynamicMetaObjectProvider>();
        }

        public static bool IsAnonymousType(this Type type)
        {
            return type.Name.StartsWith("<>", StringComparison.Ordinal)
                && type.GetCustomAttributes(
                    typeof(CompilerGeneratedAttribute),
                    inherit: false
                ).Length > 0
                && type.Name.Contains("AnonymousType");
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsRecord(this Type type)
        {
            var property = type.GetProperty(
                "EqualityContract",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );
            var method = property?.GetGetMethod(true);

            return type != null
                && property != null
                && property != null
                && method != null
                && method.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
        }

        public static bool IsTuple(this Type type)
        {
            return type.IsAssignableTo<ITuple>();
        }

        public static bool IsVirtual(this PropertyInfo info)
        {
            return info.GetMemberMethods().Any(v => v.IsVirtual);
        }

        public static bool IsAbstract(this PropertyInfo info)
        {
            return info.GetMemberMethods().Any(v => v.IsAbstract);
        }

        public static bool IsAbstract(this EventInfo info)
        {
            return info.GetMemberMethods().Any(v => v.IsAbstract);
        }

        public static bool IsVirtual(this EventInfo info)
        {
            return info.GetMemberMethods().Any(v => v.IsVirtual);
        }

        public static bool IsAbstract(this MemberInfo member)
        {
            return member.GetMemberMethods().Any(v => v.IsAbstract);
        }

        public static bool IsVirtual(this MemberInfo member)
        {
            return member.GetMemberMethods().Any(v => v.IsVirtual);
        }

        public static IEnumerable<MethodBase> GetMemberMethods(this MemberInfo member)
        {
            return member switch
            {
                ConstructorInfo info => new MethodBase[] { info },
                MethodInfo info => new MethodBase[] { info },
                EventInfo info
                    => (new MethodBase[] { info.AddMethod, info.RemoveMethod, info.RaiseMethod })
                        .Where(method => method != null)
                        .ToArray(),
                PropertyInfo info
                    => new MethodBase[] { info.GetMethod, info.SetMethod }
                        .Where(member => member != null)
                        .ToArray(),
                _ => new MethodBase[] { }
            };
        }

        public static TypeInfo? GetMemberType(this MemberInfo member)
        {
            return member switch
            {
                EventInfo info => info.EventHandlerType.GetTypeInfo(),
                PropertyInfo info => info.PropertyType.GetTypeInfo(),
                FieldInfo info => info.FieldType.GetTypeInfo(),
                _ => null
            };
        }

        public static bool IsArray(this MemberInfo member)
        {
            throw new NotImplementedException();
        }

        public static bool IsErrorType(this MemberInfo member)
        {
            throw new NotImplementedException();
        }

        public static bool IsPublic(this MemberInfo member)
        {
            return member switch
            {
                FieldInfo field => field.IsPublic,
                PropertyInfo field => field.IsPublic(),
                ConstructorInfo field => field.IsPublic,
                MethodInfo method => method.IsPublic,
                _ => false
            };
        }

        public static bool IsStatic(this MemberInfo member)
        {
            return member switch
            {
                FieldInfo field => field.IsStatic,
                PropertyInfo field => field.GetMethod.IsStatic,
                MethodInfo method => method.IsStatic,
                _ => false
            };
        }

        public static bool IsTypeParameter(this MemberInfo member)
        {
            throw new NotImplementedException();
        }
    }

    namespace Models
    {
        public class Attribute : IAttribute
        {
            private readonly CustomAttributeData data;

            public Attribute(CustomAttributeData value)
            {
                this.data = value;
            }

            public string Name => data.AttributeType.Name;
            public string FullName => data.AttributeType.FullName;

            public IClass? AttributeClass => Class.From(data.AttributeType.GetTypeInfo());
            public IEnumerable<IAttributeArgument> Arguments =>
                data
                    .ConstructorArguments.Select(arg => AttributeArgument.From(arg, true))
                    .Concat(data.NamedArguments.Select(arg => AttributeArgument.From(arg, false)))
                    .ToArray();

            public static IEnumerable<IAttribute> From(System.Type type)
            {
                return From(type.GetCustomAttributesData()).ToArray();
            }

            public static IEnumerable<IAttribute> From(IEnumerable<System.Type> types)
            {
                return From(types.Select(type => type.GetTypeInfo())).ToArray();
            }

            public static IEnumerable<IAttribute> From(IEnumerable<TypeInfo> types)
            {
                return types
                    .Where(type => type.IsAttribute())
                    .SelectMany(type => From(type.CustomAttributes))
                    .ToArray();
            }

            public static IEnumerable<IAttribute> From(IEnumerable<CustomAttributeData> attributes)
            {
                return attributes.Select(data => new Models.Attribute(data)).ToArray();
            }
        }

        public class AttributeArgument : IAttributeArgument
        {
            public AttributeArgument(
                string? name,
                object value,
                TypeInfo type,
                bool isFromConstructor
            )
            {
                this.Name = name;
                this.Value = value;
                this.Type = Models.Type.From(type);
                this.IsFromConstructor = isFromConstructor;
            }

            public bool IsFromConstructor { get; private set; }
            public string? Name { get; private set; }
            public IType? Type { get; private set; }
            public object Value { get; private set; }

            public static IAttributeArgument From(
                CustomAttributeTypedArgument arg,
                bool fromConstructor
            )
            {
                return new AttributeArgument(
                    null,
                    arg.Value,
                    arg.ArgumentType.GetTypeInfo(),
                    fromConstructor
                );
            }

            public static IAttributeArgument From(
                CustomAttributeNamedArgument arg,
                bool fromConstructor
            )
            {
                return new AttributeArgument(
                    arg.MemberName,
                    arg.TypedValue.Value,
                    arg.TypedValue.ArgumentType.GetTypeInfo(),
                    fromConstructor
                );
            }
        }

        public class Class : NamedType, IClass
        {
            private readonly TypeInfo value;

            public Class(TypeInfo value)
                : base(value)
            {
                this.value = value;
            }

            public bool IsSealed => value.IsSealed;
            public bool HasBaseClass => BaseClass != null;

            public IClass? BaseClass => From(value.BaseType?.GetTypeInfo());

            public IEnumerable<IMethod> AllConstructors => Method.From(value.GetConstructors());
            public IEnumerable<IMethod> Constructors => Method.From(value.DeclaredConstructors);
            public IEnumerable<IEvent> Events => Event.From(value.DeclaredEvents);
            public IEnumerable<IField> Fields => Field.From(value.DeclaredFields);
            public IEnumerable<IMethod> Methods => Method.From(value.DeclaredMethods);
            public IEnumerable<IProperty> Properties => Property.From(value.DeclaredProperties);

            public IEnumerable<IClass> NestedClasses => From(value.DeclaredNestedTypes);
            public IEnumerable<IDelegate> NestedDelegates =>
                Delegate.From(value.DeclaredNestedTypes);
            public IEnumerable<IEnum> NestedEnums => Enum.From(value.DeclaredNestedTypes);
            public IEnumerable<IInterface> NestedInterfaces =>
                Interface.From(value.DeclaredNestedTypes);

            public static new IClass? From(TypeInfo? type)
            {
                if (type == null || !type.IsClass)
                {
                    return null;
                }
                return new Class(type);
            }

            public static new IEnumerable<IClass> From(IEnumerable<TypeInfo?> types)
            {
                return types
                    .Where(v => v != null)
                    .Where(type => type!.IsClass)
                    .Select(From)
                    .Where(v => v != null)
                    .ToArray()!;
            }
        }

        public class Model : ICodeModel
        {
            private readonly Assembly assembly;

            public Model(Assembly assembly)
            {
                this.assembly = assembly;
            }

            public Assembly Assembly => assembly;
            public IEnumerable<IAttribute> Attributes => Attribute.From(assembly.CustomAttributes);

            public IEnumerable<IClass> Classes => Class.From(assembly.DefinedTypes);
            public IEnumerable<IDelegate> Delegates => Delegate.From(assembly.DefinedTypes);
            public IEnumerable<IEnum> Enums => Enum.From(assembly.DefinedTypes);
            public IEnumerable<IInterface> Interfaces => Interface.From(assembly.DefinedTypes);
            public IEnumerable<IStruct> Structs => Struct.From(assembly.DefinedTypes);

            public static ICodeModel From(Assembly assembly)
            {
                return new Model(assembly);
            }

            public static IEnumerable<ICodeModel> From(IEnumerable<Assembly> assemblies)
            {
                return assemblies.Select(From);
            }
        }

        public class Delegate : NamedType, IDelegate
        {
            private readonly TypeInfo value;

            public Delegate(TypeInfo value)
                : base(value)
            {
                this.value = value;
            }

            public MethodInfo Method => value.GetMethod(nameof(Action.Invoke));
            public IEnumerable<IParameter> Parameters => Parameter.From(Method.GetParameters());
            public IType? ReturnType => Type.From(Method.ReturnType.GetTypeInfo());

            public static new IDelegate? From(TypeInfo? type)
            {
                if (type == null || !type.IsDelegate())
                {
                    return null;
                }
                return new Delegate(type);
            }

            public static new IEnumerable<IDelegate> From(IEnumerable<TypeInfo> type)
            {
                return type.Select(From).Where(d => d != null).ToArray()!;
            }
        }

        public class DocumentationCommentXml : IDocumentationCommentXml
        {
            private readonly string? xml;
            private readonly DocumentationRootNode rootNode;
            private static readonly Regex ExtractElementRegex = new Regex(
                @"(\<(param|summary|returns)(.*?)\>)([\S\s]*?)(\<\/\2\>)",
                RegexOptions.Multiline | RegexOptions.Compiled
            );
            private static readonly Regex ExtractNameRegex = new Regex(
                "name=\"(.*?)\"",
                RegexOptions.Singleline | RegexOptions.Compiled
            );

            public string? Summary => rootNode?.Summary;
            public string? Returns => rootNode?.Returns;

            public IEnumerable<IDocumentationCommentXmlParam> Params => rootNode.Params;

            public DocumentationCommentXml(string? xml = null)
            {
                this.xml = xml;
                rootNode = Deserialize(xml);
            }

            public override string ToString()
            {
                return xml ?? "";
            }

            private DocumentationRootNode Deserialize(string? xml)
            {
                var result = new DocumentationRootNode();

                if (string.IsNullOrEmpty(xml))
                {
                    return result;
                }

                var matches = ExtractElementRegex.Matches(xml);
                foreach (Match match in matches)
                {
                    var elementName = match.Groups[2].Value;
                    var value = match.Groups[4].Value.Trim();

                    switch (elementName)
                    {
                        case "summary":
                            result.Summary = value;
                            break;
                        case "returns":
                            result.Returns = value;
                            break;
                        case "param":
                            var param = new DocumentationParam();
                            param.Value = value;
                            var arguments = match.Groups[3].Value;
                            var nameMatch = ExtractNameRegex.Match(arguments);
                            param.Name = nameMatch.Groups[1].Value.Trim();
                            result.Params.Add(param);
                            break;
                    }
                }

                return result;
            }
        }

        [XmlRoot(elementName: "member")]
        public class DocumentationRootNode
        {
            [XmlElement(elementName: "summary")]
            public string? Summary { get; set; }

            [XmlElement(elementName: "returns")]
            public string? Returns { get; set; }

            [XmlElement(elementName: "param")]
            public List<DocumentationParam> Params { get; set; } = new List<DocumentationParam>();
        }

        public class DocumentationParam : IDocumentationCommentXmlParam
        {
            [XmlAttribute(attributeName: "name")]
            public string? Name { get; set; }

            [XmlText]
            public string? Value { get; set; }
        }

        public class EnumValue : IEnumValue
        {
            public EnumValue(string name, object value, IEnumerable<IAttribute> attributes)
            {
                this.Value = value;
                this.Name = name;
                this.Attributes = attributes;
            }

            public object Value { get; }
            public string Name { get; }
            public IEnumerable<IAttribute> Attributes { get; }

            public static IEnumerable<IEnumValue>? From(TypeInfo? type)
            {
                if (type == null || !type.IsEnum)
                {
                    return null;
                }

                return type.GetMembers()
                    .Where(member => member.DeclaringType == type && member is FieldInfo)
                    .Select(member => member as FieldInfo)
                    .Where(v => v != null)
                    .Select(member => new EnumValue(
                        member!.Name,
                        member!.GetValue(null),
                        Attribute.From(member!.GetCustomAttributesData())
                    ))
                    .ToArray()!;
            }
        }

        public class Enum : NamedType, IEnum
        {
            private readonly TypeInfo ctx;

            public Enum(TypeInfo ctx)
                : base(ctx)
            {
                this.ctx = ctx;
            }

            public IEnumerable<IEnumValue>? Values => EnumValue.From(ctx);
            public IType? UnderlyingType => Type.From(ctx.UnderlyingSystemType.GetTypeInfo());

            public static new IEnum? From(TypeInfo? type)
            {
                if (type == null || !type.IsEnum)
                {
                    return null;
                }
                return new Enum(type);
            }

            public static new IEnumerable<IEnum> From(IEnumerable<TypeInfo> type)
            {
                return type.Select(From).Where(value => value != null).ToArray()!;
            }
        }

        public class Event : SymbolBase, IEvent
        {
            private readonly EventInfo ctx;

            public Event(EventInfo ctx)
                : base(ctx)
            {
                this.ctx = ctx;
            }

            public bool IsSealed =>
                ctx.GetMemberMethods().Count() > 0
                && ctx.GetMemberMethods().All(v => !v.IsVirtual || v.IsFinal);

            public IType? Type => Models.Type.From(ctx.GetMemberType());

            public static IEvent From(EventInfo value)
            {
                return new Event(value);
            }

            public static IEnumerable<IEvent> From(IEnumerable<EventInfo> e)
            {
                return e.Select(From).ToArray();
            }
        }

        public class Field : SymbolBase, IField
        {
            private readonly FieldInfo ctx;

            public Field(FieldInfo ctx)
                : base(ctx.FieldType.GetTypeInfo())
            {
                this.ctx = ctx;
            }

            public IType? Type => Models.Type.From(ctx.GetMemberType());
            public bool IsConst => ctx is { IsLiteral: true, IsInitOnly: false };
            public bool IsReadOnly => ctx is { IsInitOnly: true };
            public bool HasConstantValue => IsConst;
            public object ConstantValue => ctx.GetRawConstantValue();

            public static IField From(FieldInfo field)
            {
                return new Field(field);
            }

            public static IEnumerable<IField> From(IEnumerable<FieldInfo> fields)
            {
                return fields.Select(From).ToArray();
            }
        }

        public class Interface : NamedType, IInterface
        {
            private readonly TypeInfo ctx;

            public Interface(TypeInfo ctx)
                : base(ctx)
            {
                this.ctx = ctx;
            }

            public IEnumerable<IEvent> Events => Event.From(ctx.DeclaredEvents);
            public IEnumerable<IMethod> Methods => Method.From(ctx.DeclaredMethods);
            public IEnumerable<IProperty> Properties => Property.From(ctx.DeclaredProperties);

            public static new IInterface? From(TypeInfo? type)
            {
                if (type == null || !type.IsInterface)
                {
                    return null;
                }
                return new Interface(type);
            }

            public static new IEnumerable<IInterface> From(IEnumerable<TypeInfo> types)
            {
                return types
                    .Where(type => type.IsInterface)
                    .Select(From)
                    .Where(v => v != null)
                    .ToArray()!;
            }

            public static IEnumerable<IInterface> From(IEnumerable<System.Type> types)
            {
                return types
                    .Select(t => t.GetTypeInfo())
                    .Select(From)
                    .Where(v => v != null)
                    .ToArray()!;
            }
        }

        public class Location : ILocation
        {
            public bool IsInSource => false;
            public string Path => "";

            public int StartLinePosition => 0;
            public int EndLinePosition => 0;

            public static Location? Default => null;
        }

        public class Method : SymbolBase, IMethod
        {
            private readonly MethodBase mbase;
            private readonly MethodInfo? minfo;

            public bool IsGeneric => mbase.IsGenericMethod;
            public IEnumerable<ITypeParameter> TypeParameters => new ITypeParameter[] { };
            public IEnumerable<IParameter> Parameters => Parameter.From(mbase.GetParameters());
            public IEnumerable<IType> TypeArguments =>
                Type.From(mbase.GetGenericArguments().Select(t => t.GetTypeInfo()));

            public IType? ReturnType => Type.From(minfo?.ReturnType.GetTypeInfo());

            public bool IsAsync =>
                minfo?.ReturnType.IsAssignableFrom(typeof(System.Threading.Tasks.Task)) ?? false;
            public bool IsOverride => minfo?.GetBaseDefinition() != null;
            public bool IsSealed => mbase.IsFinal;

            public override string Name => mbase.Name;
            public override string FullName => mbase.Name;

            private Method(MethodBase ctx)
                : base(ctx)
            {
                this.mbase = ctx;
                this.minfo = ctx as MethodInfo;
            }

            private Method(MethodInfo ctx)
                : base(ctx)
            {
                this.minfo = ctx;
                this.mbase = ctx;
            }

            public static IMethod From(MethodInfo method)
            {
                return new Method(method);
            }

            public static IEnumerable<IMethod> From(IEnumerable<MethodInfo> methods)
            {
                return methods.Select(From).ToArray();
            }

            public static IMethod From(ConstructorInfo constructor)
            {
                return new Method(constructor);
            }

            public static IEnumerable<IMethod> From(IEnumerable<ConstructorInfo> constructors)
            {
                return constructors.Select(From).ToArray();
            }
        }

        public class NamedType : Type, INamedType
        {
            private readonly TypeInfo value;



            public NamedType(TypeInfo value)
                : base(value)
            {
                this.value = value;
            }

            public IEnumerable<ITypeParameter> TypeParameters =>
                Enumerable.Empty<ITypeParameter>().ToArray();

            public bool IsNested => value.DeclaringType != null;

            public static new NamedType? From(TypeInfo? type)
            {
                if (type == null)
                {
                    return null;
                }
                return new NamedType(type);
            }
        }

        public class Parameter : NamedType, IParameter
        {
            private readonly ParameterInfo value;

            public override string Name => value.Name;

            public Parameter(ParameterInfo value)
                : base(value.ParameterType.GetTypeInfo())
            {
                this.value = value;
            }

            public IType? Type => Models.Type.From(value.ParameterType.GetTypeInfo());
            public object? DefaultValue => value.HasDefaultValue ? value.DefaultValue : null;
            public bool HasDefaultValue => value.HasDefaultValue;

            public static IEnumerable<IParameter> From(MethodInfo method)
            {
                return From(method.GetParameters()).ToArray();
            }

            public static IParameter From(ParameterInfo parameter)
            {
                return new Parameter(parameter);
            }

            public static IEnumerable<IParameter> From(IEnumerable<ParameterInfo> parameters)
            {
                return parameters.Select(From).ToArray();
            }
        }

        public class Property : SymbolBase, IProperty
        {
            private readonly PropertyInfo property;

            public IType? Type => Models.Type.From(property.PropertyType.GetTypeInfo());
            public bool IsIndexer => property.GetIndexParameters().Length > 0;
            public bool IsWriteOnly => property.CanWrite && !property.CanRead;
            public bool IsReadOnly => property.CanRead && !property.CanWrite;
            public bool IsSealed => property.PropertyType.IsSealed;

            public Property(PropertyInfo property)
                : base(property)
            {
                this.property = property;
            }

            public static IProperty From(PropertyInfo property)
            {
                return new Property(property);
            }

            public static IEnumerable<IProperty> From(IEnumerable<PropertyInfo> properties)
            {
                return properties.Select(From).ToArray();
            }
        }

        public class Struct : NamedType, IStruct
        {
            private readonly TypeInfo value;

            public Struct(TypeInfo value)
                : base(value)
            {
                this.value = value;
            }

            public IEnumerable<IMethod> AllConstructors => Method.From(value.GetConstructors());
            public IEnumerable<IMethod> Constructors => Method.From(value.DeclaredConstructors);
            public IEnumerable<IEvent> Events => Event.From(value.DeclaredEvents);
            public IEnumerable<IField> Fields => Field.From(value.DeclaredFields);
            public IEnumerable<IMethod> Methods => Method.From(value.DeclaredMethods);
            public IEnumerable<IProperty> Properties => Property.From(value.DeclaredProperties);

            public IEnumerable<IClass> NestedClasses => Class.From(value.DeclaredNestedTypes);
            public IEnumerable<IDelegate> NestedDelegates =>
                Delegate.From(value.DeclaredNestedTypes);
            public IEnumerable<IEnum> NestedEnums => Enum.From(value.DeclaredNestedTypes);
            public IEnumerable<IInterface> NestedInterfaces =>
                Interface.From(value.DeclaredNestedTypes);

            public static new IStruct? From(TypeInfo? type)
            {
                if (type == null || !type.IsStruct())
                {
                    return null;
                }
                return new Struct(type);
            }

            public static new IEnumerable<IStruct> From(IEnumerable<TypeInfo?> type)
            {
                return type.Select(From).Where(value => value != null).ToArray()!;
            }
        }

        public class Type : ISymbolBase, IType, ITypeReferencedByMember
        {
            private readonly TypeInfo value;
            public Type(TypeInfo value)
            {
                this.value = value;
            }

            public IType? BaseType => Type.From(value.BaseType?.GetTypeInfo());

            public bool IsAnonymousType => value.IsAnonymousType();
            public bool IsEnumerable => value.IsEnumerable();
            public bool IsDelegate => value.IsDelegate();
            public bool IsEnum => value.IsEnum;
            public bool IsCollection => value.IsCollection();
            public bool IsDynamic => value.IsDynamic();
            public bool IsGeneric => value.IsGenericType;
            public bool IsInterface => value.IsInterface;
            public bool IsNullable => value.IsNullable();
            public bool IsPrimitive => value.IsPrimitive;
            public bool IsRecord => value.IsRecord();
            public bool IsTuple => value.IsTuple();
            public bool IsReferenceType => value.IsByRef;
            public bool IsValueType => value.IsValueType;
            public IEnumerable<IInterface> Interfaces =>
                Interface.From(value.GetDeclaredInterfaces());
            public IEnumerable<IInterface> AllInterfaces => Interface.From(value.GetInterfaces());
            public IType? ArrayType =>
                Type.From(value.IsArray ? value.GetElementType().GetTypeInfo() : null);
            public IEnumerable<IType> TypeArguments =>
                Type.From(value.GetGenericArguments().Select(t => t.GetTypeInfo()));

            public virtual string Name => value.Name;
            public virtual string FullName => value.FullName;
            /*
            {
                get
                {
                    var prefix = "";
                    if (string.IsNullOrEmpty(Namespace) == false)
                    {
                        prefix = Namespace + ".";
                    }
                    return $"{prefix}{this.BareName}";
                }
            }*/
            public string Namespace => value.Namespace;


            public IEnumerable<IAttribute> Attributes => Attribute.From(value.CustomAttributes);
            public INamedType? ContainingType => NamedType.From(value.DeclaringType.GetTypeInfo());
            public IDocumentationCommentXml DocComment => new DocumentationCommentXml();

            public bool IsAbstract => value.IsAbstract();
            public bool IsArray => value.IsArray();
            public bool IsErrorType => value.IsErrorType();
            public bool IsPublic => value.IsPublic();
            public bool IsStatic => value.IsStatic();
            public bool IsTypeParameter => value.IsTypeParameter();
            public bool IsVirtual => value.IsVirtual();

            public bool IsEvent => value.IsAssignableTo<EventInfo>();
            public bool IsField => value.IsAssignableTo<FieldInfo>();
            public bool IsMethod => value.IsAssignableTo<MethodInfo>();
            public bool IsProperty => value.IsAssignableTo<PropertyInfo>();

            public string BareName
            {
                get
                {
                    string name = Name;
                    int prefixLength = 0;

                    for (int i = 0; i < name.Length; ++i)
                    {
                        if (!(char.IsLetterOrDigit(Name[i]) || Name[i] == '_'))
                        {
                            break;
                        }
                        prefixLength++;
                    }

                    return name[..prefixLength];
                }
            }

            public IEnumerable<ILocation> Locations => Enumerable.Empty<ILocation>();
            public string SourceCode => "";

            ISymbolBase? ITypeReferencedByMember.Parent => null;

            public static IType? From(TypeInfo? type)
            {
                if (type == null)
                {
                    return null;
                }
                return new Type(type);
            }

            public static IEnumerable<IType> From(IEnumerable<TypeInfo> type)
            {
                return type.Select(From).Where(v => v != null).ToArray()!;
            }
        }


        public class SymbolBase : ISymbolBase
        {
            private readonly MemberInfo member;

            public IEnumerable<IAttribute> Attributes => Attribute.From(member.CustomAttributes);
            public INamedType? ContainingType => NamedType.From(member.DeclaringType.GetTypeInfo());
            public IDocumentationCommentXml DocComment => new DocumentationCommentXml();

            public bool IsAbstract => member.IsAbstract();
            public bool IsArray => member.IsArray();
            public bool IsErrorType => member.IsErrorType();
            public bool IsPublic => member.IsPublic();
            public bool IsStatic => member.IsStatic();
            public bool IsTypeParameter => member.IsTypeParameter();
            public bool IsVirtual => member.IsVirtual();

            public bool IsEvent => member is EventInfo;
            public bool IsField => member is FieldInfo;
            public bool IsMethod => member is MethodInfo;
            public bool IsProperty => member is PropertyInfo;

            public virtual string Name => member.Name;
            public virtual string FullName
            {
                get
                {
                    var prefix = "";
                    if (string.IsNullOrEmpty(Namespace) == false)
                    {
                        prefix = Namespace + ".";
                    }
                    return $"{prefix}{this.BareName}";
                }
            }

            public string BareName
            {
                get
                {
                    string name = Name;
                    int prefixLength = 0;

                    for (int i = 0; i < name.Length; ++i)
                    {
                        if (!(char.IsLetterOrDigit(Name[i]) || Name[i] == '_'))
                        {
                            break;
                        }
                        prefixLength++;
                    }

                    return name[..prefixLength];
                }
            }

            public virtual string? Namespace => member.GetMemberType()?.Namespace?.ToString();

            public IEnumerable<ILocation> Locations => Enumerable.Empty<ILocation>();
            public string SourceCode => "";

            public SymbolBase(MemberInfo type)
            {
                this.member = type;
            }
        }
    }
}
