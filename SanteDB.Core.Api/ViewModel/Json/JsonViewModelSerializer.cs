/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.ViewModel.Description;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.ViewModel.Json
{
    /// <summary>
    /// Represents a JSON view model serializer
    /// </summary>
    public class JsonViewModelSerializer : IViewModelSerializer
    {

        // Complex type cache
        private static readonly ConcurrentDictionary<Type, bool> s_complexTypeCache = new ConcurrentDictionary<Type, bool>();

        // Formatters
        private ConcurrentDictionary<Type, IJsonViewModelTypeFormatter> m_formatters = new ConcurrentDictionary<Type, IJsonViewModelTypeFormatter>();

        // Classifiers
        private ConcurrentDictionary<Type, IViewModelClassifier> m_classifiers = new ConcurrentDictionary<Type, IViewModelClassifier>();

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(JsonViewModelSerializer));

        // Related load methods
        private ConcurrentDictionary<Type, MethodInfo> m_relatedLoadMethods = new ConcurrentDictionary<Type, MethodInfo>();

        // Reloated load association
        private ConcurrentDictionary<Type, MethodInfo> m_relatedLoadAssociations = new ConcurrentDictionary<Type, MethodInfo>();

        private Dictionary<Guid, IEnumerable> m_loadedAssociations = new Dictionary<Guid, IEnumerable>();
        private Dictionary<Guid, IdentifiedData> m_loadedObjects = new Dictionary<Guid, IdentifiedData>();

        /// <summary>
        /// Creates a json view model serializer
        /// </summary>
        public JsonViewModelSerializer(bool noDelayLoad = false)
        {
            this.ViewModel = ViewModelDescription.Load(typeof(JsonViewModelSerializer).Assembly.GetManifestResourceStream("SanteDB.Core.ViewModel.Default.xml"));
            this.ForbidDelayLoad = noDelayLoad;
        }

        /// <inheritdoc/>
        public bool ForbidDelayLoad { get; }

        /// <summary>
        /// Gets or sets the view model definition for this view model serializer
        /// </summary>
        public ViewModelDescription ViewModel
        {
            get; set;
        }


        /// <summary>
        /// De-serializes the specified object from the stream
        /// </summary>
        public TModel DeSerialize<TModel>(Stream s)
        {
            try
            {
                return (TModel)this.DeSerialize(s, typeof(TModel));
            }
            catch (Exception e)
            {
                try
                {
                    if (s.CanSeek)
                    {
                        s.Seek(0, SeekOrigin.Begin);
                        using (var ms = new MemoryStream())
                        {
                            s.CopyTo(ms);
                            this.m_tracer.TraceError("{0} -> {1}", e.Message, Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length));
                        }
                    }
                }
                catch { }
                throw;
            }
        }

        /// <summary>
        /// De-serialize data from the string
        /// </summary>
        public TModel DeSerialize<TModel>(String s)
        {
            using (var sr = new StringReader(s))
            {
                return (TModel)this.DeSerialize(sr, typeof(TModel));
            }
        }

        /// <summary>
        /// De-serializes the data from the stream as the specified type
        /// </summary>
        public Object DeSerialize(Stream s, Type t)
        {
            using (StreamReader sr = new StreamReader(s))
            {
                return this.DeSerialize(sr, t);
            }
        }

        /// <summary>
        /// De-serializes data from the reader as specified type
        /// </summary>
        public Object DeSerialize(TextReader r, Type t)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            this.m_tracer.TraceVerbose("PERF >>> PARSING {0}", t);
