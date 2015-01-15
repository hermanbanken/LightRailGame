using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// Given 1s real time is 60s in game

public class SolutionIncidents {
	public static ISolution Shout = new Solution ("SHOUT at passenger to be quiet", TimeSpan.FromSeconds (3), 0.80f);
	public static ISolution Police = new Solution ("Call POLICE to arrest troublemakers", TimeSpan.FromSeconds (10), 0.90f); //stops the tram and it needs to be restarted manually
	public static ISolution Ambulance = new Solution ("Call for an AMBULANCE", TimeSpan.FromSeconds (10), 0.9f); //stops the tram and it needs to be restarted manually
	public static ISolution Calm = new Solution ("Try to CALM DOWN the passengers", TimeSpan.FromSeconds (5), 0.50f);
	public static ISolution Ventilate = new Solution ("VENTILATE the tram thoroughly", TimeSpan.FromSeconds (5), 0.80f);
	public static ISolution DeliverBaby	= new Solution ("HELP DELIVERING the baby until professionals come", TimeSpan.FromSeconds (5), 0.80f);
}

public class SolutionBlockages {
	public static ISolution PushAside = new Solution ("Ask the tram driver to PUSH the car aside", TimeSpan.FromSeconds (10), 0.25f);
	public static ISolution Horn = new Solution ("Ask the tram driver to use the HORN repeatedly", TimeSpan.FromSeconds (3), 0.20f);
	public static ISolution Tow = new Solution ("Call for a TOWING service", TimeSpan.FromSeconds (10), 1.0f);
	public static ISolution Maintenance = new Solution ("Call MAINTENANCE CREW to deal with the problem", TimeSpan.FromSeconds (45), 0.9f);	
	public static ISolution SwitchManually = new Solution ("Ask the tram driver to push the SWITCH MANUALLY", TimeSpan.FromSeconds (15), 0.75f);
	public static ISolution Crane = new Solution ("Call for a CRANE service", TimeSpan.FromSeconds (5), 1.0f);//(60), 0.90f);
	public static ISolution EmergencyServices = new Solution ("Call for EMERGENCY service", TimeSpan.FromSeconds (20), 1.0f);
	public static ISolution Backup = new Solution ("BACKUP the tram a little bit", TimeSpan.FromSeconds (11), 1f); //(10), 0.50f);
	public static ISolution ContinueAnyway = new Solution ("Try to CONTINUE despite collision", TimeSpan.FromSeconds (11), 1f);//-r (5), 0.25f); //Only to be used if the collision is between tram and not tram
}	

public class PowerUps {
	public static ISolution Magic = new PowerUp ("Call your friend GANDALF", TimeSpan.FromSeconds (5f), 1f, 1);
}

public class PowerUp : Solution, ISolution, IPowerUp {
	public PowerUp(string ProposalText, TimeSpan ResolveTime, float SuccessRatio, int initialAvailibility) : base(ProposalText, ResolveTime, SuccessRatio) {

		_powerUpCounter = initialAvailibility	;
	}
	private int _powerUpCounter;
	public void Use(){
		this._powerUpCounter --;
	}
	public bool IsAvailable(){
		return this._powerUpCounter > 0;
	}
	public void Receive(int count = 1){ // = 1 means default  = 1 in here
		_powerUpCounter += count;
	}
}

// Given two types of collision: with another tram or something else (like a car)
public class TrainCollisionBlockage : AbstractIncident, IIncident {
	Train self;
	Train other;
	Collision collision;
	bool ended = false;

	public TrainCollisionBlockage (Train self, Train other, Collision collision)
	{
		this.collision = collision;
		this.other = other;
		this.self = self;
	}
	// We still need to implement collision with an object that is not a tram
	#region IIncident implementation

	public override string Description ()
	{
		if (ended)
			return "The train is BROKEN because of a previous collision.";
		return "The train has COLLIDED with ANOTHER TRAIN.";
	}

