using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AccountAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountAPI.Validations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UniquePhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value == null) return ValidationResult.Success;

            var db = context.GetService(typeof(AccountDbContext)) as AccountDbContext;
            if (db == null) return ValidationResult.Success;

            var dto = context.ObjectInstance as DTOs.UpdateProfileDTO;
            var phone = value.ToString();

            if (string.IsNullOrWhiteSpace(phone))
                return ValidationResult.Success;

            var userId = dto?.UserId ?? 0;

            bool exists = db.Users.AsNoTracking()
                .Any(u => u.Phone == phone && u.UserId != userId);

            if (exists)
                return new ValidationResult("Số điện thoại đã được sử dụng.");

            return ValidationResult.Success;
        }
    }
}
