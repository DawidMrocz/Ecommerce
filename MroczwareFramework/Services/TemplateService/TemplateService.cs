using Microsoft.EntityFrameworkCore;
using MroczwareFramework.Attributes.DependencyInjection;
using MroczwareFramework.Data;
using MroczwareFramework.Models.Template;

namespace MroczwareFramework.Services.TemplateService
{
    [DependencyInjection(typeof(ITemplateService))]
    internal class TemplateService<TDbContext> : ITemplateService
        where TDbContext : FrameworkDbContext
    {
        private readonly DbSet<TemplateModel> _dbSet;

        public TemplateService(TDbContext dbContext)
        {
            _dbSet = dbContext.Set<TemplateModel>();
        }
        public async Task<TemplateModel?> Get(string templateName, string cultureStrongName)
        {
            return await _dbSet.Include(c => c.Culture).Where(t => t.StrongName == templateName && t.Culture.StrongName == cultureStrongName).FirstOrDefaultAsync();
        }
    }
}
