using System;
using System.ComponentModel.DataAnnotations;

namespace LimLink_API.Model
{
    public class UpdateAccountModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string FullName { get; set; }
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
