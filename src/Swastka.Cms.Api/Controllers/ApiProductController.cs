﻿// Licensed to the Swastika I/O Foundation under one or more agreements.
// The Swastika I/O Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.OData.Query;
using Newtonsoft.Json.Linq;
using Swastika.Api.Controllers;
using Swastika.Cms.Lib;
using Swastika.Cms.Lib.Models.Cms;
using Swastika.Cms.Lib.ViewModels.BackEnd;
using Swastika.Cms.Lib.ViewModels.FrontEnd;
using Swastika.Cms.Lib.ViewModels.Info;
using Swastika.Cms.Lib.ViewModels.Spa;
using Swastika.Domain.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Swastika.Common.Utility.Enums;

namespace Swastka.Cms.Api.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
    //    //, Policy = "AddEditUser"
    //    )]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/{culture}/product")]
    public class ApiProductController :
        BaseApiController
    {
        public ApiProductController()
        {
        }

        #region Get

        // GET api/products/id
        [HttpGet]
        [Route("details/{viewType}/{id}")]
        [Route("details/{viewType}")]
        public async Task<JObject> BEDetails(string viewType, string id)
        {
            switch (viewType)
            {
                case "be":
                    if (!string.IsNullOrEmpty(id))
                    {
                        var beResult = await BEProductViewModel.Repository.GetSingleModelAsync(model => model.Id == id && model.Specificulture == _lang).ConfigureAwait(false);
                        if (beResult.IsSucceed)
                        {
                            beResult.Data.DetailsUrl = SwCmsHelper.GetRouterUrl("Product", new { beResult.Data.SeoName }, Request, Url);
                        }
                        return JObject.FromObject(beResult);
                    }
                    else
                    {
                        var model = new SiocProduct() { Specificulture = _lang, Status = (int)SWStatus.Preview };
                        RepositoryResponse<BEProductViewModel> result = new RepositoryResponse<BEProductViewModel>()
                        {
                            IsSucceed = true,
                            Data = new BEProductViewModel(model)
                        };
                        return JObject.FromObject(result);
                    }
                default:
                    if (!string.IsNullOrEmpty(id))
                    {
                        var beResult = await FEProductViewModel.Repository.GetSingleModelAsync(model => model.Id == id && model.Specificulture == _lang).ConfigureAwait(false);
                        if (beResult.IsSucceed)
                        {
                            beResult.Data.DetailsUrl = SwCmsHelper.GetRouterUrl("Product", new { beResult.Data.SeoName }, Request, Url);
                        }
                        return JObject.FromObject(beResult);
                    }
                    else
                    {
                        var model = new SiocProduct();
                        RepositoryResponse<FEProductViewModel> result = new RepositoryResponse<FEProductViewModel>()
                        {
                            IsSucceed = true,
                            Data = new FEProductViewModel(model) { Specificulture = _lang, Status = SWStatus.Preview }
                        };
                        return JObject.FromObject(result);
                    }
            }
        }

        // GET api/products/id
        [HttpGet]
        [Route("create")]
        public RepositoryResponse<BEProductViewModel> Create()
        {
            SiocProduct product = new SiocProduct()
            {
                //Id = Guid.NewGuid().ToString(),
                Specificulture = _lang

            };
            return new RepositoryResponse<BEProductViewModel>()
            {
                IsSucceed = true,
                Data = new BEProductViewModel(product) { Status = SWStatus.Preview }
            };
        }

        // GET api/products/id
        [HttpGet]
        [Route("init/{viewType}")]
        public JObject Init(string viewType)
        {
            SiocProduct product = new SiocProduct()
            {
                //Id = Guid.NewGuid().ToString(),
                Specificulture = _lang

            };

            switch (viewType)
            {
                case "be":
                    var be = new RepositoryResponse<BEProductViewModel>()
                    {
                        IsSucceed = true,
                        Data = new BEProductViewModel(product) { Status = SWStatus.Preview }
                    };
                    return JObject.FromObject(be);
                default:
                    var fe = new RepositoryResponse<InfoProductViewModel>()
                    {
                        IsSucceed = true,
                        Data = new InfoProductViewModel(product) { Status = SWStatus.Preview }
                    };
                    return JObject.FromObject(fe);
            }
        }

        // GET api/products/id
        [HttpGet]
        [Route("recycle/{id}")]
        public async Task<RepositoryResponse<InfoProductViewModel>> Recycle(string id)
        {
            var getProduct = InfoProductViewModel.Repository.GetSingleModel(a => a.Id == id);
            if (getProduct.IsSucceed)
            {
                var data = getProduct.Data;
                data.Status = SWStatus.Deleted;
                return await data.SaveModelAsync().ConfigureAwait(false);
            }
            else
            {
                return new RepositoryResponse<InfoProductViewModel>() { IsSucceed = false };
            }
        }

        // GET api/products/id
        [HttpGet]
        [Route("restore/{id}")]
        public async Task<RepositoryResponse<InfoProductViewModel>> Restore(string id)
        {
            var getProduct = InfoProductViewModel.Repository.GetSingleModel(a => a.Id == id);
            if (getProduct.IsSucceed)
            {
                var data = getProduct.Data;
                data.Status = SWStatus.Preview;
                return await data.SaveModelAsync().ConfigureAwait(false);
            }
            else
            {
                return new RepositoryResponse<InfoProductViewModel>() { IsSucceed = false };
            }
        }

        // GET api/products/id
        [HttpGet]
        [Route("delete/{id}")]
        public async Task<RepositoryResponse<bool>> Delete(string id)
        {
            var getProduct = BEProductViewModel.Repository.GetSingleModel(a => a.Id == id && a.Specificulture == _lang);
            if (getProduct.IsSucceed)
            {
                return await getProduct.Data.RemoveModelAsync(true).ConfigureAwait(false);
            }
            else
            {
                return new RepositoryResponse<bool>() { IsSucceed = false };
            }
        }

        // GET api/products
        [HttpGet]
        //[Authorize]
        [Route("list")]
        [Route("list/{pageSize:int?}/{pageIndex:int?}")]
        [Route("list/{orderBy}/{direction}")]
        [Route("list/{pageSize:int?}/{pageIndex:int?}/{orderBy}/{direction}")]
        public async Task<RepositoryResponse<PaginationModel<InfoProductViewModel>>> Get(int? pageSize = 15, int? pageIndex = 0, string orderBy = "Id", OrderByDirection direction = OrderByDirection.Ascending)
        {
            var data = await InfoProductViewModel.Repository.GetModelListByAsync(
                m => m.Status != (int)SWStatus.Deleted && m.Specificulture == _lang, orderBy, direction, pageSize, pageIndex).ConfigureAwait(false); //base.Get(orderBy, direction, pageSize, pageIndex);
            if (data.IsSucceed)
            {
                data.Data.Items.ForEach(a => a.DetailsUrl = SwCmsHelper.GetRouterUrl("Product", new { a.SeoName }, Request, Url));
            }
            return data;
        }

        // GET api/products
        [HttpGet]
        [Route("search/{keyword}")]
        [Route("search/{pageSize:int?}/{pageIndex:int?}/{keyword}")]
        [Route("search/{pageSize:int?}/{pageIndex:int?}/{orderBy}/{direction}/{keyword}")]
        public async Task<RepositoryResponse<PaginationModel<InfoProductViewModel>>> Search(string keyword = null, int? pageSize = null, int? pageIndex = null, string orderBy = "Id", OrderByDirection direction = OrderByDirection.Ascending)
        {
            Expression<Func<SiocProduct, bool>> predicate = model =>
                model.Specificulture == _lang
                && model.Status != (int)SWStatus.Deleted
                && (string.IsNullOrWhiteSpace(keyword)
                    || (model.Title.Contains(keyword)
                    || model.Content.Contains(keyword)));

            var data = await InfoProductViewModel.Repository.GetModelListByAsync(predicate, orderBy, direction, pageSize, pageIndex).ConfigureAwait(false); // base.Search(predicate, orderBy, direction, pageSize, pageIndex, keyword);
            //if (data.IsSucceed)
            //{
            //    data.Data.Items.ForEach(d => d.DetailsUrl = string.Format("{0}{1}", _domain, this.Url.Action("Details", "products", new { id = d.Id })));
            //    data.Data.Items.ForEach(d => d.EditUrl = string.Format("{0}{1}", _domain, this.Url.Action("Edit", "products", new { id = d.Id })));
            //    data.Data.Items.ForEach(d => d.Domain = _domain);
            //}
            return data;
        }

        // GET api/products
        [HttpGet]
        [Route("draft/{keyword}")]
        [Route("draft/{pageSize:int?}/{pageIndex:int?}")]
        [Route("draft/{pageSize:int?}/{pageIndex:int?}/{keyword}")]
        [Route("draft/{pageSize:int?}/{pageIndex:int?}/{orderBy}/{direction}/{keyword}")]
        public async Task<RepositoryResponse<PaginationModel<InfoProductViewModel>>> Draft(
            string keyword = null, int? pageSize = null, int? pageIndex = null, string orderBy = "Id"
            , OrderByDirection direction = OrderByDirection.Ascending)
        {
            Expression<Func<SiocProduct, bool>> predicate = model =>
            model.Specificulture == _lang

            && model.Status != (int)SWStatus.Deleted
            && (
            string.IsNullOrWhiteSpace(keyword)
                || (model.Title.Contains(keyword) || model.Content.Contains(keyword))
                );
            var data = await InfoProductViewModel.Repository.GetModelListByAsync(predicate, orderBy, direction, pageSize, pageIndex).ConfigureAwait(false); // base.Search(predicate, orderBy, direction, pageSize, pageIndex, keyword);
            //if (data.IsSucceed)
            //{
            //    data.Data.Items.ForEach(d => d.DetailsUrl = string.Format("{0}{1}", _domain, this.Url.Action("Details", "products", new { id = d.Id })));
            //    data.Data.Items.ForEach(d => d.EditUrl = string.Format("{0}{1}", _domain, this.Url.Action("Edit", "products", new { id = d.Id })));
            //    data.Data.Items.ForEach(d => d.Domain = _domain);
            //}
            return data;
        }

        #endregion Get

        #region Post

        // POST api/products
        [HttpPost]
        [Route("save")]
        public async Task<RepositoryResponse<BEProductViewModel>> Save([FromBody] BEProductViewModel product)
        {
            if (product != null)
            {
                var result = await product.SaveModelAsync(true).ConfigureAwait(false);
                if (result.IsSucceed)
                {
                    result.Data.DetailsUrl = SwCmsHelper.GetRouterUrl("Product", new { seoName = product.SeoName }, Request, Url);
                }
                return result;
            }
            return new RepositoryResponse<BEProductViewModel>();
        }

        // POST api/category
        [HttpPost, HttpOptions]
        [Route("save/{id}")]
        public async Task<RepositoryResponse<bool>> SaveFields(string id, [FromBody]List<EntityField> fields)
        {
            if (fields != null)
            {
                foreach (var property in fields)
                {
                    var result = await BEProductViewModel.Repository.UpdateFieldsAsync(c => c.Id == id, fields).ConfigureAwait(false);

                    return result;
                }
            }
            return new RepositoryResponse<bool>();
        }

        // GET api/products

        [HttpPost, HttpOptions]
        [Route("list")]
        public async Task<RepositoryResponse<PaginationModel<InfoProductViewModel>>> GetList([FromBody]RequestPaging request)
        {
            string domain = string.Format("{0}://{1}", Request.Scheme, Request.Host);
            ParseRequestPagingDate(request);
            Expression<Func<SiocProduct, bool>> predicate = model =>
                model.Specificulture == _lang
                && (!request.Status.HasValue || model.Status == (int)request.Status.Value)
                && (string.IsNullOrWhiteSpace(request.Keyword)
                || (
                    model.Title.Contains(request.Keyword)

                    || model.Excerpt.Contains(request.Keyword)
                    || model.Code.Contains(request.Keyword)
                    )
                )
                && (!request.FromDate.HasValue
                    || (model.CreatedDateTime >= request.FromDate.Value)
                )
                && (!request.ToDate.HasValue
                    || (model.CreatedDateTime <= request.ToDate.Value)
                );

            var data = await InfoProductViewModel.Repository.GetModelListByAsync(predicate, request.OrderBy, request.Direction, request.PageSize, request.PageIndex).ConfigureAwait(false);
            if (data.IsSucceed)
            {
                data.Data.Items.ForEach(a =>
                {
                    a.DetailsUrl = SwCmsHelper.GetRouterUrl(
                        "Product", new { a.SeoName }, Request, Url);
                });
            }
            return data;
        }

        #endregion Post
    }
}
