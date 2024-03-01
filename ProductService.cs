using AutoMapper;
using LogicLync.DTO;
using LogicLync.Entities;
using LogicLync.Repository;
using LogicLync.Repository.Infrastructure;
using LogicLync.Service.Extension;
using LogicLync.Service.HelperClasses;
using LogicLync.Service.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nancy.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using static LogicLync.Entities.Enums;
using ProductStoreStatus = LogicLync.Entities.ProductStoreStatus;

namespace LogicLync.Service
{
    public class ProductService : IProductService
    {
        readonly IMapper _mapper;
        readonly IUnitOfWork _unitOfWork;
        readonly IProductRepository _productRepository;
        readonly IProductVersionRepository _productVersionRepository;
        readonly IProductCategoryRepository _productCategoryRepository;
        readonly IProductImageRepository _productImageRepository;
        readonly IProductVideoRepository _productVideoRepository;
        readonly IProductPropertyRepository _productPropertyRepository;
        readonly IRewardDetailsRepository _rewardDetialRepository;
        readonly IRewardTypeRepository _rewardTypeRepository;
        readonly ICategoryProductVoucherRepository _categoryProductVoucherRepository;
        readonly ICategoryProductCouponRepository _categoryProductCouponRepository;
        readonly IMasterPropertyRepository _masterPropertyRepository;
        readonly IMasterListValueRepository _masterListValueRepository;
        readonly IMasterListRepository _materListRepository;
        readonly IProductPriceListRepository _productPriceListRepository;
        readonly ICategoryPropertyRepository _categoryPropertyRepository;
        readonly IProductVariationRepository _productVariationRepository;
        readonly IProductVariationPropertyCombinationRepository _productVariationPropertyCombinationRepository;
        readonly IProductVariationImageRepository _productVariationImageRepository;
        readonly IOfferDetailRepository _offerDetailRepository;
        readonly IRatingReviewRepository _ratingReviewRepository;
        readonly IProductShippingCostVariationRepository _productShippingCostVariationRepository;
        readonly IUserAddressRepository _userAddressRepository;
        readonly IUserDetailsRepository _userDetailsRepository;
        readonly IUserRoleRepository _userRoleRepository;
        readonly IUserStoreRepository _userStoreRepository;
        readonly IShippingCostVariationRepository _shippingCostVariationRepository;
        readonly IStoreRepository _storeRepository;
        readonly ICategoryRepository _categoryRepository;
        readonly IFlashDealProductRepository _flashDealProductRepository;
        public readonly IEmailQueueService _emailQueueService;
        public readonly IWebSettingsRepository _webSettingsRepository;
        private readonly ICategoryService _categoryService;
        private readonly IOrderDetailsRepository _orderDetailsRepository;
        public ProductService(IMapper mapper,
                                IUnitOfWork unitOfWork,
                                IProductCategoryRepository productCategoryRepository,
                                IProductRepository productionRepository,
                                IProductVersionRepository productVersionRepository,
                                IProductImageRepository productImageRepository,
                                IProductVideoRepository productVideoRepository,
                                IProductPropertyRepository productPropertyRepository,
                                IRewardDetailsRepository rewardDetialRepository,
                                IRewardTypeRepository rewardTypeRepository,
                                ICategoryProductVoucherRepository categoryProductVoucherRepository,
                                IVoucherDetailRepository voucherDetailRepository,
                                ICategoryProductCouponRepository categoryProductCouponRepository,
                                IMasterPropertyRepository masterPropertyRepository,
                                IMasterListValueRepository masterListValueRepository,
                                IMasterListRepository materListRepository,
                                IProductPriceListRepository productPriceListRepository,
                                ICategoryPropertyRepository categoryPropertyRepository,
                                IProductVariationRepository productCombinationRepository,
                                IProductVariationPropertyCombinationRepository productPropertyCombinationRepository,
                                IProductVariationImageRepository productPropertyCombinationImageRepository,
                                IOfferDetailRepository offerDetailRepository,
                                IRatingReviewRepository ratingReviewRepository,
                                IProductShippingCostVariationRepository productShippingCostVariationRepository,
                                IUserAddressRepository userAddressRepository,
                                IUserDetailsRepository userDetailsRepository,
                                IUserRoleRepository userRoleRepository,
                                IShippingCostVariationRepository shippingCostVariationRepository
                                ,IStoreRepository storeRepository
                                ,ICategoryRepository categoryRepository
                                ,IFlashDealProductRepository flashDealProductRepository
                                ,IEmailQueueService emailQueueService
                                ,IUserStoreRepository userStoreRepository
                                ,IWebSettingsRepository webSettingsRepository,
                                ICategoryService categoryService
                                , IOrderDetailsRepository orderDetailsRepository
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderDetailsRepository = orderDetailsRepository;
            _productRepository = productionRepository;
            _productVersionRepository = productVersionRepository;
            _productCategoryRepository = productCategoryRepository;
            _productImageRepository = productImageRepository;
            _productVideoRepository = productVideoRepository;
            _productPropertyRepository = productPropertyRepository;
            _rewardDetialRepository = rewardDetialRepository;
            _rewardTypeRepository = rewardTypeRepository;
            _categoryProductVoucherRepository = categoryProductVoucherRepository;
            _categoryProductCouponRepository = categoryProductCouponRepository;
            _masterPropertyRepository = masterPropertyRepository;
            _masterListValueRepository = masterListValueRepository;
            _materListRepository = materListRepository;
            _productPriceListRepository = productPriceListRepository;
            _categoryPropertyRepository = categoryPropertyRepository;
            _productVariationRepository = productCombinationRepository;
            _productVariationPropertyCombinationRepository = productPropertyCombinationRepository;
            _productVariationImageRepository = productPropertyCombinationImageRepository;
            _offerDetailRepository = offerDetailRepository;
            _ratingReviewRepository = ratingReviewRepository;
            _userDetailsRepository = userDetailsRepository;
            _userRoleRepository = userRoleRepository;
            //_unitOfWork = unitOfWork;
            //_mapper = mapper;
            _productShippingCostVariationRepository = productShippingCostVariationRepository;
            _userAddressRepository = userAddressRepository;
            _shippingCostVariationRepository = shippingCostVariationRepository;
            _storeRepository = storeRepository;
            _categoryRepository = categoryRepository;
            _flashDealProductRepository = flashDealProductRepository;
            _emailQueueService = emailQueueService;
            _userStoreRepository = userStoreRepository;
            _webSettingsRepository = webSettingsRepository;
            _categoryService = categoryService;
        }
                public async Task<List<CategoryProductDTO>> getProductOncategoryId(int categoryId)
        {
           
            if (categoryId > 0)
            {
                var data = from cat in _categoryRepository.GetAll(x=>x.Id==categoryId).Where(x=>x.Status==20 && x.ParentId==0)
                            join prodc in _productCategoryRepository.GetAll()
                            on cat.Id equals prodc.CategoryId
                           join prod in _productRepository.GetAll().Where(x => x.IsSaleable == true && x.StatusId == (int)Enums.ProductStatus.Published)
                           on prodc.ProductId equals prod.Id
                           join prodimg in _productImageRepository.GetAll().Where(x => x.IsPrimary == true)
                           on prod.Id equals prodimg.ProductId
                           select new CategoryProductDTO()
                           {
                               ProductId = prod.Id,
                               CategoryId = prodc.CategoryId,
                               ProductName = prod.Name,
                               Price = prod.Price,
                               DiscountPrice = prod.DiscountedPrice,
                               Description = prod.Description,
                               ProductImageurl = "Product/Image/" + prodimg.ImagePath
                           };
                return data.ToList();
            }
            else
            {
                var data = from cat in _categoryRepository.GetAll().Where(x => x.Status == 20 && x.ParentId==0)
                           join prodc in _productCategoryRepository.GetAll()
                           on cat.Id equals prodc.CategoryId
                           join prod in _productRepository.GetAll().Where(x => x.IsSaleable == true && x.StatusId == (int)Enums.ProductStatus.Published)
                           on prodc.ProductId equals prod.Id
                           join prodimg in _productImageRepository.GetAll().Where(x => x.IsPrimary == true)
                           on prod.Id equals prodimg.ProductId
                           select new CategoryProductDTO()
                           {
                               ProductId = prod.Id,
                               CategoryId = prodc.CategoryId,
                               ProductName = prod.Name,
                               Price = prod.Price,
                               DiscountPrice = prod.DiscountedPrice,
                               Description = prod.Description,
                               ProductImageurl = "Product/Image/" + prodimg.ImagePath
                           };
                return data.ToList();
            }
        }

