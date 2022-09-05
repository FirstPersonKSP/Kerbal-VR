using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
    public interface IPinchInteractable
    {

        GameObject GameObject { get; }

        void OnPinch(Hand hand, SteamVR_Input_Sources source);
        void OnHold(Hand hand, SteamVR_Input_Sources source);
        void OnRelease(Hand hand, SteamVR_Input_Sources source);
    }
}
