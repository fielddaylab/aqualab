using System;
using System.Text;
using BeauUtil;
using BeauUtil.Services;
using UnityEngine;

namespace Aqua
{
    static public class Loc
    {
        [ServiceReference] static private LocService Service;

        static private void Process(LocService inLoc, ref object ioObject)
        {
            if (ioObject is TextId)
                ioObject = inLoc.Localize((TextId) ioObject, true);
            else if (ioObject is StringHash32)
                ioObject = inLoc.Localize((TextId) (StringHash32) ioObject, true);
        }

        static public string Find(TextId inText)
        {
            return Service.Localize(inText);
        }

        static public string Format(TextId inText, object inArg0)
        {
            var locService = Service;
            string format = locService.Localize(inText);
            Process(locService, ref inArg0);
            return string.Format(format, inArg0);
        }

        static public string Format(TextId inText, object inArg0, object inArg1)
        {
            var locService = Service;
            string format = locService.Localize(inText);
            Process(locService, ref inArg0);
            Process(locService, ref inArg1);
            return string.Format(format, inArg0, inArg1);
        }

        static public string Format(TextId inText, object inArg0, object inArg1, object inArg2)
        {
            var locService = Service;
            string format = locService.Localize(inText);
            Process(locService, ref inArg0);
            Process(locService, ref inArg1);
            Process(locService, ref inArg2);
            return string.Format(format, inArg0, inArg1, inArg2);
        }

        static public string Format(TextId inText, params object[] inArgs)
        {
            var locService = Service;
            string format = locService.Localize(inText);
            if (inArgs != null)
            {
                for(int i = 0; i < inArgs.Length; i++)
                    Process(locService, ref inArgs[0]);
            }
            return string.Format(format, inArgs);
        }
    }
}