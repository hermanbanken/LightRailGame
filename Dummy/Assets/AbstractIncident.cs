using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class AbstractIncident : IIncident {
	ISolution solution;
	DateTime? solutionChosenAt;
	bool? resolved = null;

	#region IIncident implementation

	public bool IsResolved ()
	{
		if (resolved.HasValue)
			return resolved.Value;
		if (solutionChosenAt.HasValue && solutionChosenAt.Value + solution.ResolveTime < DateTime.Now) {
			resolved = solutionChosenAt.HasValue && solutionChosenAt.Value + solution.ResolveTime < DateTime.Now; // TODO randomness here: && UnityEngine.Random.value < solution.SuccessRatio;
		}
		return resolved.HasValue && resolved.Value;
	}

	abstract public IEnumerable<ISolution> PossibleActions ();

	public void SetChosenSolution (ISolution solution)
	{
		if (resolved.HasValue && resolved.Value) {
			// Already resolved, unmodifyable
		} else {
			this.resolved = null;
			this.solution = solution;
			this.solutionChosenAt = DateTime.Now;
		}
	}

	public ISolution GetChosenSolution ()
	{
		return solution;
	}

	#endregion
}
