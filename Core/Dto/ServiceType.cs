using System.Drawing;

namespace Core.Dto
{
    public class ServiceType
    {
        /// <summary>
        /// The name of the service selected by the user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description of the offered service
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The location of the service, such as studio, home etc.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The price of the provided service
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The Color to display the events as in the Calendar.
        /// NOT shown to the end-user when booking through forms
        /// </summary>
        public Color DisplayColor { get; set; }
    }
}
