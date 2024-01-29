using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

public class WelcomeScreen : ScriptableObject
{

    [InitializeOnLoadMethod]
    private static void Awaken()
    {
        if (EditorPrefs.GetBool(nameof(ScrappiesToolbox.welcomeScreenShown)))
            return;


        ScrappiesToolbox.ShowOpenPropertyWindowButton = EditorUtility.DisplayDialog("Welcome!", "Do you wanna enables Scrappie's Toolbox? It extends the editor with convenient options. This can be changed anytime from Edit>Preferences", "Yes", "No");

        EditorPrefs.SetBool(nameof(ScrappiesToolbox.welcomeScreenShown), true);


        ScrappiesToolbox.ShowOpenPropertyWindowButton = EditorUtility.DisplayDialog("Welcome!", "To get started select from the toolbar NewsRoom>Help", "Ok!");
    }

}


public class HelpScreen : EditorWindow
{
    [MenuItem("NewsRoom/Help")]
    public static void Open()
    {
        var win = GetWindow<HelpScreen>();
    }

    private bool showCredits;
    private bool showGuide;

    TimelineAsset[] timelineAssets;

    private void OnEnable()
    {
        timelineAssets = EditorUtils.LoadAllOfType<TimelineAsset>().ToArray();

    }
    private void OnGUI()
    {
        foreach (var item in timelineAssets)
        {
            if (GUILayout.Button(new GUIContent($"Edit: {item.name}", "Opens the timeline window with the current project.")))
            {
                Assembly assemblyy = Assembly.Load("Unity.Timeline.Editor");

                var timelineWin = assemblyy.GetType("UnityEditor.Timeline.TimelineWindow");

                var openMethod = timelineWin.GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.Public);

                openMethod.Invoke(null, null);

                var director = FindObjectOfType<UnityEngine.Playables.PlayableDirector>();

                director.playableAsset = item;

                Selection.activeGameObject = director.gameObject;
            }
        }

        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox($"Coders: If you edit or add code, be sure to add your credentials to the 'Author' attributes!", MessageType.Info);
        EditorGUILayout.Separator();

        showGuide = EditorGUILayout.Foldout(showGuide, "Guide", true);

        if (showGuide)
        {
            EditorGUILayout.HelpBox($"Coming soon...For now ask Scrappie for help", MessageType.Info);
        }

        showCredits = EditorGUILayout.Foldout(showCredits, "Credits", true);

        if (showCredits)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Pepper Sprites: some fool");

            EditorGUI.indentLevel--;
        }
    }
}