using System;

namespace Assets.Script.Models
{
	[Serializable]
    public class History
    {
		#region Properties
		public User challenger;
		public string winOrLose;
		public string gameName;
		public int level;
        #endregion
	}
}