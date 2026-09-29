using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MroczwareFramework.Attributes.DependencyInjection;
using MroczwareFramework.Data;
using MroczwareFramework.Services.SettingService;
using MroczwareFramework.Services.TemplateService;

namespace MroczwareFramework.Services.EmailService
{
    [DependencyInjection(typeof(IEmailService))]
    internal partial class EmailService<TDbContext> : IEmailService
        where TDbContext : FrameworkDbContext
    {
        private readonly DbContext _dbContext;
        private readonly ISettingService _settingService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;

        public EmailService(TDbContext dbContext, ISettingService settingService, ITemplateService templateService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _templateService = templateService;
            _configuration = configuration;
        }
    }
}