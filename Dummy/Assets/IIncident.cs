using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface IIncident
{
	event Action<IIncident> OnResolved;
	event Action<IIncident> OnUserAction;
	bool IsResolved();
	TimeSpan? CountDownValue();
	string Description();
	IEnumerable<ISolution> PossibleActions();
	int Suitability(ISolution solution);
	void SetChosenSolution (ISolution solution);
	ISolution GetChosenSolution ();
	float MaxSpeedOfSubject();
	GameObject Subject();
	string HandleText();
}

public interface ISolution {
	string ProposalText { get; }
	TimeSpan ResolveTime { get; }
	float SuccessRatio { get; }
}

public class Solution : ISolution {
	#region ISolution implementation

	public string ProposalText {
		get {
			return _ProposalText;
		}
	}

	public TimeSpan ResolveTime {
		get {
			return _ResolveTime;
		}
	}

	public float SuccessRatio {
		get {
			return _SuccessRatio;
		}
	}

	#endregion

	string _ProposalText;
	TimeSpan _ResolveTime;
	float _SuccessRatio;

	public Solution(string ProposalText, TimeSpan ResolveTime, float SuccessRatio) {
		this._SuccessRatio = SuccessRatio;
		this._ResolveTime = ResolveTime;
		this._ProposalText = ProposalText;
	}
}