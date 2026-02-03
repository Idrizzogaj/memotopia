
namespace Assets.Script.Constants
{
    public class APIConstants
    {
        #region BASE_URLs
        //const string LOCAL_BASE_URL = "http://localhost:3001";
        const string LOCAL_BASE_URL = "http://172.105.68.212";
        //const string LOCAL_BASE_URL = "http://139.162.145.178"; // prod
        const string DEV_BASE_URL = "http://li2024-212.members.linode.com";
        const string BASE_URL = "";


        const string TERMS_URL = "http://memotopia.com/pp/privacy-and-policy.pdf?fbclid=IwAR09a-hC9bZbhZtb2gFGvOwAPs9jMDWenQ_wozqDADBhKW1qo2Us_jL7jBg";

        const string LOGIN_ROUTE = "/auth/signin";
        const string SIGN_UP_ROUTE = "/users";
        const string SOCIAL_LOGIN_ROUTE = "/auth/social-signin";
        const string LEVELS_ROUTE = "/levels";
        const string GET_LEVELS_ROUTE = "/levels/list";
        const string GET_LEVELS_AVG = "/levels/avg";
        const string GET_USER_ROUTE = "/me";
        const string UPDATE_ACCOUNT = "/users/update-account";
        const string GET_RANDOM_USER_ROUTE = "/users/random";
        const string FORGOT_PASSWORD = "/users/forgot";
        const string CREATE_CHALLENGE_ROUTE = "/challenges";
        const string UPDATE_CHALLENGE_ROUTE = "/challenges/update";
        const string GET_CHALLENGES = "/challenges";
        const string GET_HISTORY = "/challenges/history";
        const string SEARCH = "/users/search";
        const string FAVORITES = "/favorites";
        const string GET_USER_STATISTICS = "/user-statistics";
        const string UPDATE_PAYMENT_STATUS = "/users/update-payment-status";
        const string UPDATE_RESTRICTIONS = "/restrictions/update";
        const string GET_RESTRICTIONS = "/restrictions";
        const string GLOBAL_SCORE = "/user-statistics/global-score";
        const string ACHIEVEMENTS = "/achievements";
        const string TIME_PLAYED = "/user-statistics/time-played";
        #endregion

        public string SignUpRoute { get { return ReturnUrl(SIGN_UP_ROUTE); } }
        public string LogInRoute { get { return ReturnUrl(LOGIN_ROUTE); } }
        public string SocialLogInRoute { get { return ReturnUrl(SOCIAL_LOGIN_ROUTE); } }
        public string GetLevelsRoute { get { return ReturnUrl(GET_LEVELS_ROUTE); } }
        public string LevelsRoute { get { return ReturnUrl(LEVELS_ROUTE); } }
        public string GetLevelsAvg { get { return ReturnUrl(GET_LEVELS_AVG); } }
        public string GetUserRoute { get { return ReturnUrl(GET_USER_ROUTE); } }
        public string UpdateAccount { get { return ReturnUrl(UPDATE_ACCOUNT); } }
        public string GetRandomUserRoute { get { return ReturnUrl(GET_RANDOM_USER_ROUTE); } }
        public string CreateChallengeRoute { get { return ReturnUrl(CREATE_CHALLENGE_ROUTE); } }
        public string UpdateChallengeRoute { get { return ReturnUrl(UPDATE_CHALLENGE_ROUTE); } }
        public string GetChallengeRoute { get { return ReturnUrl(GET_CHALLENGES); } }
        public string GetHistoryRoute { get { return ReturnUrl(GET_HISTORY); } }
        public string Search { get { return ReturnUrl(SEARCH); } }
        public string GetFavorites { get { return ReturnUrl(FAVORITES); } }
        public string GetUserStatistics { get { return ReturnUrl(GET_USER_STATISTICS); } }
        public string UpdatePaymentStatus { get { return ReturnUrl(UPDATE_PAYMENT_STATUS); } }
        public string UpdateRestrictions { get { return ReturnUrl(UPDATE_RESTRICTIONS); } }
        public string GetRestrictions { get { return ReturnUrl(GET_RESTRICTIONS); } }
        public string ForgotPassword { get { return ReturnUrl(FORGOT_PASSWORD); } }
        public string GlobalScore { get { return ReturnUrl(GLOBAL_SCORE); } }
        public string Achievements { get { return ReturnUrl(ACHIEVEMENTS); } }
        public string TimePlayed { get { return ReturnUrl(TIME_PLAYED); } }
        public string TermsUrl { get { return TERMS_URL; } }

        string ReturnUrl(string requestRoute, bool isDev = true)
        {
            string url = LOCAL_BASE_URL;
            if (!isDev) url = BASE_URL;

            return url + requestRoute;
        }
    }
}
