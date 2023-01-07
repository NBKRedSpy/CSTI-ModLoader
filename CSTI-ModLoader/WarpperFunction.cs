﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using System.Runtime.Remoting;
using System.Collections;
using LitJson;

namespace ModLoader
{
    public class WarpperFunction
    {
        public enum WarpType
        {
            NONE,
            COPY,
            CUSTOM,
            REFERENCE,
            ADD,
            MODIFY
        }

        public static void JsonCommonWarpper(System.Object obj, LitJson.JsonData json)
        {
            if(!json.IsObject)
                return;

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var obj_type = obj.GetType();
            foreach (var key in json.Keys)
            {
                try
                {
                    if (key.EndsWith("WarpType"))
                    {
                        if (!json[key].IsInt || !json.ContainsKey(key.Substring(0, key.Length - 8) + "WarpData"))
                            continue;
                        if ((int)json[key] == (int)WarpType.REFERENCE)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var field = obj_type.GetField(field_name, bindingFlags);
                            var field_type = field.FieldType;

                            if (json[field_name + "WarpData"].IsString)
                            {
                                if (field_type.IsSubclassOf(typeof(UniqueIDScriptable)))
                                {
                                    ObjectReferenceWarpper(obj, json[field_name + "WarpData"].ToString(), field_name, ModLoader.AllGUIDDict);
                                }
                                else if (field_type.IsSubclassOf(typeof(ScriptableObject)))
                                {
                                    if (ModLoader.AllScriptableObjectWithoutGUIDDict.TryGetValue(field_type.Name, out var type_dict))
                                        ObjectReferenceWarpper(obj, json[field_name + "WarpData"].ToString(), field_name, type_dict);
                                    else
                                        UnityEngine.Debug.LogWarning("Error: CommonWarpper No Such Dict " + field_type.Name);
                                }
                                else if (field_type == typeof(UnityEngine.Sprite))
                                {
    
                                    ObjectReferenceWarpper(obj, json[field_name + "WarpData"].ToString(), field_name, ModLoader.SpriteDict);
                                }
                                else if (field_type == typeof(UnityEngine.AudioClip))
                                {
                                    ObjectReferenceWarpper(obj, json[field_name + "WarpData"].ToString(), field_name, ModLoader.AudioClipDict);
                                }
                                else if (field_type == typeof(ScriptableObject))
                                {
                                    ObjectReferenceWarpper(obj, json[field_name + "WarpData"].ToString(), field_name, ModLoader.AllCardOrTagDict);
                                }
                                else
                                {
                                    UnityEngine.Debug.LogWarning("Error: CommonWarpper Unexpect Object Type " + field_type.Name);
                                } 
                            }
                            else if (json[field_name + "WarpData"].IsArray)
                            {
                                Type sub_field_type = null;
                                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                {
                                    sub_field_type = field.FieldType.GetGenericArguments().Single();
                                }
                                else if (field.FieldType.IsArray)
                                {
                                    sub_field_type = field.FieldType.GetElementType();
                                }
                                else
                                {
                                    UnityEngine.Debug.LogWarning("Error: CommonWarpper Wrong WarpData Format " + field_type.Name);
                                }

                                List<string> list_data = new List<string>();
                                for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                {
                                    if (json[field_name + "WarpData"][i].IsString)
                                        list_data.Add(json[field_name + "WarpData"][i].ToString());
                                }

                                if (list_data.Count != json[field_name + "WarpData"].Count)
                                    UnityEngine.Debug.LogWarning("Error: CommonWarpper Wrong WarpData Format " + sub_field_type.Name);

                                if (sub_field_type.IsSubclassOf(typeof(UniqueIDScriptable)))
                                {
                                    ObjectReferenceWarpper(obj, list_data, field_name, ModLoader.AllGUIDDict);
                                }
                                else if (sub_field_type.IsSubclassOf(typeof(ScriptableObject)))
                                {
                                    if (ModLoader.AllScriptableObjectWithoutGUIDDict.TryGetValue(sub_field_type.Name, out var type_dict))
                                        ObjectReferenceWarpper(obj, list_data, field_name, type_dict);
                                    else
                                        UnityEngine.Debug.LogWarning("Error: CommonWarpper No Such Dict " + sub_field_type.Name);
                                }
                                else if (sub_field_type == typeof(UnityEngine.Sprite))
                                {
                                    ObjectReferenceWarpper(obj, list_data, field_name, ModLoader.SpriteDict);
                                }
                                else if (sub_field_type == typeof(UnityEngine.AudioClip))
                                {
                                    ObjectReferenceWarpper(obj, list_data, field_name, ModLoader.AudioClipDict);
                                }
                                else if (sub_field_type == typeof(ScriptableObject))
                                {
                                    ObjectReferenceWarpper(obj, list_data, field_name, ModLoader.AllCardOrTagDict);
                                }
                                else
                                {
                                    UnityEngine.Debug.LogWarning("Error: CommonWarpper Unexpect List Object Type  " + sub_field_type.Name);
                                }
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarning("Error: CommonWarpper Wrong WarpData Format " + field_type.Name);
                            }
                        }
                        else if ((int)json[key] == (int)WarpType.ADD)
                        {

                        }
                        else if ((int)json[key] == (int)WarpType.MODIFY)
                        {

                        }
                    }
                    else if (key.EndsWith("WarpData"))
                        continue;
                    else 
                    {
                        if ((json[key].IsObject))
                        {
                            var field_name = key;
                            var field = obj_type.GetField(field_name, bindingFlags);
                            if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                continue;
                            var sub_obj = field.GetValue(obj);
                            JsonCommonWarpper(sub_obj, json[key]);
                            field.SetValue(obj, sub_obj);
                        }
                        else if (json[key].IsArray)
                        {
                            var field_name = key;
                            var field = obj_type.GetField(field_name, bindingFlags);

                            for (int i = 0; i < json[key].Count; i++)
                            {
                                if (json[key][i].IsObject)
                                {
                                    if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                    {
                                        var ele_type = field.FieldType.GetGenericArguments().Single();
                                        if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                            break;
                                        var list = field.GetValue(obj) as IList;
                                        var ele = list[i];
                                        JsonCommonWarpper(ele, json[key][i]);
                                        list[i] = ele;
                                        field.SetValue(obj, list);
                                    }
                                    else if (field.FieldType.IsArray)
                                    {
                                        var ele_type = field.FieldType.GetElementType();
                                        if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                            break;
                                        var array = field.GetValue(obj) as Array;
                                        var ele = array.GetValue(i);
                                        JsonCommonWarpper(ele, json[key][i]);
                                        array.SetValue(ele, i);
                                        field.SetValue(obj, array);
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    UnityEngine.Debug.LogError(string.Format("Error: CommonWarpper {0}  {1}", obj_type.Name, ex.Message));
                }
            }
        }

        public static void ClassWarpper(System.Object obj, string field_name, WarpType warp_type, string data, string src_dir)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ClassWarpper Object IsNotClass");
            //    return;
            //}
            string method_name;
            if (warp_type == WarpType.NONE)
                return;
            else if (warp_type == WarpType.COPY)
                method_name = "WarpperCopy";
            else if (warp_type == WarpType.CUSTOM)
                method_name = "WarpperCustom";
            else if (warp_type == WarpType.REFERENCE)
                method_name = "WarpperReference";
            else if (warp_type == WarpType.ADD)
                method_name = "WarpperAdd";
            else if (warp_type == WarpType.MODIFY)
                method_name = "WarpperModify";
            else
            {
                UnityEngine.Debug.LogWarning("ClassWarpper Unkown Warp Type");
                return;
            }

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                //if (warp_type == WarpType.REFERENCE && !field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                //{
                //    UnityEngine.Debug.LogWarning("ClassWarpper Reference Warp Field Must be Subclass of ScriptableObject");
                //    return;
                //}

                Type warpper_type = Type.GetType("ModLoader." + field.FieldType.Name + "Warpper");
                var warpper = Activator.CreateInstance(warpper_type, ModLoader.CombinePaths(src_dir, field_name));
                var temp_obj = new object[] { obj, data, field_name };
                warpper_type.GetMethod(method_name, bindingFlags, null, new Type[] { obj.GetType(), typeof(string), typeof(string) }, null).Invoke(warpper, temp_obj);
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ClassWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ClassWarpper(System.Object obj, string field_name, WarpType warp_type, List<string> data, string src_dir)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ClassWarpper Object IsNotClass");
            //    return;
            //}
            string method_name;
            if (warp_type == WarpType.NONE)
                return;
            else if (warp_type == WarpType.COPY)
                method_name = "WarpperCopy";
            else if (warp_type == WarpType.CUSTOM)
                method_name = "WarpperCustom";
            else if (warp_type == WarpType.REFERENCE)
                method_name = "WarpperReference";
            else if (warp_type == WarpType.ADD)
                method_name = "WarpperAdd";
            else if (warp_type == WarpType.MODIFY)
                method_name = "WarpperModify";
            else
            {
                UnityEngine.Debug.LogWarning("ClassWarpper Unkown Warp Type");
                return;
            }

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                //if (warp_type == WarpType.REFERENCE && !field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                //{
                //    UnityEngine.Debug.LogWarning("ClassWarpper Reference Warp Field Must be Subclass of ScriptableObject");
                //    return;
                //}

                Type ele_type;
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    ele_type = field.FieldType.GetGenericArguments().Single();
                }
                else if (field.FieldType.IsArray)
                {
                    ele_type = field.FieldType.GetElementType();
                }
                else
                {
                    UnityEngine.Debug.LogWarning("ClassWarpper Object Field Must be Array or List");
                    return;
                }

                Type warpper_type = Type.GetType("ModLoader." + ele_type.Name + "Warpper");
                var warpper = Activator.CreateInstance(warpper_type, ModLoader.CombinePaths(src_dir, field_name));
                var temp_obj = new object[] { obj, data, field_name };
                warpper_type.GetMethod(method_name, bindingFlags, null, new Type[] { obj.GetType(), typeof(List<string>), typeof(string) }, null).Invoke(warpper, temp_obj);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ClassWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void UniqueIDScriptableCopyWarpper(System.Object obj, string data, string field_name)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("UniqueIDScriptableCopyWarpper Object IsNotClass");
            //    return;
            //}
            if (ModLoader.AllGUIDDict.TryGetValue(data, out var ele))
            {

                try
                {
                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var field = obj.GetType().GetField(field_name, bindingFlags);
                    if (field != ele.GetType().GetField(field_name, bindingFlags))
                    {
                        UnityEngine.Debug.LogError("UniqueIDScriptableCopyWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name + "Field not Same");
                        return;
                    }
                    field.SetValue(obj, field.GetValue(ele));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(string.Format("Error: UniqueIDScriptableCopyWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
                }
            }
        }

        public static void UniqueIDScriptableCopyWarpper(System.Object obj, List<string> data, string field_name)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("UniqueIDScriptableCopyWarpper Object IsNotClass");
            //    return;
            //}
            if (data.Count > 0 && ModLoader.AllGUIDDict.TryGetValue(data[0], out var ele))
            {
                try
                {
                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var field = obj.GetType().GetField(field_name, bindingFlags);
                    if (field != ele.GetType().GetField(field_name, bindingFlags))
                    {
                        UnityEngine.Debug.LogError("UniqueIDScriptableCopyWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name + "Field not Same");
                        return;
                    }
                    field.SetValue(obj, field.GetValue(ele));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(string.Format("Error: UniqueIDScriptableCopyWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
                }
        }
        }

        public static void ObjectCustomWarpper(System.Object obj, string data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectCustomWarpper Object IsNotClass");
            //    return;
            //}
            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
         
                var instance = field.GetValue(obj);
                if (!instance.GetType().IsClass)
                {
                    using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data)))
                    {
                        string json_data = sr.ReadToEnd();
                        instance = UnityEngine.JsonUtility.FromJson(json_data, field.FieldType);
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, warpper);
                    }
                    var temp_obj = new object[] { instance };
                    warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(warpper, temp_obj);
                    field.SetValue(obj, temp_obj[0]);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data)))
                    {
                        string json_data = sr.ReadToEnd();
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, warpper);
                    }
                    warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { field.FieldType }, null).Invoke(warpper, new object[] { instance });
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ObjectCustomWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectCustomWarpper(System.Object obj, List<string> data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectCustomWarpper Object IsNotClass");
            //    return;
            //}

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj);
                    field.FieldType.GetMethod("Clear").Invoke(instance, null);
                    for (int i = 0; i < data.Count; i++)
                    {
                        var ele_type = field.FieldType.GetGenericArguments().Single();
                        var ele = Activator.CreateInstance(ele_type);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                ele = UnityEngine.JsonUtility.FromJson(json_data, ele_type);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            field.FieldType.GetMethod("Add").Invoke(instance, temp_obj);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            field.FieldType.GetMethod("Add").Invoke(instance, new object[] { ele });
                        }
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    ArrayResize(ref instance, data.Count);
                    for (int i = 0; i < data.Count; i++)
                    {
                        var ele_type = field.FieldType.GetElementType();
                        var ele = Activator.CreateInstance(ele_type);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                ele = UnityEngine.JsonUtility.FromJson(json_data, ele_type);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            instance.SetValue(temp_obj[0], i);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            instance.SetValue(ele, i);
                        }
                    }
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ObjectCustomWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectReferenceWarpper<ValueType>(System.Object obj, string data, string field_name, Dictionary<string, ValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectReferenceWarpper Object IsNotClass");
            //    return;
            //}
            if (dict.TryGetValue(data, out var ele))
            {
                try
                {
                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var field = obj.GetType().GetField(field_name, bindingFlags);
                    field.SetValue(obj, ele);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(string.Format("Error: ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
                }
            }
        }

        public static void ObjectReferenceWarpper<ValueType>(System.Object obj, List<string> data, string field_name, Dictionary<string, ValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectReferenceWarpper Object IsNotClass");
            //    return;
            //}
            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj);
                    foreach (var name in data)
                        if (dict.TryGetValue(name, out var ele))
                        {
                            var temp_obj = new object[] { ele };
                            field.FieldType.GetMethod("Add", bindingFlags).Invoke(instance, temp_obj);
                        }
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    ArrayResize(ref instance, data.Count);
                    for (int i = 0; i < data.Count; i++)
                        if (dict.TryGetValue(data[i], out var ele))
                            instance.SetValue(ele, i);
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectAddWarpper(System.Object obj, string data, string field_name, WarpperBase warpper)
        {
            UnityEngine.Debug.LogError(string.Format("Error: ObjectAddWarpper {0}.{1} {2}", obj.GetType().Name, field_name, "AddWarpper Only Vaild in List or Array Filed"));
        }

        public static void ObjectAddWarpper(System.Object obj, List<string> data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectAddWarpper Object IsNotClass");
            //    return;
            //}

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj) as IList;
                    for (int i = 0; i < data.Count; i++)
                    {
                        var ele_type = field.FieldType.GetGenericArguments().Single();
                        var ele = Activator.CreateInstance(ele_type);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                ele = UnityEngine.JsonUtility.FromJson(json_data, ele_type);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            instance.Add(temp_obj[0]);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            instance.Add(ele);
                        }
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    int start_idx = instance.Length;
                    ArrayResize(ref instance, data.Count + instance.Length);
                    for (int i = 0; i < data.Count; i++)
                    {
                        var ele_type = field.FieldType.GetElementType();
                        var ele = Activator.CreateInstance(ele_type);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                ele = UnityEngine.JsonUtility.FromJson(json_data, ele_type);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            instance.SetValue(temp_obj[0], i + start_idx);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            instance.SetValue(ele, i + start_idx);
                        }
                    }
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ObjectAddWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectWarpperModify(System.Object obj, string data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectWarpperModify Object IsNotClass");
            //    return;
            //}
            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);

                var instance = field.GetValue(obj);
                if (!instance.GetType().IsClass)
                {
                    using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data)))
                    {
                        string json_data = sr.ReadToEnd();
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, warpper);
                    }
                    var temp_obj = new object[] { instance };
                    warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(warpper, temp_obj);
                    field.SetValue(obj, temp_obj[0]);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data)))
                    {
                        string json_data = sr.ReadToEnd();
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, warpper);
                    }
                    warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { field.FieldType }, null).Invoke(warpper, new object[] { instance });
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ObjectWarpperModify {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectWarpperModify(System.Object obj, List<string> data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectWarpperModify Object IsNotClass");
            //    return;
            //}

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj) as IList;
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i] == "")
                            continue;
                        var ele_type = field.FieldType.GetGenericArguments().Single();
                        var ele = instance[i];
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                        }
                        instance[i] = ele;
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i] == "")
                            continue;
                        var ele_type = field.FieldType.GetElementType();
                        var ele = instance.GetValue(i);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            instance.SetValue(temp_obj[0], i);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            instance.SetValue(ele, i);
                        }
                    }
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ObjectWarpperModify {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ArrayResize(ref Array array, int newSize)
        {
            Type elementType = array.GetType().GetElementType();
            Array newArray = Array.CreateInstance(elementType, newSize);
            Array.Copy(array, newArray, Math.Min(array.Length, newArray.Length));
            array = newArray;
        }

    }
}
