using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// With voice editing moved to FrogForge, this is no longer nessecery

//[CustomPropertyDrawer(typeof(CharacterVoice))]
//public class CharacterVoiceDrawer : PropertyDrawer
//{
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return 20 * 4;
//    }
//    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
//    {
//        //var colors = property.FindPropertyRelative("Colors");
//        //for (int i = 0; i < 4; i++)
//        //{
//        //    Debug.Log(colors);
//        //    EditorGUI.ColorField(new Rect(i * 50, 0, 50, 30), colors.GetArrayElementAtIndex(i).colorValue);
//        //}


//        EditorGUI.BeginProperty(rect, label, property);
//        EditorGUI.indentLevel += 2;
//        SerializedProperty name = property.FindPropertyRelative("Name");
//        SerializedProperty voiceType = property.FindPropertyRelative("VoiceType");
//        SerializedProperty pitch = property.FindPropertyRelative("Pitch");
//        rect.width = 140;
//        rect.height = 20;
//        rect.x = 105;
//        Label(rect, "Name");
//        name.stringValue = EditorGUI.TextField(rect, name.stringValue);
//        rect.y += rect.height;
//        Label(rect, "Voice Type");
//        voiceType.enumValueIndex = (int)((VoiceType)EditorGUI.EnumPopup(rect, (VoiceType)voiceType.enumValueIndex));
//        rect.y += rect.height;
//        Label(rect, "Pitch");
//        pitch.floatValue = EditorGUI.FloatField(rect, pitch.floatValue);
//        rect.y += rect.height;
//        if (GUI.Button(rect, "Play"))
//        {
//            PortraitController portraitController = (PortraitController)property.serializedObject.targetObject;
//            portraitController.DebugSource.pitch = pitch.floatValue;
//            portraitController.DebugSource.PlayOneShot(portraitController.DebugVoices[voiceType.enumValueIndex]);
//            //soundController.EditorPlaySound(conversationPlayer.TextBeeps[voiceType.enumValueIndex], pitch.floatValue);
//        }
//        EditorGUI.indentLevel -= 2;

//        EditorGUI.EndProperty();


//        //var positionProperty = property.FindPropertyRelative("Position");
//        //var normalProperty = property.FindPropertyRelative("NormalRotation");

//        //EditorGUIUtility.wideMode = true;
//        //EditorGUIUtility.labelWidth = 70;
//        //rect.height /= 2;
//        //positionProperty.vector3Value = EditorGUI.Vector3Field(rect, "Position", positionProperty.vector3Value);
//        //rect.y += rect.height;
//        //normalProperty.vector3Value = EditorGUI.Vector3Field(rect, "Normal", normalProperty.vector3Value);
//    }

//    private void Label(Rect rect, string text)
//    {
//        rect.x -= 100;
//        EditorGUI.LabelField(rect, text);
//        rect.x += 100;
//    }
//}
