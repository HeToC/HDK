﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{

    public interface IViewModel : INotificationObject
    {
    }

    public class ViewModelBase : BindableBase, IViewModel
    {

    }

    public interface IViewModelMetadata
    {
        string CorrelationToken { get; set; }
    }

    public class ViewModelMetadata : IViewModelMetadata
    {
        public string CorrelationToken { get; set; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportViewModelAttribute : ExportAttribute, IViewModelMetadata
    {
        public ExportViewModelAttribute()
            : base(typeof(IViewModel))
        {
        }

        public ExportViewModelAttribute(string token)
            : base(typeof(IViewModel))
        {
            CorrelationToken = token;
        }

        public string CorrelationToken { get; set; }
    }
}