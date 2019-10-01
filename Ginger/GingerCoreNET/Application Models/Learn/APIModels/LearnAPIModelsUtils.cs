using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Application_Models
{
    public class LearnAPIModelsUtils
    {
        public static ApplicationAPIModel CreateAPIModelObject(ApplicationAPIModel sourceAPIModel)
        {
            AutoMapper.MapperConfiguration automapAPIModel = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<ApplicationAPIModel, ApplicationAPIModel>(); });
            ApplicationAPIModel DuplicateAPIModel = automapAPIModel.CreateMapper().Map<ApplicationAPIModel, ApplicationAPIModel>(sourceAPIModel);

            return DuplicateAPIModel;
        }
    }
}
