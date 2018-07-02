﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fishman
{
    class Action
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static Action Fish = new Action() { Key = WinApi.VirtualKeys.N1, CastTime = 22 * 1000, Description = "Fishing", Trigger = Event.Fish, GCD = 250 };

        public enum Event
        {
            /// <summary>Default value. Events with such trigger type will be ignored</summary>
            None,
            /// <summary>Fishing action as it is. Should be only one action of such type per <see cref="Preset"/></summary>
            Fish,
            /// <summary>Call action once before first fishing iteration. For example: equip rod</summary>
            Once,
            /// <summary>Call action before fishing iteration</summary>
            PreFish,
            /// <summary>Call action after fishing iteration. For example: throw rare fish into water macros</summary>
            PostFish,
            /// <summary>Call action once in <see cref="Action.Interval"/>. For example: update lure</summary>
            Interval
        }

        /// <summary>Description or name of action. Unnecessary field</summary>
        public string Description = "";
        /// <summary>Use this field for manual mapping action button. Unnecessary field. Default: one of N2-N9 numeric buttons</summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public WinApi.VirtualKeys Key = WinApi.VirtualKeys.None;
        /// <summary>Trigger type that defines how action can be called. Necessary field. Default: <see cref="Event.None"/></summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Event Trigger = Event.None;
        /// <summary>Global spell cooldown in milliseconds</summary>
        public int GCD = 1500;
        /// <summary>Cast time in milliseconds. 0 if instant. Don't use it for GCD. Unnecessary field. Default: 0</summary>
        public int CastTime = 0;
        /// <summary>Event call interval in seconds. Used only with <see cref="Event.Interval"/>. Necessary field</summary>
        public int Interval = 0;
        /// <summary>Date of last event invoke. Used in <see cref="Event.Interval"/> event type</summary>
        [JsonIgnore]
        DateTime LastInvoke = DateTime.MinValue;


        /// <summary>
        /// Do described action
        /// </summary>
        /// <param name="hWnd">Main WoW Window Handle</param>
        public void Invoke(IntPtr hWnd)
        {
            logger.Info("[{0}] {1}", Trigger, ToString());

            switch (Trigger)
            {
                case Event.Once:
                case Event.PreFish:
                case Event.PostFish:
                case Event.Fish:
                    PressKey(hWnd);
                    Sleep();
                    break;

                case Event.Interval:
                    // if action already should be called or if it should be called while next fishing action - call it now
                    if (DateTime.Now.AddMilliseconds(Fish.CastTime) > LastInvoke.AddSeconds(Interval))
                    {
                        logger.Info("Calling event");
                        PressKey(hWnd);
                        Sleep();
                    }
                    break;

                case Event.None:
                default:
                    throw new Exception("Invalid trigger type to invoke current action");
            }

            LastInvoke = DateTime.Now;
        }

        private void Sleep()
        {
            if (Trigger != Event.Fish)
            {
                int sleep = GCD + CastTime;
                logger.Info("Waiting for {0} milliseconds", sleep);
                Thread.Sleep(sleep);
            }
        }

        private void PressKey(IntPtr hWnd)
        {
            logger.Debug("Pressing key \"{0}\"", Key);
            DeviceManager.PressKey(hWnd, Key);
        }

        public override string ToString()
        {
            return string.Format("Description: \"{0}\"; GCD: {1}; Trigger: {2}; Key: \"{3}\"; Cast time: {4}; Interval: {5}", Description, GCD / 1000f, Trigger, Key, CastTime / 1000f, Interval);
        }
    }
}