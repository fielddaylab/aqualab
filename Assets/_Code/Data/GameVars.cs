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

        // session
        static public readonly TableKeyPair DiveSite = TableKeyPair.Parse("session:nav.diveSite");
        static public readonly TableKeyPair ShipRoom = TableKeyPair.Parse("session:nav.shipRoom");
    }
}