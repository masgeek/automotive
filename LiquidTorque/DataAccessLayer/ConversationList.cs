using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace liquidtorque.DataAccessLayer
{
    /// <summary>
    /// Return conversation list grouped by receipent name
    /// </summary>
    public class ConversationList
    {
        /// <summary>
        /// ID of the conversation thread
        /// </summary>
        public string ConversationID { get; set; }
        /// <summary>
        /// ID of the sender i.e username
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// Id of the reciever i.e username
        /// </summary>
        public string RecipientId { get; set; }

        /// <summary>
        /// date the record was created
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}