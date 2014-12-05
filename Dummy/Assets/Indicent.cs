using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SolutionList {
	public static ISolution Backup = new Solution ("Drive tram backwards a little bit", TimeSpan.FromSeconds (5), 0.50f);
	public static ISolution Shout = new Solution ("Shout at passenger to be quiet", TimeSpan.FromSeconds (5), 0.10f);
	public static ISolution Police = new Solution ("Call police to arrest passenger", TimeSpan.FromSeconds (30), 0.95f);
	public static ISolution Magic = new Solution ("Call Magician to remove obstacle", TimeSpan.FromSeconds (30), 0.30f);
}

public class TrainCollisionIndicent : AbstractIncident, IIncident {
	Train self;
	Train other;

	public TrainCollisionIndicent (Train self, Train other)
	{
		this.other = other;
		this.self = self;
	}

	#region IIncident implementation
	public override IEnumerable<ISolution> PossibleActions ()
	{
		// Depending on this.self and this.other we can also change the possible actions
		return new [] {
			SolutionList.Backup
		};
	}
	#endregion
}

public class ObstacleIncident : AbstractIncident, IIncident {
	Obstacle obstacle;

	public ObstacleIncident (Obstacle obstacle)
	{
		this.obstacle = obstacle;
		
	}

	#region IIncident implementation

	public override IEnumerable<ISolution> PossibleActions ()
	{
		// Depending on this.obstacle we can also change the possible actions
		return new [] {
			SolutionList.Magic, SolutionList.Backup
		};
	}

	#endregion
}

