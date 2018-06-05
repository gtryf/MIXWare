namespace MIXUI.Dtos
{
    /// <summary>
    /// A user inside the database
    /// </summary>
    public class GetUserDto
    {
        /// <summary>
        /// The user's internal ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The user's external user name
        /// </summary>
        public string UserName { get; set; }
    }
}
