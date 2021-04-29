using System;
using BeauRoutine;
using BeauUtil;
using UnityEngine;

namespace Aqua
{
    static public class GameEvents
    {
        static public readonly StringHash32 ProfileLoaded = "profile:loaded";
        static public readonly StringHash32 SceneWillUnload = "scene:willUnload";
        static public readonly StringHash32 SceneLoaded = "scene:loaded";

        static public readonly StringHash32 CutsceneStart = "cutscene:start";
        static public readonly StringHash32 CutsceneEnd = "cutscene:end";
        static public readonly StringHash32 CutsceneSkip = "cutscene:skip";

        static public readonly StringHash32 KevinChatterStart = "kevin:chatter-start";
        static public readonly StringHash32 KevinChatterEnd = "kevin:chatter-end";

        static public readonly StringHash32 BestiaryUpdated = "bestiary:updated";

        static public readonly StringHash32 PortableOpened = "portable:opened";
        static public readonly StringHash32 PortableClosed = "portable:closed";

        static public readonly StringHash32 JobStarted = "job:started";
        static public readonly StringHash32 JobCompleted = "job:completed";
        static public readonly StringHash32 JobSwitched = "job:switched";

        static public readonly StringHash32 ActChanged = "act:changed";

        static public readonly StringHash32 ReceiveFact = "receive-fact";

        static public readonly StringHash32 BeginExperiment = "experiment:begin";
        static public readonly StringHash32 BeginDive = "dive:begin";
        static public readonly StringHash32 BeginArgument = "argument:begin";
        static public readonly StringHash32 BeginModel = "model:begin";
        static public readonly StringHash32 BeginSimulation = "simulation:begin";

        static public readonly StringHash32 AskForHelp = "ask-for-help";
        static public readonly StringHash32 TalkWithGuide = "talk-with-guide";

        static public readonly StringHash32 SimulationSyncAchieved = "simulation-sync-achieved";

        static public readonly StringHash32 GuideScriptTriggered = "guide-script-triggered";
    }
}