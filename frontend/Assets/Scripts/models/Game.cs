using System;

namespace Assets.Script.Models
{
	[Serializable]
    public class Game
    {
		#region Properties
		public int id;
		public string gameName;

		public int ID
		{
			get { return id; }
		}
		public string GameName
		{
			get { return gameName; }
		}

        #endregion
	}
}