	public override IEnumerable<ISolution> PossibleActions ()
	{
		if (ended)
			return new [] { SolutionBlockages.Maintenance };
		return new [] {
			// --- One complet list to check the order of menu items ---
			//
			//SolutionBlockages.PushAside,
			//SolutionBlockages.Horn,
			//SolutionBlockages.Tow,
			//SolutionBlockages.Maintenance,
			//SolutionBlockages.SwitchManually,
			SolutionBlockages.Crane, 
			SolutionBlockages.EmergencyServices, 
			SolutionBlockages.Backup,
			SolutionBlockages.ContinueAnyway, 
			//SolutionIncidents.Shout,
			//SolutionIncidents.Police,
			//SolutionIncidents.Ambulance,
			//SolutionIncidents.Calm,
			//SolutionIncidents.Ventilate,
			//SolutionIncidents.DeliverBaby,
			PowerUps.Magic 
			};
	}
	
	#endregion
	
	#region implemented abstract members of AbstractIncident

	public override float MaxSpeedOfSubject ()
	{
		// TODO maybe look to collision impact if the tram should still be able to drive
		if (!ended &&
			this.GetChosenSolution () != null && 
			//-r Suitability (this.GetChosenSolution ()) > 0 &&
			this.solutionChosenAt.Value + this.solution.ResolveTime.TotalSeconds - 10f < Time.time && // Starting five seconds before resolving
			this.solutionChosenAt.Value + this.solution.ResolveTime.TotalSeconds > Time.time // Ending @ resolving
		) {
			if(solution == SolutionBlockages.Backup || solution == SolutionBlockages.Crane)
				return -2f; //-r -0.5f;
			if(solution == SolutionBlockages.ContinueAnyway || solution == SolutionBlockages.Tow)
				return 2f; //-r 0.5f;
		}
		return 0f;
	}

	public override GameObject Subject ()
	{
		return self != null ? self.gameObject : null;
	}

	//TODO rogier - tweakit -- solution toevoegen
	public override int Suitability (ISolution solution)
	{
		if (solution == null)
			return 0;
		if (solution == SolutionBlockages.Maintenance)
			return 1;
		if (solution == SolutionBlockages.Tow || solution == SolutionBlockages.Crane)
			return 2;
		if (solution == SolutionBlockages.Backup || solution == SolutionBlockages.ContinueAnyway)
			return 1;
		if (solution as CollisionEndedSolution != null)
			return 1;
		return -1;
	}

	public void CollisionEnded(){
		ended = true;

		if (solution != null) {
			Debug.Log ("Collision with other tram ended, this incident has a solution");
		}
		else
			Debug.Log ("Collision with other tram ended, this incident does not have a solution!");

		if(solution == SolutionBlockages.Backup)
			self.desiredSpeed = 0;

		// Assigning a solution, so make sure to fix state
		var colsol = new CollisionEndedSolution (solution);
		if (solution == null) {
			SetChosenSolution (colsol);
		} else {
			solution = colsol;
		}
	}

	public class CollisionEndedSolution : Solution {
		public CollisionEndedSolution(ISolution initial) : base(initial != null ? initial.ProposalText : "The other tram moved away", initial != null ? initial.ResolveTime : TimeSpan.Zero, 1f) {}
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

	public override string Description ()
	{
		if(this.obstacle.type == ObstacleType.Car)
			return "There is a CAR ON THE TRACK.";
		// rogier // else if(this.obstacle.type == ObstacleType.Car)
		// rogier // 	return "The train has collided with a car.";
		throw new NotImplementedException ("Implement a description for the ObstacleType "+this.obstacle.type.ToString());
	}

