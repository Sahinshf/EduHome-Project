using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace Project.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required, DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
    }
}