#endif
            try
            {
                using (JsonReader jr = new JsonTextReader(r))
                {
                    // Seek to the start object token
                    while (jr.TokenType != JsonToken.StartObject && jr.Read())
                    {
                        ;
                    }

                    if (jr.TokenType == JsonToken.StartObject)
                    {
                        var retVal = this.ReadElementUtil(jr, t, new JsonSerializationContext(null, this, null));
                        if (retVal is IdentifiedData id)
                        {
                            id.HarmonizeKeys(KeyHarmonizationMode.PropertyOverridesKey);
                        }
                        return retVal;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                this.m_tracer.TraceError("Error de-serializing: {0}", ex);
                throw;
            }
            finally
            {
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceVerbose("PERF >>> PARSED {0} IN {1} ms", t, sw.ElapsedMilliseconds);
#endif
            }
        }

        /// <summary>
        /// Read the specified element
        /// </summary>
        public object ReadElementUtil(JsonReader r, Type t, JsonSerializationContext context)
        {
            var nonGenericT = t.StripGeneric();
            var classifier = this.GetClassifier(nonGenericT);
            switch (r.TokenType)
            {
                case JsonToken.StartObject:
                    {
                        var formatter = this.GetFormatter(nonGenericT);
                        bool isList = typeof(IList).IsAssignableFrom(t);
                        // Classifier???
                        if (classifier == null || !isList)
                        {
                            if (isList)
                            {
                                var retVal = Activator.CreateInstance(t) as IList;
                                retVal.Add(formatter.Deserialize(r, t, context));
                                return retVal;
                            }
                            else
                            {
                                return formatter.Deserialize(r, t, context);
                            }
                        }
                        else
                        {
                            // Classifier each of these properties aren't real properties, rather they are classified things
                            int depth = r.Depth;
                            Dictionary<String, Object> values = new Dictionary<string, object>();
                            while (r.Read() && !(r.TokenType == JsonToken.EndObject && r.Depth == depth))
                            {
                                // Classifier
                                if (r.TokenType != JsonToken.PropertyName)
                                {
                                    throw new JsonException($"Expected PropertyName token got {r.TokenType}");
                                }

                                string propertyName = (String)r.Value;
                                r.Read(); // Read proeprty name
                                values.Add(propertyName, this.ReadElementUtil(r, r.TokenType == JsonToken.StartObject ? nonGenericT : t, new JsonSerializationContext(propertyName, this, values, context)));
                            }
                            return classifier.Compose(values, t);
                        }
                    }
                // Array read, we want to re-call the specified parse
                case JsonToken.StartArray:
                    {
                        if (!typeof(IList).IsAssignableFrom(t))
                        {
                            throw new JsonSerializationException($"{t} does not implement IList at {r.Path}");
                        }

                        int depth = r.Depth;
                        var listInstance = Activator.CreateInstance(t) as IList;
                        while (r.Read() && !(r.TokenType == JsonToken.EndArray && r.Depth == depth))
                        {
                            listInstance.Add(this.ReadElementUtil(r, nonGenericT, context));
                        }

                        return listInstance;
                    }
                case JsonToken.Null:
                case JsonToken.Boolean:
                case JsonToken.Bytes:
                case JsonToken.Float:
                case JsonToken.Date:
                case JsonToken.Integer:
                case JsonToken.String:
                    if (typeof(IdentifiedData).IsAssignableFrom(nonGenericT)) // Complex object
                    {
                        var formatter = this.GetFormatter(nonGenericT);
                        return formatter.FromSimpleValue(r.Value);
                    }
                    else
                    {
                        // Not a simple value attribute so let's just handle it as normal
                        switch (r.TokenType)
                        {
                            case JsonToken.Null:
                                return null;

                            case JsonToken.Boolean:
                            case JsonToken.Bytes:
                                return r.Value;

                            case JsonToken.Float:
                                if (t.StripNullable() == typeof(Decimal))
                                {
                                    return Convert.ToDecimal(r.Value);
                                }
                                else if (t.StripNullable() == typeof(Int32))
                                {
                                    return Convert.ToInt32(r.Value);
                                }
                                else if (t.StripNullable().IsEnum)
                                {
                                    return Enum.ToObject(t.StripNullable(), Convert.ToInt32(r.Value));
                                }
                                else
                                {
                                    return (Double)r.Value;
                                }

                            case JsonToken.Date:
                                t = t.StripNullable();
                                if (t == typeof(DateTime))
                                {
                                    return (DateTime)r.Value;
                                }
                                else if (t == typeof(String))
                                {
                                    return ((DateTime)r.Value).ToString("o");
                                }
                                else
                                {
                                    return new DateTimeOffset((DateTime)r.Value);
                                }

                            case JsonToken.Integer:
                                t = t.StripNullable();
                                if (t.StripNullable().IsEnum)
                                {
                                    return Enum.ToObject(t.StripNullable(), r.Value);
                                }

                                return Convert.ChangeType(r.Value, t);

                            case JsonToken.String:
                                if (String.IsNullOrEmpty((string)r.Value))
                                {
                                    return null;
                                }
                                else if (t.StripNullable() == typeof(Guid))
                                {
                                    return Guid.Parse((string)r.Value);
                                }
                                else if (t.StripNullable() == typeof(Int32))
                                {
                                    return "NaN".Equals(r.Value) ? 0 : Int32.Parse((String)r.Value);
                                }
                                else if (t.StripNullable() == typeof(Decimal))
                                {
                                    return Decimal.Parse((String)r.Value);
                                }
                                else if (t.StripNullable() == typeof(byte[]))
                                {
                                    return Convert.FromBase64String((String)r.Value);
                                }
                                else if (t.StripNullable().IsEnum && r.ValueType == typeof(String))
                                {
                                    return Enum.Parse(t.StripNullable(), (string)r.Value);
                                }
                                else
                                {
                                    return r.Value;
                                }

                            default:
                                return r.Value;
                        }
                    }
                default:
                    throw new JsonSerializationException("Invalid serialization");
            }
        }

        /// <summary>
        /// Attempts to get the loaded object
        /// </summary>
        public object GetLoadedObject(Guid key)
        {
            this.m_loadedObjects.TryGetValue(key, out IdentifiedData value);
            return value;
        }

        /// <summary>
        /// Load related object
        /// </summary>
        internal object LoadRelated(Type propertyType, Guid key)
        {
            if (this.ForbidDelayLoad)
            {
                return null;
            }

            MethodInfo methodInfo = null;
            if (!this.m_relatedLoadMethods.TryGetValue(propertyType, out methodInfo))
            {
                methodInfo = this.GetType().GetRuntimeMethod(nameof(LoadRelated), new Type[] { typeof(Guid) }).MakeGenericMethod(propertyType);
                this.m_relatedLoadMethods.TryAdd(propertyType, methodInfo);
            }
            return methodInfo.Invoke(this, new object[] { key });
        }

        /// <summary>
        /// Load collection
        /// </summary>
        internal IEnumerable LoadCollection(Type propertyType, Guid key)
        {
            if (this.ForbidDelayLoad)
            {
                return new Object[0];
            }

            MethodInfo methodInfo = null;

            // Load
            if (!this.m_relatedLoadAssociations.TryGetValue(propertyType, out methodInfo))
            {
                methodInfo = this.GetType().GetRuntimeMethod(nameof(LoadCollection), new Type[] { typeof(Guid) }).MakeGenericMethod(propertyType.StripGeneric());
                this.m_relatedLoadAssociations.TryAdd(propertyType, methodInfo);
            }
            var listValue = methodInfo.Invoke(this, new object[] { key }) as IEnumerable;
            if (propertyType.IsAssignableFrom(listValue.GetType()))
            {
                return listValue;
            }
            else
            {
                var retVal = Activator.CreateInstance(propertyType, listValue);
                return retVal as IEnumerable;
            }
        }

        /// <summary>
        /// Delay loads the specified collection association
        /// </summary>
        public IEnumerable<TAssociation> LoadCollection<TAssociation>(Guid sourceKey) where TAssociation : IdentifiedData, ISimpleAssociation, new()
        {
            if (this.ForbidDelayLoad)
            {
                return new TAssociation[0];
            }
#if DEBUG
            this.m_tracer.TraceVerbose("Delay loading collection for object : {0}", sourceKey);
#endif
            // Have we already loaded this in the current serializer?
            IEnumerable association = null;
            if (!this.m_loadedAssociations.TryGetValue(sourceKey, out association))
            {
                try
                {
                    association = EntitySource.Current.Provider.GetRelations<TAssociation>(sourceKey);
                    if (this.m_loadedAssociations.ContainsKey(sourceKey))
                    {
                        this.m_loadedAssociations.Add(sourceKey, association);
                    }
                }
                catch { }
            }
            return association as IEnumerable<TAssociation>;
        }

        /// <summary>
        /// Load the related information
        /// </summary>
        public TRelated LoadRelated<TRelated>(Guid? objectKey) where TRelated : IdentifiedData, new()
        {

            if (this.ForbidDelayLoad)
            {
                return default(TRelated);
            }
#if DEBUG
            this.m_tracer.TraceVerbose("Delay loading related object : {0}", objectKey);
#endif
            IdentifiedData value = null;
            if (objectKey.HasValue && !this.m_loadedObjects.TryGetValue(objectKey.Value, out value))
            {
                try
                {
                    value = EntitySource.Current.Provider.Get<TRelated>(objectKey);
                    this.m_loadedObjects.Add(objectKey.Value, value);
                    return (TRelated)value;
                }
                catch { return null; }
            }
            else if (value != default(TRelated))
            {
                return (TRelated)value;
            }
            else
            {
                return default(TRelated);
            }
        }

        /// <summary>
        /// Write property utility
        /// </summary>
        public void WritePropertyUtil(JsonWriter w, String propertyName, Object instance, SerializationContext context, bool noSubContext = false)
        {
            if (instance == null)
            {
                return;
            }

            // first write the property
            if (!String.IsNullOrEmpty(propertyName))  // In an array so don't emit the property name
            {
                // Are we to never serialize this?
                if (context?.ShouldSerialize(propertyName) == false)
                {
                    return;
                }
                else
                {
                    w.WritePropertyName(propertyName);
                }
            }

            if (this.IsTypeComplex(instance.GetType()))
            {
                // Complex type .. allow the formatter to handle this
                IJsonViewModelTypeFormatter typeFormatter = this.GetFormatter(instance.GetType());

                var simpleValue = typeFormatter.GetSimpleValue(instance);
                // Is the simple value a complex object?
                if (simpleValue != null && propertyName != "$other" && context != null) // Special case for $other
                {
                    w.WriteValue(simpleValue);
                }
                else
                {
                    w.WriteStartObject();

                    var subContext = noSubContext && w.Path.EndsWith("]") ? context as JsonSerializationContext : new JsonSerializationContext(propertyName, this, instance, context as JsonSerializationContext);
                    int? parentObjectRef = null;

                    if (instance is IdentifiedData identifiedData)
                    {
                        this.WriteSimpleProperty(w, "$type", identifiedData.Type);
                        parentObjectRef = context?.GetParentObjectId(identifiedData);
                    }

                    this.WriteSimpleProperty(w, "$id", String.Format("obj{0}", subContext.ObjectId));

                    // Write ref
                    if (parentObjectRef.HasValue) // Recursive
                    {
                        this.WriteSimpleProperty(w, "$ref", String.Format("#obj{0}", parentObjectRef.Value));
                    }
                    else
                    {
                        typeFormatter.Serialize(w, instance, subContext);
                    }

                    w.WriteEndObject();
                }
            }
            else if (instance is IList ilst && !instance.GetType().IsArray)
            {
                // Classifications?
                var classifier = this.GetClassifier(instance.GetType().StripNullable());

                if (classifier == null) // no classifier
                {
                    w.WriteStartArray();
                    foreach (var itm in ilst)
                    {
                        this.WritePropertyUtil(w, null, itm, new JsonSerializationContext(propertyName, this, instance, context as JsonSerializationContext), noSubContext);
                    }
                    w.WriteEndArray();
                }
                else
                {
                    w.WriteStartObject();
                    foreach (var cls in classifier.Classify(instance as IList))
                    {
                        Object value = new List<Object>(cls.Value as IEnumerable<Object>);
                        //if (cls.Value.Count == 1)
                        //    value = cls.Value[0];
                        // Now write
                        this.WritePropertyUtil(w, cls.Key, value, new JsonSerializationContext(propertyName, this, instance, context as JsonSerializationContext), true);
                    }
                    w.WriteEndObject();
                }
            }
            else
            {
                w.WriteValue(instance);
            }
        }

        /// <summary>
        /// Determine if the type <paramref name="type"/> is complex (nested JSON object)
        /// </summary>
        private bool IsTypeComplex(Type type)
        {
            if(!s_complexTypeCache.TryGetValue(type, out var retVal))
            {
                retVal =type.StripNullable() == type // Not null
                    && type.IsClass
                    && (type.GetCustomAttribute<XmlTypeAttribute>() != null ||
                    type.GetCustomAttribute<JsonObjectAttribute>() != null);
                s_complexTypeCache.TryAdd(type, retVal);
            }
            return retVal;
        }

        /// <summary>
        /// Gets the appropriate classifier for the specified type
        /// </summary>
        public IViewModelClassifier GetClassifier(Type type)
        {
            IViewModelClassifier retVal = null;
            if (!this.m_classifiers.TryGetValue(type, out retVal))
            {
                var classifierAtt = type.StripGeneric().GetCustomAttribute<ClassifierAttribute>();
                if (classifierAtt != null)
                {
                    retVal = new JsonReflectionClassifier(type, this);
                }

                this.m_classifiers.TryAdd(type, retVal);
            }
            return retVal;
        }

        /// <summary>
        /// Write a simple property
        /// </summary>
        /// <param name="propertyName">The name of the JSON property</param>
        /// <param name="value">The value of the JSON property</param>
        /// <param name="w">The value of the property</param>
        private void WriteSimpleProperty(JsonWriter w, String propertyName, Object value)
        {
            w.WritePropertyName(propertyName);
            w.WriteValue(value);
        }

        /// <summary>
        /// Get the specified formatter
        /// </summary>
        /// <param name="type">The type to retrieve the formatter for</param>
        public IJsonViewModelTypeFormatter GetFormatter(Type type)
        {
            IJsonViewModelTypeFormatter typeFormatter = null;
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (!this.m_formatters.TryGetValue(type, out typeFormatter))
            {
                typeFormatter = new JsonReflectionTypeFormatter(type);
                this.m_formatters.TryAdd(type, typeFormatter);
            }
            return typeFormatter;
        }

        /// <summary>
        /// Load the specified serializer assembly
        /// </summary>
        public void LoadSerializerAssembly(Assembly asm)
        {

            var typeFormatters = asm.GetExportedTypesSafe().Where(o => typeof(IJsonViewModelTypeFormatter).IsAssignableFrom(o) && o.IsClass)
                .Select(o => Activator.CreateInstance(o) as IJsonViewModelTypeFormatter)
                .Where(o => !this.m_formatters.ContainsKey(o.HandlesType));
            var classifiers = asm.GetExportedTypesSafe().Where(o => typeof(IViewModelClassifier).IsAssignableFrom(o) && o.IsClass)
                .Select(o => Activator.CreateInstance(o) as IViewModelClassifier)
                .Where(o => !this.m_classifiers.ContainsKey(o.HandlesType));
            foreach (var fmtr in typeFormatters)
            {
                this.m_formatters.TryAdd(fmtr.HandlesType, fmtr);
            }

            foreach (var cls in classifiers)
            {
                this.m_classifiers.TryAdd(cls.HandlesType, cls);
            }
        }

        /// <summary>
        /// Serialize object to string
        /// </summary>
        public String Serialize(object data)
        {
            using (StringWriter sw = new StringWriter())
            {
                this.Serialize(sw, data);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Serialize the specified data
        /// </summary>
        public void Serialize(Stream s, object data)
        {
            using (StreamWriter tw = new StreamWriter(s))
            {
                this.Serialize(tw, data);
            }
        }

        /// <summary>
        /// Serialize to the specified text writer
        /// </summary>
        public void Serialize(TextWriter tw, object data)
        {
            using (JsonWriter jw = new JsonTextWriter(tw))
            {
                this.Serialize(jw, data);
            }
        }

        /// <summary>
        /// Serialize data
        /// </summary>
        /// <param name="jw"></param>
        /// <param name="data"></param>
        public void Serialize(JsonWriter jw, object data)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            this.m_tracer.TraceVerbose("PERF >>> SERIALIZING {0}", data);
#endif
            try
            {
                if (data is IdentifiedData idd)
                {
                    data = idd.HarmonizeKeys(KeyHarmonizationMode.PropertyOverridesKey);
                }
                this.WritePropertyUtil(jw, null, data, null);
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error serializing {0} : {1}", data, e);
                throw;
            }
            finally
            {
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceVerbose("PERF >>> SERIALIZED {0} IN {1} ms", data, sw.ElapsedMilliseconds);
#endif
            }
        }

        /// <summary>
        /// Add a loaded object
        /// </summary>
        public void AddLoadedObject(Guid key, IdentifiedData classifierObj)
        {
            if (!this.m_loadedObjects.ContainsKey(key))
            {
                this.m_loadedObjects.Add(key, classifierObj);
            }
        }

        /// <summary>
        /// Dispose of the specified object
        /// </summary>
        public void Dispose()
        {
            this.m_loadedObjects.Clear();
            this.m_relatedLoadMethods.Clear();
            this.m_relatedLoadAssociations.Clear();
            this.m_loadedObjects = null;
            this.m_relatedLoadAssociations = null;
            this.m_relatedLoadMethods = null;
        }
    }
}