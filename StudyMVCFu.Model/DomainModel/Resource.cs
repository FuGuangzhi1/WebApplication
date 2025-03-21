﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMVCFu.Model.DomainModel
{
    /// <summary>
    /// 资源表
    /// </summary>
    [Table("Resource")]
    public class Resource
    {
        [Key]
        public Guid ResourceId { get; set; }
        [Required]
        public string ResourceName { get; set; }
        public Nullable<Guid> ParentId { get; set; }
        public string Path { get; set; }
        public long Level { get; set; }   //层级
        public string Icon { get; set; }
        public int Sort { get; set; }
    }
}