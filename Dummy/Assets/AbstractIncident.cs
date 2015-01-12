using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class AbstractIncident : IIncident {

	public event Action<IIncident> OnResolved;
	public event Action<IIncident> OnUserAction;
	public event Action<IIncident> OnFailed;
	protected ISolution solution;
	protected float? solutionChosenAt;
	protected bool? resolved = null;

	public AbstractIncident(){
		// Fire occur event
		LightRailGame.ScoreManager.DoOccur(this);
		IncidentVisualizer.Add (this);
		// Track events
		this.OnUserAction += LightRailGame.ScoreManager.DoUserAction;
		this.OnResolved += LightRailGame.ScoreManager.DoResolved;
		this.OnResolved += (IIncident obj) => IncidentVisualizer.Remove (obj);
		this.OnFailed += LightRailGame.ScoreManager.DoFailed;
	}

	#region IIncident implementation
	public abstract string Description ();

	public bool IsResolved ()
	{
		if (resolved.HasValue)
			return resolved.Value;
		if (solutionChosenAt.HasValue && solutionChosenAt.Value + solution.ResolveTime.TotalSeconds < Time.time) {
			// Add randomness, solution might fail:
			if(Suitability(solution) > 0 && UnityEngine.Random.value < solution.SuccessRatio){
				resolved = true;
				// Fire event
				var listeners = OnResolved;
				if(resolved.Value == true && listeners != null){
					listeners(this);
				}
			} else {
				resolved = false;
				var listeners = OnFailed;
				if(listeners != null)
					listeners(this);
			}
		}
		return resolved.HasValue && resolved.Value;
	}

	abstract public IEnumerable<ISolution> PossibleActions ();

	public void SetChosenSolution (ISolution solution)
	{
		if (IsResolved ()) {
			// Already resolved, unmodifyable
		} else if (this.solution == null || CountDownValue ().HasValue && CountDownValue ().Value == TimeSpan.Zero) {
			this.resolved = null;
			this.solution = solution;
			this.solutionChosenAt = Time.time;
			// Fire event
			var listeners = OnUserAction;
			if(listeners != null){
				listeners(this);
			}
		}
	}

	public ISolution GetChosenSolution ()
	{
		return solution;
	}

	public TimeSpan? CountDownValue ()
	{
		if (solution == null || !solutionChosenAt.HasValue)
			return null;
		var remaining = solutionChosenAt.Value + solution.ResolveTime.TotalSeconds - Time.time;
		if (remaining > 0)
			return TimeSpan.FromSeconds (remaining);
		return TimeSpan.Zero;
	}

	public abstract float MaxSpeedOfSubject ();

	public abstract GameObject Subject ();

	public abstract int Suitability (ISolution solution);

	#endregion
}
