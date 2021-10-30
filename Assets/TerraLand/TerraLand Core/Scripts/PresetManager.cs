#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

public class PresetManager : MonoBehaviour
{
    public static void SavePreset (UnityEngine.Object script, string fileName)
    {
        string presetFilePath = EditorUtility.SaveFilePanel("Save Settings As Preset File", Application.dataPath, fileName, "xml");

        if (!string.IsNullOrEmpty(presetFilePath))
        {
            SerializedObject SO = new SerializedObject(script);
            SerializedProperty SP = SO.GetIterator();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = false;
            settings.NewLineOnAttributes = true;
            bool isGeneric = false;
            bool isEnteredList = false;
            string listName = "";
            SerializedPropertyType tempType = SerializedPropertyType.String;

            using (XmlWriter writer = XmlWriter.Create(presetFilePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Properties");
                writer.WriteElementString("ObjectType", script.GetType().ToString());

                while (SP.NextVisible(true))
                {
                    SerializedPropertyType type = SP.propertyType;

                    if (!SP.name.Equals("m_Script"))
                    {
                        if (type.Equals(SerializedPropertyType.Generic))
                        {
                            //writer.WriteElementString(SP.name, SP.name.ToString());
                            //writer.WriteElementString(SP.name, SP.GetArrayElementAtIndex(0).ToString());

                            //int length = SP.arraySize;
                            //
                            //for(int i = 0; i < length; i++)
                            //{
                            //    writer.WriteElementString(SP.name, SP.GetArrayElementAtIndex(i).name);
                            //}

                            if (isEnteredList)
                            {
                                writer.WriteEndElement();
                                isEnteredList = false;
                            }

                            listName = SP.name;
                            isGeneric = true;
                        }

                        if (type.Equals(SerializedPropertyType.ObjectReference))
                        {
                            if (!isEnteredList && isGeneric)
                                writer.WriteStartElement(listName);

                            if (SP.objectReferenceValue != null)
                            {
                                string objectReferenceStr = SP.objectReferenceValue.ToString();
                                string propertyName = SP.name;
                                string objectName = objectReferenceStr.Substring(0, objectReferenceStr.LastIndexOf('(') - 1);
                                string objectTypeStart = objectReferenceStr.Substring(objectReferenceStr.LastIndexOf('(') + 1);
                                string objectType = objectTypeStart.Substring(0, objectTypeStart.LastIndexOf(')'));
                                string objectPath = AssetDatabase.GetAssetOrScenePath(SP.objectReferenceValue);

                                writer.WriteStartElement(propertyName);
                                writer.WriteElementString("name", objectName);
                                writer.WriteElementString("type", objectType);
                                writer.WriteElementString("path", objectPath);
                                writer.WriteEndElement();
                            }
                            else
                                writer.WriteElementString(SP.name, "null");

                            isEnteredList = true;
                        }
                        else
                        {
                            if (isEnteredList && isGeneric)
                            {
                                writer.WriteEndElement();
                                isGeneric = false;
                            }

                            isEnteredList = false;
                        }

                        //else if (type.Equals(SerializedPropertyType.ArraySize))
                        //{
                        //    //writer.WriteElementString(SP.name, SP.intValue.ToString());
                        //
                        //    int length = SP.arraySize;
                        //
                        //    for(int i = 0; i < length; i++)
                        //    {
                        //        writer.WriteElementString(SP.GetArrayElementAtIndex(i).name, SP.intValue.ToString());
                        //    }
                        //
                        //
                        //    //writer.WriteElementString(SP., SP.intValue.ToString());
                        //
                        //}

                        if (type.Equals(SerializedPropertyType.LayerMask))
                        {
                            int layerIndex = Mathf.RoundToInt(Mathf.Log(SP.intValue, 2));
                            writer.WriteElementString(SP.name, layerIndex.ToString());
                        }
                        else if (type.Equals(SerializedPropertyType.AnimationCurve))
                        {
                            Keyframe[] keys = SP.animationCurveValue.keys;
                            writer.WriteStartElement(SP.name);
                            
                            foreach (Keyframe k in keys)
                            {
                                writer.WriteElementString("value", k.value.ToString());
                                writer.WriteElementString("time", k.time.ToString());
                                writer.WriteElementString("inTangent", k.inTangent.ToString());
                                writer.WriteElementString("outTangent", k.outTangent.ToString());
                            }
                            
                            writer.WriteEndElement();
                        }
                        else if (type.Equals(SerializedPropertyType.Boolean))
                            writer.WriteElementString(SP.name, SP.boolValue.ToString());
                        else if (type.Equals(SerializedPropertyType.Bounds))
                        {
                            string valueStr = SP.boundsValue.ToString();
                            string[] sections = valueStr.Split(')');
                            sections[0] += ")";
                            sections[1] += ")";
                            string trimStart = sections[0].Substring(sections[0].LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');

                            float X = float.Parse(values[0].Trim());
                            float Y = float.Parse(values[1].Trim());
                            float Z = float.Parse(values[2].Trim());

                            string valueCS = X + "," + Y + "," + Z;

                            trimStart = sections[1].Substring(sections[1].LastIndexOf('(') + 1);
                            trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            values = trimEnd.Split(',');

                            X = float.Parse(values[0].Trim());
                            Y = float.Parse(values[1].Trim());
                            Z = float.Parse(values[2].Trim());

                            valueCS += "," + X + "," + Y + "," + Z;

                            writer.WriteElementString(SP.name, valueCS);
                        }
                        else if (type.Equals(SerializedPropertyType.BoundsInt))
                        {
                            string valueStr = SP.boundsIntValue.ToString();
                            string[] sections = valueStr.Split(')');
                            sections[0] += ")";
                            sections[1] += ")";
                            string trimStart = sections[0].Substring(sections[0].LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');

                            int X = int.Parse(values[0].Trim());
                            int Y = int.Parse(values[1].Trim());
                            int Z = int.Parse(values[2].Trim());

                            string valueCS = X + "," + Y + "," + Z;

                            trimStart = sections[1].Substring(sections[1].LastIndexOf('(') + 1);
                            trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            values = trimEnd.Split(',');

                            X = int.Parse(values[0].Trim());
                            Y = int.Parse(values[1].Trim());
                            Z = int.Parse(values[2].Trim());

                            valueCS += "," + X + "," + Y + "," + Z;

                            writer.WriteElementString(SP.name, valueCS);
                        }
                        else if (type.Equals(SerializedPropertyType.Character))
                            writer.WriteElementString(SP.name, SP.ToString());
                        else if (type.Equals(SerializedPropertyType.Color))
                        {
                            string valueStr = SP.colorValue.ToString();
                            string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');

                            float X = float.Parse(values[0].Trim());
                            float Y = float.Parse(values[1].Trim());
                            float Z = float.Parse(values[2].Trim());
                            float W = float.Parse(values[3].Trim());

                            string valueCS = X + "," + Y + "," + Z + "," + W;
                            writer.WriteElementString(SP.name, valueCS);
                        }
                        else if (type.Equals(SerializedPropertyType.Enum))
                            writer.WriteElementString(SP.name, SP.enumValueIndex.ToString());
                        else if (type.Equals(SerializedPropertyType.FixedBufferSize))
                            writer.WriteElementString(SP.name, SP.fixedBufferSize.ToString());
                        else if (type.Equals(SerializedPropertyType.Float))
                        {
                            if
                            (
                                tempType.Equals(SerializedPropertyType.Vector2) ||
                                tempType.Equals(SerializedPropertyType.Vector3) ||
                                tempType.Equals(SerializedPropertyType.Vector4) ||
                                tempType.Equals(SerializedPropertyType.Bounds) ||
                                tempType.Equals(SerializedPropertyType.Rect) ||
                                tempType.Equals(SerializedPropertyType.Quaternion)
                            )
                            {
                                // Skip
                            }
                            else
                                writer.WriteElementString(SP.name, SP.floatValue.ToString());
                        }
                        else if (type.Equals(SerializedPropertyType.Integer))
                        {
                            if
                            (
                                tempType.Equals(SerializedPropertyType.Vector2Int) ||
                                tempType.Equals(SerializedPropertyType.Vector3Int) ||
                                tempType.Equals(SerializedPropertyType.BoundsInt) ||
                                tempType.Equals(SerializedPropertyType.RectInt)
                            )
                            {
                                // Skip
                            }
                            else
                                writer.WriteElementString(SP.name, SP.intValue.ToString());
                        }  
                        else if (type.Equals(SerializedPropertyType.Quaternion))
                        {
                            string valueStr = SP.quaternionValue.ToString();
                            string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');

                            float X = float.Parse(values[0].Trim());
                            float Y = float.Parse(values[1].Trim());
                            float Z = float.Parse(values[2].Trim());
                            float W = float.Parse(values[3].Trim());

                            string valueCS = X + "," + Y + "," + Z + "," + W;
                            writer.WriteElementString(SP.name, valueCS);

                            tempType = type;
                        }
                        else if (type.Equals(SerializedPropertyType.Rect))
                        {
                            string valueStr = SP.rectValue.ToString();
                            string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');
                            
                            float X = float.Parse(values[0].Substring(values[0].LastIndexOf(':') + 1).Trim());
                            float Y = float.Parse(values[1].Substring(values[1].LastIndexOf(':') + 1).Trim());
                            float Z = float.Parse(values[2].Substring(values[2].LastIndexOf(':') + 1).Trim());
                            float W = float.Parse(values[3].Substring(values[3].LastIndexOf(':') + 1).Trim());

                            string valueCS = X + "," + Y + "," + Z + "," + W;
                            writer.WriteElementString(SP.name, valueCS);

                            tempType = type;
                        } 
                        else if (type.Equals(SerializedPropertyType.RectInt))
                        {
                            string valueStr = SP.rectIntValue.ToString();
                            string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');

                            int X = int.Parse(values[0].Substring(values[0].LastIndexOf(':') + 1).Trim());
                            int Y = int.Parse(values[1].Substring(values[1].LastIndexOf(':') + 1).Trim());
                            int Z = int.Parse(values[2].Substring(values[2].LastIndexOf(':') + 1).Trim());
                            int W = int.Parse(values[3].Substring(values[3].LastIndexOf(':') + 1).Trim());

                            string valueCS = X + "," + Y + "," + Z + "," + W;
                            writer.WriteElementString(SP.name, valueCS);

                            tempType = type;
                        } 
                        else if (type.Equals(SerializedPropertyType.String))
                        {
                            if(!tempType.Equals(SerializedPropertyType.ExposedReference))
                                writer.WriteElementString(SP.name, SP.stringValue);
                        }
                        else if (type.Equals(SerializedPropertyType.Vector2))
                        {
                            string valueStr = SP.vector2Value.ToString();
                            string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');

                            float X = float.Parse(values[0].Trim());
                            float Y = float.Parse(values[1].Trim());

                            string valueCS = X + "," + Y;
                            writer.WriteElementString(SP.name, valueCS);

                            tempType = type;
                        } 
                        else if (type.Equals(SerializedPropertyType.Vector2Int))
                        {
                            string valueStr = SP.vector2IntValue.ToString();
                            string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');

                            int X = int.Parse(values[0].Trim());
                            int Y = int.Parse(values[1].Trim());

                            string valueCS = X + "," + Y;
                            writer.WriteElementString(SP.name, valueCS);

                            tempType = type;
                        }
                        else if (type.Equals(SerializedPropertyType.Vector3))
                        {
                            if
                            (
                                SP.name.Equals("m_Center") ||
                                SP.name.Equals("m_Extent")
                            )
                            {
                                // Skip
                            }
                            else
                            {
                                string valueStr = SP.vector3Value.ToString();
                                string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                                string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                                string[] values = trimEnd.Split(',');

                                float X = float.Parse(values[0].Trim());
                                float Y = float.Parse(values[1].Trim());
                                float Z = float.Parse(values[2].Trim());

                                string valueCS = X + "," + Y + "," + Z;
                                writer.WriteElementString(SP.name, valueCS);
                            }

                            tempType = type;
                        }
                        else if (type.Equals(SerializedPropertyType.Vector3Int))
                        {
                            if
                            (
                                SP.name.Equals("m_Position") ||
                                SP.name.Equals("m_Size")
                            )
                            {
                                // Skip
                            }
                            else
                            {
                                string valueStr = SP.vector3IntValue.ToString();
                                string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                                string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                                string[] values = trimEnd.Split(',');

                                int X = int.Parse(values[0].Trim());
                                int Y = int.Parse(values[1].Trim());
                                int Z = int.Parse(values[2].Trim());

                                string valueCS = X + "," + Y + "," + Z;
                                writer.WriteElementString(SP.name, valueCS);
                            }

                            tempType = type;
                        }
                        else if (type.Equals(SerializedPropertyType.Vector4))
                        {
                            string valueStr = SP.vector4Value.ToString();
                            string trimStart = valueStr.Substring(valueStr.LastIndexOf('(') + 1);
                            string trimEnd = trimStart.Substring(0, trimStart.LastIndexOf(')'));
                            string[] values = trimEnd.Split(',');

                            float X = float.Parse(values[0].Trim());
                            float Y = float.Parse(values[1].Trim());
                            float Z = float.Parse(values[2].Trim());
                            float W = float.Parse(values[3].Trim());

                            string valueCS = X + "," + Y + "," + Z + "," + W;
                            writer.WriteElementString(SP.name, valueCS);

                            tempType = type;
                        }
                        else if (type.Equals(SerializedPropertyType.ExposedReference))
                        {
                            // TODO
                            //if (SP.exposedReferenceValue != null)
                            //    writer.WriteElementString(SP.name, SP.exposedReferenceValue.ToString());
                            //else
                            //    writer.WriteElementString(SP.name, "null");

                            tempType = type;
                        }
                        else if (type.Equals(SerializedPropertyType.Gradient))
                        {
                            // TODO
                        }
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            AssetDatabase.Refresh();
        }
    }

    public static void LoadPreset (MonoBehaviour script)
    {
        string[] filters = new string[] { "Preset Files", "xml" };
        string presetFilePath = EditorUtility.OpenFilePanelWithFilters("Load Preset File", Application.dataPath, filters);

        Type t = script.GetType();
        //object o = FI.GetValue(script);

        if (!string.IsNullOrEmpty(presetFilePath))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(presetFilePath);

            string objectType = doc.DocumentElement.SelectSingleNode("ObjectType").InnerText;

            if (!t.Name.Equals(objectType))
            {
                EditorUtility.DisplayDialog("PRESET MISMATCH", "Selected preset file does not match with this script!", "Ok");
                return;
            }

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string key = node.Name;
                string value = node.InnerText;

                if (!key.Equals("ObjectType") && !value.Equals("null"))
                {
                    FieldInfo FI = t.GetField(key);
                    string FT = FI.FieldType.Name;
                    //UnityEngine.Debug.Log(key +"   "+ FT);

                    if (FT.Equals("List`1"))
                    {
                        XmlNodeList nodeList = node.ChildNodes;
                        List<GameObject> list = new List<GameObject>();

                        foreach (XmlNode childNode in nodeList)
                        {
                            string name = childNode.ChildNodes[0].InnerText;
                            string type = childNode.ChildNodes[1].InnerText;
                            string path = childNode.ChildNodes[2].InnerText;

                            if (path.EndsWith(".unity"))
                            {
                                Scene scene = SceneManager.GetSceneByPath(path);
                                List<GameObject> sceneObjects = new List<GameObject>();
                                scene.GetRootGameObjects(sceneObjects);

                                foreach (GameObject g in sceneObjects)
                                {
                                    if (g.name.Equals(name))
                                    {
                                        list.Add(g.gameObject);
                                        break;
                                    }

                                    foreach (Transform tr in g.transform)
                                    {
                                        if (tr.name.Equals(name))
                                        {
                                            list.Add(tr.gameObject);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                GameObject objectReference = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                                list.Add(objectReference);
                            }
                        }

                        FI.SetValue(script, list);
                    }
                    else if (FT.Equals("GameObject"))
                    {
                        string name = node.ChildNodes[0].InnerText;
                        string type = node.ChildNodes[1].InnerText;
                        string path = node.ChildNodes[2].InnerText;

                        if (path.EndsWith(".unity"))
                        {
                            Scene scene = SceneManager.GetSceneByPath(path);
                            List<GameObject> sceneObjects = new List<GameObject>();
                            scene.GetRootGameObjects(sceneObjects);

                            foreach (GameObject g in sceneObjects)
                            {
                                if (g.name.Equals(name))
                                {
                                    FI.SetValue(script, g.gameObject);
                                    break;
                                }

                                foreach (Transform tr in g.transform)
                                {
                                    if (tr.name.Equals(name))
                                    {
                                        FI.SetValue(script, tr.gameObject);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            GameObject objectReference = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                            FI.SetValue(script, objectReference);
                        }
                    }
                    else if (FT.Equals("Object"))
                    {
                        string name = node.ChildNodes[0].InnerText;
                        string type = node.ChildNodes[1].InnerText;
                        string path = node.ChildNodes[2].InnerText;

                        if (path.EndsWith(".unity"))
                        {
                            Scene scene = SceneManager.GetSceneByPath(path);
                            List<GameObject> sceneObjects = new List<GameObject>();
                            scene.GetRootGameObjects(sceneObjects);

                            foreach (GameObject g in sceneObjects)
                            {
                                if (g.name.Equals(name))
                                {
                                    FI.SetValue(script, g.gameObject);
                                    break;
                                }

                                foreach (Transform tr in g.transform)
                                {
                                    if (tr.name.Equals(name))
                                    {
                                        FI.SetValue(script, tr.gameObject);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            UnityEngine.Object objectReference = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)) as UnityEngine.Object;
                            FI.SetValue(script, objectReference);
                        }
                    }
                    else if (FT.Equals("Color32"))
                    {
                        string[] values = value.ToString().Split(',');
                        byte X = (byte)(float.Parse(values[0]) * 255);
                        byte Y = (byte)(float.Parse(values[1]) * 255);
                        byte Z = (byte)(float.Parse(values[2]) * 255);
                        byte W = (byte)(float.Parse(values[3]) * 255);

                        Color32 valueCS = new Color32(X, Y, Z, W);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("Color"))
                    {
                        string[] values = value.ToString().Split(',');
                        float X = float.Parse(values[0]);
                        float Y = float.Parse(values[1]);
                        float Z = float.Parse(values[2]);
                        float W = float.Parse(values[3]);

                        Color valueCS = new Color(X, Y, Z, W);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("LayerMask"))
                    {
                        LayerMask layermask = 1 << int.Parse(value);
                        FI.SetValue(script, layermask);
                    }
                    else if (FT.Equals("Boolean"))
                        FI.SetValue(script, bool.Parse(value));
                    else if (FT.Equals("Single"))
                        FI.SetValue(script, float.Parse(value));
                    else if (FT.Equals("Int32"))
                        FI.SetValue(script, int.Parse(value));
                    else if (FT.Equals("String"))
                        FI.SetValue(script, value);
                    else if (FT.Equals("AnimationCurve"))
                    {
                        int length = node.ChildNodes.Count;
                        Keyframe[] keys = new Keyframe[length / 4];
                        int index = 0;

                        for (int i = 0; i < length; i += 4)
                        {
                            keys[index].value = float.Parse(node.ChildNodes[i + 0].InnerText);
                            keys[index].time = float.Parse(node.ChildNodes[i + 1].InnerText);
                            keys[index].inTangent = float.Parse(node.ChildNodes[i + 2].InnerText);
                            keys[index].outTangent = float.Parse(node.ChildNodes[i + 3].InnerText);

                            index++;
                        }

                        AnimationCurve animationCurve = new AnimationCurve(keys);
                        FI.SetValue(script, animationCurve);
                    }
                    else if (FT.Equals("Bounds"))
                    {
                        string[] values = value.ToString().Split(',');
                        float X = float.Parse(values[0]);
                        float Y = float.Parse(values[1]);
                        float Z = float.Parse(values[2]);
                        float A = float.Parse(values[3]);
                        float B = float.Parse(values[4]);
                        float C = float.Parse(values[5]);

                        Vector3 center = new Vector3(X, Y, Z);
                        Vector3 size = new Vector3(A, B, C);
                        Bounds valueCS = new Bounds(center, size * 2f);
                        FI.SetValue(script, valueCS);
                    } 
                    else if (FT.Equals("BoundsInt"))
                    {
                        string[] values = value.ToString().Split(',');
                        int X = int.Parse(values[0]);
                        int Y = int.Parse(values[1]);
                        int Z = int.Parse(values[2]);
                        int A = int.Parse(values[3]);
                        int B = int.Parse(values[4]);
                        int C = int.Parse(values[5]);

                        Vector3Int position = new Vector3Int(X, Y, Z);
                        Vector3Int size = new Vector3Int(A, B, C);
                        BoundsInt valueCS = new BoundsInt(position, size);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("enumList"))
                        FI.SetValue(script, int.Parse(value));
                    else if (FT.Equals("Quaternion"))
                    {
                        string[] values = value.ToString().Split(',');
                        float X = float.Parse(values[0]);
                        float Y = float.Parse(values[1]);
                        float Z = float.Parse(values[2]);
                        float W = float.Parse(values[3]);

                        Quaternion valueCS = new Quaternion(X, Y, Z, W);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("Rect"))
                    {
                        string[] values = value.ToString().Split(',');
                        float X = float.Parse(values[0]);
                        float Y = float.Parse(values[1]);
                        float Z = float.Parse(values[2]);
                        float W = float.Parse(values[3]);

                        Rect valueCS = new Rect(X, Y, Z, W);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("RectInt"))
                    {
                        string[] values = value.ToString().Split(',');
                        int X = int.Parse(values[0]);
                        int Y = int.Parse(values[1]);
                        int Z = int.Parse(values[2]);
                        int W = int.Parse(values[3]);

                        RectInt valueCS = new RectInt(X, Y, Z, W);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("Vector2"))
                    {
                        string[] values = value.ToString().Split(',');
                        float X = float.Parse(values[0]);
                        float Y = float.Parse(values[1]);

                        Vector2 valueCS = new Vector2(X, Y);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("Vector2Int"))
                    {
                        string[] values = value.ToString().Split(',');
                        int X = int.Parse(values[0]);
                        int Y = int.Parse(values[1]);

                        Vector2Int valueCS = new Vector2Int(X, Y);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("Vector3"))
                    {
                        string[] values = value.ToString().Split(',');
                        float X = float.Parse(values[0]);
                        float Y = float.Parse(values[1]);
                        float Z = float.Parse(values[2]);

                        Vector3 valueCS = new Vector3(X, Y, Z);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("Vector3Int"))
                    {
                        string[] values = value.ToString().Split(',');
                        int X = int.Parse(values[0]);
                        int Y = int.Parse(values[1]);
                        int Z = int.Parse(values[2]);

                        Vector3Int valueCS = new Vector3Int(X, Y, Z);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("Vector4"))
                    {
                        string[] values = value.ToString().Split(',');
                        float X = float.Parse(values[0]);
                        float Y = float.Parse(values[1]);
                        float Z = float.Parse(values[2]);
                        float W = float.Parse(values[3]);

                        Vector4 valueCS = new Vector4(X, Y, Z , W);
                        FI.SetValue(script, valueCS);
                    }
                    else if (FT.Equals("ArraySize"))
                    {
                        // TODO
                    }
                    else if (FT.Equals("Character"))
                    {
                        // TODO
                    }
                    else if (FT.Equals("ExposedReference"))
                    {
                        // TODO
                    }
                    else if (FT.Equals("FixedBufferSize"))
                    {
                        // TODO
                    }
                    else if (FT.Equals("Gradient"))
                    {
                        // TODO
                    }
                    else
                    {
                        string name = node.ChildNodes[0].InnerText;
                        string type = node.ChildNodes[1].InnerText;
                        string path = node.ChildNodes[2].InnerText;
                        
                        if (path.EndsWith(".unity"))
                        {
                            Scene scene = SceneManager.GetSceneByPath(path);
                            List<GameObject> sceneObjects = new List<GameObject>();
                            scene.GetRootGameObjects(sceneObjects);
                        
                            foreach (GameObject g in sceneObjects)
                            {
                                if (g.name.Equals(name))
                                {
                                    Component component = g.GetComponent(FI.FieldType.Name);
                                    FI.SetValue(script, component);
                                    break;
                                }
                        
                                foreach (Transform tr in g.transform)
                                {
                                    if (tr.name.Equals(name))
                                    {
                                        Component component = tr.GetComponent(FI.FieldType.Name);
                                        FI.SetValue(script, component);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            UnityEngine.Object objectReference = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)) as UnityEngine.Object;
                            FI.SetValue(script, objectReference);
                        }
                    }
                }
            }
        }
    }
}
#endif

