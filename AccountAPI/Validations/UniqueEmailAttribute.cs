using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AccountAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountAPI.Validations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value == null) return ValidationResult.Success; // cho phép null

            var db = context.GetService(typeof(AccountDbContext)) as AccountDbContext;
            if (db == null) return ValidationResult.Success; // skip khi Swagger build model

            var dto = context.ObjectInstance as DTOs.UpdateProfileDTO;
            var email = value.ToString();

            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Success;

            var userId = dto?.UserId ?? 0;

            bool exists = db.Users.AsNoTracking()
                .Any(u => u.Email == email && u.UserId != userId);

            if (exists)
                return new ValidationResult("Email đã được sử dụng.");

            return ValidationResult.Success;
        }
    }
}