         public async Task<List<ProductDTOReport>> GetALlProductForReport()
        {
            List<ProductDTOReport> dtos = new List<ProductDTOReport>();
            var result = _productRepository.GetAll(x => x.StatusId == 20).OrderBy(x=>x.Name).ToList();
            dtos = _mapper.Map<List<ProductDTOReport>>(result);
            return dtos;

        }
        public async Task<ProductQueryResult<ProductModifiedDTO>> GetQueryProducts(int storeId, ProductQueryObject query)
        {
            if (string.IsNullOrEmpty(query.SortBy))
            {
                query.SortBy = "id";
            }
            var columnMap = new Dictionary<string, Expression<Func<Product, object>>>()
            {
                ["id"] = p => p.Id,
                ["name"] = p => p.Name,
                ["sku"] = p => p.Sku,
                ["status"] = p => p.StatusId,
                ["price"] = p => p.Price,
                ["discountedPrice"] = p => p.DiscountedPrice,
                ["isSaleable"] = p => p.IsSaleable
            };

            var products = _productRepository.AllIncluding(x => x.ProductCategory, x => x.Store);                          

            if (query.IsStoreStaff)
            {
                products = products.Where(x => x.CreatedBy == query.UserId);
            }
            else
            {
                products = products.Where(x => x.StoreId == storeId || storeId == 0);
            }

            if (query.StoreId > 0)
            {
                products = products.Where(x => x.StoreId == query.StoreId);
                
            }
            if(query.StatusId > 0)
            {
                products = products.Where(x => x.StatusId == query.StatusId);
            }
            if (query.CategoryId > 0)
            {
                products = products.Where(x => x.ProductCategory.Any(y => y.CategoryId == query.CategoryId));
            }
            if (query.ProductStoreStatusId > 0)
            {
                products = products.Where(x => x.ProductStoreStatusId == query.ProductStoreStatusId);
            }

            if (!string.IsNullOrEmpty(query.SearchString))
            {
                products = products.Where(x => x.Name.ToLower().Contains(query.SearchString.ToLower())
                                            || x.Description.ToLower().Contains(query.SearchString.ToLower())
                                            || x.Sku.Contains(query.SearchString));
            }

            var _productList = products.ApplyOrdering(query, columnMap).ToList();

            var productList = (from pro in _productList
                               select new ProductModifiedDTO()
                               {
                                   Id = pro.Id,
                                   Name = pro.Name,
                                   Sku = pro.Sku,
                                   Status = ((ProductStatus)pro.StatusId).ToString(),
                                   IsSaleable = pro.IsSaleable,
                                   Price = pro.Price,
                                   DiscountedPrice = pro.DiscountedPrice,
                                   Commission = pro.AffiliateCommission ?? 0,
                                   ProductStoreStatus = ((Enums.ProductStoreStatus)(pro.ProductStoreStatusId??0)).ToString() == "0"
                                                       ? string.Empty :
                                                       ((Enums.ProductStoreStatus)pro.ProductStoreStatusId).ToString(),
                                   ProductStoreStatusDescription = ((Enums.ProductStoreStatus)(pro.ProductStoreStatusId ?? 0)).ToString() == "0"
                                                       ? string.Empty :
                                                       EnumHelper.GetDescription(((Enums.ProductStoreStatus)pro.ProductStoreStatusId)),
                                   AutoApproval = pro.Store.AllowAutoProductApproval ?? false,
                                   IsVersion= _productVersionRepository.FindBy(x => x.ParentProductId == pro.Id).FirstOrDefault() != null,
                               });

           

           

            var queryResult = new ProductQueryResult<ProductModifiedDTO>
            {
                TotalItems = products.Count(),
                Items = productList
            };

            return queryResult;
        }
        public string GeneratePdfTemplateString(QueryResult<ProductModifiedDTO> productList)
        {
            var sb = new StringBuilder();

            sb.Append(@"<html>
                            <head>
                                <h1>Product List</h1>
                            </head>
                            <body>
                                <table align='center'>");
            sb.Append(@"<thead>
                            <tr>
                                        <th>Id</th>
                                        <th>Product Name</th>
                                        <th>SKU</th>
                                        <th>Status</th>
                                        <th>Is Seleable</th>
                                        <th>Price</th>
                                        <th>Discounted Price</th>
                                        
                                    </tr></thead>");
            foreach (var item in productList.Items)
            {
                                sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    <td>{6}</td>
                                    
                                  </tr>", item.Id, item.Name, item.Sku, item.Status, item.IsSaleable, item.Price, item.DiscountedPrice);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }

        public async Task<ProductStoreStatusDTO> SaveProduct(ProductDTO productDTO)
        {
            try
            {
                var product = _mapper.Map<Product>(productDTO);
                var userDetails = _userDetailsRepository.FindBy(x => x.UserId == productDTO.userId).FirstOrDefault();
                if (userDetails != null)
                {
                    var websettings = _webSettingsRepository.All.FirstOrDefault();
                    if (websettings != null)
                    {
                        if (websettings.ProductReturnDays != null)
                        {
                            product.ProductReturnDays = websettings.ProductReturnDays;
                        }
                    }
                    //product.StatusId = (int)ProductStatus.Draft;
                    product.CreatedBy = userDetails.Id;
                    product.CreatedDate = DateTime.Now;
                }

                await _productRepository.Add(product);

                var ProductCategory = new List<ProductCategory>();
                foreach (var item in productDTO.CategoryId)
                {
                    var productCat = new ProductCategory
                    {
                        CategoryId = item,
                        ProductId = product.Id,
                    };
                    ProductCategory.Add(productCat);
                }
                product.ProductCategory = ProductCategory;
                var status=await ProductStoreStatus(product, userDetails, productDTO.Role);
               
                await _unitOfWork.Commit();
                var productStaus = new ProductStoreStatusDTO
                {
                    Id = product.Id,
                    Status = status,
                };
                return productStaus;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private async Task<string> ProductStoreStatus(Product product, UserDetails userDetails, string role)
        {
            var status = await CheckAutoApproval(product,role);
            product.ProductStoreStatus = new List<ProductStoreStatus>
                {
                    new ProductStoreStatus
                    {
                        ProductId = product.Id,
                        ProductStoreStatusId = product.ProductStoreStatusId,
                        Status = status,
                        Comment = string.Empty,
                        CreatedBy = userDetails.Id,
                        CreatedDate = DateTime.Now,
                    }
                };
            return status;
        }

        public async Task<OrderCheck> CheckIfProductIsPurchable(int productId, int quantity, List<int> variantionId)
        {
            var product = await _productRepository.GetSingle(productId);
            if (product == null) return new OrderCheck();
           
            var flashdealexpired = false;
           
            if (variantionId!=null && variantionId.Any())
            {
                var combination = new List<long>();
                foreach (var item in variantionId)
                {
                    var PropertyCombination = await _productVariationPropertyCombinationRepository
                                            .AllIncluding(x => x.ProductVariation)
                                            .Where(x => x.MasterListValuesId == item && x.ProductVariation.ProductId == productId
                                             && (x.ProductVariation.IsDelete == null || x.ProductVariation.IsDelete == false))
                                            .Select(x => x.ProductVariationId).ToListAsync();
                    if (!combination.Any())
                    {
                            combination = PropertyCombination;
                    }
                    else
                    {
                        combination=combination.Intersect(PropertyCombination).ToList();
                    }
                }

              
                if (combination.Any())
                {
                    var Combinationquantity = 0; 
                    var flashdealCheck = _flashDealProductRepository.FindBy(x => x.ProductId == productId && x.IsDeleted != true && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now).ToList();
                    if (flashdealCheck.Any())
                    {
                        var flashdealCheckData = flashdealCheck
                                               .FirstOrDefault(x => x.ProductVariationId == combination.FirstOrDefault());                       
                        if (flashdealCheckData != null)
                        {
                            Combinationquantity = flashdealCheckData.Quantity ?? 0;
                            flashdealexpired = DateTime.Now > flashdealCheckData.EndDate;
                        }                      

                    }
                    else
                    {

                        var combinationquantity = await _productVariationRepository.GetSingle(combination.FirstOrDefault());
                        Combinationquantity = combinationquantity.Quantity;
                    }
                    return new OrderCheck
                    {
                        IsSaleable = product.IsSaleable,
                        IsOutOfStock = quantity > Combinationquantity,
                        FlashDealExpired = flashdealexpired
                    };
                }             
                return new OrderCheck
                {
                    IsSaleable =false,
                    IsOutOfStock = true,
                    FlashDealExpired=false
                };
            }
            else
            {
                var normalquantity = 0;
                var flashdeal = await _flashDealProductRepository.FindBy(x => x.ProductId == productId && x.IsDeleted != true && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now).FirstOrDefaultAsync();
                if (flashdeal != null)
                {
                    flashdealexpired = DateTime.Now > flashdeal.EndDate;
                    normalquantity = flashdeal.Quantity??0;
                }
                else
                {
                    normalquantity = (int)product.Stock;
                }
                return new OrderCheck
                {
                    IsSaleable = product.IsSaleable,
                    IsOutOfStock = quantity > normalquantity,
                    FlashDealExpired = flashdealexpired
                };
            }        
            

        }

        public async Task<bool> CheckFlashDeal(int productId)
        {
            var result = await _flashDealProductRepository.FindBy(x => x.ProductId == productId && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now).FirstOrDefaultAsync();
            if (result == null) return false;
            return DateTime.Now > result.EndDate;

        }


        private async Task<string> CheckAutoApproval(Product product, string role)
        {
            var store = await _storeRepository.FindBy(x => x.Id == product.StoreId).FirstOrDefaultAsync();
            var status = string.Empty;

            if (store != null)
            {
                product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.Added;
                status = Enums.ProductStoreStatus.Added.ToString();
                role = role?.ToLower();
                if (Convert.ToBoolean(store.AllowAutoProductApproval))
                {
                    if(role == EnumHelper.GetDescription(Entities.Enums.Role.STOREADMINISTRATOR).ToLower()
                        || role == EnumHelper.GetDescription(Entities.Enums.Role.INSTRUCTOR).ToLower())
                    {
                        //product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.StoreApproved;
                        //status = Enums.ProductStoreStatus.StoreApproved.ToString();

                        product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.Approved;
                        status = Enums.ProductStoreStatus.Approved.ToString();
                    }
                    else if(role == EnumHelper.GetDescription(Entities.Enums.Role.STORESTAFF).ToLower())
                    {
                  var checkautoapproval = _userStoreRepository.All.FirstOrDefault(x => x.UserDetailId == product.CreatedBy);
                        if (checkautoapproval != null)
                        {
                            if (Convert.ToBoolean(checkautoapproval.AutoApproval))
                            {
                                product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.Approved;
                                status = Enums.ProductStoreStatus.Approved.ToString();
                            }
                            else
                            {
                                product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.Added;
                                status = Enums.ProductStoreStatus.Added.ToString();
                            }
                        }
                    }
                    
                }
                else if(role == EnumHelper.GetDescription(Entities.Enums.Role.STOREADMINISTRATOR).ToLower()
                    || role == EnumHelper.GetDescription(Entities.Enums.Role.INSTRUCTOR).ToLower())
                {
                    product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.StoreApproved;
                    status = Enums.ProductStoreStatus.StoreApproved.ToString();
                }
                
                else if(role == EnumHelper.GetDescription(Entities.Enums.Role.STORESTAFF).ToLower())
                {
                    var checkautoapproval = _userStoreRepository.All.FirstOrDefault(x => x.UserDetailId == product.CreatedBy);
                    if (checkautoapproval != null)
                    {
                        if (Convert.ToBoolean(checkautoapproval.AutoApproval))
                        {
                            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.StoreApproved;
                            status = Enums.ProductStoreStatus.StoreApproved.ToString();
                        }
                        else
                        {
                            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.Added;
                            status = Enums.ProductStoreStatus.Added.ToString();
                        }
                    }
                }
               

            }

            return status;
        }

        public async Task UpdateProduct(ProductDTO productDTO)
        {
            Product product = await _productRepository.GetSingle(productDTO.Id);
            if (product != null)
            {
                var userDetails = _userDetailsRepository.FindBy(x => x.UserId == productDTO.userId).FirstOrDefault();
                if (userDetails != null)
                {
                    product.UpdatedBy = userDetails.Id;
                    product.UpdatedDate = DateTime.Now;
                }

                product.StatusId = productDTO.StatusId;
                product.Sku = productDTO.Sku;
                product.Name = productDTO.Name;
                product.StoreId = productDTO.StoreId;
                //product.StatusId = productDTO.StatusId;
                product.IsSaleable = productDTO.IsSaleable;
                product.Price = productDTO.Price;
                product.CostPrice = productDTO.CostPrice;
                product.DiscountedPrice = productDTO.DiscountedPrice;
                product.Description = productDTO.Description;
                product.BrandId = productDTO.BrandId;
                product.IsFreeShipping = productDTO.IsFreeShipping;
                product.BaseDeliveryCharge = productDTO.BaseDeliveryCharge;
                product.ReturnPolicyWarranty = productDTO.ReturnPolicyWarranty;
                product.AffiliateCommission = productDTO.AffiliateCommission;
                product.AffiliateCommissionPercentage = productDTO.AffiliateCommissionPercentage;
                product.InstructorCommissionPercentage = productDTO.InstructorCommissionPercentage;
                product.InstructorCommission = productDTO.InstructorCommission;
                product.DiscountPercentage = productDTO.DiscountPercentage;
                product.Expirydays = productDTO.Expirydays;
                var productcategory = _productCategoryRepository.FindBy(x => x.ProductId == product.Id);
                foreach (var item in productDTO.CategoryId)
                {
                    var check = productcategory.FirstOrDefault(x => x.CategoryId == item);
                    if (check == null)
                    {
                        var model = new ProductCategory
                        {
                            CategoryId = item,
                            ProductId = product.Id
                        };
                        await _productCategoryRepository.Add(model);
                    }
                }

                foreach (var item in productcategory)
                {
                    var check = productDTO.CategoryId.FirstOrDefault(x => x == item.CategoryId);
                    if (check == 0)
                    {
                        _productCategoryRepository.Delete(item);
                    }
                }
            }
            await _unitOfWork.Commit();
        }

        public async Task<bool> PublishProduct(int id)
        {
            var product = await _productRepository.GetSingle(id);
            if (product == null) return false;
            product.StatusId = (int)ProductStatus.Published;
            await _unitOfWork.Commit();
            return true;
        }

        public async Task<bool> ArchiveProduct(int id)
        {
            var product = await _productRepository.GetSingle(id);
            if (product == null) return false;
            product.StatusId = (int)ProductStatus.Archived;
            await _unitOfWork.Commit();
            return true;
        }


        public bool CheckProductName(ProductDTO productDTO)
        {
            if (productDTO.Id > 0)
            {
                var check = _productRepository.FindBy(x => x.Name.ToLower().Trim() == productDTO.Name.ToLower().ToLower() && x.Id != productDTO.Id);
                return check.Any();
            }
            else
            {
                var check = _productRepository.FindBy(x => x.Name.ToLower().Trim() == productDTO.Name.ToLower().ToLower());
                return check.Any();
            }
        }
        public bool CheckProductNameByString(ProductNameStatusDTO product)
        {
            if (product.Id > 0)
            {
                var check = _productRepository.FindBy(x => x.Name.ToLower().Trim() == product.Name.ToLower().ToLower() && x.Id != product.Id);
                return check.Any();
            }
            else
            {
                var check = _productRepository.FindBy(x => x.Name.ToLower().Trim() == product.Name.ToLower().ToLower());
                return check.Any();
            }
        }

        public async Task<ProductDTO> GetProductById(int Id)
        {
            //await _productCategoryRepository.SQLQuery();
            var result = await _productRepository.GetSingle(Id);
            if (result == null) return new ProductDTO();
           
            var product= _mapper.Map<ProductDTO>(result);
            if (result.ProductStoreStatusId != null)
            {
                product.ProductStoreStatus = ((Enums.ProductStoreStatus)result.ProductStoreStatusId).ToString();
            }
            else
            {
                product.ProductStoreStatus = string.Empty;
            }
           
            return product;
        }

        public async Task<bool> UpdatePortalCommission(int Id, decimal commission)
        {
            var result = await _productRepository.GetSingle(Id);
            if (result == null) return false;
            result.PortalCommission = commission;
            await _unitOfWork.Commit();
            return true;
        }


        public async Task<ProductDTO> GetProductById(int Id, int storeId)
        {
            //await _productCategoryRepository.SQLQuery();
            var result = await _productRepository.FindBy(x => x.Id == Id && x.StoreId == storeId).FirstOrDefaultAsync();
            if (result == null) return new ProductDTO();

            var product = _mapper.Map<ProductDTO>(result);
            product.ProductStoreStatus = ((Enums.ProductStoreStatus)result.ProductStoreStatusId).ToString();
            return product;
        }


        public async Task<ProductDTO> GetClientProductById(int Id, Int64 UserId = 0)
        {
            DateTime currentDate = DateTime.Now;

            var result = _productRepository.AllIncluding(x => x.Store, x => x.Brand).Include(x=>x.ProductCategory).Include(x => x.Contents).ThenInclude(x=>x.Contentfiles).Where(x => x.Id == Id && x.StatusId ==(int)Enums.ProductStatus.Published).FirstOrDefault();
            //var result = await _productRepository.GetSingle(Id);
            if (result == null) return new ProductDTO();
           
            var product = _mapper.Map<ProductDTO>(result);
            if (result.Contents.Count() > 0)
            {
                int estimatedHours = 0;
                int estimatedminutes = 0;
                int calDuration = 0;
                double diff = 0.0;
                foreach (var ites in result.Contents) 
                {
                    if (ites.Contentfiles.Count() > 0)//for each contentfile
                    {
                        int hours = 0;
                        int minute = 0;
                        int dur = 0;
                        double diffe = 0.0;
                        foreach (var item in ites.Contentfiles)
                        {
                            hours = hours + item.EstimatedHour ?? 0;
                            minute = minute + item.Estimatedminutes ?? 0;
                            if (minute % 2 != 0 && minute > 60)
                                minute = minute - 1;
                        }
                        if (minute >= 60)
                        {
                            dur = (int)(minute / 60);
                            hours = hours + dur;
                            diffe = ((float)(minute) / (float)60) - dur;
                            minute = (int)Math.Ceiling(diffe * 60);
                        }
                        ites.EstimatedHour = hours;
                        ites.Estimatedminutes = minute;

                    }
                   
                    estimatedHours = estimatedHours + ites.EstimatedHour ?? 0;
                    estimatedminutes = estimatedminutes + ites.Estimatedminutes ?? 0;
                }
                if (estimatedminutes >= 60)
                {
                    calDuration = (int)(estimatedminutes / 60);
                    estimatedHours = estimatedHours + calDuration;
                    diff = ((float)estimatedminutes / (float)60) - calDuration;
                    estimatedminutes = (int)Math.Ceiling(diff * 60);
                }
                product.EstimatedHour = estimatedHours;
                product.EstimatedMinute = estimatedminutes;
            }
            //product.EstimatedHour = result.Contents.FirstOrDefault()?.EstimatedHour;
            //product.EstimatedMinute = result.Contents.FirstOrDefault()?.Estimatedminutes;
            product.IsProductPurchable = CheckIfPurchable(result);
            result.ProductImage = await _productImageRepository
                            .FindBy(x => x.ProductId == Id && (x.IsDelete == false || x.IsDelete == null)).ToListAsync();
            if (result.ProductImage != null && result.ProductImage.Any())
            {
                product.ProductImageList = _mapper.Map<List<ProductImageDTO>>(result.ProductImage.OrderByDescending(x => x.IsPrimary));
            }

            var video = _productVideoRepository.FindBy(x => (x.ProductId == Id && x.IsDelete != true)).Where(x=>x.IsPrimary==true);
            if(video!=null && video.Any())
            {
                product.ProductVideo = new List<string>();
                product.ProductVideoList = new List<ProductVideoListDTO>();
                foreach (var item in video)
                {
                    product.ProductVideo.Add("Product/Video/" + item.VideoPath);
                    product.ProductVideoList.Add(new ProductVideoListDTO()
                    {
                        VideoUrl = item.VideoType == (int)Enums.VideoType.Normal ? "Product/Video/" + item.VideoPath : item.VideoPath,
                        ProductId = item.ProductId,
                        CoverImageUrl = item.VideoType == (int)Enums.VideoType.Normal ? "Product/Video/Thumbnail/" + item.ThumbnailImage : item.ThumbnailImage,
                        VideoType = item.VideoType
                    });
                }
            }
           
         
            product.Stock = product.Stock - product.NoOfSales;

            var ratingReviews = _ratingReviewRepository.All.Where(x => x.ProductId == Id && x.Status == 1);
            product.TotalReviews = ratingReviews != null ? ratingReviews.Count() : 0;
            product.AverageRating = ratingReviews != null && ratingReviews.Count() > 0 ? ratingReviews.Average(x => x.ProductRating) : 0;
            product.StoreName = product.Store != null ? product.Store.Name : "";
            FlashDealProduct flashDetailProduct = _flashDealProductRepository.All.Where(x => x.ProductId == Id && x.IsDeleted !=true && x.StartDate <= currentDate && x.EndDate > currentDate ).FirstOrDefault();
            if (flashDetailProduct != null)
            {
                product.FlashDealDetail = new FlashDealDetailDTO()
                {
                    Id = flashDetailProduct.Id,
                    Quantity = flashDetailProduct.Quantity.Value,
                    SalesPercentage = flashDetailProduct.Quantity.HasValue && flashDetailProduct.Quantity > 0 ? (((float)flashDetailProduct.SoldQuantity / (float)flashDetailProduct.Quantity) * 100) : 0,
                    SoldQuantity = flashDetailProduct.SoldQuantity.Value,
                    EndDate = flashDetailProduct.EndDate,
                    TimeLeft = (flashDetailProduct.EndDate - currentDate).TotalSeconds,
                    //ItemLeft = flashDetailProduct.Quantity - flashDetailProduct.SoldQuantity
                    ItemLeft = flashDetailProduct.Quantity
                };
                product.Stock = (long)flashDetailProduct.Quantity;
                product.Price = flashDetailProduct.Price??0;
                product.DiscountedPrice = flashDetailProduct.DiscountedPrice??0;
            }
            if (product.IsFreeShipping)
            {
                product.BaseDeliveryCharge = 0;
            }
            else
            {
                var shippingDetails = _productShippingCostVariationRepository.All.Where(x => x.ProductId == Id && x.IsActive == true);
                product.FreeShippingArea = (from s in shippingDetails.Where(x => x.IsFreeShipping == true)
                                            select s.CityList.Name
                                            ).ToList();

                if (shippingDetails != null && shippingDetails.Count() > 0 && UserId > 0)
                {
                    var defaultAddress = _userAddressRepository.All.Where(x => x.UserId == UserId && x.AddressTypeId == (int)LogicLyncEnum.AddressType.Shipping && x.IsPrimaryAdress == true).FirstOrDefault();
                    if (defaultAddress != null && shippingDetails.Where(x => x.CityListId == defaultAddress.CityId).Count() > 0)
                    {
                        var shippingDetail = shippingDetails.Where(x => x.CityListId == defaultAddress.CityId).FirstOrDefault();
                        if (shippingDetail.IsFreeShipping)
                        {
                            product.BaseDeliveryCharge = 0;
                        }
                        else if (shippingDetail != null && shippingDetail.VariationType == (int)Enums.VariationType.Flat)
                        {
                            product.BaseDeliveryCharge += shippingDetail.Amount;
                        }
                        else
                        {
                            product.BaseDeliveryCharge += (decimal)(product.BaseDeliveryCharge / shippingDetail.Amount * 100);
                        }
                    }
                }
            }
            List<int> productCategories = _productCategoryRepository.All.Where(x => x.ProductId == product.Id).Select(x => x.CategoryId).ToList();
            product.OfferDetail = (from o in _categoryProductCouponRepository.All
                                   .Where(x => (x.ProductId == product.Id || productCategories.Contains(x.CategoryId))
                                    && (x.IsDelete == false || x.IsDelete == null))
                                   select new OfferCouponDetailDTO()
                                   {
                                       Id = o.OfferDetails.Id,
                                       Amount = o.OfferDetails.DiscountAmount,
                                       Code = o.OfferDetails.CouponDetails.FirstOrDefault().CouponCode,
                                       MinimumAmount = o.OfferDetails.MinimumCartValue,
                                       MaximumDiscountAmount = o.OfferDetails.MaximumDiscountAmount,
                                       StartDate = o.OfferDetails.StartDate,
                                       EndDate = o.OfferDetails.EndDate,
                                       DiscountType = o.OfferDetails.DiscountType
                                   }).ToList();
            var val = "";
            var result1 = "";
            Category child = _categoryRepository.GetById(result.ProductCategory.FirstOrDefault().CategoryId);
            val = child.Name;
            if (child.ParentId != 0)
            {
                result1 = _categoryService.getCategoryWithoutparent(child.ParentId ?? 0, val);
            }
            else
            {
                result1 = val;
            }
            product.CategoryName = result1;
            return product;
        }

        //public bool CheckIfPurchable(Product product, FlashDeal flashdeal=null)
         public bool CheckIfPurchable(Product product, FlashDealProduct flashdeal=null)
        {
            var isproductpurchable = false;
            if (product is null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (flashdeal == null)
            {
                flashdeal = _flashDealProductRepository.All.FirstOrDefault(x => x.ProductId == product.Id && (x.IsDeleted == false || x.IsDeleted.HasValue == false) && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now);
            }
            if (flashdeal != null)
            {
                if (flashdeal.ProductVariationId != null && flashdeal.ProductVariationId > 0)
                {
                    isproductpurchable = true;
                }
                if (product.IsSaleable && flashdeal.Quantity > 0 && flashdeal.EndDate > DateTime.Now)
                {
                    isproductpurchable = true;
                }
            }
            else
            {
                if (product.IsSaleable && product.Stock > 0)
                {
                    isproductpurchable = true;
                }
            }
            

            return isproductpurchable;

        }      
           
            
          
            
        


        public async Task<List<ProductDTO>> GetAllProduct(long storeId = 0)
        {
            var productImage = _productImageRepository.GetAll();
            //var result = await _productRepository.GetAll(x => x.StoreId == storeId || storeId == 0).ToListAsync();
            var result = await _productRepository.GetAll(x => (x.StoreId == storeId || storeId == 0)&&(x.StatusId==20)).ToListAsync(); //to 
            var product = _mapper.Map<List<ProductDTO>>(result);
            foreach (var item in product)
            {
                item.productId = item.Id;
                item.imgUrl = "Product//Image//" + productImage.Where(x => x.ProductId == item.Id && x.IsPrimary == true).FirstOrDefault().ImagePath;
               item.IsVersion= _productVersionRepository.FindBy(x => x.ParentProductId == item.Id).FirstOrDefault()!=null;
            }
            
            return product;
        }
        public List<int> GetCategoryBasedOnProduct(int productId)
        {
            var categoryIds = _productCategoryRepository.FindBy(x => x.ProductId == productId)
                .Select(x => x.CategoryId).ToList();
            return categoryIds;


        }


        public async Task UploadImage(int productId, List<string> filepath)
        {
            var product = await _productRepository.GetSingle(productId);

            var list = new List<ProductImage>();
            foreach (var item in filepath)
            {
                var image = new ProductImage
                {
                    ProductId = productId,
                    ImagePath = item,

                };
                list.Add(image);
            }

            product.ProductImage = list;
            await _unitOfWork.Commit();

        }

        public async Task<ProductImageDTO> DeleteImage(int id)
        {
            var proudctImage = await _productImageRepository.GetSingle(id);
            var model = new ProductImageDTO
            {
                ProductId = proudctImage.ProductId,
                ImagePath = proudctImage.ImagePath
            };
            _productImageRepository.Delete(proudctImage);
            //added because of foreign key exception
            //var results = _productVariationImageRepository.FindBy(x => x.ProductImageId == id).ToList();
            //_productVariationImageRepository.DeleteRange(results);

            await _unitOfWork.Commit();
            return model;


        }

        public async Task<ProductImageDTO> DeleteVideo(int id)
        {
            var productvideo = await _productVideoRepository.GetSingle(id);
            var model = new ProductImageDTO
            {
                ProductId = productvideo.ProductId,
                ImagePath = productvideo.VideoPath
            };

            _productVideoRepository.Delete(productvideo);
            await _unitOfWork.Commit();
            return model;


        }

        public async Task UploadVideo(int productId, List<string> filepath)
        {
            var product = await _productRepository.GetSingle(productId);
            //var video = _productVideoRepository.FindBy(x => x.ProductId == productId);
            //foreach (var item in video)
            //{
            //    _productVideoRepository.Delete(item);
            //}
            var list = new List<ProductVideo>();
            foreach (var item in filepath)
            {
                var image = new ProductVideo
                {
                    ProductId = productId,
                    VideoPath = item,
                    VideoType = (int)Enums.VideoType.Normal
                };
                list.Add(image);
            }

            product.ProductVideo = list;
            await _unitOfWork.Commit();

        }

        public async Task SaveYouTubeVideo(ProductVideoListDTO model)
        {
            ProductVideo data = new ProductVideo()
            {
                ProductId = model.ProductId,
                VideoType = (int)Enums.VideoType.YouTube,
                VideoPath = model.VideoUrl,
                IsDelete = false,
                ThumbnailImage = model.VideoUrl
            };
            await _productVideoRepository.Add(data);
            await _unitOfWork.Commit();
        }

        public async Task SaveProductProperty(int productId, List<ProductPropertyDTO> productProperty)
        {
            productProperty = productProperty.Where(x => string.IsNullOrEmpty(x.Value) == false).ToList();            
            var totalCombination = this.GetCombination(productProperty);
            var list = new List<string>();
            foreach (var item in productProperty)
            {
                await SaveMultiSelect(productId, item, list);
                //await SaveMultiSelectProductProperty(productId, item);
            }

            var checkIfEmpty = productProperty.Where(x => x.Value == string.Empty);
            var product = _productRepository.GetById(productId);
            if (checkIfEmpty.Count() == productProperty.Count())
            {
                product.HasVariation = false;                
            }
            else
            {
                product.HasVariation = true;
            }

            var productpropertyTodelete = _productPropertyRepository.FindBy(x => x.ProductId == productId
                                            && !list.Contains(x.Value)).ToList();
            _productPropertyRepository.DeleteRange(productpropertyTodelete);

            var existingCombination = _productVariationRepository.All.Where(x => x.ProductId == productId && (x.IsDelete == false|| x.IsDelete==null))
                                        .Include(x=>x.ProductVariationPropertyCombination).ToList();

            foreach(var itemDelete in existingCombination)
            {
                List<int> oldIds = itemDelete.ProductVariationPropertyCombination.Select(x => x.MasterListValuesId).ToList();
                var filteredData = totalCombination.Where(x => oldIds.Count() == x.ProductVariationPropertyCombination.Count() && x.ProductVariationPropertyCombination.All(y => oldIds.Contains(y.MasterListValuesId))).ToList();
                var isNotDelete = filteredData.Count() > 0;
                if (!isNotDelete)
                {
                    var existingItem = await _productVariationRepository.GetSingle(itemDelete.Id);
                    existingItem.IsDelete = true;
                    itemDelete.IsDelete = true;
                    //await _unitOfWork.Commit();
                }
            }

            existingCombination = existingCombination.Where(x => x.IsDelete == false).ToList();

            foreach (var item in totalCombination)
            {
                List<int> newIds = item.ProductVariationPropertyCombination.Select(x => x.MasterListValuesId).ToList();
                var filteredData = existingCombination
                                  .Where(x => newIds.Count() == x.ProductVariationPropertyCombination.Count() 
                                  && x.ProductVariationPropertyCombination.All(y => newIds.Contains(y.MasterListValuesId)));
                var isOld = filteredData.Count() > 0;
                if (!isOld && item.ProductVariationPropertyCombination.Count() > 0)
                {
                    item.ProductId = productId;
                    item.Price = 0;
                    item.Quantity = 0;
                    item.IsDelete = false;
                    await _productVariationRepository.Add(item);
                    //await _unitOfWork.Commit();
                }
            }
            await _unitOfWork.Commit();
        }


        private async Task SaveMultiSelectProductProperty(int productId, ProductPropertyDTO item)
        {
            var arr = item.Value.Split(',');
            var existingproperty = _productPropertyRepository
                                .FindBy(x => x.ProductId == productId 
                                 && x.CategoryPropertyId == item.CategoryPropertyId);

            foreach (var productproperty in existingproperty)
            {
                var check = arr.FirstOrDefault(x => x == productproperty.Value);
                if (check != null) continue;
                _productPropertyRepository.Delete(productproperty);
            }


            foreach (var item1 in arr)
            {
                if (item1 == "0" || item1 == string.Empty) continue;
             
                var check = _productPropertyRepository
                        .FindBy(x => x.ProductId == productId
                        && x.CategoryPropertyId == item.CategoryPropertyId
                        && x.Value == item1)
                         .FirstOrDefault();
                if (check != null)
                {
                    check.Value = item1;
                }
                else
                {
                    item.Value = item1;
                    var mapper = _mapper.Map<ProductProperty>(item);
                    await _productPropertyRepository.Add(mapper);
                }
            }
        }

        private async Task SaveProductProperty_Backup(int productId, List<ProductPropertyDTO> productProperty)
        {
            var checkIfEmpty = productProperty.Where(x => x.Value == string.Empty);
            var product = _productRepository.GetById(productId);
            if (checkIfEmpty.Count() == productProperty.Count())
            {
                product.HasVariation = false;
                await DeleteIfEmpty(productId);
                return;
            }

            var checkempty = productProperty.Any(x => x.Value != string.Empty);            
            if (product != null)
            {
                if (checkempty)
                {
                    product.HasVariation = true;

                }
            }
            

            var list = new List<string>();
            var result = _productPropertyRepository.FindBy(x => x.ProductId == productId)
                        .Include(x => x.CategoryProperty)
                        .Include(x => x.CategoryProperty.MasterProperty).ToList();

            var fisrtListVariance = new List<string>(); var secondListVariance = new List<string>(); var finalListvariant = new List<string>();
            var singleListVariant = new List<string>();
            if (productProperty.Count(x => x.Value != string.Empty) == 1)
            {
                singleListVariant = productProperty.Where(x => x.Value != string.Empty ).Select(x => x.Value).ToList();
            }
            foreach (var item in productProperty)
            {
                var masterlistvalue = item.Value.Split(',').Distinct().ToList();
                if (masterlistvalue.FirstOrDefault() == string.Empty) continue;
                finalListvariant.AddRange(masterlistvalue);

                var checkType = _categoryPropertyRepository
                           .FindBy(x => x.Id == item.CategoryPropertyId)
                           .Include(x => x.MasterProperty).FirstOrDefault();

                if (checkType.MasterProperty.Type.ToLower() == MasterPropertyType.MultiSelectDropDown.ToString().ToLower() ||
                    checkType.MasterProperty.Type.ToLower() == MasterPropertyType.MultiSelectDropDownColor.ToString().ToLower())
                {

                    await SaveMultiSelect(productId, item, list);
                    if (!fisrtListVariance.Any()) { fisrtListVariance = masterlistvalue; }
                    else
                    {
                        secondListVariance = masterlistvalue;
                    }

                }


            }



            var records = productProperty.Count(x => x.Value != string.Empty);

            if (records == 1)
            {
                var proudctProperty = _productPropertyRepository
                                    .FindBy(x =>
                                    (x.CategoryProperty.MasterProperty.Type.ToLower()
                                    == MasterPropertyType.MultiSelectDropDown.ToString().ToLower())
                                    || (x.CategoryProperty.MasterProperty.Type.ToLower()
                                    == MasterPropertyType.MultiSelectDropDownColor.ToString().ToLower())
                                    && x.ProductId == productId).ToList();
                var requiredarray = singleListVariant.FirstOrDefault().Split(',');
                foreach (var item in proudctProperty)
                {
                    var checkTodelete = requiredarray.FirstOrDefault(x => x == Convert.ToString(item.Value));
                    if (checkTodelete == null)
                    {
                        _productPropertyRepository.Delete(item);
                    }
                }

                var check = DeleteIfVariationIsMorethanOne(productId);
                await SaveSingleVariance(productId, singleListVariant, check);
            }
            else
            {
                MultiSelectDelete(list, result);
                var productVarianceCheck = _productVariationRepository.FindBy(x => x.ProductId == productId)
                                        .Include(x => x.ProductVariationPropertyCombination);
                var previousSingleDate = false;
                if (productVarianceCheck != null)
                {
                    var check = productVarianceCheck.Select(x => x.ProductVariationPropertyCombination)
                                .Any(x => x.Count == 1);
                    if (check)
                    {
                        DeleteVariance(productVarianceCheck);
                        previousSingleDate = true;
                    }

                }
                if (!previousSingleDate)
                {
                    DeletePreviousMultiSelectedValues(productId, finalListvariant);
                }

                await SaveMulitSelectValues(productId, fisrtListVariance, secondListVariance);
            }



            await _unitOfWork.Commit();
        }

        private List<ProductVariation> GetCombination(List<ProductPropertyDTO> productProperty)
        {
            List<ProductVariation> result = new List<ProductVariation>();

            List<int> CategoryPropertyIds = productProperty.Select(x => x.CategoryPropertyId).ToList();
            var combinationCategoryProperties = Permutations(CategoryPropertyIds); 
            var filteredCombinationCategoryProperties = new List<int[]>().AsEnumerable();
            if (combinationCategoryProperties.ToList().Count(x => x.Length > 0) > 1)
            {
                 filteredCombinationCategoryProperties = combinationCategoryProperties.Where(x => x.Count() > 1);
            }
            else
            {
                filteredCombinationCategoryProperties = combinationCategoryProperties; //combinationCategoryProperties.Where(x => x.Count() > 1);
            }
             

            foreach (var item in filteredCombinationCategoryProperties)
            {
                List<int>[] arrNew = new List<int>[item.Count()];
                for (int count = 0; count < item.Count(); count++)
                {
                    arrNew[count] = new List<int>();
                    string propertyValuesString = productProperty.Where(x => x.CategoryPropertyId == item[count]).FirstOrDefault().Value;

                    if(string.IsNullOrEmpty(propertyValuesString) == false)
                    {
                        arrNew[count].AddRange(propertyValuesString.Split(',').Select(Int32.Parse));
                    }
                }
                var fullCombination = ArrayPermutations(arrNew);

                foreach (var singleCombination in fullCombination)
                {
                    List<ProductVariationPropertyCombination> listVariationDTO = new List<ProductVariationPropertyCombination>();
                    foreach (var singleProperty in singleCombination)
                    {
                        ProductVariationPropertyCombination singleVariationDTO = new ProductVariationPropertyCombination() {
                            MasterListValuesId = singleProperty
                        };
                        listVariationDTO.Add(singleVariationDTO);
                    }
                    result.Add(new ProductVariation()
                    {
                        ProductVariationPropertyCombination = listVariationDTO
                    });
                }
            }

            return result;
        }

        private static IEnumerable<T[]> Permutations<T>(IEnumerable<T> source)
        {
            if (null == source)
                throw new ArgumentNullException(nameof(source));

            T[] data = source.ToArray();

            return Enumerable
              .Range(0, 1 << (data.Length))
              .Select(index => data
                 .Where((v, i) => (index & (1 << i)) != 0)
                 .ToArray());
        }

        private static List<List<int>> ArrayPermutations(List<int>[] arr)
        {
            List<List<int>> combination = new List<List<int>>();

            // Number of arrays
            int n = arr.Length;

            // To keep track of next 
            // element in each of 
            // the n arrays
            int[] indices = new int[n];

            // Initialize with first 
            // element's index
            for (int i = 0; i < n; i++)
                indices[i] = 0;

            while (true)
            {
                List<int> innerCombination = new List<int>();
                // Print current combination
                for (int i = 0; i < n; i++)
                {
                    if(arr[i].Count() > 0)
                    {
                        innerCombination.Add(arr[i][indices[i]]);
                    }
                }

                combination.Add(innerCombination);
                // Find the rightmost array 
                // that has more elements 
                // left after the current 
                // element in that array
                int next = n - 1;
                while (next >= 0 &&
                    (indices[next] + 1 >=
                    arr[next].Count))
                    next--;

                // No such array is found 
                // so no more combinations left
                if (next < 0)
                    return combination;

                // If found move to next 
                // element in that array
                indices[next]++;

                // For all arrays to the right 
                // of this array current index 
                // again points to first element
                for (int i = next + 1; i < n; i++)
                    indices[i] = 0;
            }
        }

        private async Task DeleteIfEmpty(int productId)
        {
            DeleteAllVariance(productId);
            var proudctProperty = _productPropertyRepository
                                .FindBy(x =>
                                (x.CategoryProperty.MasterProperty.Type.ToLower()
                                == MasterPropertyType.MultiSelectDropDown.ToString().ToLower()
                                || x.CategoryProperty.MasterProperty.Type.ToLower()
                                    == MasterPropertyType.MultiSelectDropDownColor.ToString().ToLower())
                                && x.ProductId == productId).ToList();
            _productPropertyRepository.DeleteRange(proudctProperty);

            await _unitOfWork.Commit();
        }

        private bool DeleteIfVariationIsMorethanOne(int productId)
        {
            var productVarianceCheck = _productVariationRepository.FindBy(x => x.ProductId == productId)
                                        .Include(x => x.ProductVariationPropertyCombination);
            if (productVarianceCheck != null)
            {
                var check = productVarianceCheck.Select(x => x.ProductVariationPropertyCombination)
                            .Any(x => x.Count > 1);
                if (check)
                {
                    DeleteVariance(productVarianceCheck);
                }
                return check;
            }
            return false;
        }


        private void DeleteAllVariance(int productId)
        {
            var productVarianceCheck = _productVariationRepository.FindBy(x => x.ProductId == productId)
                                        .Include(x => x.ProductVariationPropertyCombination);

            DeleteVariance(productVarianceCheck);

        }

        private async Task SaveSingleVariance(int productId, List<string> singleSelectArray, bool ispreviousMultiple)
        {
            //var singleVariance = productProperty.Where(x => x.Value != string.Empty);
            var product = await _productRepository.GetSingle(productId);
            //var values = singleVariance.FirstOrDefault().Value;
            var checkSingleCombination = _productVariationPropertyCombinationRepository.
                                      FindBy(x => x.ProductVariation.ProductId == productId).ToList();
            singleSelectArray = singleSelectArray.FirstOrDefault().Split(',').Distinct().ToList();
            if (!ispreviousMultiple)
            {


                foreach (var item in checkSingleCombination)
                {
                    var checkToDelete = singleSelectArray.Any(x => x == item.MasterListValuesId.ToString());
                    if (!checkToDelete)
                    {
                        _productVariationPropertyCombinationRepository.Delete(item);
                    }
                }
            }


            foreach (var item in singleSelectArray)
            {

                if (checkSingleCombination != null && checkSingleCombination.Any() && !ispreviousMultiple)
                {
                    //var listvalueId = Convert.ToInt32(item);
                    var checkAlreadyExist = checkSingleCombination.FirstOrDefault(x => x.MasterListValuesId == Convert.ToInt32(item));
                    if (checkAlreadyExist != null) continue;
                }
                var combination = AddProductCombination(productId, product);
                combination.ProductVariationPropertyCombination = new List<ProductVariationPropertyCombination>();
                var ProductPropertyCombination = new ProductVariationPropertyCombination
                {
                    MasterListValuesId = Convert.ToInt32(item)
                };
                combination.ProductVariationPropertyCombination.Add(ProductPropertyCombination);
            }
        }

        private void DeletePreviousMultiSelectedValues(int productId, List<string> finalListvariant)
        {
            var productCombinationResult = _productVariationRepository
                                            .FindBy(x => x.ProductId == productId).ToList();
            foreach (var item in productCombinationResult)
            {
                var productPropertyCombination = _productVariationPropertyCombinationRepository
                                                        .FindBy(x => x.ProductVariationId == item.Id)
                                                        .ToList();
                var isdeletetrue = false;
                var masterlistvalueId = productPropertyCombination.Select(x => x.MasterListValuesId.ToString());
                var check = masterlistvalueId.Where(x => !finalListvariant.Contains(x));

                if (check.FirstOrDefault() != null)
                {
                    _productVariationPropertyCombinationRepository.DeleteRange(productPropertyCombination);
                    isdeletetrue = true;
                }


                if (isdeletetrue)
                {

                    _productVariationRepository.Delete(item);
                    var checkImage = _productVariationImageRepository.FindBy(x => x.ProductVariationId == item.Id);
                    foreach (var images in checkImage)
                    {
                        _productVariationImageRepository.Delete(images);
                    }
                }

            }
        }

        private void DeleteVariance(Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ProductVariation, ICollection<ProductVariationPropertyCombination>> productVarianceCheck)
        {
            foreach (var item in productVarianceCheck.ToList())
            {
                var images = _productVariationImageRepository.FindBy(x => x.ProductVariationId == item.Id).ToList();
                _productVariationImageRepository.DeleteRange(images);

                var materlistValues = _productVariationPropertyCombinationRepository.FindBy(x => x.ProductVariationId == item.Id).ToList();
                _productVariationPropertyCombinationRepository.DeleteRange(materlistValues);

                _productVariationRepository.Delete(item);
            }
        }

        private async Task SaveMulitSelectValues(int productId, List<string> fisrtListVariance, List<string> secondListVariance)
        {
            var product = await _productRepository.GetSingle(productId);
            var dbProductVariationPropertyCombination = _productVariationPropertyCombinationRepository
                                                .FindBy(x => x.ProductVariation.ProductId == productId).ToList().
                                                GroupBy(x => x.ProductVariationId)
                                               .Select(obj =>
                                               new { key = obj.Key, Combination = obj });


            foreach (var first in fisrtListVariance)
            {
                foreach (var second in secondListVariance)
                {
                    var firstId = Convert.ToInt32(first);
                    var secondId = Convert.ToInt32(second);


                    var flag = true;
                    foreach (var item in dbProductVariationPropertyCombination)
                    {
                        var MasterListValuesId = item.Combination.Select(x => x.MasterListValuesId).ToList();
                        if (MasterListValuesId.Any(x => x == firstId) && MasterListValuesId.Any(x => x == secondId))
                        {
                            flag = false; break;
                        }
                    }



                    ////var list = new List<int> { Convert.ToInt32(first), Convert.ToInt32(second) };
                    //var check = _productVariationPropertyCombinationRepository
                    //    .FindBy(x => x.MasterListValuesId==firstId && x.MasterListValuesId==secondId
                    //    && x.ProductVariation.ProductId==productId).FirstOrDefault();

                    //var checksecond = _productVariationPropertyCombinationRepository
                    //    .FindBy(x => (x.MasterListValuesId == secondId || x.MasterListValuesId == firstId)
                    //    && x.ProductVariation.ProductId == productId);



                    if (flag == true)
                    {
                        var combination = AddProductCombination(productId, product);

                        combination.ProductVariationPropertyCombination = new List<ProductVariationPropertyCombination>();
                        var ProductPropertyCombination = new ProductVariationPropertyCombination
                        {
                            MasterListValuesId = Convert.ToInt32(first)
                        };
                        combination.ProductVariationPropertyCombination.Add(ProductPropertyCombination);

                        var ProductPropertyCombinationSecond = new ProductVariationPropertyCombination
                        {
                            MasterListValuesId = Convert.ToInt32(second)
                        };
                        combination.ProductVariationPropertyCombination.Add(ProductPropertyCombinationSecond);

                    }
                }
            }
        }

        public async Task SaveProductPropertyWithoutMultiSelect(int productId, List<ProductPropertyDTO> productProperty)
        {
            var result = _productPropertyRepository.FindBy(x => x.ProductId == productId)
                       .Include(x => x.CategoryProperty)
                       .Include(x => x.CategoryProperty.MasterProperty).ToList();
            foreach (var item in productProperty)
            {
                await SaveOtherThanMultiSelect(productId, item);
                foreach (var item1 in result)
                {
                    if (item1.CategoryProperty.MasterProperty.Type != MasterPropertyType.MultiSelectDropDown.ToString().ToLower()
                        || item1.CategoryProperty.MasterProperty.Type != MasterPropertyType.MultiSelectDropDownColor.ToString().ToLower())
                    {
                        var checkfordelete = productProperty
                                        .FirstOrDefault(x => x.ProductId == productId
                                          && x.CategoryPropertyId == item.CategoryPropertyId);
                        if (checkfordelete == null) _productPropertyRepository.Delete(item1);
                    }
                }
            }

            await _unitOfWork.Commit();

        }

        private ProductVariation AddProductCombination(int productId, Product product)
        {
            var productcombination = new ProductVariation
            {
                ProductId = productId,
                Price = product.Price,
                Quantity = 0
            };
            _productVariationRepository.Add(productcombination);

            return productcombination;
        }

        private void MultiSelectDelete(List<string> list, List<ProductProperty> result)
        {
            var CategoryPropertyId = result.Select(x => x.CategoryPropertyId);

            var multiselectDelete = _productPropertyRepository
                                   .FindBy(x => CategoryPropertyId.Contains(x.CategoryPropertyId)
                                    && !list.Contains(x.Value)
                                    && (x.CategoryProperty.MasterProperty.Type.ToLower()
                                         == MasterPropertyType.MultiSelectDropDown.ToString().ToLower()
                                         || x.CategoryProperty.MasterProperty.Type.ToLower()
                                         == MasterPropertyType.MultiSelectDropDownColor.ToString().ToLower())).ToList();

            if (multiselectDelete.Any()) _productPropertyRepository.DeleteRange(multiselectDelete);
        }

        private async Task SaveMultiSelect(int productId, ProductPropertyDTO item, List<string> valuelist)
        {

            foreach (var item1 in item.Value.Split(','))
            {
                if (item1 == "0" || item1 == string.Empty) continue;
                valuelist.Add(item1);
                var check = _productPropertyRepository
                        .FindBy(x => x.ProductId == productId
                        && x.CategoryPropertyId == item.CategoryPropertyId
                        && x.Value == item1)
                         .FirstOrDefault();
                if (check != null)
                {
                    check.Value = item1;
                }
                else
                {
                    item.Value = item1;
                    var mapper = _mapper.Map<ProductProperty>(item);
                    await _productPropertyRepository.Add(mapper);
                }
            }
        }

        private async Task SaveOtherThanMultiSelect(int productId, ProductPropertyDTO item)
        {
            var check = _productPropertyRepository
                   .FindBy(x => x.ProductId == productId && x.CategoryPropertyId == item.CategoryPropertyId)
                    .FirstOrDefault();
            if (check != null)
            {
                check.Value = item.Value;
            }
            else
            {
                var mapper = _mapper.Map<ProductProperty>(item);
                await _productPropertyRepository.Add(mapper);
            }
        }

        public async Task<List<ProductVariationPropertyCombinationDTO>> GetProductCombination(int productId)
        {
            var productCombination = _productVariationPropertyCombinationRepository
                                    .FindBy(x => x.ProductVariation.ProductId == productId 
                                    && (x.ProductVariation.IsDelete==null || x.ProductVariation.IsDelete==false))
                                    .Include(x => x.ProductVariation).ToList()
                                    .GroupBy(x => x.ProductVariationId,
                                    (key, g) =>
                                    new ProductVariationPropertyCombinationDTO
                                    {
                                        ProductVariationId = key,
                                        ProductVariation = _mapper.Map<ProductVariationDTO>(g.FirstOrDefault().ProductVariation),
                                        ProductVariationProperty =
                                         g.Select(y => new ProductVariationPropertyCombinationDTO
                                         {
                                             MasterListValuesId = y.MasterListValuesId,
                                             ProductVariationId = y.ProductVariationId,

                                         }).ToList()

                                    }).ToList();

            foreach (var item in productCombination)
            {

                var materlistValue = item.ProductVariationProperty.Select(x => x.MasterListValuesId);
                var masterListValue = _masterListValueRepository.All.Where(x => materlistValue.Contains(x.Id));
                item.MasterListValueCombination = string.Join(",", masterListValue.Select(x => x.OptionLabel));
                item.NumberOfImages = _productVariationImageRepository.FindBy(x => x.ProductVariationId == item.ProductVariationId).Count();
                //list.Add(materListValueDTO);
            }

            return productCombination.ToList();
        }

        public async Task<ProductVariantDTO> ProductPropert(int proudctId)
        {
            var property = _productPropertyRepository
                        .FindBy(x => x.ProductId == proudctId && (x.IsDelete==false || x.IsDelete==null))
                        .Include(x => x.CategoryProperty).ToList();

            var result = property.GroupBy(x => x.CategoryProperty.MasterPropertyId
                          , (key, g) => new { key, g }).OrderBy(x => x.key);
            var productvariant = new ProductVariantDTO() { Variant = new List<ProductVariationMasterPropertyDTO>() };
            foreach (var item in result)
            {
                var variance = await _masterPropertyRepository.GetSingle(item.key);
                if (variance != null)
                {

                    if (variance.Type.ToLower() == MasterPropertyType.MultiSelectDropDown.ToString().ToLower()
                        || variance.Type.ToLower() == MasterPropertyType.MultiSelectDropDownColor.ToString().ToLower())
                    {
                        var listId = item.g.FirstOrDefault().CategoryProperty.MasterListId;
                        var values = item.g.Select(x => int.Parse(x.Value));
                        var variancekey = _materListRepository.FindBy(x => x.Id == listId).FirstOrDefault();
                        var list = new List<MasterListValueDTO>();
                        int count = 0;
                        Int64 defaultPropertyId = 0;
                        foreach (var value in values)
                        {
                            var dropdownValues = _masterListValueRepository
                                   .FindBy(x => x.MasterListId == listId && x.Id == value)
                                   .ToList();
                            var mapper = _mapper.Map<List<MasterListValueDTO>>(dropdownValues);
                            foreach (var item1 in mapper)
                            {
                                item1.Type = variance.Type;
                                item1.IsSelected = count == 0 ? true : false;
                                if(count == 0)
                                {
                                    defaultPropertyId = item1.Id;
                                }
                            }
                            list.AddRange(mapper);
                            count++;
                        }
                        productvariant.Variant.Add(new ProductVariationMasterPropertyDTO() { 
                            Id = variancekey.Id,
                            Name = variancekey.Name, 
                            DefaultPropertyId = defaultPropertyId,
                            Properties = list });

                        //var dropdownValues = _masterListValueRepository
                        //            .FindBy(x => x.MasterListId == listId && values.Contains(x.Id))
                        //            .Select(x => x.OptionLabel).ToList();
                        //not checking firstOrDefault as it should always value;

                    }
                    else if (variance.Type.ToLower() == MasterPropertyType.Text.ToString().ToLower())
                    {
                        //var variancevalue=item.g.Select(x => x.Value);                    
                        //productvariant.Variant.Add(variance.Name, variancevalue);
                    }
                }
            }

            return productvariant;


        }

        public async Task<List<ProductVariantListDTO>> ProductProperties(int productId)
        {
            List<ProductVariantListDTO> productVariations = new List<ProductVariantListDTO>();
            return productVariations;
        }
        public async Task SaveCombinationImage(ProductVariationImageDTO variationImage)
        {
            var proudctVariationImage = _productVariationImageRepository
                                        .FindBy(x => x.ProductVariationId == variationImage.ProductVariationId);

            foreach (var item in proudctVariationImage)
            {
                var check = variationImage.ImageId.Any(x => x == item.ProductImageId);
                if (!check) { _productVariationImageRepository.Delete(item); }
            }
            foreach (var item in variationImage.ImageId)
            {
                var dbCheck = proudctVariationImage.FirstOrDefault(x => x.ProductImageId == item);
                if (dbCheck != null) continue;
                var productVariationImage = new ProductVariationImage
                {
                    ProductImageId = item,
                    ProductVariationId = variationImage.ProductVariationId,
                };

                await _productVariationImageRepository.Add(productVariationImage);
            }
            await _unitOfWork.Commit();
        }

        public List<ProductImage> GetProductImages(int productId)
        {
            var image = _productImageRepository.FindBy(x => x.ProductId == productId && (x.IsDelete == null || x.IsDelete == false));
            return image.ToList();
        }

        public List<ProductVideo> GetProductVideos(int productId)
        {
            var image = _productVideoRepository.FindBy(x => x.ProductId == productId && (x.IsDelete == null || x.IsDelete == false));
            return image.ToList();
        }

        public async Task<List<ProductImageDTO>> GetProductVariantImage(int productId)
        {
            var image = _productImageRepository.FindBy(x => x.ProductId == productId && (x.IsDelete==null|| x.IsDelete==false));
            var mapper = _mapper.ProjectTo<ProductImageDTO>(image);
            return await mapper.ToListAsync();
        }

        public async Task PatchProductImage(ProductImageDTO2 productimagedto)
        {
            var images = this._productImageRepository.FindBy(p => p.ProductId == productimagedto.ProductId).ToList();
            foreach (var image in images)
            {
                if (image.Id == productimagedto.Id)
                {
                    image.IsPrimary = true;
                }
                else
                {
                    image.IsPrimary = false;
                }
                await _unitOfWork.Commit();
            }
        }
        public async Task PatchProductVideo(ProductVideoDTO2 productvideodto)
        {
            var images = _productVideoRepository.FindBy(p => p.ProductId == productvideodto.ProductId).ToList();
            foreach (var video in images)
            {
                if (video.Id == productvideodto.Id)
                {
                    video.IsPrimary = true;
                }
                else
                {
                    video.IsPrimary = false;
                }
                await _unitOfWork.Commit();
            }
        }

        #region reward
        public IEnumerable<SelectListItem> GetReward()
        {
            var reward = _rewardTypeRepository.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return reward;

        }

        public async Task SaveReward(List<RewardDetailDTO> rewardDetail, int productId)
        {
            var reward = _mapper.Map<List<RewardDetail>>(rewardDetail);
            foreach (var item in reward)
            {
                item.StartDate = DateTime.Now;
                item.EndDate = DateTime.Now.AddYears(1);
                item.Status = 1;
                item.CategoryId = 0;
                if (item.Id > 0)
                {
                    var check = await _rewardDetialRepository.GetSingle(item.Id);
                    check.ProductId = item.ProductId;
                    check.RewardTypeId = item.RewardTypeId;
                    check.Value = item.Value;
                }
                else
                {
                    await _rewardDetialRepository.Add(item);
                }
            }

            var rewardPreviousvalue = _rewardDetialRepository.FindBy(x => x.ProductId == productId);
            foreach (var item in rewardPreviousvalue)
            {
                var check = rewardDetail.FirstOrDefault(x => x.Id == item.Id);
                if (check == null)
                {
                    _rewardDetialRepository.Delete(item);
                }
            }

            await _unitOfWork.Commit();
        }

        public async Task<List<RewardDetailDTO>> GetRewardDetailsByProductId(int productId)
        {
            var result = _rewardDetialRepository.FindBy(x => x.ProductId == productId && (x.IsDelete == null || x.IsDelete == false)).ToList();
            var mapper = _mapper.Map<List<RewardDetailDTO>>(result);
            return mapper;

        }
        #endregion

        #region VoucherDetail
        public IEnumerable<SelectListItem> GetVoucherDetails()
        {
            //var reward = _voucherDetailRepository.GetAll().OrderBy(x=>x.VoucherCode)
            //              .Select(x => new SelectListItem
            //{
            //    Text =x.Description+"("+x.VoucherCode+")",
            //    Value = x.Id.ToString()
            //});
            var offer = _offerDetailRepository.GetAll(x => x.Method == (int)Enums.Method.SubjectVoucher)
                            .Select(x => new SelectListItem {
                                Text = x.OfferName,
                                Value = x.Id.ToString()
                            });
            return offer;

        }
        public IEnumerable<SelectListItem> GetVoucherDetailsByCategoryId(int categoryid)
        {
          
            var offer = _offerDetailRepository.GetAll(x => x.Method == (int)Enums.Method.SubjectVoucher).Where(x=>x.parentCategory== categoryid)
                            .Select(x => new SelectListItem
                            {
                                Text = x.OfferName,
                                Value = x.Id.ToString()
                            });
            return offer;

        }
        public IEnumerable<SelectListItem> GetVoucherbynull()
        {

            var offer = _offerDetailRepository.GetAll(x => x.Method == (int)Enums.Method.SubjectVoucher).Where(x => x.parentCategory == 0)
                            .Select(x => new SelectListItem
                            {
                                Text = x.OfferName,
                                Value = x.Id.ToString()
                            });
            if (offer == null)
            {
                return null;
            }
            return offer;

        }

        public async Task SaveVoucherDetails(List<CategoryProductVoucherDTO> productVoucher, int productId)
        {
            var voucher = _mapper.Map<List<CategoryProductVoucher>>(productVoucher);
            foreach (var item in voucher)
            {

                item.CategoryId = 0;
                if (item.Id > 0)
                {
                    var check = await _categoryProductVoucherRepository.GetSingle(item.Id);
                    check.ProductId = productId;
                    check.OfferDetailsId = item.OfferDetailsId;

                }
                else
                {
                    item.ProductId = productId;
                    await _categoryProductVoucherRepository.Add(item);
                }
            }

            var previousValues = _categoryProductVoucherRepository.FindBy(x => x.ProductId == productId);
            foreach (var item in previousValues)
            {
                var check = productVoucher.FirstOrDefault(x => x.Id == item.Id);
                if (check == null)
                {
                    _categoryProductVoucherRepository.Delete(item);
                }
            }

            await _unitOfWork.Commit();
        }

        public List<CategoryProductVoucherDTO> GetProductVoucherByProductId(int productId)
        {
            var result = _categoryProductVoucherRepository.FindBy(x => x.ProductId == productId && (x.IsDelete == null || x.IsDelete == false)).ToList();
            var mapper = _mapper.Map<List<CategoryProductVoucherDTO>>(result);
            return mapper;

        }

        #endregion

        #region CouponDetails
        public async Task SaveCouponDetails(List<CategoryProductCouponDTO> productCoupon, int productId)
        {
            var voucher = _mapper.Map<List<CategoryProductCoupon>>(productCoupon);
            foreach (var item in voucher)
            {
                item.CategoryId = 0;
                if (item.Id > 0)
                {
                    var check = await _categoryProductCouponRepository.GetSingle(item.Id);
                    check.ProductId = productId;
                    check.OfferDetailsId = item.OfferDetailsId;
                }
                else
                {
                    item.ProductId = productId;
                    await _categoryProductCouponRepository.Add(item);
                }
            }

            var previousValues = _categoryProductCouponRepository.FindBy(x => x.ProductId == productId);
            foreach (var item in previousValues)
            {
                var check = productCoupon.FirstOrDefault(x => x.Id == item.Id);
                if (check == null)
                {
                    _categoryProductCouponRepository.Delete(item);
                }
            }

            await _unitOfWork.Commit();
        }

        public List<CategoryProductCouponDTO> GetProductCouponByProductId(int productId)
        {
            var result = _categoryProductCouponRepository.FindBy(x => x.ProductId == productId && (x.IsDelete == null || x.IsDelete == false)).ToList();
            var mapper = _mapper.Map<List<CategoryProductCouponDTO>>(result);
            return mapper;

        }
        #endregion

        public void CheckPriceforVarients(ClientHomeCommonDTO product)
        {
            if (product.HasVariation)
            {
                var max = default(decimal);
                var min = default(decimal);
                var checkflashdeal = _flashDealProductRepository.All.Where(x => x.ProductId == product.ProductId && x.IsDeleted != true && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now);
                if (checkflashdeal.FirstOrDefault() != null)
                {            
                    foreach (var flashdeal in checkflashdeal)
                    {
                        var discountprice = flashdeal.DiscountedPrice ?? 0;
                        var price = flashdeal.Price ?? 0;
                        if (discountprice == 0)
                        {
                            discountprice = price;
                        }
                        var finalprice = discountprice;
                        var finalresultdiscountpercentange = (price > 0 && discountprice > 0) ? ((price - discountprice) / price * 100) : 0;

                        if (min > finalprice || min == 0)
                        {
                            min = finalprice;
                        }
                        if (max < finalprice || max == 0)
                        {
                            max = finalprice;
                        }


                    }
                    product.MaxPrice = max;
                    product.MinPrice = min;

                    if (checkflashdeal.Any(x => x.DiscountedPrice != null || x.DiscountedPrice > 0))
                    {
                        //product.MaxPrice = checkflashdeal.Max(x => x.DiscountedPrice) ?? 0;
                        //product.MinPrice = checkflashdeal.Min(x => x.DiscountedPrice) ?? 0;

                        var maxprice = checkflashdeal.Max(x => x.Price) ?? 0;
                        var minprice = checkflashdeal.Min(x => x.Price) ?? 0;
                        if (maxprice > minprice)
                        {
                            product.PriceWithoutDiscount = minprice.ToString(".##") + "-" + maxprice.ToString(".##");
                        }
                        else
                        {
                            product.PriceWithoutDiscount = maxprice == 0 ? string.Empty : string.Format("{0:.##}", maxprice);
                        }

                    }
                    //else
                    //{
                    //    product.MaxPrice = checkflashdeal.Max(x => x.Price) ?? 0;
                    //    product.MinPrice = checkflashdeal.Min(x => x.Price) ?? 0;
                    //}

                }
                else
                {
                    var productvariation = _productVariationRepository.FindBy(x => x.ProductId == product.ProductId);
                    if (productvariation.FirstOrDefault() != null)
                    {
                        foreach (var variation in productvariation)
                        {
                            var discountprice = variation.DiscountedPrice ?? 0;
                            var price = variation.Price;
                            if (discountprice == 0)
                            {
                                discountprice = price;
                            }
                            var finalprice = discountprice;
                            var finalresultdiscountpercentange = (price > 0 && discountprice > 0) ? ((price - discountprice) / price * 100) : 0;

                            if (min > finalprice || min == 0)
                            {
                                min = finalprice;
                            }
                            if (max < finalprice || max == 0)
                            {
                                max = finalprice;
                            }
                        }
                        product.MaxPrice = max;
                        product.MinPrice = min;
                        if (productvariation.FirstOrDefault().DiscountedPrice != null || productvariation.FirstOrDefault().DiscountedPrice > 0)
                        {
                            //product.MaxPrice = productvariation.Max(x => x.DiscountedPrice);
                            //product.MinPrice = productvariation.Max(x => x.DiscountedPrice);

                            var maxprice = productvariation.Max(x => x.Price);
                            var minprice = productvariation.Min(x => x.Price);
                            if (maxprice > minprice)
                            {
                                product.PriceWithoutDiscount = minprice.ToString(".##") + "-" + maxprice.ToString(".##");
                            }
                            else
                            {
                                product.PriceWithoutDiscount = maxprice == 0 ? string.Empty : string.Format("{0:.##}", maxprice);
                            }

                        }
                        //else
                        //{
                        //    product.MaxPrice = productvariation.Max(x => x.Price);
                        //    product.MinPrice = productvariation.Max(x => x.Price);

                        //}

                    }
                }
            }
            else
            {
                var checkflashdeal = _flashDealProductRepository.All.FirstOrDefault(x => x.ProductId == product.ProductId && x.IsDeleted != true && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now);
                if (checkflashdeal != null)
                {
                    product.Price = checkflashdeal.Price ?? 0;
                    product.Discount= checkflashdeal.DiscountedPrice ?? 0;
                }
            }
        }

        #region affiliate
        public async Task<QueryResult<ProductDTO>>  FilterProduct(AffiliateLinkQueryObject queryObject, long userId)
        {

            var columnMap = new Dictionary<string, Expression<Func<Product, object>>>()
            {
                ["Name"] = p => p.Name,
                
            };

            var columnMapDecimal = new Dictionary<string, Expression<Func<Product, decimal?>>>()
            {
              
                ["Price"] = p => p.Price,
                ["AffiliateCommission"] = p => p.AffiliateCommission,
            };

            if (string.IsNullOrEmpty(queryObject.SortBy))
            {
                queryObject.SortBy = "Name";
            }

            queryObject.ProductName = queryObject.ProductName ?? "";
            var result = _productRepository.All
                          //.FindBy(x => x.ProductCategory.Any(y => y.CategoryId == categoryId))
                          //.Include(x => x.ProductImage)
                          .Include(x => x.AffilateLink)
                          .Where(x => x.Name.Trim().ToLower().Contains(queryObject.ProductName.Trim().ToLower()) 
                          && (x.AffiliateCommission!=null||x.AffiliateCommission>0));

            if (queryObject.CategoryId > 0)
            {
                result = result.Where(x => x.ProductCategory.Any(y => y.CategoryId == queryObject.CategoryId));
            }
            var totalcount = await result.CountAsync();

            if (columnMapDecimal.ContainsKey(queryObject.SortBy))
            {
                result = result.ApplyOrderingDecimal(queryObject, columnMapDecimal);
            }
            else
            {
                result = result.ApplyOrdering(queryObject, columnMap);
            }

            

            foreach (var item in result)
            {
                if (item.AffilateLink != null && item.AffilateLink.Any())
                {
                    item.AffilateLink = item.AffilateLink.Where(x => x.AffilateUserId == userId).ToList();
                }
            }

            //result = result.ToList();

            var product = _mapper.Map<List<ProductDTO>>(result.ToList());
            foreach (var item in product)
            {
                var affliate = result.FirstOrDefault(x => x.Id == item.Id);
                if (affliate != null)
                {
                    var productimage = _productImageRepository.FindBy(x => x.ProductId == item.Id
                                     && (x.IsDelete == null || x.IsDelete == false) && x.IsPrimary)
                                     .FirstOrDefault();
                    item.ProductImage = productimage == null ? "" : productimage.ImagePath;
                }
                    

                if (affliate.AffilateLink != null && affliate.AffilateLink.Any())
                {
                    item.AffiliateLink = _mapper.Map<AffilateLinkDTO>(affliate.AffilateLink.FirstOrDefault());
                }

            }

            var queryResult = new QueryResult<ProductDTO>
            {
                TotalItems = totalcount, //affiliate.Count(),
                Items = product
            };
            return queryResult;
        }


        public async Task<bool> IsProductBoughtWhileCheckOut(int productId, int packageId,int contentId, long userid)
        {
            try
            {
                if (productId == 0 && packageId == 0) return false;
                List<OrderDetails> result;

                if (packageId != 0)
                {
                    result = _orderDetailsRepository.GetAll(x => x.Order.CreatedBy == userid && x.PackageId == packageId && x.OrderStatusId == 11).ToList();
                    if (result.Count() > 0) return true;
                    return false;
                }
                else if(contentId != 0)
                {
                    result = _orderDetailsRepository.GetAll(x => x.Order.CreatedBy == userid && x.ProductId == productId && x.ContentId == contentId && x.OrderStatusId == 11).ToList();
                    if (result.Count() > 0) return true;
                    return false;
                }
                else
                {
                    result = _orderDetailsRepository.GetAll(x => x.Order.CreatedBy == userid && x.ProductId == productId && (x.ContentId == null || x.ContentId ==0)  && x.OrderStatusId == 11).ToList();
                    if (result.Count() > 0) return true;
                    return false;
                }
                return false;
            }
            catch(Exception ex)
            {
                throw ex;
            }

         
        }


        public async Task<ProductDTO> AffiliateProductByProductId(int productId)
        {
            var result = await _productRepository.All
                          .Include(x => x.ProductImage)                                               
                          .FirstOrDefaultAsync(x => x.Id == productId);           

            var product = _mapper.Map<ProductDTO>(result);         


            if (result.ProductImage != null && result.ProductImage.Any())
                product.ProductImage = result.ProductImage.FirstOrDefault(x => x.IsPrimary).ImagePath;

            return product;
        }

        #endregion

        #region ClientProduct
        public async Task<List<ProductDTO>> ProductByCategoryId(int categoryId)
        {
            var product = await _productRepository
                          .FindBy(x => x.ProductCategory.Any(y => y.CategoryId == categoryId))
                          .Include(x => x.ProductImage)
                          .ToListAsync();
            var mapper = _mapper.Map<List<ProductDTO>>(product);
            foreach (var item in mapper)
            {
                var productlist = product.FirstOrDefault(x => x.Id == item.Id);
                if (productlist.ProductImage.FirstOrDefault() != null)
                {
                    var productImage = productlist.ProductImage.FirstOrDefault(x => x.IsPrimary);
                    if (productImage != null)
                    {
                        item.ProductImage = productImage.ImagePath;
                    }
                    else
                    {
                        item.ProductImage = productlist.ProductImage.FirstOrDefault().ImagePath;
                    }
                }
                else
                {
                    item.ProductImage = string.Empty;
                }



            }
            return mapper;
        }
        #endregion

        #region ProductPrice
        public async Task SaveProductPriceList(ProductPriceListDTO productpriceDTO)
        {
            var productPrice = _mapper.Map<ProductPriceList>(productpriceDTO);
            await _productPriceListRepository.Add(productPrice);
            await _unitOfWork.Commit();
        }

        public async Task UpdateProductPriceList(ProductPriceListDTO productpriceDTO)
        {
            var productPrice = await _productPriceListRepository.GetSingle(productpriceDTO.Id);
            if (productPrice != null)
            {
                productPrice.Title = productpriceDTO.Title;
                productPrice.Description = productpriceDTO.Description;
                productPrice.Comment = productpriceDTO.Comment;
                productPrice.StartDate = productpriceDTO.StartDate;
                productPrice.EndDate = productpriceDTO.EndDate;
                productPrice.IsWeb = productpriceDTO.IsWeb;
                productPrice.IsMobile = productpriceDTO.IsMobile;
                productPrice.Status = productpriceDTO.Status;
                await _unitOfWork.Commit();
            }
        }

        public async Task UpdateProductPriceListStatus(int id, bool isActive)
        {
            var productPrice = await _productPriceListRepository.GetSingle(id);
            if (productPrice != null)
            {
                productPrice.Status = isActive;
                await _unitOfWork.Commit();
            }
        }

        //public async Task ProductPriceListDetail(ProductPriceListDetailDTO productPriceDeatils)
        //{

        //}

        #endregion

        #region SearchProduct
        public async Task<FinalSearchResult> SearchProduct(ProductFilterSearchDTO product, string connectionString)
        {
            
            var str = string.Empty;
            var data = (JObject)JsonConvert.DeserializeObject(product.MasterList);
            var varianceresult = data.ToObject<Dictionary<int, List<string>>>();

            var textdata= (JObject)JsonConvert.DeserializeObject(product.MasterListText);
            var varienttextresult = textdata.ToObject<Dictionary<int, List<string>>>();
            //varianceresult = varianceresult.Where(x => x.Value.Count > 0);
            var i = 0;
            var strText = string.Empty;
            var multiselectQuery = string.Empty;
            var textQuery = string.Empty;
            

            if (varienttextresult.Keys.Any())
            {
                textQuery = @"SELECT DISTINCT P.Id as Id
		                FROM   [dbo].[Product] P			
		                INNER JOIN [dbo].[ProductProperty]PP on p.Id=pp.ProductId
		                INNER JOIN [dbo].[CategoryProperty] CP
		                ON PP.CategoryPropertyId= CP.Id
		                INNER JOIN (SELECT Id,[Name],[Type] FROM [dbo].[MasterProperties] WHERE TYPE='Text' OR Type='Numeric') MP
		                ON CP.MasterPropertyId=MP.Id
                        WHERE  P.StatusId=20";
                foreach (var variancetext in varienttextresult)
                {
                    var variancevalue = variancetext.Value.ToList();                    
                    var requiredvalues = string.Join("','", variancevalue.ToArray());
                    requiredvalues = "'" + requiredvalues + "'";
                    str = str + textQuery + " AND (pp.Value in (" + requiredvalues + ") AND MP.Id="+ variancetext.Key + ")";                    

                    if (i < varienttextresult.Keys.Count - 1)
                    {
                        str += " INTERSECT ";
                    }
                    i++;
                }
            }

            

            if (varianceresult.Keys.Any())
            {
                if (varienttextresult.Keys.Any())
                {
                    str += " INTERSECT ";
                }
                    multiselectQuery = @"SELECT distinct  p.Id   
                                   FROM[dbo].[Product] P
                                   INNER JOIN[dbo].[ProductProperty]PP on p.Id = pp.ProductId
                                   INNER JOIN[dbo].[CategoryProperty]CP
                                   ON PP.CategoryPropertyId= CP.Id
                                   INNER JOIN(SELECT Id, [Name], [Type] FROM[dbo].[MasterProperties] WHERE TYPE = 'MultiSelectDropDown' 
                                    or Type = 'MultiSelectDropDownColor' or Type='DropDown') MP
                                   ON CP.MasterPropertyId=MP.Id
                                   INNER JOIN[dbo].[MasterLists]ML                            
                                   ON ML.Id = Cp.MasterListId
                                   INNER JOIN[dbo].[MasterListValues]  MLV                           
                                  ON MLV.Id= pp.Value
                                  WHERE  P.StatusId=20";

                i = 0;
                foreach (var kat in varianceresult)
                {
                    var variancevalue = kat.Value.ToList();

                    if (kat.Key != 0)
                    {
                        var requiredvalues = string.Join("','", variancevalue.ToArray());
                        requiredvalues = "'" + requiredvalues + "'";
                        str = str + multiselectQuery + " AND (pp.Value in (" + requiredvalues + ") AND ML.Id =" + kat.Key + ")";

                    }                    

                    if (i < varianceresult.Keys.Count - 1)
                    {
                        str += " INTERSECT ";
                    }
                    i++;




                }


            }



       

            System.Data.SqlClient.SqlParameter[] spParameter = new System.Data.SqlClient.SqlParameter[16];
            spParameter[0] = new System.Data.SqlClient.SqlParameter("@keyword", SqlDbType.VarChar, 4000)
            {
                Direction = ParameterDirection.Input,
                Value = product.Keyword ?? string.Empty,
            };

            spParameter[1] = new System.Data.SqlClient.SqlParameter("@CategoryId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Input,
                Value = product.CategoryId
            };

            spParameter[2] = new System.Data.SqlClient.SqlParameter("@VarianceValue", SqlDbType.NVarChar)
            {
                Direction = ParameterDirection.Input,
                //Value = product.ProductVariance ?? ""
                Value = str
            };

          

            spParameter[3] = new System.Data.SqlClient.SqlParameter("@BrandValue", SqlDbType.VarChar)
            {
                Direction = ParameterDirection.Input,
                Value = product.BrandValue ?? ""
            };

            spParameter[4] = new System.Data.SqlClient.SqlParameter("@PriceDirection", SqlDbType.VarChar)
            {
                Value = product.PriceDirection ?? ""
            };

            spParameter[5] = new System.Data.SqlClient.SqlParameter("@MinPrice", SqlDbType.Decimal)
            {
                Value = product.MinPrice
            };

            spParameter[6] = new System.Data.SqlClient.SqlParameter("@MaxPrice", SqlDbType.Decimal)
            {
                Value = product.MaxPrice
            };

            spParameter[7] = new System.Data.SqlClient.SqlParameter("@IsFreeShipping", SqlDbType.Bit)
            {
                Value = product.IsFreeShipping
            };

            spParameter[8] = new System.Data.SqlClient.SqlParameter("@Newest", SqlDbType.Bit)
            {
                Value = product.Newest
            };

            spParameter[9] = new System.Data.SqlClient.SqlParameter("@PageNumber", SqlDbType.Int)
            {
                Value = product.PageNumber
            };

            spParameter[10] = new System.Data.SqlClient.SqlParameter("@RowsOfPage", SqlDbType.Int)
            {
                Value = product.RowsOfPage
            };

            spParameter[11] = new System.Data.SqlClient.SqlParameter("@Orders", SqlDbType.Bit)
            {
                Value = product.Orders
            };

            spParameter[12] = new System.Data.SqlClient.SqlParameter("@Rating", SqlDbType.Decimal)
            {
                Value = product.Rating ?? default
            };

            spParameter[13] = new System.Data.SqlClient.SqlParameter("@StoreId", SqlDbType.Int)
            {
                Value = product.StoreId
            };

            spParameter[14] = new System.Data.SqlClient.SqlParameter("@BrandId", SqlDbType.BigInt)
            {
                Value = product.BrandId
            };

            spParameter[15] = new System.Data.SqlClient.SqlParameter("@UserId", SqlDbType.BigInt)
            {
                Value = product.UserId
            };

            //spParameter[16] = new System.Data.SqlClient.SqlParameter("@VarianceTextValue", SqlDbType.NVarChar)
            //{
            //    Direction = ParameterDirection.Input,
            //    //Value = product.ProductVariance ?? ""
            //    Value = strText
            //};





            var ds = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "[dbo].[GetProduct]", spParameter);
            var totalCount = ds.Tables[0].Rows[0]["TotalCount"];
            var result = ds.Tables[1];

            //Products
            List<ProductSearchDTO> items = ProductResult(result);

            //Brands
            List<BrandDTO> brand = GetBrands(ds.Tables[2]);

            //SideMenu
            var sideMenu = ds.Tables[3];
            List<CategoryDTO> requiredSideMenu = SideMenu(sideMenu);

            //Variance
            var variance = ProductVariance(ds)
                           .GroupBy(item => new { item.Type,item.MasterListId, item.VarianceName })
                            .Select(group =>
                            new SearchProductPropertyDTO
                            {                               
                                Type = group.Key.Type,
                                MasterListId = group.Key.MasterListId,
                                VarianceName = group.Key.VarianceName,
                                GroupProductProperty = group.ToList()
                            }).ToList();

            var price = MaxMinPrice(ds.Tables[5]);

            var searchResult = new FinalSearchResult
            {
                TotalCount = Convert.ToInt32(totalCount),
                LeftCategoryMenu = requiredSideMenu,
                ProductSearchDTO = items,
                Brand = brand,
                ProductProperty = variance,
                SearchBreadcrumbs = GetParentBasedOnProductCategory(product.CategoryId).Reverse(),
                PriceRange = price,
            };


            //var reuslt = GetParentBasedOnProductCategory(product.CategoryId).Reverse();
            return searchResult;

        }

        public IEnumerable<SearchBreadcrumbs> GetParentBasedOnProductCategory(int categoryId)
        {
            var category = _categoryRepository.GetAll().Where(x => x.Id==categoryId);
            var parent = new List<SearchBreadcrumbs>();
            foreach (var item in category.ToList())
            {
                parent.Add(new SearchBreadcrumbs { Name = item.Name, CategoryId = item.Id });
                AppendParent(item, parent);
                
            }

            return parent;
        }

        public void AppendParent(Category item, List<SearchBreadcrumbs> ParentList)
        {
            try
            {
                if (item.ParentId == null || item.ParentId == 0)
                {
                    //ParentList.Add(new SelectListItem { Text = item.Name, Value = item.ParentId.ToString() });
                    return;

                }

                var category = _categoryRepository.GetSingle((int)item.ParentId).Result;
                if (category != null)
                {
                    ParentList.Add(new SearchBreadcrumbs { Name = category.Name, CategoryId = category.Id });                    
                    AppendParent(category, ParentList);
                }              
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private static List<SearchProductPropertyDTO> ProductVariance(DataSet ds)
        {
            return ds.Tables[4].AsEnumerable().Select(row =>
                                    new SearchProductPropertyDTO
                                    {
                                        CategoryPropertyId = 0,
                                        Value = row.Field<string>("Value"),
                                        OptionLabel = row.Field<string>("OptionLabel"),
                                        OptionValue = row.Field<string>("OptionValue"),
                                        Type = row.Field<string>("Type"),
                                        VarianceName = row.Field<string>("VarianceName"),
                                        MasterListId = row.Field<int>("MasterListId"),
                                        MasterPropertyId = row.Field<int>("MasterPropertyId"),
                                    }).ToList();
        }

        private  List<ProductSearchDTO> ProductResult(DataTable result)
        {
            var product= result.AsEnumerable().Select(row =>
                        new ProductSearchDTO
                        {
                            Id = row.Field<int>("Id"),
                            Name = row.Field<string>("Name"),
                            Price = row.Field<decimal>("Price"),//Price(row.Field<int>("Id"),row.Field<decimal>("Price")) ,
                            DiscountedPrice =row.Field<decimal>("DiscountedPrice"),
                            ProductImage = row.Field<string>("ProductImage"),
                            BrandName = row.Field<string>("BrandName"),
                            BrandId = row.Field<long>("BrandId"),
                            Rating= row.Field<decimal?>("Rating"),
                            ColorList= row.Field<string>("Color").Split(',').ToList(),
                            IsInWishList=row.Field<string>("WhishlistProduct")=="0"?false:true,
                            TotalSales= row.Field<long>("NoOfSales")
                        }).ToList();

            foreach (var item in product)
            {
                 Price(item);
            }
            return product;
        }

      

        public  void Price(ProductSearchDTO product)
        {
            var max = default(decimal);
            var min = default(decimal);
            var currentDate = DateTime.Now;
            var checkflashdeal = _flashDealProductRepository.FindBy(x => x.ProductId == product.Id && x.IsDeleted!=true && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now);
            //&& x.StartDate <= currentDate && x.EndDate >= currentDate);
            if (checkflashdeal != null && checkflashdeal.Any())
            {
                foreach (var flashdeal in checkflashdeal)
                {
                    var discountprice = flashdeal.DiscountedPrice ?? 0;
                    var flashdealprice = flashdeal.Price ?? 0;
                    if (discountprice == 0)
                    {
                        discountprice = flashdealprice;
                    }
                    var finalprice = discountprice;
                    var finalresultdiscountpercentange = (flashdealprice > 0 && discountprice > 0) ? ((flashdealprice - discountprice) / flashdealprice * 100) : 0;

                    if (min > finalprice || min == 0)
                    {
                        min = finalprice;
                    }
                    if (max < finalprice || max == 0)
                    {
                        max = finalprice;
                    }

                }
                var mindiscountedprice = checkflashdeal.Where(x => x.Price > 0).Min(x => x.Price) ??0;
                var maxdiscountedprice = checkflashdeal.Where(x => x.Price > 0).Max(x => x.Price) ??0;
                if(min == max)
                {
                    product.DiscountedPriceStr = min.ToString("0.##");//mindiscountedprice.ToString("0.##");
                }
                else
                {
                    product.DiscountedPriceStr = min.ToString("0.##") + "-" + max.ToString("0.##"); //mindiscountedprice.ToString("0.##")+"-"+ maxdiscountedprice.ToString("0.##");
                }

                if (mindiscountedprice == maxdiscountedprice)
                {
                    product.PriceStr=mindiscountedprice.ToString("0.##"); //min.ToString("0.##");
                }
                else
                {
                    product.PriceStr = mindiscountedprice.ToString("0.##") + "-" + maxdiscountedprice.ToString("0.##");
                    //return min.ToString("0.##") + "-" + max.ToString("0.##");
                }

               


            }
            else
            {
                var variation = _productVariationRepository.FindBy(x => x.ProductId == product.Id);
                if (variation != null && variation.Any())
                {
                    foreach (var item in variation)
                    {
                        var discountprice = item.DiscountedPrice ?? 0;
                        var variationprice = item.Price;
                        if (discountprice == 0)
                        {
                            discountprice = variationprice;
                        }
                        var finalprice = discountprice;
                        var finalresultdiscountpercentange = (variationprice > 0 && discountprice > 0) ? ((variationprice - discountprice) / variationprice * 100) : 0;

                        if (min > finalprice || min == 0)
                        {
                            min = finalprice;
                        }
                        if (max < finalprice || max == 0)
                        {
                            max = finalprice;
                        }

                    }

                    var mindiscountedprice = variation.Min(x => x.DiscountedPrice) ?? 0;
                    var maxdiscountedprice = variation.Max(x => x.DiscountedPrice) ?? 0;
                    if (mindiscountedprice == maxdiscountedprice)
                    {
                        product.DiscountedPriceStr = mindiscountedprice.ToString("0.##");
                    }
                    else
                    {
                        product.DiscountedPriceStr = mindiscountedprice.ToString("0.##") + "-" + maxdiscountedprice.ToString("0.##");
                    }

                    if (min == max)
                    {
                        product.PriceStr= min.ToString("0.##");
                    }
                    else
                    {
                        product.PriceStr = min.ToString("0.##") + "-" + max.ToString("0.##");
                    }
                }
                product.DiscountedPriceStr = product.DiscountedPrice == 0 ? string.Empty: product.DiscountedPrice.ToString("0.##");
                if (product.DiscountedPrice>0 && product.Price>product.DiscountedPrice)
                {
                    product.PriceStr = product.Price.ToString("0.##");
                    product.DiscountedPriceStr = product.DiscountedPrice.ToString("0.##");
                }
                else if(product.DiscountedPrice==0|| product.DiscountedPrice> product.Price)
                {
                    product.DiscountedPriceStr = "";
                    product.PriceStr = product.Price.ToString("0.##");
                }
                product.PriceStr = product.Price.ToString("0.##");               

            }
        }

        private static List<BrandDTO> GetBrands(DataTable brands)
        {
            return brands.AsEnumerable().Select(row => new BrandDTO
            {
                Name = row.Field<string>("Name"),
                Id = row.Field<long>("Id"),
            }).ToList();
        }

        private List<CategoryDTO> SideMenu(DataTable sideMenu)
        {
            var menu = sideMenu.AsEnumerable().Select(row =>
                        new CategoryDTO
                        {
                            Id = row.Field<int>("Id"),
                            Name = row.Field<string>("Name"),
                            ParentId = row.Field<int>("ParentId"),
                        }).ToList();



            var requiredSideMenu = menu.Where(x => x.ParentId == 0).ToList();

            GetChildrenCategory(menu, requiredSideMenu);
            return requiredSideMenu;
        }

        //We have to optimize  this
        private void GetChildrenCategory(List<CategoryDTO> item, List<CategoryDTO> requiredCategoryMenu)
        {
            foreach (var categoryMenu in requiredCategoryMenu)
            {
                var ChildrenCategory = item.FirstOrDefault(x => x.ParentId == categoryMenu.Id);
                if (ChildrenCategory == null) continue;
                categoryMenu.ChildrenCategory = item.Where(x => x.ParentId == categoryMenu.Id).ToList();
                GetChildrenCategory(item, categoryMenu.ChildrenCategory);
            }
        }

        private PriceRange MaxMinPrice(DataTable Price)
        {
            var PriceRange = new PriceRange
            {
                MaxPrice = 0,
                MinPrice = 0
            };
            if (Price.Rows.Count > 0)
            {
                PriceRange.MinPrice = Convert.ToDecimal(Price.Rows[0]["MinPrice"]);
                PriceRange.MaxPrice = Convert.ToDecimal(Price.Rows[0]["MaxPrice"]);
            }
            
            return PriceRange;
        }
        #endregion

        #region ProductPropertyCombination
        public async Task<List<List<MasterListValueDTO>>> ProductPropertyCombinationList(int productId)
        {
            var list = new List<List<MasterListValueDTO>>();
            var product = _productPropertyRepository
                         .FindBy(x => x.Product.Id == productId
                          && (x.CategoryProperty.MasterProperty.Type == MasterPropertyType.MultiSelectDropDown.ToString() ||
                          x.CategoryProperty.MasterProperty.Type == MasterPropertyType.MultiSelectDropDownColor.ToString()))
                         .ToList()
                         .GroupBy(x => x.CategoryPropertyId,
                         (key, g) => new { key, MasterListValueId = g.Select(x => x.Value).ToList() });


            foreach (var item in product)
            {
                var masterListValue = _masterListValueRepository.All.Where(x => item.MasterListValueId.Contains(x.Id.ToString()));
                var materListValueDTO = _mapper.Map<List<MasterListValueDTO>>(masterListValue);
                list.Add(materListValueDTO);
            }

            return list;



        }
        public bool CheckVariationImageExist(int imageId, long ProductVariationId)
        {
            return _productVariationImageRepository.FindBy(x => x.ProductVariationId == ProductVariationId && x.ProductImageId == imageId)
                .FirstOrDefault() != null;
        }

        public async Task SavePriceQuantity(ProductVariationDTO productVariationDTO)
        {
            var productvariation = await _productVariationRepository.GetSingle(productVariationDTO.Id);
            if (productvariation != null)
            {
                productvariation.Price = productVariationDTO.Price;
                productvariation.DiscountedPrice = productVariationDTO.DiscountedPrice;
                productvariation.Quantity = productVariationDTO.Quantity;
                await _unitOfWork.Commit();
            }
        }

        public async Task<List<ProductVariationDTO>> GetProductVariant(int productId)
        {
            var productvariation = new List<ProductVariation>();

            var currentDate = DateTime.Now;
           
            /* 
            var flashdeal = _flashDealProductRepository.FindBy(x => x.ProductId == productId && x.IsDeleted!=true);
            if (flashdeal.FirstOrDefault()!= null)
            {
                var productvariationId = flashdeal.Select(x => x.ProductVariationId);
                productvariation = await _productVariationRepository.FindBy(x => productvariationId.Contains(x.Id)                                    
                                    && (x.IsDelete == null || x.IsDelete == false))
                                    .Include(x => x.ProductVariationPropertyCombination)
                                   .Include(x => x.ProductVariationImage).ToListAsync();

                foreach (var item in productvariation)
                {
                    var flashdealrecord = flashdeal.FirstOrDefault(x => x.ProductVariationId == item.Id && x.IsDeleted!=true);
                    if (flashdealrecord == null) continue;
                    item.Price = flashdealrecord.Price??0;
                    item.DiscountedPrice = flashdealrecord.DiscountedPrice??0;
                    item.Quantity = flashdealrecord.Quantity??0;
                   
                    
                }

                var productvariationnotinflashdeal = await _productVariationRepository.FindBy(x => !productvariationId.Contains(x.Id)
                                    && x.ProductId == productId
                                    && (x.IsDelete == null || x.IsDelete == false))
                                    .Include(x => x.ProductVariationPropertyCombination)
                                   .Include(x => x.ProductVariationImage).ToListAsync();
                foreach (var item in productvariationnotinflashdeal)
                {
                    item.Price =  0;
                    item.DiscountedPrice =  0;
                    item.Quantity = 0;
                    productvariation.Add(item);
                }
            }
            else
            {
                productvariation = await _productVariationRepository.FindBy(x => x.ProductId == productId
                                   && (x.IsDelete == null || x.IsDelete == false))
                                   .Include(x => x.ProductVariationPropertyCombination)
                                  .Include(x => x.ProductVariationImage).ToListAsync();

                
            }
            */
            productvariation = await _productVariationRepository.FindBy(x => x.ProductId == productId
                                   && (x.IsDelete == null || x.IsDelete == false))
                                   .Include(x => x.ProductVariationPropertyCombination)
                                  .Include(x => x.ProductVariationImage).ToListAsync();
            var result = _mapper.Map<List<ProductVariationDTO>>(productvariation);
            var product = await _productRepository.GetSingle(productId);
          
            if (product == null) return new List<ProductVariationDTO>();
            var flashdealvalidation = _flashDealProductRepository
                                            .FindBy(x => x.ProductId == productId && x.IsDeleted != true && x.StartDate <= currentDate && x.EndDate >= currentDate);
            foreach (var item in result)
            {
                item.IsProductPurchable = false;
                if (product.IsSaleable)
                {
                    var flashdealresult = flashdealvalidation.FirstOrDefault(x => x.ProductVariationId == item.Id);
                    if (flashdealresult != null)
                    {
                        item.Price = flashdealresult.Price ?? 0;
                        item.DiscountedPrice = flashdealresult.DiscountedPrice ?? 0;
                        item.Quantity = flashdealresult.Quantity ?? 0;
                        item.FlashDealDetail = new FlashDealDetailDTO()
                        {
                            Id = flashdealresult.Id,
                            Quantity = flashdealresult.Quantity.Value,
                            SalesPercentage = flashdealresult.Quantity.HasValue
                               && flashdealresult.Quantity > 0 ? (((float)flashdealresult.SoldQuantity / (float)flashdealresult.Quantity) * 100) : 0,
                            SoldQuantity = flashdealresult.SoldQuantity.Value,
                            EndDate = flashdealresult.EndDate,
                            TimeLeft = (flashdealresult.EndDate - DateTime.Now).TotalSeconds,
                            //ItemLeft = flashdealvalidation.Quantity - flashdealvalidation.SoldQuantity
                            ItemLeft = flashdealresult.Quantity
                        };
                    }
                    if (item.Quantity > 0 && (item.DiscountedPrice > 0 || item.Price > 0))
                        item.IsProductPurchable = true;
                    /*
                    if (flashdealvalidation.FirstOrDefault() != null)
                    {
                        var flashdealresult = flashdealvalidation.FirstOrDefault(x => x.ProductVariationId == item.Id && x.StartDate <= DateTime.Now && x.EndDate > DateTime.Now);
                        if (flashdealresult == null) 
                        { 
                            item.IsProductPurchable = false; 
                            //item.FlashDealDetail = new FlashDealDetailDTO();
                          
                        }
                        else
                        {
                             item.FlashDealDetail = new FlashDealDetailDTO()
                            {
                                Id = flashdealresult.Id,
                                Quantity = flashdealresult.Quantity.Value,
                                SalesPercentage = flashdealresult.Quantity.HasValue
                                && flashdealresult.Quantity > 0 ? (((float)flashdealresult.SoldQuantity / (float)flashdealresult.Quantity) * 100) : 0,
                                SoldQuantity = flashdealresult.SoldQuantity.Value,
                                EndDate = flashdealresult.EndDate,
                                TimeLeft = (flashdealresult.EndDate - DateTime.Now).TotalSeconds,
                                //ItemLeft = flashdealvalidation.Quantity - flashdealvalidation.SoldQuantity
                                    ItemLeft = flashdealresult.Quantity
                            };

                            if (DateTime.Now > flashdealresult.EndDate)
                            {
                                item.IsProductPurchable = false;
                            }
                            else if (flashdealresult.Quantity == 0)
                            {
                                item.IsProductPurchable = false;
                            }
                            else { item.IsProductPurchable = true; }
                        }
                       
                    }
                    else
                    {
                       
                    }*/

                }

                var variation = productvariation.FirstOrDefault(x => x.Id == item.Id);
                
                var masterlistvalue = variation.ProductVariationPropertyCombination.OrderBy(x => x.MasterListValuesId).Select(x => x.MasterListValuesId).ToList();
                item.PropertyCombination = string.Join(",", masterlistvalue);
                item.PropertyCombinationIds = masterlistvalue;

                var images = variation.ProductVariationImage.Select(x => x.ProductImageId);
                var imagesresult = await _productImageRepository.All.Where(x => images.Contains(x.Id))
                                  .ToListAsync();
                if (!imagesresult.Any())
                {
                    var allImages = _productImageRepository
                                    .FindBy(x => x.ProductId == productId && (x.IsDelete==null|| x.IsDelete==false)).OrderBy(x => x.IsPrimary);
                    item.ImageList = _mapper.Map<List<ProductImageDTO>>(allImages);
                }
                else
                {
                    item.ImageList = _mapper.Map<List<ProductImageDTO>>(imagesresult);
                }



                if (!item.ImageList.Any())
                {
                    item.ImageList.Add(new ProductImageDTO { ImagePath = "Product/NoImage.png" });
                }
                else
                {
                    foreach (var productImage in item.ImageList)
                    {
                        productImage.ImagePath = "Product/Image" + "/" + productImage.ImagePath;
                    }
                }
            }
            return result;
        }

        public string GetProductCombinationLables(int productId, Int64 variationId)
        {
            string combinedLabels = "";
            var variations = _productVariationRepository.GetAll(x => x.ProductId == productId && x.Id == variationId).Include(x => x.ProductVariationPropertyCombination).ThenInclude(x => x.MasterListValue).FirstOrDefault();
            if(variations != null)
            {
                List<string> combinationLabels = variations.ProductVariationPropertyCombination.Select(x => x.MasterListValue.OptionLabel).ToList();
                if (combinationLabels != null && combinationLabels.Count() > 0)
                {
                    combinedLabels = string.Join(", ", combinationLabels);
                }
                if (string.IsNullOrEmpty(combinedLabels) == false)
                {
                    combinedLabels = " (" + combinedLabels + ")";
                }
            }
            return combinedLabels;
        }

        public int GetProductStock(int productId, Int64 variationId)
        {
            int totalQuantity = 0;
            var variations = _productVariationRepository.GetAll(x => x.ProductId == productId && x.Id == variationId).ToList();
            if (variations != null && variations.Count() > 0)
            {
                totalQuantity = variations.Sum(x => x.Quantity);
            }
            return totalQuantity;
        }
        #endregion

        #region Inventory Price Application
        public async Task<bool> UpdatePriceQuantityByStock(List<InventoryItemDTO> products)
        {
            foreach (InventoryItemDTO singleProduct in products)
            {
                if (singleProduct.VariationId.HasValue && singleProduct.VariationId.Value > 0)
                {
                    ProductVariation productVariation = await _productVariationRepository.GetSingle(singleProduct.VariationId.Value);
                    productVariation.Quantity += singleProduct.Quantity;
                    productVariation.Price = singleProduct.Price;
                } else
                {
                    Product product = _productRepository.GetById(singleProduct.ProductId);
                    product.Price = singleProduct.Price;
                    product.Stock += singleProduct.Quantity;
                }
            }
            return await _unitOfWork.Commit() > 0;
        }
        public async Task<bool> UpdateAveragePriceQuantityByStock(List<InventoryItemDTO> products)
        {
            foreach (InventoryItemDTO singleProduct in products)
            {
                if (singleProduct.VariationId.HasValue)
                {
                    ProductVariation productVariation = await _productVariationRepository.GetSingle(singleProduct.VariationId.Value);
                    productVariation.Quantity += singleProduct.Quantity;
                    productVariation.Price = (singleProduct.Price + productVariation.Price) / 2;
                }
                else
                {
                    Product product = _productRepository.GetById(singleProduct.ProductId);
                    product.Price = (singleProduct.Price + product.Price) / 2;
                    product.Stock += singleProduct.Quantity;
                }
            }
            return await _unitOfWork.Commit() > 0;
        }
        #endregion Inventory Price Application

        #region Shipping Price Calculation
        public async Task<List<ProductShippingChargeDTO>> CalculateDeliveryCharge(List<ProductShippingChargeDTO> productList, Int64 addressId)
        {
            UserAddress userAddress = await _userAddressRepository.GetSingle(addressId);
            List<int> productIds = productList.Select(x => x.ProductId).ToList();
            List<Product> products = _productRepository.All.Where(x => productIds.Contains(x.Id)).Include(x=>x.ProductShippingCostVariation).ToList();
            ShippingCostVariation defaultShippingCost = _shippingCostVariationRepository.All.Where(x => x.CityListId == userAddress.CityId && x.IsActive == true).FirstOrDefault();
            foreach (ProductShippingChargeDTO singleItem in productList)
            {
                Product singleProduct = products.Where(x => x.Id == singleItem.ProductId).FirstOrDefault();
                if (singleProduct != null && singleProduct.IsFreeShipping)
                {
                    singleItem.BaseDeliveryCharge = 0;
                } else if(singleProduct != null)
                {
                    var shippingVariation = singleProduct.ProductShippingCostVariation.Where(x => x.CityListId == userAddress.CityId).FirstOrDefault();
                    if(shippingVariation != null) {
                        if (shippingVariation.IsFreeShipping)
                        {
                            singleItem.BaseDeliveryCharge = 0;
                        } else if(shippingVariation.VariationType == (int)Enums.VariationType.Flat)
                        {
                            singleItem.BaseDeliveryCharge = singleProduct.BaseDeliveryCharge.Value + shippingVariation.Amount; 
                        } else if(shippingVariation.VariationType == (int)Enums.VariationType.Percentage)
                        {
                            singleItem.BaseDeliveryCharge = singleProduct.BaseDeliveryCharge.Value + ((shippingVariation.Amount / 100) * singleProduct.BaseDeliveryCharge.Value);
                        }
                    }else if(defaultShippingCost != null){
                        if (defaultShippingCost.VariationType == (int)Enums.VariationType.Flat)
                        {
                            singleItem.BaseDeliveryCharge = singleProduct.BaseDeliveryCharge.Value + defaultShippingCost.Amount;
                        }
                        else if (defaultShippingCost.VariationType == (int)Enums.VariationType.Percentage)
                        {
                            singleItem.BaseDeliveryCharge = singleProduct.BaseDeliveryCharge.Value + ((defaultShippingCost.Amount / 100) * singleProduct.BaseDeliveryCharge.Value);
                        }
                    }
                } else
                {
                    singleItem.BaseDeliveryCharge = 0;
                }
            }
            return productList;
        }
        #endregion Shipping Price Calculation
 

        #region ProductStoreStatus
        public async Task<bool> RequestProductForApproval(long userId, int id, string comment, string  adminlink)
        {
            var product = _productRepository.All
                       .Include(x => x.Store)
                       .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == id);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.RequestForApproval;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.RequestForApproval.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };

            await _unitOfWork.Commit();
            var callbackurl = adminlink + "product/view-product-details/" + id;
            var emailTemplate = EmailTemplateType.REQUESTPRODUCTFORAPPROVAL.ToString();
            string logiclyncAdmin = EnumHelper.GetDescription(Entities.Enums.Role.SUPERADMINISTRATOR);
            string administrator= EnumHelper.GetDescription(Entities.Enums.Role.ADMINISTRATOR);
            var admin = _userRoleRepository.All.Include(x => x.Role).Include(x => x.UserDetail)
                       .Where(x => x.Role.Name.ToLower() == logiclyncAdmin.ToLower() 
                       || x.Role.Name.ToLower() == administrator.ToLower()).Select(x=>x.UserDetail);

            foreach (var item in admin.ToList())
            {
                var userdetailsDTO = new UserDetailsDTO
                {
                    Email = item.Email,
                    FirstName = item.FirstName,
                    LastName=  item.LastName,
                    UserId = item.UserId,
                    Id = item.Id
                };
                await _emailQueueService.InsertoEmailQueue(userdetailsDTO, emailTemplate, callbackurl);
            }
          
            return true;

        }
        public async Task<bool> RequestForStoreApproval(long userId, int id, string comment, string adminlink)
        {
            var product = _productRepository.All
                       .Include(x => x.Store)
                       .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == id);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.RequestForStoreApproval;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.RequestForStoreApproval.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };

            await _unitOfWork.Commit();


           

            string storeownerrole = EnumHelper.GetDescription(Entities.Enums.Role.INSTRUCTOR);
            string storeAdmin = EnumHelper.GetDescription(Entities.Enums.Role.STOREADMINISTRATOR);
            var callbackurl = adminlink + "product/view-product-details/" + id;
            foreach (var item in product.Store.UserStore)
            {
                //item.UserDetail = _userDetailsRepository.All.Include(x=>x.UserRole)
                //                  .Where(x => x.Id == item.UserDetailId).FirstOrDefault();
                //item.UserDetail.UserRole.FirstOrDefault(x => (x.Role.Name == storeownerrole || x.Role.Name == storeAdmin));

                var result = _userRoleRepository
                          .AllIncluding(x => x.Role, x => x.UserDetail)
                          .Where(x => (x.Role.Name == storeownerrole || x.Role.Name == storeAdmin) && x.UserId==item.UserDetailId).ToList();

                foreach (var userrole in result)
                {
                    var userdetailsDTO = new UserDetailsDTO
                    {
                        Email = userrole.UserDetail.Email,
                        FirstName = userrole.UserDetail.FirstName,
                        LastName = userrole.UserDetail.LastName,
                        UserId = userrole.UserDetail.UserId,
                        Id = userrole.UserDetail.Id
                    };
                    await _emailQueueService.InsertoEmailQueue(userdetailsDTO, EmailTemplateType.REQUESTSTOREPRODUCTAPPROVAL.ToString(), callbackurl);
                }
            }
           
            ////var result = _userRoleRepository
            //              .AllIncluding(x => x.Role, x => x.UserDetail)
            //              .Where(x =>(x.Role.Name == storeownerrole || x.Role.Name == storeAdmin)).ToList();

           
           
           
            return true;

        }


        public async Task<bool> ApproveProduct(long userId,int id, string comment)
        {
            var product = _productRepository.All
                       .Include(x => x.Store)
                       .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == id);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.Approved;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.Approved.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };

            await _unitOfWork.Commit();


            
            

            var result = RolesAssociatedToStore(product).ToList();
            var emailTemplate = EmailTemplateType.PRODUCTAPPROVE.ToString();
            await SendEmail(result, emailTemplate);
            return true;
            
        }

        public async Task<bool> checkVersion(int productId)
        {
            return await _productVersionRepository.FindBy(x => x.ParentProductId == productId).AnyAsync(); 
        }

        public async Task<bool> StoreApproveProduct(long userId, int id, string comment, string adminlink)
        {
            var product = _productRepository.All
                       .Include(x => x.Store)
                       .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == id);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.StoreApproved;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.StoreApproved.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };

            await _unitOfWork.Commit();
            var callbackurl = adminlink + "product/view-product-details/" + id;
            var userDetails = _userDetailsRepository.FindBy(x => x.Id == product.CreatedBy).FirstOrDefault();
            if (userDetails != null)
            {
                var userdetailsDTO = new UserDetailsDTO
                {
                    Email = userDetails.Email,
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    UserId = userDetails.UserId,
                    Id = userDetails.Id
                };
                await _emailQueueService.InsertoEmailQueue(userdetailsDTO, EmailTemplateType.STOREPRODUCTAPPROVE.ToString(), callbackurl);
            }  
            return true;

        }