	//Implementing four types of blockages, each with different set of correct actions: Car on tracks, Tram defect, Switch defect, Derailment
	public override IEnumerable<ISolution> PossibleActions ()
	{
		// Depending on this.obstacle we can also change the possible actions
			if (this.obstacle.type == ObstacleType.Car) {
					return new [] {
						SolutionBlockages.PushAside,
						SolutionBlockages.Horn, 
						SolutionBlockages.Tow,
						SolutionIncidents.Ventilate,
						PowerUps.Magic
					};
			} 
			else if (this.obstacle.type == ObstacleType.Defect) {
					return new [] {
						SolutionBlockages.Maintenance,
						SolutionBlockages.ContinueAnyway,
						PowerUps.Magic
					};
			} 
			else if (this.obstacle.type == ObstacleType.SwitchDefect) { //one needs to make sure it can happen only at a node with more than one possible direction
					return new [] {
						SolutionBlockages.Maintenance, 
						SolutionBlockages.SwitchManually,
						PowerUps.Magic
					};	
			}
			else {
					return new [] {
						SolutionBlockages.Crane, 
						SolutionBlockages.EmergencyServices,
						PowerUps.Magic
					};
			}
	}

	#endregion

	#region implemented abstract members of AbstractIncident

	public override float MaxSpeedOfSubject ()
	{
		return 0;
	}

	public override GameObject Subject ()
	{
		return obstacle == null ? null : obstacle.gameObject;
	}

	public override int Suitability (ISolution solution)
	{
		if (this.obstacle.type == ObstacleType.Car && solution == SolutionIncidents.Ventilate) return -1;
		return 1;
	}

	#endregion
}

// For tramcar incident we implement four different types: Drunken passenger, angry mob, women in labour and stench on board
public class TramCarIncident : AbstractIncident, IIncident {
	ObstacleType type;
	private GameObject train;
	
	public TramCarIncident (GameObject subject, ObstacleType type) : base()
	{
		this.train = subject;
		this.type = type;
	}	
	
	#region IIncident implementation
	public override string Description ()
	{
		if(this.type == ObstacleType.DrunkenPassenger)
			return "There seems to be some DRUNKEN PASSENGER onboard.";
		else if (this.type == ObstacleType.WomenInLabour)
			return "There IS a WOMAN IN LABOUR onboard.";
		else if (this.type == ObstacleType.StenchOnBoard)
			return "There is a STENCH onboard.";
		else if (this.type == ObstacleType.AngryMob)
			return "There is an ANGRY MOB onboard.";
		throw new NotImplementedException ("Implement a description for the ObstacleType "+this.type);
	}

	public override IEnumerable<ISolution> PossibleActions ()
	{
		if (this.type == ObstacleType.DrunkenPassenger) {
			return new [] {
				SolutionIncidents.Shout, 
				SolutionIncidents.Police,
			};
		}
		else if (this.type == ObstacleType.AngryMob) {
			return new [] {
				SolutionIncidents.Police,
				SolutionIncidents.Calm, 
			};
		}
		else if (this.type == ObstacleType.WomenInLabour) {
			return new [] {
				SolutionBlockages.Crane,
				SolutionIncidents.Ambulance, 
				SolutionIncidents.DeliverBaby, 
			};
		}
		else if (this.type == ObstacleType.StenchOnBoard) {
			return new [] {
				SolutionIncidents.Calm,
				SolutionIncidents.Ventilate, 
				};
		}

		return new ISolution[] {};
	}
	#endregion

	#region implemented abstract members of AbstractIncident

	public override float MaxSpeedOfSubject ()
	{
		// TODO do more smart things, now:
		// - if we are waiting on police/ambulance we can not move
		// - if there windows are open for ventilation: max speed
		var s = GetChosenSolution ();
		if (s == SolutionIncidents.Ambulance || s == SolutionIncidents.Police) {
			return 0f;
		}
		if (s == SolutionIncidents.Ventilate) {
			return 0.5f;
		}

		return 2f;
	}

	public override GameObject Subject ()
	{
		return train;
	}

	//rogier - tweakit
	public override int Suitability (ISolution solution)
	{
		if (this.type == ObstacleType.WomenInLabour && solution == SolutionBlockages.Crane) return -1;
		return 1;
	}

	#endregion
}