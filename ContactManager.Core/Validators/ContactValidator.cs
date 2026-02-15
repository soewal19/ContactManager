using ContactManager.Core.Models;
using FluentValidation;

namespace ContactManager.Core.Validators
{
    public class ContactValidator : AbstractValidator<Contact>
    {
        public ContactValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(System.DateTime.Now);
            RuleFor(x => x.Phone).NotEmpty().Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.");
            RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
        }
    }
}
