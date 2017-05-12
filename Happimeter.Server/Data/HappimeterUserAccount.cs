using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Happimeter.Server.Data
{
    public class HappimeterUserAccount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Email { get; set; } 

        public DateTime LastSendMovie { get; set; }

    }
}