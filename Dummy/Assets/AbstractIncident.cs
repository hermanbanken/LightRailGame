using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class AbstractIncident : IIncident {
	private ISolution solution;
	private float? solutionChosenAt;
	private bool? resolved = null;

	#region IIncident implementation

	public bool IsResolved ()
	{
		if (resolved.HasValue)
			return resolved.Value;
		if (solutionChosenAt.HasValue && solutionChosenAt.Value + solution.ResolveTime.TotalSeconds < Time.time) {
			resolved = solutionChosenAt.HasValue && solutionChosenAt.Value + solution.ResolveTime.TotalSeconds < Time.time; // TODO randomness here: && UnityEngine.Random.value < solution.SuccessRatio;
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

	#endregion
}
