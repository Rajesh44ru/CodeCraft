using AutoMapper;
using LogicLync.DTO;
using LogicLync.Entities;
using LogicLync.Repository;
using LogicLync.Repository.Infrastructure;
using LogicLync.Service.Extension;
using LogicLync.Service.HelperClasses;
using LogicLync.Service.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static LogicLync.Entities.Enums;
using Item = LogicLync.Entities.Item;

namespace LogicLync.Service
{
    public class ItemRequisitionService: IItemRequisitionService
    {
        public IConfiguration Configuration { get; }
        IMapper _mapper;
        IUnitOfWork _unitOfWork;
        private readonly IItemRequisitionRepository _itemRequisitionRepository;
        private readonly IMST_CodeGenerationTemplateService _codeGenerationTemplateService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserDetailsRepository _userDetailsRepository;
        private readonly IProcessTaskRepository _processTaskRepository;
        private readonly IItemRequistionTaskRepository _itemRequistionTaskRepository;
        private readonly IPickingListService _pickingListService;
        private readonly IItemRequisitionDetailRepository _itemRequisitionDetailRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IWebSettingRepository _webSettingRepository;
        private readonly IWareHouseReservedItemRepository _warehouseReservedItemRepository;
        private readonly IItemTransactionRepository _itemTransactionRepository;
        private readonly IPickingListRepository _pickingListRepository;
        private readonly IItemService _itemService;
        private readonly IItemTransactionService _itemTransactionService;
        private readonly IMST_WareHouseDefaultLocationRepository _wareHouseDefaultLocationRepository;
        private readonly IWareHouseLocationRepository _wareHouseLocationRepository;

        public ItemRequisitionService(IMapper mapper, IUnitOfWork unitOfWork, IItemRequisitionRepository itemRequisitionRepository, IConfiguration config
                                        ,IMST_CodeGenerationTemplateService codeGenerationTemplateService, IHttpContextAccessor httpContextAccessor, IUserDetailsRepository userDetailsRepository,
                                        IProcessTaskRepository processTaskRepository, IItemRequistionTaskRepository itemRequistionTaskRepository, IPickingListService pickingListService,
                                        IItemRequisitionDetailRepository itemRequisitionDetailRepository, IItemRepository itemRepository, IWebSettingRepository webSettingRepository
                                        , IWareHouseReservedItemRepository warehouseReservedItemRepository, IItemTransactionRepository itemTransactionRepository, IPickingListRepository pickingListRepository,
                                        IItemService itemService, IItemTransactionService itemTransactionService, IMST_WareHouseDefaultLocationRepository wareHouseDefaultLocationRepository,
                                        IWareHouseLocationRepository wareHouseLocationRepository)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _itemRequisitionRepository = itemRequisitionRepository;
            Configuration = config;
            _codeGenerationTemplateService = codeGenerationTemplateService;
            _httpContextAccessor = httpContextAccessor;
            _userDetailsRepository = userDetailsRepository;
            _processTaskRepository = processTaskRepository;
            _itemRequistionTaskRepository = itemRequistionTaskRepository;   
            _pickingListService = pickingListService;
            _itemRequisitionDetailRepository = itemRequisitionDetailRepository;
            _itemRepository = itemRepository;
            _webSettingRepository = webSettingRepository;
            _warehouseReservedItemRepository = warehouseReservedItemRepository;
            _itemTransactionRepository = itemTransactionRepository;
            _pickingListRepository = pickingListRepository;
            _itemService = itemService;
            _itemTransactionService = itemTransactionService;
            _wareHouseDefaultLocationRepository = wareHouseDefaultLocationRepository;
            _wareHouseLocationRepository = wareHouseLocationRepository;
        }

    

        public async Task<long> CreateItemRequisition(ItemRequisitionDTO model)
        {
            try
            {
                ItemRequisition itemRequisition = _mapper.Map<ItemRequisition>(model);

                itemRequisition.ToWarehouse = null;
                itemRequisition.ToSubInventory = null;
                itemRequisition.ToWareHouseLocation = null;
                itemRequisition.ToWareHouseSubLocation = null;

                itemRequisition.FromWarehouse = null;
                itemRequisition.FromSubInventory = null;
                itemRequisition.FromWareHouseLocation = null;
                itemRequisition.FromWareHouseSubLocation = null;

                itemRequisition.MSTItemRequisitionStatus = null;

                if (model.Id == 0)
                {
                    //var code = new CodeGenerationDTO { Code = "PURCHASE_ORDER", GCode = " " };
                    var code = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.ITEM_REQUISITION), GCode = "" };
                    itemRequisition.ItemRequisitionNumber =  _codeGenerationTemplateService.GenerateCode(code).Result.GCode ;
                    

                    if (model.IsFromProd == true)
                    {
                        var WIPDefaultLocation = _wareHouseDefaultLocationRepository.GetAll(x => x.IsDeleted != true && x.Code == MainEnumHelper.GetDescription(Enums.DefaultLocationCode.WIP)).FirstOrDefault();
                        var WIPDefaultWareHouseLocation= _wareHouseLocationRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.WareHouseId == WIPDefaultLocation.SubInventoryId && x.IsDefault == true).FirstOrDefault();
                        if(WIPDefaultLocation != null)
                        {
                            itemRequisition.ToWarehouseId = WIPDefaultLocation.WareHouseId ?? 0;
                            itemRequisition.ToSubInventoryId = WIPDefaultLocation.SubInventoryId ?? 0;
                            itemRequisition.ToWareHouseLocationId = WIPDefaultWareHouseLocation != null ? WIPDefaultWareHouseLocation.MSTWareHouseLocationId : 0;
                        }
                    }

