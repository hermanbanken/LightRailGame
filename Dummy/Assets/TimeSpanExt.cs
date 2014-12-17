using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using System;

public static class TimeSpanExt {
	
	public static string FormatMinSec(this TimeSpan span){
		return span.Minutes.ToString ("D2") + ":" + span.Seconds.ToString ("D2");
	}

}