﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMVCFu.Model.DomainModel
{
    /// <summary>
    /// 学习类型表
    /// </summary>
    public class StudyType
    {
        [Key]
        public int StudyTypeId { get; set;  }
        public string StudyTypeName { get; set; }
        public  List<Studyknowledge> Studyknowledge { get; set; }
    }
}
