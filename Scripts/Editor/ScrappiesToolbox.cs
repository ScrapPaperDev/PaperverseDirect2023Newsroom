using UnityEditor;
using UnityEngine;

public class ScrappiesToolbox : SettingsProvider
{

    private static bool showOpenProp = true;

    public bool welcomeScreenShown;

    public static bool ShowOpenPropertyWindowButton
    {
        get => showOpenProp;
        set
        {
            showOpenProp = value;
            EditorPrefs.SetBool("showPropWinButton", value);
        }
    }

    public ScrappiesToolbox(string path, SettingsScope scopes) : base(path, scopes)
    {
        if (!EditorPrefs.GetBool("firstUse"))
        {

        }
        showOpenProp = EditorPrefs.GetBool("showPropWinButton");
    }

    public override void OnGUI(string searchContext)
    {
        base.OnGUI(searchContext);

        GUILayout.Space(16f);

        bool enabled = ShowOpenPropertyWindowButton;
        bool val = EditorGUILayout.Toggle("Show Open In Property Window Drawers", enabled);

        if (enabled != val)
            ShowOpenPropertyWindowButton = val;

        if (GUILayout.Button(new GUIContent("Reset to Default", "Clears all saved settings related to Scrappie's Toolbox settings.")))
        {
            ShowOpenPropertyWindowButton = true;
            EditorPrefs.SetBool(nameof(welcomeScreenShown), false);
        }

    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        return new ScrappiesToolbox("Scrappie's Toolbox/Property Drawer Ops", SettingsScope.User);
    }
}
