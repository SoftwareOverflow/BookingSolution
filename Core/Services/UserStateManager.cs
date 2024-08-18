namespace Core.Services
{
    internal class UserStateManager
    {

        /// <summary>
        /// Unique reference to the user
        /// </summary>
        internal string? UserId { get; private set; } = null;

        /// <summary>
        /// The first name of the user, usable for a personalized experience when greeting the user for example
        /// </summary>
        public string? UserFirstName { get; private set; } = null;

        /// <summary>
        /// Update the current user information
        /// </summary>
        /// <param name="id">Id of the current user</param>
        /// <param name="firstName">First name of the current user</param>
        public void UpdateUser(string? id, string? firstName)
        {
            UserId = id;
            UserFirstName = firstName;

            NullifyEmptyIds();
        }

        private void NullifyEmptyIds()
        {
            if (UserId == string.Empty)
            {
                UserId = null;
            }
            if (UserFirstName == string.Empty)
            {
                UserFirstName = null;
            }
        }
    }
}
