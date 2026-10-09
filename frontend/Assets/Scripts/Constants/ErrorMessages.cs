
namespace Assets.Script.Constants
{
    public static class ErrorMessages
    {
        #region MessagesToDisplay
        public static readonly string s_wrongCredentials = "Wrong username or password";
        public static readonly string s_fillAllFields = "You should fill all the fields";
        public static readonly string s_toggleNotClickedMessage = "Please accept our terms and conditions in order to sign up";
        public static readonly string s_invalidEmailMessage = "Please write a valid email";
        public static readonly string s_shortPasswordMessage = "Please write a password between 8 and 20 characters";
        public static readonly string s_shortUsernameMessage = "Please write a username between 5 and 25 characters";
        public static readonly string s_passwordsShouldMatchMessage = "Two passwords that you provided should match";
        public static readonly string s_userRegisterBadInputMessage = "Username or email already exist";
        public static readonly string s_somethingWentWrong = "Something went wrong";
        public static readonly string s_usernameExistsMessage = "Username already exist, try another one.";
        public static readonly string s_invalidProviderMessage = "Please use facebook or iOS login";
        public static readonly string s_emailNotVerifiedMessage = "Please confirm your email first. We have sent you a new link - check your inbox and spam folder, then log in again.";
        public static readonly string s_tooManyRequestsMessage = "Too many attempts. Please wait a while and try again.";
        public static readonly string s_checkYourEmailMessage = "Almost done! We sent a confirmation link to your email. Open it, then log in here.";
        public static readonly string s_mailNotSentMessage = "Your account was created, but we could not send the confirmation email. Try logging in to get a new link.";
        #endregion

        #region ErrorMessage
        public static readonly string s_invalidCredentials = "INVALID_CREDENTIALS";
        public static readonly string s_toggleNotClicked = "TOGGLE_NOT_CLICKED";
        public static readonly string s_invalidEmailCode = "NOT_VALID_EMAIL";
        public static readonly string s_shortPasswordCode = "SHORT_PASSWORD";
        public static readonly string s_passwordsShouldMatchCode = "PASSWORDS_SHOULD_MATCH";
        public static readonly string s_userRegisterBadInputCode = "USERNAME_OR_EMAIL_EXISTS";
        public static readonly string s_shortUsernameCode = "SHORT_USERNAME";
        public static readonly string s_usernameExistsCode = "USERNAME_EXISTS";
        public static readonly string s_invalidProviderCode = "INVALID_PROVIDER";
        public static readonly string s_emailNotVerifiedCode = "EMAIL_NOT_VERIFIED";
        public static readonly string s_tooManyRequestsCode = "TOO_MANY_REQUESTS";
        #endregion
    }
}
