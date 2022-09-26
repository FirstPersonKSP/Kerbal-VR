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

		void OnPinch(Hand hand);
		void OnHold(Hand hand);
		void OnRelease(Hand hand);
	}
}
