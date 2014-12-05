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
		resolved = solutionChosenAt.HasValue && solutionChosenAt.Value + solution.ResolveTime < DateTime.Now && UnityEngine.Random.value < solution.SuccessRatio;
		return resolved.Value;
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

	#endregion
}