                    List<ItemRequisitionDetail> itemRequisitionDetailList = new List<ItemRequisitionDetail>();

                    if(model.ItemForm.Count() > 0)
                    {
                        foreach (var data in model.ItemForm)
                        {
                            ItemRequisitionDetail itemRequisitionDetail = new ItemRequisitionDetail()
                            {
                                ItemId = data.ItemId ?? 0,
                                Quantity = data.Quantity ?? 0,
                                UnitId = data.UnitId ?? 0,
                                Remarks = data.Remarks,
                                Price = data.Price,
                                IsActive = true,
                                IsDeleted = false,
                                BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId,
                                CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                                CreatedOn = DateTime.Now,
                                TotalPrice = data.Quantity * data.Price,
                                RemainingQantity = data.Quantity,
                                AvailableQuantity = 0
                        };
                            itemRequisitionDetailList.Add(itemRequisitionDetail);
                        }
                        itemRequisition.ItemRequisitionDetails = itemRequisitionDetailList;
                    }
                    itemRequisition.TotalAmount = itemRequisition.ItemRequisitionDetails.Sum(x => x.TotalPrice);
                    await _itemRequisitionRepository.Add(itemRequisition);
                    await _unitOfWork.Commit();
                }
                return itemRequisition.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> DeleteItemRequisationDetails(long id)
        {
            try
            {
                if (id != 0)
                {
                    ItemRequisitionDetail ItemRequisitiondetails = _itemRequisitionDetailRepository.GetById(x => x.Id == id && x.IsDeleted!=true);
                    if (ItemRequisitiondetails != null)
                    {
                        // _itemRequisitionRepository.Delete(ItemRequisition);
                        ItemRequisitiondetails.IsDeleted = true;
                        await _unitOfWork.Commit();
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> Delete(long id)
        {
            try
            {
                if (id != 0)
                {
                    ItemRequisition ItemRequisition = _itemRequisitionRepository.GetById(x => x.Id == id && x.IsDeleted == false);
                    if (ItemRequisition != null)
                    {
                        // _itemRequisitionRepository.Delete(ItemRequisition);
                        ItemRequisition.IsDeleted = true;
                        await _unitOfWork.Commit();
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<ItemRequisitionDTO> GetAll()
        {
            try
            {
                IEnumerable<ItemRequisition> ItemRequisitions = _itemRequisitionRepository.GetAll(x => x.IsDeleted == false);
                return _mapper.Map<IEnumerable<ItemRequisitionDTO>>(ItemRequisitions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ItemRequisitionDTO> GetItemRequisitionById(long id)
        {
            try
            {
                ItemRequisitionDTO dtos = new ItemRequisitionDTO();
                if (id != null || id > 0)
                {
                    ItemRequisition itemRequisition = _itemRequisitionRepository.GetAll(x => x.Id == id && x.IsDeleted !=true, x => x.ItemRequisitionDetails).Include(x => x.ItemRequisitionTasks).ThenInclude(x => x.ProcessTask).ThenInclude(x => x.PickingLists).FirstOrDefault();
                    List<Item> items = _itemRepository.GetAll(x => x.IsDeleted != true).ToList();
                    if (itemRequisition != null)
                    {
                        long pId = itemRequisition.ItemRequisitionTasks != null && itemRequisition.ItemRequisitionTasks.ProcessTask != null && itemRequisition.ItemRequisitionTasks.ProcessTask.PickingLists.Count() > 0 ? itemRequisition.ItemRequisitionTasks.ProcessTask.PickingLists.FirstOrDefault().Id : 0;
                        dtos = _mapper.Map<ItemRequisitionDTO>(itemRequisition);

                        var pickListDetails = _pickingListRepository.GetAll(x => x.Id == pId, x => x.PickingListDetails).FirstOrDefault();
                         if(pickListDetails.PickingListDetails.Count() > 0) 
                        {
                            dtos.PickingListDetailsId = pickListDetails.PickingListDetails.Select(x => x.Id).ToList();
                         }
                        dtos.PickingListId = pId;
                        dtos.ItemForm = itemRequisition.ItemRequisitionDetails.Where(x=>x.IsDeleted!=true).Select(x => new ItemRequisitionDetailItem()
                        {
                            Id = x.Id ,
                            ItemId = x.ItemId,
                            ItemName = x.ItemId > 0 ? items.FirstOrDefault(y => y.Id == x.ItemId).Name : null,
                            UnitId = x.UnitId,
                            Price = x.Price,
                            Quantity = x.Quantity,
                            Remarks = x.Remarks,
                            RemainingQantity=x.RemainingQantity,
                            ReservedQuantity=x.ReservedQuantity,
                            AvailableQuantity=x.AvailableQuantity,
                            LastCheckedDate=x.LastCheckedDate,
                        }).ToList();
                        return dtos;
                    }
                    return dtos;
                }
                return dtos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

      

        public async Task<long> UpdateItemRequisition(ItemRequisitionDTO model)
        {
            try
            {
                ItemRequisition itemRequisition = _itemRequisitionRepository.GetAll(x => x.Id == model.Id && x.IsDeleted != true, x=> x.ItemRequisitionDetails).FirstOrDefault();
                if (itemRequisition == null) return 0;
                //var itemdetailslist = itemRequisition.ItemRequisitionDetails.Where( x => model.ItemForm.Any(y => y.Id != x.Id)).Select(x => x.Id);
                if (itemRequisition != null)
                {
                    itemRequisition.FromWarehouseId = model.FromWarehouseId;
                    itemRequisition.ToWarehouseId = model.ToWarehouseId;
                    itemRequisition.FromSubInventoryId = model.FromSubInventoryId;
                    itemRequisition.ToSubInventoryId = model.ToSubInventoryId;
                    itemRequisition.FromWareHouseLocationId = model.FromWareHouseLocationId;
                    itemRequisition.ToWareHouseLocationId = model.ToWareHouseLocationId;
                    itemRequisition.FromWareHouseSubLocationId = model.FromWareHouseSubLocationId;
                    itemRequisition.ToWareHouseSubLocationId = model.ToWareHouseSubLocationId;
                    itemRequisition.MSTItemRequisitionStatusId = model.MSTItemRequisitionStatusId;
                    itemRequisition.Remarks = model.Remarks;
                    itemRequisition.Name = model.Name;
                    itemRequisition.TotalAmount = model.TotalAmount;
                    itemRequisition.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
                    itemRequisition.ItemRequisitionDate = model.ItemRequisitionDate;

                    itemRequisition.VerifiedBy = model.VerifiedBy;
                    //itemRequisition.ActualDeliveryDate = model.ActualDeliveryDate;

                    foreach (var data in model.ItemForm)
                    {
                        if(data.Id > 0)
                        {
                            ItemRequisitionDetail detail = _itemRequisitionDetailRepository.GetById(data.Id);
                            if (detail == null) return 0 ;
                            detail.ItemId = data.ItemId ?? 0;
                            detail.Quantity = data.Quantity ?? 0;
                            //detail.UnitId = data.UnitId ?? 0;
                            detail.Remarks = data.Remarks;
                            detail.Price = data.Price;
                            detail.BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId;
                            detail.UpdatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId;
                            detail.UpdatedOn = DateTime.Now;
                            detail.TotalPrice = data.Quantity * data.Price;
                            detail.RemainingQantity = data.Quantity;
                            detail.AvailableQuantity = 0;
                        }
                        else
                        {
                            ItemRequisitionDetail itemRequisitionDetail = new ItemRequisitionDetail()
                            {
                                ItemId = data.ItemId ?? 0,
                                Quantity = data.Quantity ?? 0,
                                ItemRequisitionId = model.Id,
                                UnitId = data.UnitId ?? 0,
                                Remarks = data.Remarks,
                                Price = data.Price,
                                IsActive = true,
                                IsDeleted = false,
                                BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId,
                                CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                                CreatedOn = DateTime.Now,
                                TotalPrice = data.Quantity * data.Price,
                                RemainingQantity = data.Quantity,
                                AvailableQuantity = 0
                            };
                            await _itemRequisitionDetailRepository.Add(itemRequisitionDetail);
                        }

                        
                    }
                    await _unitOfWork.Commit();
                    var itemreq = _itemRequisitionRepository.GetAll( x=> x.IsDeleted != true && x.Id == model.Id , x => x.ItemRequisitionDetails).FirstOrDefault();
                    if (itemreq != null && itemreq.ItemRequisitionDetails.Count() > 0)
                    {
                        itemreq.TotalAmount = itemreq.ItemRequisitionDetails.Sum(x => x.TotalPrice);
                        await _unitOfWork.Commit();
                    }

                }
                return model.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ReseveditemDTO>> GetWareHouseReservedItems(ItemRquestionProc model)
        {
            try
            {


                if (model != null)
                {
                    List<ReseveditemDTO> reseveditems =new List<ReseveditemDTO>();
                    var connectionString = Configuration.GetConnectionString("DefaultConnection");
                    System.Data.SqlClient.SqlParameter[] spParameter = new System.Data.SqlClient.SqlParameter[2];
                    spParameter[0] = new System.Data.SqlClient.SqlParameter("@TableName", SqlDbType.NVarChar)
                    {
                        Value = model.TableName
                    };
                    spParameter[1] = new System.Data.SqlClient.SqlParameter("@ObjectId", SqlDbType.BigInt)
                    {
                        Value = model.ObjectId
                    };
                  
                    var ds = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "[dbo].[sp_GetWareHouseReservedItems]", spParameter);
                    var result = ds.Tables[0];
                    reseveditems = getitemReserved(result);
                    return reseveditems;
                }
                return new List<ReseveditemDTO>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private List<ReseveditemDTO> getitemReserved(DataTable result)
        {
            var Item = result.AsEnumerable().Select(row =>
                         new ReseveditemDTO
                         {
                             ID = (row.Field<long>("ID")),
                             ITEMCODE = (row.Field<string>("ITEMCODE")),
                             RemainingQunatity = (row.Field<decimal?>("RemainingQantity")),
                             NAME = (row.Field<string>("NAME")),
                             RequiredQuantity = (row.Field<decimal?>("RequiredQuantity")),
                             ActualReservedQunatity = (row.Field<decimal?>("ActualReservedQuantity")),
                             ReservedQunatity = (row.Field<decimal?>("ReservedQuantity")),
                         }).ToList();
            return Item;
        }

        public async Task<QueryResult<ItemRequisitionDTO>> Search(ItemRequisitionQueryObject query)
        {
            try
            {
                if (string.IsNullOrEmpty(query.SortBy))
                {
                    query.SortBy = "Id";
                }

                var columnMap = new Dictionary<string, Expression<Func<ItemRequisitionDTO, object>>>()
                {
                    ["Id"] = p => p.Id,
                    ["Name"]=p => p.Name,
                    ["FromWarehouseName"]=p=>p.FromWarehouseName,
                    ["FromSubInventoryName"]=p=>p.FromSubInventoryName,
                    ["FromWareHouseLocationName"]=p=>p.FromWareHouseLocationName,
                    ["FromWareHouseSubLocationName"] = p => p.FromWareHouseSubLocationName,
                    ["ToWarehouseName"] = p => p.ToWarehouseName,
                    ["ToSubInventoryName"] = p => p.ToSubInventoryName,
                    ["ToWareHouseLocationName"] = p => p.ToWareHouseLocationName,
                    ["ToWareHouseSubLocationName"] = p => p.ToWareHouseSubLocationName,
                    ["MSTItemRequisitionStatusName"] = p => p.MSTItemRequisitionStatusName,
                    ["FromWarehouseId"] = p => p.FromWarehouseId,
                    ["FromSubInventoryId"] = p => p.FromSubInventoryId,
                    ["FromWareHouseLocationId"] = p => p.FromWareHouseLocationId,
                    ["FromWareHouseSubLocationId"] = p => p.FromWareHouseSubLocationId,
                    ["ToWarehouseId"] = p => p.ToWarehouseId,
                    ["ToSubInventoryId"] = p => p.ToSubInventoryId,
                    ["ToWareHouseLocationId"] = p => p.ToWareHouseLocationId,
                    ["ToWareHouseSubLocationId"] = p => p.ToWareHouseSubLocationId,
                    ["MSTItemRequisitionStatusId"] = p => p.MSTItemRequisitionStatusId,
                    ["ItemRequisitionNumber"] = p => p.ItemRequisitionNumber,
                    ["ExpectedDeliveryDate"] = p => p.ExpectedDeliveryDate,
                    ["ItemRequisitionDate"] = p => p.ItemRequisitionDate,
                    ["ActualDeliveryDate"] = p => p.ActualDeliveryDate,
                    ["TotalAmount"] = p => p.TotalAmount,
                    ["Remarks"] = p => p.Remarks,


                };

                var itemRequisitionList = _itemRequisitionRepository.GetAll(x => x.IsDeleted !=true).Include(a=>a.ToWareHouseLocation).Include(a=>a.ToWarehouse).Include(a=>a.ToSubInventory).Include(a=>a.ToWareHouseSubLocation)
                    .Include(a => a.FromWarehouse).Include(a => a.FromSubInventory).Include(a => a.FromWareHouseLocation).Include(a => a.FromWareHouseSubLocation).Include(a=>a.MSTItemRequisitionStatus).AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchString))
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.FromWarehouse.Name.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                                                        || x.ToWarehouse.Name.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                                                        || x.FromSubInventory.Name.ToLower().Equals(query.SearchString.Trim().ToLower())
                                                        || x.ToSubInventory.Name.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                                                        || x.FromWareHouseLocation.Name.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                                                        || x.ToWareHouseLocation.Name.ToLower().Equals(query.SearchString.Trim().ToLower())
                                                        || x.FromWareHouseSubLocation.Name.Trim().ToLower().Contains(query.SearchString.Trim().ToLower()) 
                                                        || x.ToWareHouseSubLocation.Name.ToLower().Equals(query.SearchString.Trim().ToLower())
                                                        || x.MSTItemRequisitionStatus.Name.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                                                        || x.Name.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                                                        );
                }
                if (query.IsActive != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.IsActive == query.IsActive);
                }
                if (query.FromWarehouseId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.FromWarehouseId == query.FromWarehouseId);
                }
                if (query.FromSubInventoryId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.FromSubInventoryId == query.FromSubInventoryId);
                }
                if (query.FromWareHouseLocationId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.FromWareHouseLocationId == query.FromWareHouseLocationId);
                }
                if (query.FromWareHouseSubLocationId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.FromWareHouseSubLocationId == query.FromWareHouseSubLocationId);
                }
                if (query.ToWarehouseId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.ToWarehouseId == query.ToWarehouseId);


                }
                if (query.ToSubInventoryId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.ToSubInventoryId == query.ToSubInventoryId);
                }
                if (query.ToWareHouseLocationId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.ToWareHouseLocationId == query.ToWareHouseLocationId);
                }
                if (query.ToWareHouseSubLocationId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.ToWareHouseSubLocationId == query.ToWareHouseSubLocationId);
                }
                if (query.MSTItemRequisitionStatusId != null)
                {
                    itemRequisitionList = itemRequisitionList.Where(x => x.MSTItemRequisitionStatusId == query.MSTItemRequisitionStatusId);
                }
                var itemList = itemRequisitionList.Select(x => new ItemRequisitionDTO()
                {
                    Id = x.Id,
                    Name=x.Name,
                    FromWarehouseName = x.FromWarehouse.Name,
                    FromSubInventoryName = x.FromSubInventory.Name,
                    FromWareHouseLocationName = x.FromWareHouseLocation.Name,
                    FromWareHouseSubLocationName = x.FromWareHouseSubLocation.Name,
                    ToWarehouseName = x.ToWarehouse.Name,
                    ToSubInventoryName = x.ToSubInventory.Name,
                    ToWareHouseLocationName = x.ToWareHouseLocation.Name,
                    ToWareHouseSubLocationName = x.ToWareHouseSubLocation.Name,
                    MSTItemRequisitionStatusName=x.MSTItemRequisitionStatus.Name,
                    ItemRequisitionNumber=x.ItemRequisitionNumber,
                    ItemRequisitionDate=x.ItemRequisitionDate,
                    ExpectedDeliveryDate=x.ExpectedDeliveryDate,
                    ActualDeliveryDate=x.ActualDeliveryDate,
                    TotalAmount=x.TotalAmount,
                    Remarks=x.Remarks,
                }).AsQueryable();
                var result = await itemList.ApplyOrdering(query, columnMap).ToListAsync();

                var filterdatacount = itemRequisitionList.Count();
                var pagination = _mapper.Map<List<ItemRequisitionDTO>>(result);

                var queryResult = new QueryResult<ItemRequisitionDTO>
                {
                   TotalItems = itemList.Count(),
                    Items = pagination
                };
                return queryResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<long> GenerateTask(long id)
        {
            try
            {
                if(id == 0) return 0;
                var userId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId;

                ItemRequisition itemreq = _itemRequisitionRepository.GetById(id);
                itemreq.IsAccepted = true;

                var code = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.ITEM_REQUISITION_TASK), GCode = "" };

                ProcessTask task = new ProcessTask()
                {
                    TaskNumber = _codeGenerationTemplateService.GenerateCode(code).Result.GCode,
                    Name = "Picking Task for Item Requisition : " + id,
                    Description = "Picking Task for Item Requisition : " + id,
                    CreatedFromProcess = "Item Requisition",
                    AssignedBy = userId,
                    AssignedOn = DateTime.Now,
                    TaskDate = DateTime.Now,
                    SendEmailNotification = false,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId,
                    MSTProcessTaskStatusId = (int)Enums.MST_ProcessTaskStatus.Pending,
                    MSTProcessTaskPriorityId = (int)Enums.MSTProcessTaskPriorityStatus.MEDIUM,
                    MSTProcessTaskTypeId = (int)Enums.MSTProcessTaskType.Item_Requisition,
                    TaskStartedOn = DateTime.Now,
                    
                };
                await _processTaskRepository.Add(task);
                await _unitOfWork.Commit();

                ItemRequistionTask itemRequistionTask = new ItemRequistionTask()
                {
                    ItemRequistionId = id,
                    ProcessTaskId = task.Id,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };

                await _itemRequistionTaskRepository.Add(itemRequistionTask);
                await _unitOfWork.Commit();
                //var pickingId = await GeneratePicking(id, task.Id);
                return task.Id;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<long> GeneratePicking(long ItemReqId, long processTaskId)
        {

            try
            {
                var userId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId;
                //WebSetting websettings = _webSettingRepository.GetAll().ToList().FirstOrDefault();
                var itemReq = _itemRequisitionRepository.GetById(x => x.Id == ItemReqId);
                //var data = _itemRequisitionDetailRepository.GetAll(x => x.ItemRequisitionId == ItemReqId).ToList();
                var code = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.ITEM_REQUISITION_PICKING_LIST), GCode = "" };
                PickingList plist = new PickingList()
                {
                    Code = _codeGenerationTemplateService.GenerateCode(code).Result.GCode,
                    SourceWareHouseId = itemReq.FromWarehouseId,
                    SourceSubInventoryId = itemReq.FromSubInventoryId,
                    SourceWareLocationId = itemReq.FromWareHouseLocationId,
                    SourceWareHouseSubLocationId = itemReq.FromWareHouseSubLocationId,
                    DeliverToWareHouseId = itemReq.ToWarehouseId,
                    DeliverToSubInventoryId = itemReq.ToSubInventoryId,
                    DeliverToWareLocationId = itemReq.ToWareHouseLocationId,
                    DeliverToWareHouseSubLocationId = itemReq.ToWareHouseSubLocationId,
                    ProcessTaskId = processTaskId,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,

                };

                //var webdata = _webSettingRepository.GetAll().ToList().FirstOrDefault();
                var ItemReqDetails = _itemRequisitionDetailRepository.GetAll(x => x.ItemRequisitionId == ItemReqId && x.IsDeleted != true).ToList();
                List<PickingListDetail> plistDetails = new List<PickingListDetail>();
                foreach (var item in ItemReqDetails)
                {
                    //WareHouseReservedItem wareHouseReservedItem = _warehouseReservedItemRepository.GetById(a => a.ObjectId == item.Id && a.ObjectName == "ItemRequisitionDetail" && a.ItemId == item.ItemId && a.IsDeleted != true, x => x.ItemTransactionDetail);
                    //if (wareHouseReservedItem == null) return 0;
                    //PickingListDetail plistdetail = new PickingListDetail()
                    //{
                    //    ItemId = item.ItemId,
                    //    PalletIDNumber = wareHouseReservedItem.PalletIDNumber,
                    //    CaseNumber = wareHouseReservedItem.CaseNumber,
                    //    CreatedBy = userId,
                    //    CreatedOn = DateTime.Now,
                    //    IsActive = true,
                    //    IsDeleted = false,
                    //    BoxStatus = wareHouseReservedItem.BoxStatus,
                    //    ObjectName = wareHouseReservedItem.ObjectName,
                    //    ObjectId = wareHouseReservedItem.ObjectId,
                    //    RequiredQuantity = wareHouseReservedItem.Quantity,
                    //    WareHouseId = wareHouseReservedItem.ItemTransactionDetail != null ? wareHouseReservedItem.ItemTransactionDetail.WareHouseId : 0,
                    //    SubInventoryId = wareHouseReservedItem.ItemTransactionDetail != null ? wareHouseReservedItem.ItemTransactionDetail.SubInventoryId : 0,
                    //    WareHouseLocationId = wareHouseReservedItem.ItemTransactionDetail != null ? wareHouseReservedItem.ItemTransactionDetail.WareHouseLocationId : 0,
                    //    WareHouseSubLocationId = wareHouseReservedItem.ItemTransactionDetail != null ? wareHouseReservedItem.ItemTransactionDetail.WareHouseSubLocationId : 0,
                    //    TransactionDetailId = wareHouseReservedItem.ItemTransactionDetailId
                    //};
                    //plistDetails.Add(plistdetail);
                    //wareHouseReservedItem.PickingCompletionDate = DateTime.Now;
                    //wareHouseReservedItem.IsPickingCompleted = true;


                    //Multiple splitted Quantity of same Item is Reserved.



                    List<WareHouseReservedItem> wareHouseReservedItem = _warehouseReservedItemRepository.GetAll(a => a.ObjectId == item.Id && a.ObjectName == "ItemRequisitionDetail" && a.ItemId == item.ItemId && a.IsDeleted != true, x => x.ItemTransactionDetail).ToList();
                    if (wareHouseReservedItem.Count() == 0) return 0;
                    foreach (var witem in wareHouseReservedItem)
                    {
                        PickingListDetail plistdetail = new PickingListDetail()
                        {
                            ItemId = item.ItemId,
                            PalletIDNumber = witem.PalletIDNumber,
                            CaseNumber = witem.CaseNumber,
                            CreatedBy = userId,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            BoxStatus = witem.BoxStatus,
                            ObjectName = witem.ObjectName,
                            ObjectId = witem.ObjectId,
                            RequiredQuantity = witem.Quantity,
                            WareHouseId = witem.ItemTransactionDetail != null ? witem.ItemTransactionDetail.WareHouseId : 0,
                            SubInventoryId = witem.ItemTransactionDetail != null ? witem.ItemTransactionDetail.SubInventoryId : 0,
                            WareHouseLocationId = witem.ItemTransactionDetail != null ? witem.ItemTransactionDetail.WareHouseLocationId : 0,
                            WareHouseSubLocationId = witem.ItemTransactionDetail != null ? witem.ItemTransactionDetail.WareHouseSubLocationId : 0,
                            //WareHouseId = itemReq.FromWarehouseId,
                            //SubInventoryId = itemReq.FromSubInventoryId,
                            //WareHouseLocationId = itemReq.FromWareHouseLocationId,
                            //WareHouseSubLocationId = itemReq.FromWareHouseSubLocationId,
                            TransactionDetailId = witem.ItemTransactionDetailId,
                            KittingTransactionDetailId = witem.ItemTransactionDetailId
                        };
                        plistDetails.Add(plistdetail);
                        witem.PickingCompletionDate = DateTime.Now;
                        witem.IsPickingCompleted = true;
                    }






                    /*
                    var itemId = item.ItemId;
                    List<WareHouseReservedItem> warehouseReservedItems = _warehouseReservedItemRepository.GetAll().ToList();
                    List<ItemTransaction> itemTransaction = _itemTransactionRepository.GetAll(x => x.ItemId == itemId && x.TransactionType == "IN" &&

                    (x.ItemTransactionDetails.WareHouseId == webdata.ItemWareHouseId || x.ItemTransactionDetails.SubInventoryId == webdata.ItemSubInventoryId || x.ItemTransactionDetails.WareHouseLocationId == webdata.ItemWareHouseLocationId ||
                    x.ItemTransactionDetails.WareHouseSubLocationId == webdata.ItemWareHouseSubLocationId), x => x.ItemTransactionDetails).OrderBy(x => x.Id).ToList();

                    itemTransaction.RemoveAll(item => warehouseReservedItems.Any(item2 => item.ObjectName == item2.ObjectName && item.ObjectId == item2.ObjectId && item.ItemId == item2.ItemId));
                    Decimal count = 0;
                    foreach (var items in itemTransaction)
                    {
                        if (count < item.Quantity)
                        {
                            count += items.Quantity ?? 0;

                            WareHouseReservedItem wareHouseReservedItem = new WareHouseReservedItem()
                            {
                                ItemId = itemId,
                                PalletIDNumber = items.ItemTransactionDetails.PalletIDNumber,
                                CaseNumber = items.ItemTransactionDetails.CaseNumber,
                                CreatedBy = userId,
                                CreatedOn = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                BoxStatus = count < item.Quantity ? "F" : "H",
                                ObjectName = items.ObjectName,
                                ObjectId = items.ObjectId,
                                Quantity = item.Quantity,
                            };

                            await _warehouseReservedItemRepository.Add(wareHouseReservedItem);

                            PickingListDetail plistdetail = new PickingListDetail()
                            {
                                ItemId = itemId,
                                PalletIDNumber = items.ItemTransactionDetails.PalletIDNumber,
                                CaseNumber = items.ItemTransactionDetails.CaseNumber,
                                CreatedBy = userId,
                                CreatedOn = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                BoxStatus = count < item.Quantity ? "F" : "H",
                                ObjectName = items.ObjectName,
                                ObjectId = items.ObjectId,
                                RequiredQuantity = item.Quantity,
                                WareHouseId = items.ItemTransactionDetails.WareHouseId,
                                SubInventoryId = items.ItemTransactionDetails.SubInventoryId,
                                WareHouseLocationId = items.ItemTransactionDetails.WareHouseLocationId,
                                WareHouseSubLocationId = items.ItemTransactionDetails.WareHouseSubLocationId,
                            };
                            plistDetails.Add(plistdetail);
                        }
                        else break;

                    }*/



                }

                plist.PickingListDetails = plistDetails;
                await _pickingListRepository.Add(plist);
                await _unitOfWork.Commit();
                return plist.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ItemRequisitionDetailDTO>> GetItemRequisitionDetail(long ItemRequisitionId)
        {
            try
            {
                if (ItemRequisitionId > 0)
                {
                    List<ItemRequisitionDetailDTO> data = new List<ItemRequisitionDetailDTO>();
                    var  details = _itemRequisitionDetailRepository.FindBy(x => x.ItemRequisitionId == ItemRequisitionId).ToList();
                    if (details != null)
                    {
                        foreach(var item in details)
                        {
                           data.Add( _mapper.Map<ItemRequisitionDetailDTO>(item));
                        }
                        return data;
                    }
                }
                return new List<ItemRequisitionDetailDTO>();

            }catch(Exception e)
            {
                throw (e);
            }
        }

        public async Task<bool> CheckAvailablity(long ItemReqId)
        {

            try
            {
                var IsSufficientAvailble = false;
                if (ItemReqId > 0)
                {
                    var itemReq = _itemRequisitionRepository.GetById(x => x.Id == ItemReqId);
                    var data = _itemRequisitionDetailRepository.GetAll(x => x.ItemRequisitionId == ItemReqId).ToList();
                    //var webdata = _webSettingRepository.GetAll().ToList().FirstOrDefault();
                    var itemStockLists = await _itemService.GetCurrentStock(new ItemCurrentStockDTO()
                    {
                        WareHouseId = itemReq.FromWarehouseId,
                        SubInventoryId = itemReq.FromSubInventoryId
                    });
                    IsSufficientAvailble = data.Where(a => a.RemainingQantity > 0).Count() == 0;
                    if (IsSufficientAvailble == false)
                    {
                        IsSufficientAvailble = true;

                        foreach (var item in data.Where(a => a.RemainingQantity > 0))
                        {
                            ItemCurrentStockResultDTO itemCurrentStockResultDTO = itemStockLists.FirstOrDefault(a => a.ItemId == item.ItemId);
                            if (itemCurrentStockResultDTO != null)
                            {
                                item.AvailableQuantity = itemCurrentStockResultDTO.StockQuantity;
                                item.LastCheckedDate = DateTime.Now;
                                await _unitOfWork.Commit();
                                if (item.RemainingQantity > itemCurrentStockResultDTO.StockQuantity)
                                    IsSufficientAvailble = false;
                            }
                            else
                                IsSufficientAvailble = false;
                        }
                    }
                    itemReq.MSTItemRequisitionStatusId = IsSufficientAvailble == true ? (int?)ItemRequitionStatus.ITEMAVIALABLE : (int?)ItemRequitionStatus.ITEMSHORTAGE;
                    
                    await _unitOfWork.Commit();
                    return IsSufficientAvailble;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> ReserveItems(long ItemReqId)
        {

            try
            {
                if (ItemReqId > 0)
                {
                    var itemReq = _itemRequisitionRepository.GetById(x => x.Id == ItemReqId);
                    var data = _itemRequisitionDetailRepository.GetAll(x => x.ItemRequisitionId == ItemReqId).ToList();
                    ItemReserveDTO itemReserveDTO = new ItemReserveDTO();
                    itemReserveDTO.ItemReserveDetailDTOs = new List<ItemReserveDetailDTO>();
                    itemReserveDTO.SourceWareHouseId = itemReq.FromWarehouseId;
                    itemReserveDTO.SourceSubInventoryId = itemReq.FromSubInventoryId;
                    itemReserveDTO.SourceWareHouseLocationId = itemReq.FromWareHouseLocationId;
                    itemReserveDTO.SourceWareHouseSubLocationId = itemReq.FromWareHouseSubLocationId;
                    foreach (var item in data)
                    {
                        decimal? qty = item.AvailableQuantity < item.RemainingQantity ? (item.AvailableQuantity ?? 0) : item.RemainingQantity;
                        if (qty > 0)
                        {
                            itemReserveDTO.ItemReserveDetailDTOs.Add(new ItemReserveDetailDTO()
                            {
                                ItemId = item.ItemId,
                                ObjectId = item.Id,
                                ObjectName = "ItemRequisitionDetail",
                                Quantity = qty
                            });
                        }
                    }
                    var res = await _itemTransactionService.ReserveItemTransNItemTransDetail(itemReserveDTO);

                    if (res)
                    {
                        itemReq.MSTItemRequisitionStatusId = (int?)ItemRequitionStatus.ITEMRESERVED;
                        await _unitOfWork.Commit();
                    }
                    return res;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GeneratePdfTemplateItemRequisition(QueryResult<ItemRequisitionDTO> result)
        {
            var sb = new StringBuilder();

            sb.Append(@"<html>
                            <head>
                                <h1>List Item Requisition</h1>
                            </head>
                            <body>
                                <table border='1' align='center' style='border-collapse:collapse;'>");
            sb.Append(@"<thead>
                            <tr>
                                        <th>Id</th>
                                        <th>Item Requisition Number</th>
                                        <th>Total Amount</th>
                                        <th>Remarks</th>
                                        <th>From Warehouse Name</th>
                                        <th>from SubInventory Name</th>
                                        <th>From WareHouse Location Name</th>
                                        <th>From WareHouse Sub-Location Name</th>
                                        <th>To Warehouse Name</th>
                                        <th>To SubInventory Name</th>
                                        <th>To WareHouse Location Name</th>
                                        <th>To WareHouse Sub-Location Name</th>
                                        <th>Item Requisition Status Name</th>
                                        <th>Item Requisition Date</th>
                                        <th>Expected Delivery Date</th>
                                        <th>Actual Delivery Date</th>
                                       
                                       
                           </tr></thead>");
            foreach (var item in result.Items)
            {


                sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    <td>{6}</td>
                                    <td>{7}</td>
                                    <td>{8}</td>
                                    <td>{9}</td>
                                    <td>{10}</td>
                                    <td>{11}</td>
                                    <td>{12}</td>
                                    <td>{13}</td>
                                    <td>{14}</td>
                                    <td>{15}</td>
                                  
                                   
                     
                                   </tr>", item.Id, item.ItemRequisitionNumber, item.TotalAmount, item.Remarks, item.FromWarehouseName, item.FromSubInventoryName, item.FromWareHouseLocationName, item.FromWareHouseSubLocationName, item.ToWarehouseName, item.ToSubInventoryName, item.ToWareHouseLocationName, item.ToWareHouseSubLocationName,
                                            item.MSTItemRequisitionStatusName, item.ItemRequisitionDate, item.ExpectedDeliveryDate, item.ActualDeliveryDate);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }

        public string GeneratePdfTemplateProductionItemRequisition(QueryResult<ItemRequisitionDTO> result)
        {
            var sb = new StringBuilder();

            sb.Append(@"<html>
                            <head>
                                <style>
                                       
                                        thead {
                                            display: table-header-group;
                                        }

                                        tr {
                                            page-break-inside: avoid;
                                        }
                                    </style>
                                <h1>Item Requisition</h1>
                            </head>
                            <body>
                                <table border='1' align='center' style='border-collapse:collapse;'>");
            sb.Append(@"<thead>
                            <tr>
                                        <th>Id</th>
                                        <th>Item Requisition Number</th>
                                        <th>Total Amount</th>
                                        <th>Remarks</th>
                                        <th>From Warehouse</th>
                                        <th>from SubInventory </th>
                                        <th>From Location </th>

                                        <th>To Warehouse</th>
                                        <th>To SubInventory </th>
                                        <th>To Location</th>

                                        <th>Item Requisition Status </th>
                                        <th>Item Requisition Date</th>
                                        <th>Expected Delivery Date</th>
                                        <th>Actual Delivery Date</th>
                                       
                                       
                           </tr></thead>");
            foreach (var item in result.Items)
            {


                sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    <td>{6}</td>
                                    <td>{7}</td>
                                    <td>{8}</td>
                                    <td>{9}</td>
                                    <td>{10}</td>
                                    <td>{11}</td>
                                    <td>{12}</td>
                                    <td>{13}</td>
                                  
                                   
                     
                                   </tr>", item.Id, item.ItemRequisitionNumber, item.TotalAmount, item.Remarks, item.FromWarehouseName, item.FromSubInventoryName, item.FromWareHouseLocationName, item.ToWarehouseName, item.ToSubInventoryName, item.ToWareHouseLocationName,
                                            item.MSTItemRequisitionStatusName, item.ItemRequisitionDate, item.ExpectedDeliveryDate, item.ActualDeliveryDate);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }
    }
}