        public async Task<bool> RejectProduct(long userId, int id, string comment)
        {
            var product = _productRepository.All
                        .Include(x => x.Store)
                        .ThenInclude(x => x.UserStore).FirstOrDefault(x=>x.Id==id);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.Rejected;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.Rejected.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };
         

            await _unitOfWork.Commit();
            var result = RolesAssociatedToStore(product).ToList();
            var emailTemplate = EmailTemplateType.PRODUCTREJECT.ToString();
            await SendEmail(result, emailTemplate);
            return true;

        }

        public async Task<bool> StoreRejectProduct(long userId, int id, string comment, string adminlink)
        {
            var product = _productRepository.All
                       .Include(x => x.Store)
                       .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == id);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.StoreRejected;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.StoreRejected.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };

            await _unitOfWork.Commit();
            var callbackurl = adminlink + "product/view-product-details/" + id;
            var userDetails = _userDetailsRepository.FindBy(x => x.Id == product.CreatedBy).FirstOrDefault();
            if (userDetails != null)
            {
                var userdetailsDTO = new UserDetailsDTO
                {
                    Email = userDetails.Email,
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    UserId = userDetails.UserId,
                    Id = userDetails.Id
                };
                await _emailQueueService.InsertoEmailQueue(userdetailsDTO, EmailTemplateType.STOREPRODUCTREJECT.ToString(), callbackurl);
            }
            return true;

        }

        public async Task<bool> SuspendProduct(long userId, int id, string comment)
        {
            var product = _productRepository.All
                         .Include(x => x.Store)
                         .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == id);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.Suspend;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.Suspend.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };

            await _unitOfWork.Commit();
            var result = RolesAssociatedToStore(product).ToList();
            var emailTemplate = EmailTemplateType.PRODUCTSUSPEND.ToString();
            await SendEmail(result, emailTemplate);

            return true;

        }

        public async Task<bool> UnSuspendProduct(long userId, int id, string comment)
        {
            var product = _productRepository.All
                        .Include(x => x.Store)
                        .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == id);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.UnSuspend;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.UnSuspend.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };

            await _unitOfWork.Commit();
            var result = RolesAssociatedToStore(product).ToList();
            var emailTemplate = EmailTemplateType.PRODUCTUNSUSPEND.ToString();
            await SendEmail(result, emailTemplate);
            return true;

        }


        private IQueryable<UserRole> RolesAssociatedToStore(Product product)
        {
            string storeownerrole = EnumHelper.GetDescription(Entities.Enums.Role.INSTRUCTOR);
            string storeAdmin = EnumHelper.GetDescription(Entities.Enums.Role.STOREADMINISTRATOR);

            foreach (var item in product.Store.UserStore)
            {
               item.UserDetail =  _userDetailsRepository.FindBy(x => x.Id == item.UserDetailId).FirstOrDefault();
            }

            var userdetails = product.Store.UserStore.Select(x => x.UserDetail);
            var userdetailsUserId = userdetails.Select(y => y.Id).ToList();
            var userrole = _userRoleRepository
                          .AllIncluding(x => x.Role, x => x.UserDetail)
                          .Where(x => userdetailsUserId.Contains(x.UserDetail.Id) &&
                          (x.Role.Name == storeownerrole || x.Role.Name == storeAdmin));

            var userolecreatedby = _userRoleRepository
                                .AllIncluding(x => x.Role, x => x.UserDetail)
                                .Where(x => x.UserId == product.CreatedBy);
            var result = userrole.Union(userolecreatedby);
            return result;
        }

        private IQueryable<UserRole> AdminRole()
        {
            
            string storeAdmin = EnumHelper.GetDescription(Entities.Enums.Role.STOREADMINISTRATOR);           
            var userrole = _userRoleRepository
                          .AllIncluding(x => x.Role, x => x.UserDetail)                         
                          .Where(x=>x.Role.Name == storeAdmin);           
            return userrole;
        }

        private async Task SendEmail(IEnumerable<UserRole> userrole, string emailTemplate)
        {
            foreach (var item in userrole)
            {
                var email = item.UserDetail.Email;

                var userdetailsDTO = new UserDetailsDTO
                {
                    Email = email,
                    FirstName = item.UserDetail.FirstName,
                    LastName = item.UserDetail.LastName,
                    UserId = item.UserDetail.UserId,
                    Id = item.Id
                };
                await _emailQueueService.InsertoEmailQueue(userdetailsDTO, emailTemplate);

            }
        }


        public async Task<List<ProductStoreStatusDTO>> ProductStoreLog(int id)
        {
            var result = _productRepository.AllIncluding(x => x.ProductStoreStatus)
                         .Where(x => x.ProductStoreStatus.Any(p => p.ProductId == id))                         
                         .Select(x => x.ProductStoreStatus).FirstOrDefault();
            if (result == null) return new List<ProductStoreStatusDTO>();
            var mapper = _mapper.Map<List<ProductStoreStatusDTO>>(result.OrderByDescending(x=>x.CreatedDate).ToList());
            foreach (var item in mapper)
            {
                var userDetails = _userDetailsRepository.FindBy(x => x.Id == item.CreatedBy).FirstOrDefault();
                if (userDetails != null)
                {
                    item.CreatedByName = userDetails.FirstName + " " + userDetails.LastName;
                }
            }
            return mapper;
        }

        public async Task<string> GetVideoThumbnail(int VideoId)
        {
            var video = _productVideoRepository.FindBy(x => x.Id == VideoId).FirstOrDefault();
            if(video != null)
            {
                return video.ThumbnailImage;
            }
            return "";
        }

        public async Task<bool> UpdateVideoThumbnail(ProductVideo model)
        {
            var video = _productVideoRepository.FindBy(x => x.Id == model.Id).FirstOrDefault();
            if (video != null)
            {
                video.ThumbnailImage = model.ThumbnailImage;
                await _unitOfWork.Commit();
                return true;
            }
            return false;
        }

        #endregion

        #region ProductVersioning
        public async Task RequestVersion(int productId, string comment,long userId,string adminlink)
        {
            var productversion = _productVersionRepository.GetById(productId);
            if (productversion != null)
            {
                productversion.ProductStoreStatusId = (int)Enums.ProductStoreStatus.RequestForVersion;
                var product = _productRepository.GetById(productversion.ParentProductId);
                product.ProductStoreStatusId = (int)Enums.ProductStoreStatus.RequestForVersion;
                product.ProductStoreStatus = new List<ProductStoreStatus>
                {
                    new ProductStoreStatus
                    {
                        ProductId = product.Id,
                        ProductStoreStatusId = product.ProductStoreStatusId,
                        Status = Enums.ProductStoreStatus.RequestForVersion.ToString(),
                        Comment = comment,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                    }
                };
                await _unitOfWork.Commit();

                var callbackurl = adminlink + "/product/view-details-version/" + productId;
                var emailTemplate = EmailTemplateType.REQUESTPRODUCTFORVERSIONAPPROVAL.ToString();
                string logiclyncAdmin = EnumHelper.GetDescription(Entities.Enums.Role.SUPERADMINISTRATOR);
                string administrator = EnumHelper.GetDescription(Entities.Enums.Role.ADMINISTRATOR);
                var admin = _userRoleRepository.All.Include(x => x.Role).Include(x => x.UserDetail)
                           .Where(x => x.Role.Name.ToLower() == logiclyncAdmin.ToLower()
                           || x.Role.Name.ToLower() == administrator.ToLower()).Select(x => x.UserDetail);

                foreach (var item in admin.ToList())
                {
                    var userdetailsDTO = new UserDetailsDTO
                    {
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        UserId = item.UserId,
                        Id = item.Id
                    };
                    await _emailQueueService.InsertoEmailQueue(userdetailsDTO, emailTemplate, callbackurl);
                }
            }
        }

        public async Task RequestStoreVersion(int productId, string comment, long userId, string adminlink)
        {
            var productversion = _productVersionRepository.GetById(productId);
            if (productversion != null)
            {
                productversion.ProductStoreStatusId = (int)Enums.ProductStoreStatus.RequestStoreForVersion;
                var product = _productRepository.GetById(productversion.ParentProductId);
                product.ProductStoreStatusId = (int)Enums.ProductStoreStatus.RequestStoreForVersion;
                product.ProductStoreStatus = new List<ProductStoreStatus>
                {
                    new ProductStoreStatus
                    {
                        ProductId = product.Id,
                        ProductStoreStatusId = product.ProductStoreStatusId,
                        Status = Enums.ProductStoreStatus.RequestStoreForVersion.ToString(),
                        Comment = comment,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                    }
                };
                await _unitOfWork.Commit();

                var callbackurl = adminlink + "/product/view-details-version/" + productId;

                var userstore = _userStoreRepository.All.Include(x => x.UserDetail)
                                .Where(x => x.StoreId == product.StoreId).Select(x => x.UserDetailId);
               

                var emailTemplate = EmailTemplateType.REQUESTPRODUCTFORVERSIONSTOREAPPROVAL.ToString();
                string storeAdmin = EnumHelper.GetDescription(Entities.Enums.Role.STOREADMINISTRATOR);
                string storeOwner = EnumHelper.GetDescription(Entities.Enums.Role.INSTRUCTOR);
                var admin = _userRoleRepository.All.Include(x => x.Role).Include(x => x.UserDetail)                            
                           .Where(x => x.Role.Name.ToLower() == storeAdmin.ToLower()
                           || x.Role.Name.ToLower() == storeOwner.ToLower()
                           && userstore.Contains(x.UserId)).Select(x => x.UserDetail);

                foreach (var item in admin.ToList())
                {
                    var userdetailsDTO = new UserDetailsDTO
                    {
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        UserId = item.UserId,
                        Id = item.Id
                    };
                    await _emailQueueService.InsertoEmailQueue(userdetailsDTO, emailTemplate, callbackurl);
                }
            }
        }
        public async Task<bool> ApproveStoreProductVersion(int productId, string comment, long userId, string adminlink)
        {
            var product = _productRepository.All
                        .Include(x => x.Store)
                        .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == productId);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.StoreVersionApproved;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.StoreVersionApproved.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };

            var version = _productVersionRepository
                         .FindBy(x => x.ParentProductId == productId).Max(x => x.Id);
            var resultversion = _productVersionRepository.All.FirstOrDefault(x => x.Id == version);
            if (resultversion != null)
            {
                resultversion.ProductStoreStatusId = product.ProductStoreStatusId;
            }


            await _unitOfWork.Commit();
            var callbackurl = adminlink + "/product/view-details-version/" + productId;
            var result = _userDetailsRepository.FindBy(x => x.Id == product.CreatedBy).FirstOrDefault();
            var emailTemplate = EmailTemplateType.PRODUCTFORVERSIONSTOREAPPROVED.ToString();
            var userdetailsDTO = new UserDetailsDTO
            {
                Email = result.Email,
                FirstName = result.FirstName,
                LastName = result.LastName,
                UserId = result.UserId,
                Id = result.Id
            };
            await _emailQueueService.InsertoEmailQueue(userdetailsDTO, emailTemplate, callbackurl);
            return true;

        }

        public async Task<bool> RejectStoreProductVersion(int productId, string comment, long userId, string adminlink)
        {
            var product = _productRepository.All
                        .Include(x => x.Store)
                        .ThenInclude(x => x.UserStore).FirstOrDefault(x => x.Id == productId);
            if (product == null) return false;
            product.ProductStoreStatusId = (int)LogicLync.Entities.Enums.ProductStoreStatus.StoreVersionReject;
            product.ProductStoreStatus = new List<ProductStoreStatus>
            {
                new ProductStoreStatus
                {
                    ProductId = product.Id,
                    ProductStoreStatusId = product.ProductStoreStatusId,
                    Status = Enums.ProductStoreStatus.StoreVersionReject.ToString(),
                    Comment = comment,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }
            };
            await _unitOfWork.Commit();

            var callbackurl = adminlink + "/product/view-details-version/" + productId;
            var result = _userDetailsRepository.FindBy(x => x.Id == product.CreatedBy).FirstOrDefault();
            var emailTemplate = EmailTemplateType.PRODUCTFORVERSIONSTOREREJECT.ToString();
            var userdetailsDTO = new UserDetailsDTO
            {
                Email = result.Email,
                FirstName = result.FirstName,
                LastName = result.LastName,
                UserId = result.UserId,
                Id = result.Id
            };
            await _emailQueueService.InsertoEmailQueue(userdetailsDTO, emailTemplate, callbackurl);
            return true;

        }

        public async Task<bool> Checkflashdeal(int productId)
        {
            var flashdeal = await _flashDealProductRepository.FindBy(x => x.ProductId == productId 
                              && x.IsDeleted!=true && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now).AnyAsync();
            return flashdeal;
        }


        public async Task<DataTable> ProductVersioning(int productId, long userId, string comment, string connectionString, bool changstatus=true)
        {
            System.Data.SqlClient.SqlParameter[] spParameter = new System.Data.SqlClient.SqlParameter[6];
            spParameter[0] = new System.Data.SqlClient.SqlParameter("@ProductId", SqlDbType.Int)
            {
                
                Value = productId,
            };

            spParameter[1] = new System.Data.SqlClient.SqlParameter("@UserId", SqlDbType.BigInt)
            {

                Value = userId,
            };

            spParameter[2] = new System.Data.SqlClient.SqlParameter("@ProductStoreStatusId", SqlDbType.BigInt)
            {

                Value = (int)Enums.ProductStoreStatus.VersionRunning,
            };

            spParameter[3] = new System.Data.SqlClient.SqlParameter("@Status", SqlDbType.VarChar)
            {

                Value = Enums.ProductStoreStatus.RequestForVersion.ToString(),
            };

            spParameter[4] = new System.Data.SqlClient.SqlParameter("@Comment", SqlDbType.VarChar)
            {

                Value = comment,
            };
            spParameter[5] = new System.Data.SqlClient.SqlParameter("@ChangeStatus", SqlDbType.Bit)
            {

                Value = changstatus,
            };

            var ds = SqlHelper.ExecuteDataset(connectionString,CommandType.StoredProcedure, "[dbo].[InsertProductVersion]",
                                               spParameter);
            var datatable = ds.Tables[0];
            return datatable;
        }

        public int LastestVesion(int id)
        {
            var result = _productVersionRepository.FindBy(x => x.ParentProductId == id).Max(x=>x.Id);
            return result;
        }
        #endregion
    }
}
