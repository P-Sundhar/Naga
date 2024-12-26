using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NagaMaster.Models
{
    public class LoginModel
    {
        [Required]
        [MaxLength(20)]
        public string UserId { get; set; }
        [Required]
        [MaxLength(20)]
        public string UserPWD { get; set; }
        public string PlantId { get; set; }
        public string PlantName { get; set; }
    }
    public class LoginMessage
    {
        public int Flag { get; set; }
        public string Message { get; set; }
    }
}