using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cybex.DVSuperGauges
{
	public class SGManager : MonoBehaviour
	{
		public IEnumerator ExecuteAfterInteriorLoaded (params Action[] actions)
		{
			yield return new WaitUntil(() => PlayerManager.Car.IsInteriorLoaded);
			actions.ToList().ForEach(a => a.Invoke());
			yield break;
		}

		public void Destroy () { Destroy(this.gameObject); }
	}
}
