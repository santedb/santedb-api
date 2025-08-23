/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SanteDB.Core.ViewModel.Json
{
    /// <summary>
    /// Represents a simple reflection based classifier
    /// </summary>
    internal class JsonReflectionClassifier : IViewModelClassifier
    {
        // Classifier attribute
        private ClassifierAttribute m_classifierAttribute;

        // Classifier hash map
        private static ConcurrentDictionary<Type, ClassifierAttribute> m_classifierCache = new ConcurrentDictionary<Type, ClassifierAttribute>();

        private static ConcurrentDictionary<Type, Dictionary<String, Object>> m_classifierObjectCache = new ConcurrentDictionary<Type, Dictionary<string, object>>();

        // Type
        private Type m_type;

        // The serializer that owns this serializer
        private IViewModelSerializer m_serializer;

        /// <summary>
        /// Creates a new reflection based classifier
        /// </summary>
        public JsonReflectionClassifier(Type type, IViewModelSerializer owner)
        {
            this.m_type = type;
            var classifierAtt = type.StripGeneric().GetCustomAttribute<ClassifierAttribute>();
            this.m_classifierAttribute = classifierAtt;
            this.m_serializer = owner;
        }

        /// <summary>
        /// Gets the type this handles
        /// </summary>
        public Type HandlesType
        {
            get
            {
                return this.m_type;
            }
        }

        /// <summary>
        /// Classify the specified data element
        /// </summary>
        public Dictionary<string, IList> Classify(IList data)
        {
            Dictionary<String, IList> retVal = new Dictionary<string, IList>();

            // copy for the enumeration check
            object[] copy;
            lock (data.SyncRoot)
            {
                copy = new object[data.Count];
                data.CopyTo(copy, 0);
            }

            foreach (var itm in copy)
            {
                var classifier = this.GetClassifierObj(itm, this.m_classifierAttribute);
                String classKey = classifier?.ToString();

                if (string.IsNullOrEmpty(classKey))
                {
                    classKey = "$other";
                }


                IList group = null;
                if (!retVal.TryGetValue(classKey, out group))
                {
                    group = new List<Object>();
                    retVal.Add(classKey, group);
                }
                group.Add(itm);
            }
            return retVal;
        }

        /// <summary>
        /// Perform a re-classification of values
        /// </summary>
        public IList Compose(Dictionary<string, object> values, Type retValType)
        {
            //var retValType = typeof(List<>).MakeGenericType(this.m_type);
            var retVal = Activator.CreateInstance(retValType) as IList;

            foreach (var itm in values)
            {
                PropertyInfo classifierProperty = this.m_type.GetRuntimeProperty(this.m_classifierAttribute.ClassifierProperty),
                    setProperty = classifierProperty;

                String propertyName = setProperty.Name;
                Object itmClassifier = null, target = itmClassifier;

                // Construct the classifier
                if (itm.Key != "$other")
                {
                    while (propertyName != null)
                    {
                        var classifierValue = typeof(IdentifiedData).IsAssignableFrom(setProperty.PropertyType) ?
                            this.LoadClassifier(setProperty.PropertyType, itm.Key) :
                            itm.Key;

                        if (target != null)
                        {
                            setProperty.SetValue(target, classifierValue);
                        }
                        else
                        {
                            itmClassifier = target = classifierValue;
                        }

                        propertyName = setProperty.PropertyType.GetCustomAttribute<ClassifierAttribute>()?.ClassifierProperty;
                        if (propertyName != null)
                        {
                            setProperty = setProperty.PropertyType.GetRuntimeProperty(propertyName);
                            target = classifierValue;
                        }
                    }

                    if (classifierProperty.PropertyType.IsEnum)
                    {
                        itmClassifier = Enum.Parse(classifierProperty.PropertyType, (String)itmClassifier);
                    }

                }



                // Now set the classifiers
                foreach (var inst in itm.Value as IList ?? new List<Object>() { itm.Value })
                {
                    if (inst == null)
                    {
                        continue;
                    }

                    if (itm.Key != "$other")
                    {
                        classifierProperty.SetValue(inst, itmClassifier);

                        // Set the key property as well 
                        if (itmClassifier is IIdentifiedResource irc)
                        {
                            var keyProperty = classifierProperty.GetSerializationRedirectProperty();
                            if (keyProperty != null)
                            {
                                keyProperty.SetValue(inst, irc.Key);
                            }
                        }
                    }


                    retVal.Add(inst);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Load classifier
        /// </summary>
        private Object LoadClassifier(Type type, string classifierValue)
        {
            Dictionary<String, Object> classValue = new Dictionary<string, object>();
            if (!m_classifierObjectCache.TryGetValue(type, out classValue))
            {
                classValue = new Dictionary<string, object>();
                lock (m_classifierObjectCache)
                {
                    if (!m_classifierObjectCache.ContainsKey(type))
                    {
                        m_classifierObjectCache.TryAdd(type, classValue);
                    }
                }
            }

            Object retVal = null;
            if (!classValue.TryGetValue(classifierValue, out retVal))
            {
                var funcType = typeof(Func<,>).MakeGenericType(type, typeof(bool));
                var exprType = typeof(Expression<>).MakeGenericType(funcType);
                var mi = typeof(IEntitySourceProvider).GetGenericMethod(nameof(IEntitySourceProvider.Query), new Type[] { type }, new Type[] { exprType });
                var classPropertyName = type.GetCustomAttribute<ClassifierAttribute>()?.ClassifierProperty;
                if (classPropertyName != null)
                {
                    var rtp = type.GetRuntimeProperty(classPropertyName);
                    if (rtp != null && typeof(String) == rtp.PropertyType)
                    {
                        var parm = Expression.Parameter(type);
                        var exprBody = Expression.MakeBinary(ExpressionType.Equal, Expression.MakeMemberAccess(parm, rtp), Expression.Constant(classifierValue));
                        var builderMethod = typeof(Expression).GetGenericMethod(nameof(Expression.Lambda), new Type[] { funcType }, new Type[] { typeof(Expression), typeof(ParameterExpression[]) });
                        var funcExpr = builderMethod.Invoke(null, new object[] { exprBody, new ParameterExpression[] { parm } });
                        retVal = (mi.Invoke(EntitySource.Current.Provider, new object[] { funcExpr }) as IEnumerable).OfType<Object>().FirstOrDefault();
                    }
                }
                retVal = retVal ?? Activator.CreateInstance(type);
                lock (classValue)
                {
                    if (!classValue.ContainsKey(classifierValue))
                    {
                        classValue.Add(classifierValue, retVal);
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Get classifier object
        /// </summary>
        private object GetClassifierObj(object o, ClassifierAttribute classifierAttribute)
        {
            if (o == null)
            {
                return null;
            }

            var classProperty = o.GetType().GetRuntimeProperty(classifierAttribute.ClassifierProperty);
            var classifierObj = classProperty.GetValue(o);
            if (classifierObj == null)
            {
                // Force load
                var keyPropertyName = classProperty.GetCustomAttribute<SerializationReferenceAttribute>()?.RedirectProperty;
                if (keyPropertyName == null)
                {
                    return null;
                }

                var keyPropertyValue = o.GetType().GetRuntimeProperty(keyPropertyName).GetValue(o);

                // Does the owner serializer already load this?
                if (keyPropertyValue != null)
                {
                    classifierObj = this.m_serializer.GetLoadedObject((Guid)keyPropertyValue);
                }

                if (classifierObj == null)
                {
                    // Now we want to force load!!!!
                    var getValueMethod = typeof(EntitySource).GetGenericMethod("Get", new Type[] { classProperty.PropertyType }, new Type[] { typeof(Guid?) });
                    classifierObj = getValueMethod.Invoke(EntitySource.Current, new object[] { keyPropertyValue });
                    classProperty.SetValue(o, classifierObj);
                    if (keyPropertyValue != null)
                    {
                        this.m_serializer.AddLoadedObject((Guid)keyPropertyValue, (IdentifiedData)classifierObj);
                    }
                }
            }

            if (classifierObj != null)
            {
                if (!m_classifierCache.TryGetValue(classifierObj.GetType(), out classifierAttribute))
                {
                    lock (m_classifierCache)
                    {
                        if (!m_classifierCache.ContainsKey(classifierObj.GetType()))
                        {
                            classifierAttribute = classifierObj?.GetType().GetCustomAttribute<ClassifierAttribute>();
                            m_classifierCache.TryAdd(classifierObj.GetType(), classifierObj.GetType().GetCustomAttribute<ClassifierAttribute>());
                        }
                    }
                }
            }

            if (classifierAttribute != null)
            {
                return this.GetClassifierObj(classifierObj, classifierAttribute);
            }

            return classifierObj;
        }
    }
}