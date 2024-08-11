namespace Auth.Interfaces
{
    internal interface IUserServiceInternal
    {
        /// <summary>
        /// Send a notification to any observers of a change in the user authentication state.
        /// </summary>
        /// <param name="userEvent">The event to nofiy about</param>
        /// <param name="userId">The id of the user who performed the event</param>
        public void NotifyUserEvent(UserEvent userEvent, string userId);
    }
}
