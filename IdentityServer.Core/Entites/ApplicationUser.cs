using System;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Core.Entites
{
    public class ApplicationUser : IdentityUser
    {
        public string LastName { get; private set; }
        public string FirstName { get; private set; }
        public DateTime InsertDate { get; set; }
        public DateTime? DisableDate { get; set; } 
        public bool AcceptsInformativeEmails { get; private set; }

        public static ApplicationUser Create(string email, string lastName, string firstName, bool acceptsEmails)
        {
            Guard.Against.NullOrEmpty(email, nameof(email));
            Guard.Against.NullOrEmpty(lastName, nameof(lastName));
            Guard.Against.NullOrEmpty(firstName, nameof(firstName));

            return new ApplicationUser
            {
                UserName = email,
                Email = email,
                LastName = lastName,
                FirstName = firstName,
                AcceptsInformativeEmails = acceptsEmails,
                InsertDate = DateTime.UtcNow
            };
        }
    }
}