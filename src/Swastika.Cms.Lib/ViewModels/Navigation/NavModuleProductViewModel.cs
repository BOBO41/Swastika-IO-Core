﻿using Swastika.Cms.Lib.Models.Cms;
using Swastika.Domain.Data.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using Swastika.Cms.Lib.ViewModels.Info;

namespace Swastika.Cms.Lib.ViewModels
{
    public class NavModuleProductViewModel : ViewModelBase<SiocCmsContext, SiocModuleProduct, NavModuleProductViewModel>
    {
        public NavModuleProductViewModel(SiocModuleProduct model, SiocCmsContext _context = null, IDbContextTransaction _transaction = null) : base(model, _context, _transaction)
        {
        }
        public NavModuleProductViewModel() : base()
        {

        }
        public string ProductId { get; set; }
        public int ModuleId { get; set; }
        //public string Specificulture { get; set; }
        public bool IsActived { get; set; }
        public string Description { get; set; }
        #region Views
        public InfoProductViewModel Product { get; set; }
        #endregion
        #region Override

        public override void ExpandView(SiocCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var getProduct = InfoProductViewModel.Repository.GetSingleModel(p => p.Id == ProductId && p.Specificulture == Specificulture
                , _context: _context, _transaction: _transaction
            );
            if (getProduct.IsSucceed)
            {
                Product = getProduct.Data;
            }
        }

        #region Async

        //public override async Task<RepositoryResponse<ModuleProductViewModel>> CloneAsync(string desSpecificulture, SiocCmsContext _context = null, IDbContextTransaction _transaction = null)
        //{
        //    //Check is destinate cate and product already defined in des culture
        //    bool isValidDes = ModuleListItemViewModel.Repository.CheckIsExists(
        //        c => c.Id == this.ModuleId && c.Specificulture == desSpecificulture, _context, _transaction)
        //    && ModuleListItemViewModel.Repository.CheckIsExists(
        //        c => c.Id == this.ModuleId && c.Specificulture == desSpecificulture, _context, _transaction);
        //    RepositoryResponse<ModuleProductViewModel> result = new RepositoryResponse<ModuleProductViewModel>();

        //    if (isValidDes)
        //    {
        //        var data = new ModuleProductViewModel(
        //            new SiocModuleProduct()
        //            {
        //                ProductId = this.ProductId,
        //                Specificulture = desSpecificulture,
        //                ModuleId = this.ModuleId
        //            },
        //            _context, _transaction)
        //        {
        //            IsActived = this.IsActived,
        //            Description = this.Description
        //        };
        //        var saveResult = await data.SaveModelAsync(_context: _context, _transaction: _transaction);
        //        if (saveResult.IsSucceed)
        //        {
        //            result.IsSucceed = true;
        //            result.Data = data;
        //        }
        //        return result;
        //    }
        //    else
        //    {
        //        return result;
        //    }
        //}

        #endregion

        #region Sync

        #endregion

        #endregion
        #region Expand

        #endregion
    }
}
