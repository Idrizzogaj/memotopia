using System;

namespace Assets.Script.Models
{
	[Serializable]
    public class Challenge
    {
		#region Properties
		public int id;
		public int level;

		public Participant [] participants;

		public Game game;

		public int ID
		{
			get { return id; }
		}
		public int Level
		{
			get { return level; }
		}

        #endregion
	}
}