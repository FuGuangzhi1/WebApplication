﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MvcStudyFu.EFCore.SQLSever;
using MvcStudyFu.Interface.DomainInterface;
using StudyMVCFu.Model;
using StudyMVCFu.Model.DomainModel;
using StudyMVCFu.Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace MvcStudyFu.Services.DomainServices
{
    public class Study : BaseService, IStudy
    {
        private readonly IDBContextFactory _dBContextFactory;

        public Study(IDBContextFactory dBContextFactory) : base(dBContextFactory)
        {
            this._dBContextFactory = dBContextFactory;
        }

        public async Task<AjaxResult> DeleteStudyTypeDataAsync(Guid id)
        {
            AjaxResult ajaxResult = new();
            await this.DeleteAsync<Studyknowledge>(id);
            var res = await base.CommitAsync();
            ajaxResult.Success = res.Item1;
            ajaxResult.Message = res.Item2;
            await base.DisposeAsync();
            return ajaxResult;
        }

        public Task<AjaxResult> DeleteStudyTypeDataAsync(IList<Guid> id)
        {
            throw new NotImplementedException();
        }

        public async Task<PageResult<StudyKnowledgeView>> GetStudyKnowledgeAsync
            (string StudyKnowledgeName, int stydyTypeId, int pageSize, int pageIndex)
        {
            var DBCotent = _dBContextFactory.CreateDbContext(ReadWriteEnum.Read);
            PageResult<StudyKnowledgeView> pageResult = new();
            IQueryable<StudyKnowledgeView> data = from a in DBCotent.Set<Studyknowledge>()
                                                  join b in DBCotent.Set<StudyType>()
                                                  on a.StudyTypeId equals b.StudyTypeId
                                                  select new StudyKnowledgeView()
                                                  {
                                                      StudyknowledgeContent = a.StudyknowledgeContent,
                                                      StudyknowledgeName = a.StudyknowledgeName,
                                                      StudyknowledgeId = a.StudyknowledgeId,
                                                      CreateDateTime = a.CreateDateTime,
                                                      UpdateDateTime = a.UpdateDateTime,
                                                      StudyTypeId = b.StudyTypeId,
                                                      StudyknowledgeNameType = b.StudyTypeName,

                                                  };
            Expression<Func<StudyKnowledgeView, bool>> StudyKnowledgeNameWhere = x => true;  //条件表达式
            Expression<Func<StudyKnowledgeView, bool>> stydyTypeIdWhere = x => true;
            if (!string.IsNullOrEmpty(StudyKnowledgeName))
            {
                StudyKnowledgeNameWhere = x => x.StudyknowledgeName == StudyKnowledgeName;
            }
            if (stydyTypeId > 0)
            {
                stydyTypeIdWhere = x => x.StudyTypeId == stydyTypeId;
            }
            try
            {
                pageResult = await base.QueryPageAsync<StudyKnowledgeView, DateTime?>
                   (tList: data, funWhere: StudyKnowledgeNameWhere, funWhere1: stydyTypeIdWhere, pageSize: pageSize,
                   pageIndex: pageIndex, funcOderby: x => x.CreateDateTime);
                pageResult.Success = true;
                pageResult.Message = "操作成功";
            }
            catch (Exception ex)
            {
                pageResult.Message = ex.Message;
            }
            await DBCotent.DisposeAsync();
            return pageResult;
        }

        public async Task<List<StudyType>> GetStudyTypeAsync()
        {
            List<StudyType> data = new();
            IQueryable<StudyType> StudyType = base.Set<StudyType>();
            if (StudyType != null)
                data = await StudyType.ToListAsync();
            return data;
        }

        public async Task<AjaxResult> UpdateOrInsertStudyTypeDataAsync(Studyknowledge studyknowledge)
        {
            AjaxResult ajaxResult = new();
            if (studyknowledge == null) return ajaxResult;
            IQueryable<Studyknowledge> data;
            //区分更新和添加
            if (studyknowledge.StudyknowledgeId == Guid.Empty)
            {
                data = await this.QueryAsync<Studyknowledge>
                    (x => x.StudyknowledgeName == studyknowledge.StudyknowledgeName);
                if (data == null || !data.Any())
                {
                    studyknowledge.CreateDateTime = DateTime.Now;
                    await this.InsertAsync<Studyknowledge>(studyknowledge);
                }
                else ajaxResult.Message = "添加失败，数据已存在";
            }
            else
            {
                data = await this.QueryAsync<Studyknowledge>
                  (x => x.StudyknowledgeName == studyknowledge.StudyknowledgeName && x.StudyknowledgeId != studyknowledge.StudyknowledgeId);
                if (data == null || !data.Any())
                    await this.UpdateAsync<Studyknowledge>(studyknowledge);
                else ajaxResult.Message = "修改失败，数据已存在";
            }
            if (ajaxResult.Message == "操作失败")
            {
                var res = await base.CommitAsync();
                ajaxResult.Success = res.Item1;
                ajaxResult.Message = res.Item2;
            }
            await base.DisposeAsync();
            return ajaxResult;
        }
        public Task<AjaxResult> UpdateOrInsertStudyTypeDataAsync(IList<Studyknowledge> studyknowledge)
        {
            throw new NotImplementedException();
        }
    }
}
