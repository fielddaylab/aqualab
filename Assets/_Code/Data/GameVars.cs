using System;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Variants;
using UnityEngine;

namespace Aqua
{
    static public class GameVars
    {
        // temporary
        static public readonly TableKeyPair CameraRegion = TableKeyPair.Parse("temp:camera.region");
        static public readonly TableKeyPair InteractObject = TableKeyPair.Parse("temp:interact.object");

        // session
        static public readonly TableKeyPair DiveSite = TableKeyPair.Parse("session:nav.diveSite");
        static public readonly TableKeyPair ShipRoom = TableKeyPair.Parse("session:nav.shipRoom");

        // jobs
        static public readonly TableKeyPair CurrentJob = TableKeyPair.Parse("player:currentJob");
        static public readonly TableKeyPair CurrentStation = TableKeyPair.Parse("player:currentStation");

        // global
        static public readonly TableKeyPair Weekday = TableKeyPair.Parse("date:weekday");
        static public readonly TableKeyPair PlayerGender = TableKeyPair.Parse("player:gender");
        static public readonly TableKeyPair SceneName = TableKeyPair.Parse("scene:name");
        static public readonly TableKeyPair ActNumber = TableKeyPair.Parse("global:actNumber");
    }

    static public class GameConsts
    {
        static public readonly StringHash32 CashId = "Cash";
        static public readonly StringHash32 GearsId = "Gear";

        static public readonly StringHash32 Target_Kevin = "kevin";
        static public readonly StringHash32 Target_Player = "player";

        public const int GameSceneIndexStart = 2;
    }
}