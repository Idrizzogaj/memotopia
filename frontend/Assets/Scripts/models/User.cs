using System;

namespace Assets.Script.Models
{
	[Serializable]
    public class User
    {
		#region Properties
		public int id;
		public string username;
		public string email;
		public string avatar;
		public string paymentStatus;
		public UserStatistics userStatistics;

		public int ID
		{
			get { return id; }
		}
		public string Username
		{
			get { return username; }
		}
		public string Email
		{
			get { return email; }
		}
		public string Avatar
		{
			get { return avatar; }
		}
		public string PaymentStatus
		{
			get { return paymentStatus; }
		}
		public UserStatistics UserStatisticsObj
        {
            get { return userStatistics; }
        }

        #endregion
	}
}