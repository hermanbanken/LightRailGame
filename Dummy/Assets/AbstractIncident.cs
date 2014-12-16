using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class AbstractIncident : IIncident {

	public event Action<IIncident> OnResolved;
	public event Action<IIncident> OnUserAction;
	private ISolution solution;
	private float? solutionChosenAt;
	private bool? resolved = null;

	public AbstractIncident(){
		// Fire occur event
		LightRailGame.ScoreManager.DoOccur(this);
		// Track events
		this.OnUserAction += LightRailGame.ScoreManager.DoUserAction;
		this.OnResolved += LightRailGame.ScoreManager.DoResolved;
	}

	#region IIncident implementation

	public bool IsResolved ()
	{
		if (resolved.HasValue)
			return resolved.Value;
		if (solutionChosenAt.HasValue && solutionChosenAt.Value + solution.ResolveTime.TotalSeconds < Time.time) {
			// Add randomness, solution might fail:
			if(UnityEngine.Random.value < solution.SuccessRatio){
				resolved = true;
				// Fire event
				var listeners = OnResolved;
				if(resolved.Value == true && listeners != null){
					listeners(this);
				}
			} else {
				resolved = false;
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

	#endregion
}
