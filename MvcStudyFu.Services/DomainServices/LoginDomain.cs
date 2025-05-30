﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MvcStudyFu.Common;
using MvcStudyFu.Common.ConvertTpye;
using MvcStudyFu.EFCore.SQLSever;
using MvcStudyFu.Interface.DomainInterface;
using StudyMVCFu.Model;
using StudyMVCFu.Model.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcStudyFu.Services.DomainServices
{
    public class LoginDomain : BaseService, ILoginDomain
    {
        public LoginDomain(IDBContextFactory dBContextFactory) : base(dBContextFactory) { }

        public async Task<AjaxResult> CeateUserAsync(User user, UserPassword userPassword)
        {
            AjaxResult ajaxResult = new();
            bool isEmail = (await base.QueryAsync<User>(x => x.Email == user.Email)).Any();
            if (isEmail)
            {
                ajaxResult.Message = "一个邮箱只能申请一个账号";
            }
            else
            {
                bool isName = (await base.QueryAsync<User>(x => x.Email == user.Name)).Any();
                if (isName)
                {
                    ajaxResult.Message = "已有用户这个名字";
                }
                else
                {
                    ulong? account = await (await base.SetAsync<User>()).Select(x => x.Account).MaxAsync() + 1;
                    user.Account = account;
                    Guid roleId = await Query<Role>(x => x.RoleName == "平民").Select(x => x.RoleId).FirstOrDefaultAsync();
                    UserRole userRole = new() { UserId = user.Id, RoleId = roleId, UserRoleId = Guid.NewGuid() };

                    using StudyMVCDBContext dBContext = _contextFactory.CreateDbContext();
                    using IDbContextTransaction transaction = dBContext.Database.BeginTransaction();
                    try
                    {
                        await dBContext.Set<User>().AddAsync(user);
                        await dBContext.SaveChangesAsync();

                        await dBContext.Set<UserPassword>().AddAsync(userPassword);
                        await dBContext.SaveChangesAsync();

                        await dBContext.Set<UserRole>().AddAsync(userRole);
                        await dBContext.SaveChangesAsync();
                        transaction.Commit();
                        ajaxResult.Success = true;
                        ajaxResult.Message = "创建成功！";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ajaxResult.Message = ex.Message;
                    }
                    if (ajaxResult.Success)
                        ajaxResult.Data = $"创建成功，您的账号是：{account}";
                }
            }
            return ajaxResult;
        }

        public async Task<List<string>> GetRoleAsync(Guid? id)
        {
            List<Guid> roleIds = await (await base.QueryAsync<UserRole>(x => x.UserId == id)).Select(x => x.RoleId).ToListAsync();
            List<string> roleNames = await (await base.QueryAsync<Role>(x => roleIds.Contains(x.RoleId))).Select(x => x.RoleName).ToListAsync();
            return roleNames;
        }

        public async Task<(bool, Guid?)> GetUserAsync(string name, string password)
        {
            ulong account = name.TouLong32();
            Guid? id = Guid.Empty;
            bool iswater = false;
            id = await base.Query<User>(x => x.Name == name & x.IsDel == true).Select(x => x.Id).FirstOrDefaultAsync();
            if (id == Guid.Empty)
            {
                id = await base.Query<User>(x => x.Account == account & x.IsDel == true)
                   .Select(x => x.Id).FirstOrDefaultAsync();
            }
            if (id != Guid.Empty)
            {
                iswater =(await base.Set<UserPassword>().Where(x => x.NewPassword == password.ToMD5() & x.UserId == id)
                    .FirstOrDefaultAsync())!=null;
            }
            if (!iswater) 
            {
                id = Guid.Empty;
            }
            await base.DisposeAsync();
            return new(iswater, id);
        }
    }
}