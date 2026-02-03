using System;

namespace Assets.Script.Models
{
	[Serializable]
    public class Participant
    {
		#region Properties
		public int id;
		public float score;

        public string status;

        public User user;

        public Challenge challenge;

		public int ID
		{
			get { return id; }
		}
		public float Score
		{
			get { return score; }
		}

        public string Status
		{
			get { return status; }
		}

        public User User
		{
			get { return user; }
		}
        public Challenge Challenge
		{
			get { return challenge; }
		}
        #endregion
	}
}