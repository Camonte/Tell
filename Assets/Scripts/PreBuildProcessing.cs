 #if UNITY_EDITOR
 using UnityEditor;
 using UnityEditor.Build;
 using UnityEditor.Build.Reporting;
 using UnityEngine;
 
 // Necessary to be able to build the project on macOS
 // Change of path needed towards your python folder
 // Can ignore this file when building on Windows

 public class PreBuildProcessing : IPreprocessBuildWithReport
 {
     public int callbackOrder => 1;
     public void OnPreprocessBuild(BuildReport report)
     {
         System.Environment.SetEnvironmentVariable("EMSDK_PYTHON", "/Users/camillemontemagni/.pyenv/versions/2.7.18/bin/python");
     }
 }
 #endif