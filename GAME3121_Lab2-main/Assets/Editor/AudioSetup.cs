using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time setup for Assignment 1 (Part 1) audio. Runs automatically the first
// time the project is opened, then writes a marker file so it won't run again.
// You can re-run it any time from the menu: Tools > Setup Assignment Audio.
[InitializeOnLoad]
public static class AudioSetup
{
    const string Marker = "ProjectSettings/AudioSetupDone.txt";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";

    static AudioSetup()
    {
        if (!File.Exists(Marker))
            EditorApplication.delayCall += Run;
    }

    [MenuItem("Tools/Setup Assignment Audio")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        AssetDatabase.Refresh();

        AudioClip wall = Clip("WallHit.wav"), win = Clip("Win.wav"),
                  death = Clip("Death.wav"), music = Clip("BackgroundMusic.mp3");
        if (!wall || !win || !death || !music)
        {
            Debug.LogError("[AudioSetup] Audio clips not found in Assets/Audio yet. Run Tools > Setup Assignment Audio again.");
            return;
        }

        EnsureTag("Wall");

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(ScenePath);
        }

        // 1. Tag the boundary walls
        foreach (string n in new[] { "EastWall", "WestWall", "SouthWall", "NorthWall" })
        {
            GameObject w = Find(scene, n);
            if (w) w.tag = "Wall"; else Debug.LogWarning("[AudioSetup] Wall not found: " + n);
        }

        // SoundManager (plays SFX; never destroyed)
        GameObject sm = Find(scene, "SoundManager");
        if (!sm) sm = new GameObject("SoundManager");
        AudioSource sfx = GetOrAdd<AudioSource>(sm);
        sfx.playOnAwake = false; sfx.loop = false; sfx.spatialBlend = 0f;
        SoundManager mgr = GetOrAdd<SoundManager>(sm);
        mgr.sfxSource = sfx;
        EditorUtility.SetDirty(mgr);

        // 3. Background music
        GameObject bgm = Find(scene, "BackgroundMusic");
        if (!bgm) bgm = new GameObject("BackgroundMusic");
        AudioSource ms = GetOrAdd<AudioSource>(bgm);
        ms.clip = music; ms.loop = true; ms.playOnAwake = true; ms.volume = 0.35f; ms.spatialBlend = 0f;
        EditorUtility.SetDirty(ms);

        // 2 + 4. Player clips
        GameObject player = Find(scene, "Player");
        PlayerController pc = player ? player.GetComponent<PlayerController>() : null;
        if (pc)
        {
            pc.wallSound = wall; pc.winSound = win; pc.deathSound = death;
            EditorUtility.SetDirty(pc);
        }
        else Debug.LogError("[AudioSetup] Player with PlayerController not found.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        File.WriteAllText(Marker, "Audio setup done " + System.DateTime.Now);
        Debug.Log("[AudioSetup] Done: walls tagged, SoundManager + BackgroundMusic created, Player clips assigned. Scene saved.");
    }

    static AudioClip Clip(string file) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + file);

    static T GetOrAdd<T>(GameObject g) where T : Component
    {
        T c = g.GetComponent<T>();
        return c ? c : g.AddComponent<T>();
    }

    static GameObject Find(Scene s, string name)
    {
        foreach (GameObject root in s.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name.Trim() == name) return t.gameObject;
        return null;
    }

    static void EnsureTag(string tag)
    {
        SerializedObject tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tm.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tm.ApplyModifiedPropertiesWithoutUndo();
    }
}
