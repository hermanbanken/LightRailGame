using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


// Given 1s real time is 60s in game

public class SolutionIncidents {
	public static ISolution Shout = new Solution ("Shout at passenger to be quiet", TimeSpan.FromSeconds (5), 0.80f);
	public static ISolution Police = new Solution ("Call police to arrest troublemakers", TimeSpan.FromSeconds (10), 0.90f); //stops the tram and it needs to be restarted manually
	public static ISolution Ambulance = new Solution ("Call for an ambulance", TimeSpan.FromSeconds (10), 0.90f); //stops the tram and it needs to be restarted manually
	public static ISolution Calm = new Solution ("Try to calm the passengers down", TimeSpan.FromSeconds (5), 0.50f);
	public static ISolution Ventilate = new Solution ("Ventilate the tram thoroughly", TimeSpan.FromSeconds (5), 0.80f);
	public static ISolution DeliverBaby	= new Solution ("Help in delivering the baby until professionals come", TimeSpan.FromSeconds (5), 0.80f);
}

public class SolutionBlockages {
	public static ISolution Backup = new Solution ("Drive tram backwards a little bit", TimeSpan.FromSeconds (10), 0.50f);
	public static ISolution PushAside = new Solution ("Ask the tram driver to push the car aside", TimeSpan.FromSeconds (10), 0.25f);
	public static ISolution Horn = new Solution ("Ask the tram driver to use the horm repeatedly", TimeSpan.FromSeconds (3), 0.10f);
	public static ISolution Tow = new Solution ("Call for a towing service", TimeSpan.FromSeconds (30), 1.0f);
	public static ISolution Maintenance = new Solution ("Call maintenance crew to deal with the problem", TimeSpan.FromSeconds (45), 0.9f);	
	public static ISolution SwitchManually = new Solution ("Ask the tram driver to push the switch manually", TimeSpan.FromSeconds (15), 0.75f);
	public static ISolution Crane = new Solution ("Call for a crane service", TimeSpan.FromSeconds (90), 0.90f);
	public static ISolution EmergencyServices = new Solution ("Call for emergency services", TimeSpan.FromSeconds (120), 1.0f);
	public static ISolution ContinueAnyway = new Solution ("Try to continue despite collision", TimeSpan.FromSeconds (20), 0.10f); //Only to be used if the collision is between tram and not tram

}	
// Given two types of collision: with another tram or something else (like a car)
public class TrainCollisionBlockage : AbstractIncident, IIncident {
	Train self;
	Train other;

	public TrainCollisionBlockage (Train self, Train other)
	{
		this.other = other;
		this.self = self;
	}
	// We still need to implement collision with an object that is not a tram
	#region IIncident implementation
	public override IEnumerable<ISolution> PossibleActions ()
	{
		// Depending on this.self and this.other we can also change the possible actions
		if (other == null) {
			return new [] {
				SolutionBlockages.EmergencyServices, SolutionBlockages.ContinueAnyway, SolutionBlockages.Backup
			};
		}
		// if another
		else {
			return new [] {
				SolutionBlockages.EmergencyServices, SolutionBlockages.ContinueAnyway
			};
		}
	}
	#endregion
}

public class ObstacleBlockage : AbstractIncident, IIncident {
	Obstacle obstacle;


	public ObstacleBlockage (Obstacle obstacle)
	{
		this.obstacle = obstacle;
		
	}

	#region IIncident implementation
	//Implementing four types of blockages, each with different set of correct actions: Car on tracks, Tram defect, Switch defect, Derailment
	public override IEnumerable<ISolution> PossibleActions ()
	{
		// Depending on this.obstacle we can also change the possible actions
			if (this.obstacle.type == ObstacleType.Car) {
					return new [] {
						SolutionBlockages.Tow, SolutionBlockages.Horn, SolutionBlockages.PushAside
					};
			} 
			else if (this.obstacle.type == ObstacleType.Defect) {
					return new [] {
						SolutionBlockages.Maintenance, SolutionBlockages.ContinueAnyway
					};
			} 
			else if (this.obstacle.type == ObstacleType.SwitchDefect) { //one needs to make sure it can happen only at a node with more than one possible direction
					return new [] {
						SolutionBlockages.Maintenance, SolutionBlockages.SwitchManually
					};	
			}
			else {
					return new [] {
						SolutionBlockages.Crane, SolutionBlockages.EmergencyServices,
					};
			}
	}

	#endregion
}

// For tramcar incident we implement four different types: Drunken passenger, angry mob, women in labour and stench on board
public class TramCarIncident : AbstractIncident, IIncident {
	Train self;
	Obstacle obstacle;
	
	
	public TramCarIncident (Obstacle obstacle)
	{
		this.obstacle = obstacle;
		
	}	
	
	#region IIncident implementation
	public override IEnumerable<ISolution> PossibleActions ()
	{
		if (this.obstacle.type == ObstacleType.DrunkenPassenger) {
			return new [] {
				SolutionIncidents.Shout, SolutionIncidents.Police,
			};
		}
		else if (this.obstacle.type == ObstacleType.AngryMob) {
			return new [] {
				SolutionIncidents.Calm, SolutionIncidents.Police,
			};
		}
		else if (this.obstacle.type == ObstacleType.WomenInLabour) {
			return new [] {
				SolutionIncidents.Ambulance, SolutionIncidents.DeliverBaby,
			};
		}
		else if (this.obstacle.type == ObstacleType.StenchOnBoard) {
			return new [] {
				SolutionIncidents.Ventilate, SolutionIncidents.Calm,
			};
		}

		return new ISolution[] {};
	}
	#endregion
}

		
