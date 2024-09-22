using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TopSpeed.Domain.Common;
using TopSpeed.Infrastructure.Migrations;

namespace TopSpeed.Infrastructure.Common
{
    public static class ExtensionMethods
    {
        public static async Task<string> GetCurrentUserId(UserManager<IdentityUser> _userManger,IHttpContextAccessor _contextAccessor)
        {
            var userId = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                var user = await _userManger.GetUserAsync(_contextAccessor.HttpContext.User);
                userId = user?.Id;

            }
            return userId;
        }

        public static async void SaveCommonFields(this ApplicationDbContext dbContext, UserManager<IdentityUser> _userManger, IHttpContextAccessor _contextAccessor)
        {
            var userId = await GetCurrentUserId(_userManger, _contextAccessor);

            IEnumerable<Domain.Common.BaseModel> insertEntities = dbContext.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .OfType<Domain.Common.BaseModel>();

            IEnumerable<Domain.Common.BaseModel> updateEntities = dbContext.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<Domain.Common.BaseModel>();

            foreach (var item in insertEntities)
            {
                item.CreatedOn = DateTime.UtcNow;
                item.CreatedBy = userId;
                item.ModificationOn = DateTime.UtcNow;
            }
            foreach (var item in updateEntities)
            {
               
                item.ModifiedBy = userId;
                item.ModificationOn = DateTime.UtcNow;
            }
        }
    }
}
