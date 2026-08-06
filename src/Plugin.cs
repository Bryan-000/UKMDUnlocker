namespace UKMDUnlocker;

using System.Collections.Generic;
using System.Linq;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using TMPro;
using UKMDUnlocker.Compat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[BepInPlugin(Information.GUID, Information.Name, Information.Version)]
[BepInDependency(ALLPlugin.PLUGIN_GUID, DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static class Information
    {
        public const string GUID = "Bryan_-000-.UKMDUnlocker";
        public const string Name = "UKMDUnlocker";
        public const string Version = "0.4.0";
    }

    /// <summary> The current instance of the plugin, accessable by all parts of the code </summary>
    public static Plugin Instance;

    /// <summary> The "interactable" components of the difficulty select menu (mostly just difficulty buttons and infos) </summary>
    public Transform Interactables { private set; get; }

    /// <summary> Easy and convenient variable for accessing the Canvas </summary>
    public Transform Canvas { private set; get; }

    /// <summary> Public version of the Logger so that the rest of the mod can acess it </summary>
    public static ManualLogSource Log => Instance.Logger;

    /// <summary> We need to have an instance of this in order to do patches </summary>
    public static readonly Harmony Harmony = new(Information.GUID);

    public void Awake()
    {
        Instance = this;
        SceneManager.activeSceneChanged += (_, _) =>
        {
            LeaderboardProperties.Difficulties[5] = (SceneHelper.CurrentScene == "Main Menu") ? "UKMD" : "Ultrakill Must Die";

            if (SceneHelper.CurrentScene == "Main Menu")
            {
                Canvas = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(obj => obj.name == "Canvas").transform;

                // difficulty buttons and difficulty infos
                Interactables = Canvas.Find("Difficulty Select (1)/Interactables");

                // create the new UKMD button and Info
                AddInfo();
                AddButton();
            }
        };

        AngryFix.Init();

        Harmony.PatchAll(typeof(Patches));
        Log.LogInfo($"Loaded {Information.Name}");
    }

    /// <summary> Add the UKMD button and info to the difficulty select menu </summary>
    public void AddButton()
    {
        KeyValuePair<string, GameObject> FindElem(string name) =>
            new(name, Interactables.Find(name).gameObject);

        Log.LogInfo("Adding UKMD Button...");

        Dictionary<string, GameObject> buttons = new([
            FindElem("Casual Easy"), // Harmless
            FindElem("Casual Hard"), // Lenient
            FindElem("Standard"),
            FindElem("Violent"),
            FindElem("Brutal"),
            FindElem("V1 Must Die"), // Real UKMD button
        ]);

        Dictionary<string, GameObject> infos = new([
            FindElem("Harmless Info"),
            FindElem("Lenient Info"),
            FindElem("Standard Info"),
            FindElem("Violent Info"),
            FindElem("Brutal Info"),
        ]);

        // clone the brutal button
        GameObject UKMDButton = Instantiate(buttons.GetValueSafe("Brutal"), Interactables);
        UKMDButton.GetComponent<DifficultySelectButton>().difficulty = 5;
        UKMDButton.transform.Find("Name").GetComponent<TMP_Text>().text = "ULTRAKILL MUST DIE";
        UKMDButton.transform.position = buttons.GetValueSafe("V1 Must Die").transform.position;
        UKMDButton.name = "UKMD Button";

        // disable the original ukmd button so that it doesn't get in the way
        buttons.GetValueSafe("V1 Must Die").gameObject.SetActive(false);

        // the event triggers that the button uses to show/hide its description
        var ukmdTrigger = UKMDButton.GetComponent<EventTrigger>();

        // remove old triggers because those use Brutal's description instead of UKMD's description
        ukmdTrigger.triggers.Clear();

        // If the info hasn't been created yet, try to create it
        if (!UKMDInfo) AddInfo();

        // hide ukmd info if any of the other buttons are hovered over
        foreach (var button in buttons.Values)
        {
            var trigger = button.GetComponent<EventTrigger>();
            if (!trigger) continue;

            trigger.triggers.Add(
                Tools.CreateTriggerEntry(EventTriggerType.PointerEnter, _ => UKMDInfo.SetActive(false))
            );
        }

        // add new triggers to ukmd button
        ukmdTrigger.triggers.AddRange([
            Tools.CreateTriggerEntry(EventTriggerType.PointerEnter, _ =>
            {
                UKMDInfo.SetActive(true);
                foreach (var info in infos.Values) info.SetActive(false);
            }),

            Tools.CreateTriggerEntry(EventTriggerType.PointerExit,  _ => UKMDInfo.SetActive(false)),
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, eventData =>
            {
                Tools.Difficulty = GameDifficulty.UKMD;
                UKMDInfo.SetActive(false);
            }),
        ]);

        if (AngryFix.hasAngry)
        {
            ukmdTrigger.triggers.Add(
                Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ =>
                {
                    AngryFix.angryDifficultyField.difficultyListValueIndex = 5;
                    AngryFix.angryDifficultyField.difficultyListValue = "UKMD";
                    Log.LogInfo("Setting Angry difficulty to UKMD");
                })
            );
        }

        // add ukmd button to the button activation sequence
        var activationSequence = Interactables.GetComponent<ObjectActivateInSequence>();
        activationSequence.objectsToActivate[14 /* index of real ukmd button */] = UKMDButton;

        Log.LogInfo("Added UKMD Button");
    }

    public GameObject UKMDInfo;

    public void AddInfo()
    {
        Log.LogInfo("Adding UKMD Info...");

        UKMDInfo = Instantiate(Interactables.Find("Brutal Info").gameObject, Interactables);
        UKMDInfo.name = "UKMD Info";

        var ukmdTitle = UKMDInfo.transform.Find("Title (1)").GetComponent<TMP_Text>();

        // set the font size to 29 because if it's the default it'll span multiple lines
        ukmdTitle.fontSize = 29;
        ukmdTitle.text = $"--ULTRAKILL MUST DIE--";

        // set the description of UKMD
        UKMDInfo.transform.Find("Text").GetComponent<TMP_Text>().text =
            """
            <color=yellow>The unfinished version of UKMD in the game's files.</color>

            <color=white>Fast and extremely aggresive enemies with very high damage.

            Quick thinking and a full arsenal are expected. Slip-ups are often fatal.</color>

            <b>Recommended for players who have achieved near mastery over the game and are looking for a fitting challenge.</b>
            """;

        Log.LogInfo("Added UKMD Info");
    }
}