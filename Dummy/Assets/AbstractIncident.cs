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
			Debug.Log ("Incident already resolved");
			// Already resolved, unmodifyable
		} else if (solution == null || CountDownValue ().HasValue && CountDownValue ().Value == TimeSpan.Zero) {
			Debug.Log ("Setting solution on incident");
			this.resolved = null;
			this.solution = solution;
			this.solutionChosenAt = Time.time;
		} else
			Debug.LogWarning ("Can't change indicent while evaluating: "+solution.ProposalText);
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
