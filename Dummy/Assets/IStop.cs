using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStop
{
	void Arrive(Train train);
	bool IsPresent(Train train);
	bool TryLeave(Train train);

	float MaxSpeed(Train train);
}