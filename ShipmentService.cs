using AutoMapper;
using LogicLync.DTO;
using LogicLync.Entities;
using LogicLync.Repository;
using LogicLync.Repository.Infrastructure;
using LogicLync.Repository.Repository;
using LogicLync.Service.Extension;
using LogicLync.Service.HelperClasses;
using LogicLync.Service.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static LogicLync.Entities.Enums;

namespace LogicLync.Service
{
    public class ShipmentService:IShipmentService
    {
        public IConfiguration Configuration { get; }
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IShipmentApprovalLogRepository _approvalLogRepository;
        private readonly IApprovalAssignmentDetailRepository _approvalAssignmentDetailRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPushQueueRepository _pushQueueRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IShipmentPackingListRepository _shipmentPackingListRepository;
        private readonly IPackingListDetailRepository _packingListDetailRepository;
        private readonly IShipmentDetailRepository _shipmentDetailRepository;
        private readonly IPOQCTaskRepository _pOQCTaskRepository;
        private readonly IPackingListRepository _packingListRepository;
        private readonly ISemiFinishGoodsPalletRepository _semiFinishGoodsPalletRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserDetailsRepository _userDetailsRepository;
        private readonly IPackingListLogRepository _packingListLogRepository;
        private readonly IPutAwayTaskDetailsRepository _putAwayTaskDetailsRepository;
        private readonly IPutAwayTaskRepository _putAwayTaskRepository;
        private readonly IActionLogService _actionLogService;
        private readonly IMST_CodeGenerationTemplateService _codeGenerationTemplateService;
        private readonly IShipmentPlanPalletRepository _shipmentPlanPalletRepository;
        private readonly IMST_WareHouseDefaultLocationRepository _mst_WareHouseDefaultLocationRepository;
        private readonly IItemTransactionService _itemTransactionService;
        private readonly IShipmentTaskRepository _shipmentTaskRepository;
        private readonly IWebSettingRepository _webSettingRepository;
        private readonly IMST_ApprovalAssignmentStatusRepository _mst_ApprovalAssignmentStatusRepository;
        private readonly IProcessTaskService _processTaskService;
        private readonly IPickingListRepository _pickingListRepository;
        private readonly IWareHouseReservedItemRepository _wareHouseReservedItemRepository;
        private readonly IItemTransactionDetailRepository _itemTransactionDetailRepository;
        private readonly IPickingListDetailRepository _pickingListDetailRepository;
        private readonly IBarcodeService _barcodeService;
        private readonly ITruckLoadingSheetRepository _truckLoadingSheetRepository;
        private readonly ISalesOrderPlanTaskRepository _salesOrderPlanTaskRepository;
        private readonly IProcessTaskRepository _processTaskRepository;
        private readonly IWareHouseLocationRepository _wareHouseLocationRepository;
        private readonly ISalesOrderPlanRepository _salesOrderPlanRepository;
        public ShipmentService(IShipmentRepository shipmentRepository, IUnitOfWork unitOfWork, IMapper mapper, IVehicleRepository vehicleRepository,ICompanyRepository companyRepository,
                                IShipmentPackingListRepository shipmentPackingListRepository, IPackingListDetailRepository packingListDetailRepository, IShipmentDetailRepository shipmentDetailRepository,
                                IPOQCTaskRepository pOQCTaskRepository,
                                IShipmentApprovalLogRepository approvalLogRepository,
                                    IUserRoleRepository userRoleRepository,
                                IHttpContextAccessor httpContextAccessor,
                                 IMST_ApprovalAssignmentStatusRepository mst_ApprovalAssignmentStatusRepository,
                                IPushQueueRepository pushQueueRepository,
                                IApprovalAssignmentDetailRepository approvalAssignmentDetailRepository,
                                IUserDetailsRepository userDetailsRepository,
                                IPackingListRepository packingListRepository,
                                 IRoleRepository roleRepository,
                                ISemiFinishGoodsPalletRepository semiFinishGoodsPalletRepository,
                                IPackingListLogRepository packingListLogRepository,
                                IPutAwayTaskDetailsRepository putAwayTaskDetailsRepository,
                                IPutAwayTaskRepository putAwayTaskRepository,
                                IActionLogService actionLogService,
                                IMST_CodeGenerationTemplateService codeGenerationTemplateService,
                                IShipmentPlanPalletRepository shipmentPlanPalletRepository,
                                IMST_WareHouseDefaultLocationRepository mst_WareHouseDefaultLocationRepository,
                                IItemTransactionService itemTransactionService,
                                IShipmentTaskRepository shipmentTaskRepository,
                                IWebSettingRepository webSettingRepository,
                                IProcessTaskService processTaskService,
                                IPickingListRepository pickingListRepository,
                                IWareHouseReservedItemRepository wareHouseReservedItemRepository,
                                IConfiguration config,
                                IItemTransactionDetailRepository itemTransactionDetailRepository,
                                IPickingListDetailRepository pickingListDetailRepository,
                                IBarcodeService barcodeService,
                                ITruckLoadingSheetRepository truckLoadingSheetRepository,
                                ISalesOrderPlanTaskRepository salesOrderPlanTaskRepository,
                                IProcessTaskRepository processTaskRepository,
                                ISalesOrderPlanRepository salesOrderPlanRepository,
                                IWareHouseLocationRepository wareHouseLocationRepository)
        {
            _shipmentRepository = shipmentRepository;
            _approvalLogRepository = approvalLogRepository;
            _approvalAssignmentDetailRepository = approvalAssignmentDetailRepository;
            _pushQueueRepository = pushQueueRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mst_ApprovalAssignmentStatusRepository = mst_ApprovalAssignmentStatusRepository;
            _vehicleRepository = vehicleRepository;
            _companyRepository = companyRepository;
            _shipmentPackingListRepository = shipmentPackingListRepository;
            _packingListDetailRepository = packingListDetailRepository;
            _shipmentDetailRepository = shipmentDetailRepository;
            _pOQCTaskRepository = pOQCTaskRepository;
            _packingListRepository = packingListRepository;
            _semiFinishGoodsPalletRepository = semiFinishGoodsPalletRepository;
            _httpContextAccessor = httpContextAccessor;
            _userDetailsRepository = userDetailsRepository;
            _packingListLogRepository = packingListLogRepository;
            _putAwayTaskDetailsRepository = putAwayTaskDetailsRepository;
            _putAwayTaskRepository = putAwayTaskRepository;
            _actionLogService = actionLogService;
            _codeGenerationTemplateService = codeGenerationTemplateService;
            _shipmentPlanPalletRepository = shipmentPlanPalletRepository;
            _mst_WareHouseDefaultLocationRepository = mst_WareHouseDefaultLocationRepository;
            _itemTransactionService = itemTransactionService;
            _shipmentTaskRepository = shipmentTaskRepository;
            _webSettingRepository = webSettingRepository;
            _processTaskService = processTaskService;
            _pickingListRepository = pickingListRepository;
            _wareHouseReservedItemRepository = wareHouseReservedItemRepository;
            Configuration = config;
            _itemTransactionDetailRepository = itemTransactionDetailRepository;
            _pickingListDetailRepository = pickingListDetailRepository;
            _barcodeService = barcodeService;
            _truckLoadingSheetRepository = truckLoadingSheetRepository;
            _salesOrderPlanTaskRepository = salesOrderPlanTaskRepository;
            _processTaskRepository = processTaskRepository;
            _wareHouseLocationRepository = wareHouseLocationRepository;
            _salesOrderPlanRepository = salesOrderPlanRepository;
        }
        public async Task<List<long?>> GetSalesOrderplanIdByShipment(long shipmentId)
        {
            try
            {
                if (shipmentId > 0)
                {
                    var shipmentPackingLists = _shipmentPackingListRepository
                                       .GetAll(x => x.ShipmentId == shipmentId)
                                       .Include(x => x.PackingList)
                                       .ThenInclude(x => x.SalesOrder).Select(y => y.PackingList.SalesOrder.SalesOrderPlanId).ToList();
                    
                    return shipmentPackingLists;
                }
                return null;
               

            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<ShipmentDTO> GetAll()
        {
            try
            {
                IEnumerable<Shipment> shipmentData = _shipmentRepository.GetAll();
                return _mapper.Map<IEnumerable<ShipmentDTO>>(shipmentData);

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<long> Create(ShipmentDTO model)
        {
            try
            {
                Shipment obj = _mapper.Map<Shipment>(model);
                var code = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.Shipment), GCode = "" };
                obj.Code = _codeGenerationTemplateService.GenerateCode(code).Result.GCode;
                obj.CreatedOn = DateTime.Now;
                obj.CreatedBy = model.UserId;
                obj.IsActive = true;
                obj.IsDeleted = false;
                await _shipmentRepository.Add(obj);
                await _unitOfWork.Commit();
                return obj.Id;
            }catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Int64> Update(ShipmentDTO model)
        {
            try
            {
                var shipmentData = await _shipmentRepository.GetSingle(model.Id);
                if (shipmentData != null)
                {
                    shipmentData.ShipmentDate = model.ShipmentDate;
                    shipmentData.ShipTo = model.DriverName;
                    shipmentData.DocumentCode = model.DocumentCode;
                    //shipmentData.VehicleId = model.VehicleId;
                    shipmentData.UpdatedBy = model.UserId;
                    shipmentData.UpdatedOn = DateTime.Now;

                    await _unitOfWork.Commit();
                    return shipmentData.Id;
                }
                return 0;
            }catch(Exception e)
            {
                throw e;
            }
            
        }


        public async Task<Int64> UpdateDriverDetails(ShipmentDTO model)
        {
            try
            {
                var shipmentData = await _shipmentRepository.GetSingle(model.Id);
                if (shipmentData != null)
                {
                    shipmentData.DriverName = model.DriverName;
                    shipmentData.DriverPhoneNumber = model.DriverPhoneNumber;
                    shipmentData.DriverAddress = model.DriverAddress;
                    shipmentData.DriverLicenseNumber = model.DriverLicenseNumber;
                    shipmentData.TransportAgencyName = model.TransportAgencyName;
                    shipmentData.TransportAgencyAddress = model.TransportAgencyAddress;
                    shipmentData.TransportAgencyContactPersonName = model.TransportAgencyContactPersonName;
                    shipmentData.TransportAgencyContactPersonPhoneNumber = model.TransportAgencyContactPersonPhoneNumber;
                    shipmentData.UpdatedBy = model.UserId;
                    shipmentData.UpdatedOn = DateTime.Now;
                    shipmentData.IsCompleted = true;
                    shipmentData.VehicleId = model.VehicleId;

                    if(model.VehicleId > 0)
                    {
                        var vehicleData = await _vehicleRepository.GetSingle(model.VehicleId ?? 0);
                        if (vehicleData != null) vehicleData.LRNo = model.LrNo;
                    }

                    TruckLoadingSheetUpsertDTO truckLoadingSheetUpsert = new TruckLoadingSheetUpsertDTO()
                    {
                        UserId = model.UserId ?? 0,
                        BranchId = model.BranchId ?? 0,
                        RootPath = model.RootPath,
                        ShipmentId = model.Id

                    };

                    var TruckResult = await UpsertTruckLoadingDetailOnCompletion(truckLoadingSheetUpsert);
                    if (TruckResult == false) return 0;
                    var TransactionResult = await DeductShippedItemByItemTransaction(model.Id);
                    if (TransactionResult == false) return 0;
                    var isupdatestatus = await UpdateOrderStatus(model.Id, "PendingForDelivery");
                    await _unitOfWork.Commit();
                    return shipmentData.Id;
                }
                return 0;
            }
            catch (Exception e)
            {
                throw e;
            }

        }



        public async Task<bool> DeductShippedItemByItemTransaction(long shipmentId)
        {
            try
            {
                if (shipmentId == 0) return false;
                var shipmentTaskData = _shipmentTaskRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == shipmentId).FirstOrDefault();
                if(shipmentTaskData != null && shipmentTaskData.ProcessTaskId > 0)
                {
                    List<PickingListDetail> pickingListDetail = await _pickingListDetailRepository.GetAll(x =>x.IsDeleted != true && x.PickingList.ProcessTaskId == shipmentTaskData.ProcessTaskId, a => a.PickingList).ToListAsync();
                    if (pickingListDetail.Count() > 0)
                    {
                        var OutBoundDefaultLocation = _mst_WareHouseDefaultLocationRepository.GetAll(x => x.IsDeleted != true && x.Code == MainEnumHelper.GetDescription(Enums.DefaultLocationCode.OUTBOUND)).Include(x => x.WareLocation).FirstOrDefault();
                        var OutBoundDefaultWareHouseLocation = _wareHouseLocationRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.IsDefault == true && x.WareHouseId == OutBoundDefaultLocation.SubInventoryId).FirstOrDefault();

                        foreach (PickingListDetail detail in pickingListDetail)
                        {
                            detail.IsPicked = true;
                            ItemTransactionNTransDetailDTO itemtrans = new ItemTransactionNTransDetailDTO()
                            {
                                Id = detail.KittingTransactionDetailId ?? 0,
                                ItemId = detail.ItemId ?? 0,
                                Quantity = detail.RequiredQuantity,
                                MSTItemTransactionStageId = (int)Enums.ItemTransactionStage.PickingListDetail,
                                ObjectName = "PickingListDetail",
                                ObjectId = detail.Id,
                                PalletIDNumber = detail.PalletIDNumber,
                                CaseNumber = detail.CaseNumber,
                                SourceWareHouseId = detail.WareHouseId,
                                SourceSubInventoryId = detail.SubInventoryId,
                                SourceWareHouseLocationId = detail.WareHouseLocation != null && detail.TransactionDetailId != detail.KittingTransactionDetailId ? detail.WareHouseLocation.KittingLocationId : detail.WareHouseLocationId,
                                SourceWareHouseSubLocationId = detail.WareHouseSubLocationId,
                                IsInbound = false,

                                WareHouseId = OutBoundDefaultLocation != null ? OutBoundDefaultLocation.WareHouseId : null,
                                SubInventoryId = OutBoundDefaultLocation != null ? OutBoundDefaultLocation.SubInventoryId : null,
                                WareHouseLocationId = OutBoundDefaultWareHouseLocation != null ? OutBoundDefaultWareHouseLocation.MSTWareHouseLocationId : null,
                                WareHouseSubLocationId = OutBoundDefaultLocation != null ? OutBoundDefaultLocation.WareHouseSubLocationId : null,
                                UpdateTransactionRemainingItem = true,
                                IsSalesOrderPlanDetails = detail.ObjectName == "SalesOrderPlanDetails" ? true : false,
                            };

                            await _itemTransactionService.createItemTransPickingList(itemtrans);
                        }
                        await _unitOfWork.Commit();
                        return true;
                    }
                    return false;
                }
                return false;
            }catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> Delete(long id)
        {
            try
            {
                var shipmentData = await _shipmentRepository.GetSingle(id);
                if (shipmentData == null) return false;
                shipmentData.IsDeleted = true;
                await _unitOfWork.Commit();
                return true;
            }catch(Exception ex)
            {
                throw ex;
            }  
        }

        public async Task<ShipmentResponseDTO> CreateShipmentAfterScanning(CreateAfterScanDTO model)
        {

            try
            {
                if (model.VehicleId > 0 && !string.IsNullOrEmpty(model.PackingListNumber))
                {
                    var vehicleData = _vehicleRepository.GetAll(x => x.IsDeleted != true && x.Id == model.VehicleId, x => x.MST_VehicleType).FirstOrDefault();
                    if (vehicleData == null) return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Something went Wrong",
                        IsError = true

                    };
                    var packingListData = _packingListRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.PackingListNumber == model.PackingListNumber.Trim() && x.MST_ShipmentStatusId != (int)Enums.MST_ShipmentStatus.COMPLETED, x=> x.SalesOrder).FirstOrDefault();
                    if (packingListData == null) return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Packing List Already Completed.",
                        IsError = true

                    };
                    var companyData = _companyRepository.GetAll(x => x.IsDeleted != true && x.Code == MainEnumHelper.GetDescription(Enums.CompanyCode.WattPower)).FirstOrDefault();
                    if (companyData == null) return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Something went Wrong",
                        IsError = true

                    };


                    List<ShipmentPackingList> shipmentPackingLists = new List<ShipmentPackingList>();
                    var code = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.Shipment), GCode = "" };
                    Shipment shipp = new Shipment()
                    {
                        ShipFrom = companyData.StreetAddress1,
                        VehicleNumber = vehicleData.VehicleNumber,
                        MSTVehicleTypeId = vehicleData.MSTVehicleTypeId,
                        Code = _codeGenerationTemplateService.GenerateCode(code).Result.GCode,
                        VehicleId = model.VehicleId,
                        CreatedBy = model.UserId,
                        CreatedOn = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        ShipmentDate = DateTime.Now,
                        BranchId = model.BranchId,

                    };




                    ShipmentPackingList shipmentPackingList = new ShipmentPackingList()
                    {
                        PackingListId = packingListData.Id,
                        CreatedBy = model.UserId,
                        CreatedOn = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        BranchId = model.BranchId,
                    };

                    shipmentPackingLists.Add(shipmentPackingList);


                    shipp.ShipmentPackingLists = shipmentPackingLists;
                    await _shipmentRepository.Add(shipp);
                    if (packingListData.MST_ShipmentStatusId == null || packingListData.MST_ShipmentStatusId == 0)
                    {
                        PackingListLog packingListLog = new PackingListLog()
                        {
                            CreatedBy = model.UserId,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.STARTED,
                            MST_PackingListStageId = (int)Enums.MST_PackingListStage.SHIPMENT,
                            PackingListId = packingListData.Id,
                            EventById = model.UserId,
                            EventDate = DateTime.Now,
                        };
                        await _packingListLogRepository.Add(packingListLog);

                        packingListData.MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.STARTED;

                        ActionLogDTO actionLogPOQC = new ActionLogDTO()
                        {
                            Description = "Shipment Started For PackingList : " + packingListData.PackingListNumber,
                            IsPerformedByUser = true,
                            //MST_LogStageId = (int)Enums.MST_LogStageEnum.SalesOrderPlan,
                            MST_LogStageId = (int)Enums.MST_LogStageEnum.ShipmentPlan,
                            MST_ActionStageId = (int)Enums.MST_ActionStageEnum.Shipment_Started,
                            //ObjectId = packingListData.SalesOrder != null ? packingListData.SalesOrder.SalesOrderPlanId : 0,
                            ObjectId = shipp.Id,
                        };
                        await _actionLogService.CreateActionLog(actionLogPOQC);
                    }

                   
                    await _unitOfWork.Commit();
                    return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Successfully Created",
                        IsSuccess = true,
                        ShipmentId = shipp.Id

                    }; ;
                }
                return new ShipmentResponseDTO()
                {
                    ErrMssg = "Either PackingList Number or Vehicle Invalid.",
                    IsError = true

                };
            }catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<ShipmentDTO> shipmentWithAllData(long shipmentId)
        {

            try
            {
                if (shipmentId > 0)
                {
                    var shipmentData = _shipmentRepository.GetAll(x => x.IsDeleted != true && x.Id == shipmentId , x => x.Acc_InvoiceShipment).Include(x => x.ShipmentPackingLists).ThenInclude(x => x.PackingList)
                        .Include(x =>x.ShipmentDetails).ThenInclude(x => x.PackingListDetail).ThenInclude(x => x.SemiFinishGoodsPallet).FirstOrDefault();
                    if (shipmentData == null) return null;
                    var data =  _mapper.Map<ShipmentDTO>(shipmentData);
                    data.ShipmentDetails = _mapper.Map<List<ShipmentDetailDTO>>(shipmentData.ShipmentDetails);
                    data.ShipmentDetails = data.ShipmentDetails.OrderBy(x => x.PackingListDetail.SemiFinishGoodsPallet.PalletId).ToList();
                    data.ShipmentPackingLists = _mapper.Map<List<ShipmentPackingListDTO>>(shipmentData.ShipmentPackingLists);
                    data.IsApproved = shipmentData.Acc_InvoiceShipment.Where(x => x.IsAccepted != true).Count() == 0;
                    foreach(var item in data.ShipmentPackingLists)
                    {
                        var shipmentPlanPalletCount = _shipmentPlanPalletRepository.GetAll( x => x.IsDeleted != true && x.ShipmentId == shipmentId && x.PackingListDetail.PackingListId == item.PackingListId).Count();
                        item.PackingList.UsedPallet = shipmentPlanPalletCount;
                    }

                    var shipmenttask = _shipmentTaskRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == shipmentId).Include(x => x.ProcessTasks).
                    ThenInclude(x => x.PickingLists).ThenInclude(x => x.PickingListDetails).FirstOrDefault();
                    data.ProcessTaskId = shipmenttask.ProcessTaskId;
                    data.IsAllScanned = shipmenttask.ProcessTasks.PickingLists.FirstOrDefault().PickingListDetails.Where(x => x.IsDeleted != true && x.IsActive != false && x.IsAddedToShipment != true).ToList().Count() == 0;
                    data.TruckLoadingSheetsId = _truckLoadingSheetRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == shipmentId).ToList().Select(y => new FileDownloadDTO() { Id = y.Id, FileName = y.QrCode}).ToList();
                    return data;

                }
                return null;
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<InvoiceShipmentDTO>> ShipmentInvoiceStatusDetail(long ShipmentId)
        {
            try
            {
                var shipmentData = _shipmentRepository.GetAll(x => x.IsDeleted != true && x.Id == ShipmentId).Include(x => x.Acc_InvoiceShipment).ThenInclude(x => x.Invoice).FirstOrDefault();
                if(shipmentData != null && shipmentData.Acc_InvoiceShipment.Count() > 0)
                {
                    var shipmentInvoiceData = shipmentData.Acc_InvoiceShipment.Where(x => x.IsDeleted != true);
                    var data = shipmentInvoiceData.Select(x => new InvoiceShipmentDTO()
                    {
                        InvoiceName = x.Invoice != null ? x.Invoice.InvoiceNumber : "",
                        InvoiceId =  x.InvoiceId,
                        Status = x.Invoice != null ? x.Invoice.IsAccepted == true ? "Approved" : "Waiting For Approval" : null
                    }).ToList();
                    return data;
                }
                return null;
            }catch(Exception ex)
            {
                throw ex;
            }
        }



        public async Task<ShipmentDTO> shipmentPlanPalletWithAllData(long shipmentId)
        {

            try
            {
                if (shipmentId > 0)
                {
                    var shipmentData = _shipmentRepository.GetAll(x => x.IsDeleted != true && x.Id == shipmentId,x => x.MSTReservationStatus, x => x.MSTPONStatus).Include(x => x.ShipmentPackingLists).ThenInclude(x => x.PackingList)
                        .Include(x => x.ShipmentPlanPallets).ThenInclude(x => x.PackingListDetail).ThenInclude(x => x.SemiFinishGoodsPallet).FirstOrDefault();
                    if (shipmentData == null) return null;
                    var data = _mapper.Map<ShipmentDTO>(shipmentData);
                    data.MSTPONStatusName = shipmentData.MSTPONStatus != null ? shipmentData.MSTPONStatus.Name : "";
                    data.MSTReservationStatusName = shipmentData.MSTReservationStatus != null ? shipmentData.MSTReservationStatus.Name : "";
                    var shipmentTaskData = _shipmentTaskRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == shipmentId).FirstOrDefault();
                    if(shipmentTaskData != null) data.ProcessTaskId = shipmentTaskData.ProcessTaskId;
                    data.ShipmentPlanPallets = _mapper.Map<List<ShipmentPlanPalletDTO>>(shipmentData.ShipmentPlanPallets.Where(x => x.IsDeleted != true));
                    data.ShipmentPackingLists = _mapper.Map<List<ShipmentPackingListDTO>>(shipmentData.ShipmentPackingLists.Where(x=> x.IsDeleted != true));

                    foreach(var packingItem in data.ShipmentPackingLists)
                    {
                        packingItem.PackingList.UsedPallet = data.ShipmentPlanPallets.Where(x => x.PackingListDetail.PackingListId == packingItem.PackingListId).ToList().Count();
                        
                    }

                    //foreach(var packinglistitem in data.ShipmentPackingLists)
                    //{
                    //    foreach(var packingdetailitem in packinglistitem.PackingList.PackingListDetails)
                    //    {
                    //        var putawaytask = _putAwayTaskRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.PackingListDetailId == packingdetailitem.Id).FirstOrDefault();
                    //        if(putawaytask == null || putawaytask.IsCompleted != true) packingdetailitem.IsPutAwayCompleted = false;
                    //        else packingdetailitem.IsPutAwayCompleted = true;
                    //    }
                    //}
                    return data;

                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<ShipmentDTO> GetPalletsByScanningPackingList(long packingListId)
        //{

        //    if (packingListId > 0)
        //    {
        //        var shipmentData = _shipmentRepository.GetAll(x => x.IsDeleted != true && x.Id == shipmentId, x => x.ShipmentPackingLists, x => x.ShipmentDetails).FirstOrDefault();
        //        if (shipmentData == null) return null;
        //        return _mapper.Map<ShipmentDTO>(shipmentData);
        //    }
        //    return null;
        //}


        public async Task<ShipmentResponseDTO> UpdatePackingListByScan(UpdatePackingListByScanDTO model)
        {
            try
            {
                var shipres = new ShipmentResponseDTO();
                if (!string.IsNullOrEmpty(model.PackingListNumber) && model.ShipmentId > 0)
                {

                    var packingListData = _packingListRepository.GetAll(x => x.IsDeleted != true && x.PackingListNumber == model.PackingListNumber.Trim() && x.MST_ShipmentStatusId != (int)Enums.MST_ShipmentStatus.COMPLETED, x => x.SalesOrder).FirstOrDefault();
                    if (packingListData == null) return new ShipmentResponseDTO()
                    {
                        ErrMssg = "PackingList is already Completed.",
                        IsError = true,
                        ShipmentId = model.ShipmentId

                    };



                    var shipmentdata = _shipmentPackingListRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.PackingListId == packingListData.Id && x.ShipmentId == model.ShipmentId).ToList();
                    if (shipmentdata.Count() > 0) return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Packing List Already Scanned.",
                        IsError = true,
                        ShipmentId = model.ShipmentId

                    }; ;
                    PackingListLog packingListLog = new PackingListLog()
                    {
                        CreatedBy = model.UserId,
                        CreatedOn = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.STARTED,
                        MST_PackingListStageId = (int)Enums.MST_PackingListStage.SHIPMENT,
                        PackingListId = packingListData.Id,
                        EventDate = DateTime.Now,
                        EventById = model.UserId
                    };
                    await _packingListLogRepository.Add(packingListLog);

                    packingListData.MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.STARTED;

                    ShipmentPackingList shipmentpl = new ShipmentPackingList()
                    {
                        PackingListId = packingListData.Id,
                        ShipmentId = model.ShipmentId,
                        CreatedBy = model.UserId,
                        CreatedOn = DateTime.Now,
                        BranchId = model.BranchId,
                        IsActive = true,
                        IsDeleted = false
                    };

                    ActionLogDTO actionLogPOQC = new ActionLogDTO()
                    {
                        Description = "Scanned PackingList : " + packingListData.PackingListNumber,
                        IsPerformedByUser = true,
                        //MST_LogStageId = (int)Enums.MST_LogStageEnum.SalesOrderPlan,
                        MST_LogStageId = (int)Enums.MST_LogStageEnum.ShipmentPlan,
                        MST_ActionStageId = (int)Enums.MST_ActionStageEnum.Shipment_Updated,
                        //ObjectId = packingListData.SalesOrder != null ? packingListData.SalesOrder.SalesOrderPlanId : 0,
                        ObjectId = model.ShipmentId,
                    };
                    await _actionLogService.CreateActionLog(actionLogPOQC);

                    await _shipmentPackingListRepository.Add(shipmentpl);
                    await _unitOfWork.Commit();
                    return new ShipmentResponseDTO()
                    {
                        ErrMssg = "PackingList Successfully Added.",
                        IsSuccess = true,
                        ShipmentId = model.ShipmentId

                    };
                }
                return new ShipmentResponseDTO()
                {
                    ErrMssg = "Either PackingList Number or ShipmentId Invalid.",
                    IsError = true,
                    ShipmentId = model.ShipmentId

                }; ;
            }catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<List<waitingScanPalletDTO>> GetAllPalletsDataByShipmentId(long shipmentId)
        {

            try
            {
                var data = (from pqctask in _pOQCTaskRepository.GetAll(x => x.IsDeleted != true && x.IsCompleted == true)
                            join packingdetail in _packingListDetailRepository.GetAll(x => x.IsDeleted != true) on pqctask.PackingListDetailId equals packingdetail.Id 
                            join semifgpallet in _semiFinishGoodsPalletRepository.GetAll(x => x.IsDeleted != true) on packingdetail.SemiFinishGoodsPalletId equals semifgpallet.Id
                            join shipmentpackinglist in _shipmentPackingListRepository.GetAll(x => x.IsDeleted != true && x.ShipmentId == shipmentId) on packingdetail.PackingListId equals shipmentpackinglist.PackingListId
                            join putawaytask in _putAwayTaskRepository.GetAll(x => x.IsDeleted != true && x.IsCompleted == true) on packingdetail.Id equals putawaytask.PackingListDetailId
                            select new waitingScanPalletDTO{ 
                                PalletNumber =  semifgpallet.PalletId, 
                                PackingListNumber = packingdetail.PackingList.PackingListNumber,
                                PackingListDetalId = packingdetail.Id,
                                PackingListId = packingdetail.PackingListId,
                                IsAddedToShipment = packingdetail.IsAddedToShipment,  
                                IsCompleted = putawaytask.IsCompleted,

                            }).ToList();

                var shipmentData = await _shipmentRepository.GetSingle(shipmentId);
                var isReleased = shipmentData.MSTPONStatusId == (int)Enums.PONStatus.Released;
                if(data.Count() > 0) data[0].IsReleased = isReleased;

                return data.ToList();
            }catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<QueryResult<ShipmentDTO>> Search(ShipmentQueryObject query)
        {
            try
            {
                if (string.IsNullOrEmpty(query.SortBy))
                {
                    query.SortBy = "Id";
                }

                var columnMap = new Dictionary<string, Expression<Func<ShipmentDTO, object>>>()
                {
                    ["Id"] = p => p.Id,
                    ["Code"] = p => p.Code,
                    ["MSTReservationStatusName"] = p => p.MSTReservationStatusName,
                    ["MSTPONStatusName"] = p => p.MSTPONStatusName,
                    ["ShipFrom"] = p => p.ShipFrom,
                    ["ShipTo"] = p => p.ShipTo,
                    ["IsCompleted"] = p => p.IsCompleted,
                    ["IsActive"] = p => p.IsActive,
                    ["VehicleName"] = p => p.VehicleName,
                    ["MSTVehicleTypeName"] = p => p.MSTVehicleTypeName,
                    ["VehicleNumber"] = p => p.VehicleNumber,
                    ["GrossVolume"] = p => p.GrossVolume,
                    ["GrossWeight"] = p => p.GrossWeight,
                    ["NetVolume"] = p => p.NetVolume,
                    ["NetWeight"] = p => p.NetWeight,
                    ["ShipmentDate"] = p => p.ShipmentDate,
                    ["DriverName"] = p => p.DriverName,
                    ["DriverPhoneNumber"] = p => p.DriverPhoneNumber,
                    ["DriverAddress"] = p => p.DriverAddress,
                    ["DriverLicenseNumber"] = p => p.DriverLicenseNumber,
                    ["TransportAgencyName"] = p => p.TransportAgencyName,
                    ["TransportAgencyAddress"] = p => p.TransportAgencyAddress,
                    ["TransportAgencyContactPersonName"] = p => p.TransportAgencyContactPersonName,
                    ["TransportAgencyContactPersonPhoneNumber"] = p => p.TransportAgencyContactPersonPhoneNumber,
                    ["TotalBox"] = p => p.TotalBox,
                    ["TotalPallet"] = p => p.TotalPallet,
                    ["DocumentCode"] = p => p.DocumentCode,

                };

                var shipments = _shipmentRepository
                            .GetAll(x => x.IsDeleted != true, x => x.MSTReservationStatus, x => x.MSTPONStatus)
                            .Include(x=>x.TruckLoadingSheets).AsQueryable();
                if (!string.IsNullOrEmpty(query.SearchString))
                {
                    shipments = shipments.Where(x => x.DriverName.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.DriverPhoneNumber.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.DriverLicenseNumber.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.DriverAddress.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.DocumentCode.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.TransportAgencyName.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.TransportAgencyAddress.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.TransportAgencyContactPersonName.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.TransportAgencyContactPersonPhoneNumber.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.ShipFrom.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.ShipTo.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.IsActive.ToString().Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.IsCompleted.ToString().Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.Vehicle.OWnerName.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.Vehicle.VehicleNumber.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.MSTVehicleType.Name.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.GrossWeight.ToString().Trim().Contains(query.SearchString.Trim())
                    || x.GrossVolume.ToString().Trim().Contains(query.SearchString.Trim())
                    || x.NetWeight.ToString().Trim().Contains(query.SearchString.Trim())
                    || x.NetVolume.ToString().Trim().Contains(query.SearchString.Trim())
                    || x.ShipmentDate.Value.Date.ToString().Contains(query.SearchString.Trim())
                    || x.Vehicle.VehicleNumber.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.TotalPallet.ToString().Trim().Contains(query.SearchString.Trim())
                    || x.TotalBox.ToString().Trim().Contains(query.SearchString.Trim())
                    || x.DocumentCode.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    || x.Code.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    );
                }

                if (query.MSTVehicleTypeId > 0)
                {
                    shipments = shipments.Where(x => x.MSTVehicleTypeId == query.MSTVehicleTypeId);
                }
                if (query.VehicleId > 0)
                {
                    shipments = shipments.Where(x => x.VehicleId == query.VehicleId);
                }
           

                var itemList = shipments.Select(x => new ShipmentDTO()
                {
                    Id = x.Id,
                    DriverName=x.DriverName,
                    DriverPhoneNumber=x.DriverPhoneNumber,
                    DriverLicenseNumber=x.DriverLicenseNumber,
                    DriverAddress = x.DriverAddress,
                    DocumentCode= x.DocumentCode,
                    TransportAgencyName=x.TransportAgencyName,
                    TransportAgencyAddress=x.TransportAgencyAddress,
                    TransportAgencyContactPersonName=x.TransportAgencyContactPersonName,
                    TransportAgencyContactPersonPhoneNumber=x.TransportAgencyContactPersonPhoneNumber,
                    ShipFrom=x.ShipFrom,
                    ShipTo= x.TruckLoadingSheets.Count()>0?string.Join("  ,   ",x.TruckLoadingSheets.Select(y=>y.ShipTo.Replace(","," ")).ToList()):"",
                    ShipmentDate=x.ShipmentDate,
                    TotalPallet=x.TotalPallet,
                    TotalBox=x.TotalBox,
                    GrossWeight=x.GrossWeight,
                    NetWeight=x.NetWeight,
                    NetVolume=x.NetVolume,
                    GrossVolume=x.GrossVolume,
                    MSTVehicleTypeId=x.MSTVehicleTypeId,
                    MSTVehicleTypeName= x.MSTVehicleType != null ? x.MSTVehicleType.Name : "",
                    VehicleId=x.VehicleId,
                    VehicleName= x.Vehicle != null ? x.Vehicle.VehicleNumber : "",
                    IsActive=x.IsActive,
                    MSTPONStatusId = x.MSTPONStatusId,
                    MSTReservationStatusId=x.MSTReservationStatusId,
                    MSTPONStatusName = x.MSTPONStatus != null ? x.MSTPONStatus.Name : null, 
                    MSTReservationStatusName = x.MSTReservationStatus != null ? x.MSTReservationStatus.Name : null,
                    IsCompleted = x.IsCompleted ?? false,
                    Code=x.Code,
                });

                var result = await itemList.ApplyOrdering(query, columnMap).ToListAsync();
                var filterdatacount = result.Count();
                var pagination = _mapper.Map<List<ShipmentDTO>>(result);

                foreach(var item in pagination)
                {
                    var shipmentTask = _shipmentTaskRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == item.Id, x => x.ProcessTasks.MSTProcessTaskStatus).FirstOrDefault();
                    if(shipmentTask != null && shipmentTask.ProcessTasks != null && shipmentTask.ProcessTasks.MSTProcessTaskStatus != null)
                    {
                        item.MSTProcessTaskStatusName = shipmentTask.ProcessTasks.MSTProcessTaskStatus.Name;
                        item.MSTProcessTaskStatusId = shipmentTask.ProcessTasks.MSTProcessTaskStatusId;
                    }
                }

                var queryResult = new QueryResult<ShipmentDTO>
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

        public async Task<ShipmentResponseDTO> ScanPalletForShipment(ScanPalletShipmentInputDTO model)
        {
            try
            {
                model.PalletId = model.PalletId.Trim(); 
                //bool IsUpdate = false;
                var pldetail = _packingListDetailRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.SemiFinishGoodsPallet.PalletId == model.PalletId).ToList();
                if(pldetail.Count() == 0) return new ShipmentResponseDTO()
                {
                    ErrMssg = "Invalid Pallet",
                    IsError = true

                };

                var packingListDetail = _packingListDetailRepository
                    .GetAll(x =>x.IsDeleted != true && x.IsActive != false && x.IsAddedToShipment != true && x.SemiFinishGoodsPallet.PalletId == model.PalletId)
                    .Include(x=>x.SemiFinishGoodsPallet).ThenInclude(x=>x.SemiFinishGoods)
                    .FirstOrDefault();
                if(packingListDetail == null) return new ShipmentResponseDTO()
                {
                    ErrMssg = "Pallet Already Scanned, Cannot Scan Anymore.",
                    IsError = true

                };
                if (packingListDetail != null)
                {
                    PackingListDetail dataTOAdd = _packingListDetailRepository.GetById(packingListDetail.Id);
                    if (dataTOAdd.IsAddedToShipment == true) return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Pallet Already Scanned, Cannot Scan Anymore.",
                        IsError = true

                    };


                    var totalScannedShipment = _shipmentDetailRepository.GetAll(x => x.IsActive != false && x.IsDeleted != true && x.ShipmentId == model.ShipmentId).ToList();
                    var TotalPackingList = _packingListDetailRepository.GetAll(x => x.IsActive != false && x.IsDeleted != true && x.PackingListId ==packingListDetail.PackingListId).ToList();  


                    PackingList packingList = _packingListRepository.GetById(x => x.IsDeleted != true && x.IsActive != false && x.Id == packingListDetail.PackingListId, x => x.SalesOrder);
                    if (packingList != null && packingList.MST_ShipmentStatusId == (int)Enums.MST_ShipmentStatus.STARTED)
                    {
                        packingList.MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.PARTIALLY_COMPLETED;
                        PackingListLog packingListLog = new PackingListLog()
                        {
                            CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.PARTIALLY_COMPLETED,
                            MST_PackingListStageId = (int)Enums.MST_PackingListStage.SHIPMENT,
                            PackingListId = packingList.Id,
                            EventById = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            EventDate = DateTime.Now,
                        };
                        await _packingListLogRepository.Add(packingListLog);
                    }

                    if (packingList != null && (TotalPackingList.Count() > 0 && 
                        (totalScannedShipment.Count() + 1) == TotalPackingList.Count()))
                    {
                        packingList.MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.COMPLETED;
                        PackingListLog packingListLog = new PackingListLog()
                        {
                            CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.COMPLETED,
                            MST_PackingListStageId = (int)Enums.MST_PackingListStage.SHIPMENT,
                            PackingListId = packingList.Id,
                            EventById = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            EventDate = DateTime.Now,
                        };
                        await _packingListLogRepository.Add(packingListLog);
                    }

                    ShipmentDetail shipmentDetail = new ShipmentDetail();
                    shipmentDetail.ShipmentId = model.ShipmentId;
                    shipmentDetail.NetWeight = packingListDetail.NetWeight;
                    shipmentDetail.GrossWeight = packingListDetail.GrossWeight ?? 0;
                    shipmentDetail.GrossVolume = packingListDetail.Volume;
                    shipmentDetail.NetVolume = packingListDetail.Volume;
                    shipmentDetail.BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId;
                    shipmentDetail.CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId;
                    shipmentDetail.CreatedOn = DateTime.Now;
                    shipmentDetail.IsActive = true;
                    shipmentDetail.IsDeleted = false;
                    shipmentDetail.PackingListDetailId = packingListDetail.Id;
                    await _shipmentDetailRepository.Add(shipmentDetail);
                    dataTOAdd.IsAddedToShipment = true;
                    
                    //update SHipment
                    Shipment shipment = _shipmentRepository.GetById(x => x.Id == model.ShipmentId);
                    shipment.TotalBox=shipment.TotalBox ??0 + packingListDetail.SemiFinishGoodsPallet.Quantity;
                    shipment.TotalPallet = shipment.TotalPallet ?? 0 + 1;
                    shipment.GrossWeight = shipment.GrossWeight ?? 0 + packingListDetail.GrossWeight;
                    shipment.NetWeight = shipment.NetWeight ?? 0 + packingListDetail.NetWeight;
                    shipment.NetVolume = shipment.NetVolume ?? 0 + packingListDetail.Volume;

                    ActionLogDTO actionLogPOQC = new ActionLogDTO()
                    {
                        Description = "Shipment Pallet : " + model.PalletId +" Scanned.",
                        IsPerformedByUser = true,
                        //MST_LogStageId = (int)Enums.MST_LogStageEnum.SalesOrderPlan,
                        MST_LogStageId = (int)Enums.MST_LogStageEnum.ShipmentPlan,
                        MST_ActionStageId = (int)Enums.MST_ActionStageEnum.Shipment_Pallet_Scanned,
                        //ObjectId = packingList.SalesOrder != null ? packingList.SalesOrder.SalesOrderPlanId : 0,
                        ObjectId = model.ShipmentId,
                    };
                    await _actionLogService.CreateActionLog(actionLogPOQC);
                    await _unitOfWork.Commit();

                    return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Pallet Successfully Scanned.",
                        IsSuccess = true

                    };
                    //IsUpdate = true;

                }
                return new ShipmentResponseDTO()
                {
                    ErrMssg = "Something Went Wrong.",
                    IsError = true

                }; ;
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        

        public async Task<ShipmentResponseDTO> AddToPackingListForShipment(SelectedPalletShipmentDTO model)
        {
            try
            {
                if (model.PackingListId > 0)
                    model.PackingListDetailIds = _packingListDetailRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.PackingListId == model.PackingListId && x.IsAddedToShipment != true && x.IsPutAwayCompleted == true).Select(a => a.Id).ToList();
                foreach (long plistid in model.PackingListDetailIds)
                {
                    var packinglistdetail = _packingListDetailRepository.GetById(plistid);
                    packinglistdetail.IsAddedToShipment = true;
                    if (!_shipmentDetailRepository.All.Any(a => a.IsActive == true && a.IsDeleted != true && a.PackingListDetailId == packinglistdetail.Id))
                    {
                        await _shipmentPlanPalletRepository.Add(new ShipmentPlanPallet()
                        {
                            IsAddedToShipment = false,
                            IsAddedToPickingList = false,
                            BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId,
                            CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            PackingListDetailId = packinglistdetail.Id,
                            ShipmentId = model.ShipmentId,
                            Quantity = packinglistdetail.Quantity,
                            RemainingQuantity = packinglistdetail.Quantity,
                            ReservedQuantity = 0
                        });
                    }
                    await _unitOfWork.Commit();
                }
                return new ShipmentResponseDTO()
                {
                    ErrMssg = "Pallet added Successfully.",
                    IsSuccess = true

                };
            }
            catch (Exception e)
            {
                //throw e;
                return new ShipmentResponseDTO()
                {
                    ErrMssg = e.Message,
                    IsSuccess = false
                };
            }
        }
        public async Task<List<ShipmentSheetDTO>> GetTruckLoadingBoxDetailDataForPrint(ShipmentPrintInputDTO model)
        {
            try
            {
                List<ShipmentSheetDTO> ShipmentSheeListDTO = new List<ShipmentSheetDTO>();
                Company company = _companyRepository.GetAll().FirstOrDefault();
                if (model.ShipmentId>0 && model.TruckLoadingSheetId > 0) //singledata
                {
                    ShipmentSheetDTO shipmentDto = new ShipmentSheetDTO();
                    var TruckLoadingsheetData = _truckLoadingSheetRepository
                                             .GetAll(x => x.Id == model.TruckLoadingSheetId && x.ShipmentId == model.ShipmentId)
                                             .Include(x => x.TruckLoadingSheetDetails).Include(x=>x.Shipment)
                                             .FirstOrDefault();
                    if (TruckLoadingsheetData != null)
                    {
                        
                        var userDetails = _userDetailsRepository.GetAll(x => x.Id == TruckLoadingsheetData.Shipment.UpdatedBy).FirstOrDefault();
                        shipmentDto.ShipFrom= TruckLoadingsheetData.ShipFrom;
                        shipmentDto.ShipTo = TruckLoadingsheetData.ShipTo;
                        shipmentDto.ShipmentId = TruckLoadingsheetData.ShipmentId;
                        shipmentDto.DriverName= TruckLoadingsheetData.Shipment.DriverName;
                        shipmentDto.OperatorName = userDetails != null ? userDetails.FirstName + "" + userDetails.LastName : "";
                        shipmentDto.ShipmentId= TruckLoadingsheetData.ShipmentId;
                        shipmentDto.NetWeight= TruckLoadingsheetData.NetWeight;
                        shipmentDto.GrossWeight= TruckLoadingsheetData.GrossWeight;
                        shipmentDto.TotalPallet= TruckLoadingsheetData.TotalPallet;
                        shipmentDto.TotalBox= TruckLoadingsheetData.BoxQty;
                        shipmentDto.Volume= TruckLoadingsheetData.Volume;
                        shipmentDto.VehicleNumber= TruckLoadingsheetData.TruckNumber;
                        shipmentDto.ShipmentDate= TruckLoadingsheetData.Date;
                        shipmentDto.FileName= TruckLoadingsheetData.QrCode;
                        shipmentDto.Barcode = TruckLoadingsheetData.BarCode;
                        shipmentDto.BarcodeUrl = TruckLoadingsheetData.BarCodeUrl;
                        shipmentDto.CompanyLogo = company!=null? company.LogoFilePath:"";
                        shipmentDto.shipmentPackingSheets = _mapper.Map<List<shipmentPackingSheetDTO>>(TruckLoadingsheetData.TruckLoadingSheetDetails);
                    }
                    ShipmentSheeListDTO.Add(shipmentDto);
                    return ShipmentSheeListDTO;
                }
                if(model.ShipmentId>0 && model.TruckLoadingSheetId == 0) // for all Toshow sheetData
                {
                    List<ShipmentSheetDTO> shipmentSheetListDTO = new List<ShipmentSheetDTO>();
                    var TruckLoadingsheetData = _truckLoadingSheetRepository
                                             .GetAll(x => x.ShipmentId == model.ShipmentId)
                                             .Include(x => x.TruckLoadingSheetDetails).Include(x => x.Shipment).ToList();
                    if (TruckLoadingsheetData.Count()>0)
                    {
                        foreach (var item in TruckLoadingsheetData)
                        {
                            ShipmentSheetDTO shipmentDtodata = new ShipmentSheetDTO();

                            var userDetails = _userDetailsRepository.GetAll(x => x.Id == item.Shipment.UpdatedBy).FirstOrDefault();
                            shipmentDtodata.TruckLoadingSheetId = item.Id;
                            shipmentDtodata.ShipFrom = item.ShipFrom;
                            shipmentDtodata.ShipTo = item.ShipTo;
                            shipmentDtodata.ShipmentId = item.ShipmentId;
                            shipmentDtodata.DriverName = item.Shipment.DriverName;
                            shipmentDtodata.OperatorName = userDetails != null ? userDetails.FirstName + "" + userDetails.LastName : "";
                            shipmentDtodata.ShipmentId = item.ShipmentId;
                            shipmentDtodata.NetWeight = item.NetWeight;
                            shipmentDtodata.GrossWeight = item.GrossWeight;
                            shipmentDtodata.TotalPallet = item.TotalPallet;
                            shipmentDtodata.TotalBox = item.BoxQty;
                            shipmentDtodata.Volume = item.Volume;
                            shipmentDtodata.VehicleNumber = item.TruckNumber;
                            shipmentDtodata.ShipmentDate = item.Date;
                            shipmentDtodata.FileName = item.QrCode;
                            shipmentDtodata.Barcode = item.BarCode;
                            shipmentDtodata.BarcodeUrl = item.BarCodeUrl;
                            shipmentDtodata.CompanyLogo = company!=null?company.LogoFilePath:"";
                            shipmentDtodata.shipmentPackingSheets = _mapper.Map<List<shipmentPackingSheetDTO>>(item.TruckLoadingSheetDetails);
                            shipmentSheetListDTO.Add(shipmentDtodata);
                        }
                        return shipmentSheetListDTO;
                    }
                    return shipmentSheetListDTO;
                }
                return ShipmentSheeListDTO;
            }
            catch(Exception e)
            {
                throw e;
            }

        }


        public async Task<List<ShipmentSheetDTO>> GetTruckLoadingBoxDetail(long shipmentId)
        {

            try
            {

                var dataddss = _shipmentPackingListRepository.GetAll(a => a.IsActive == true && a.IsDeleted != true && a.ShipmentId == shipmentId)
                  .Include(a => a.Shipment)
                  .Include(a => a.PackingList)
                  .ThenInclude(a => a.PackingListDetails)
                  .ThenInclude(a => a.SemiFinishGoodsPallet)
                  .Include(a => a.PackingList)
                  .ThenInclude(a => a.PackingListDetails)
                  .ThenInclude(a => a.ShipmentDetails)
                  .Include(a => a.PackingList)
                  .ThenInclude(a => a.CustomerPurchaseOrder)
                  .ThenInclude(a => a.Customer)
                  .ThenInclude(a => a.CityList)
                  .ThenInclude(a => a.State)
                  .ThenInclude(a => a.Country);

                var filteredData = dataddss.Where(x => x.PackingList.PackingListDetails.Any(y => y.ShipmentDetails.Count() > 0)).ToList();
                var selectdata = filteredData.Select(x => new ShipmentSheetDTO()
                {
                    ShipmentId=x.ShipmentId,
                    ShipFrom = x.Shipment != null ? x.Shipment.ShipFrom : null,
                    ShipTo = x.PackingList != null && x.PackingList.CustomerPurchaseOrder != null && x.PackingList.CustomerPurchaseOrder.Customer != null ? x.PackingList.CustomerPurchaseOrder.Customer.StreetAddress1 + ", " + x.PackingList.CustomerPurchaseOrder.Customer.StreetAddress2 + ", "+
                    ( x.PackingList.CustomerPurchaseOrder.Customer.CityList != null ? x.PackingList.CustomerPurchaseOrder.Customer.CityList.Name : " ") + ", "+ (x.PackingList.CustomerPurchaseOrder.Customer.CityList.State != null ? x.PackingList.CustomerPurchaseOrder.Customer.CityList.State.Name : "") +
                    ", "+(x.PackingList.CustomerPurchaseOrder.Customer.CityList.State.Country != null ? x.PackingList.CustomerPurchaseOrder.Customer.CityList.State.Country.Name : ""): "",
                    VehicleNumber = x.Shipment != null ? x.Shipment.VehicleNumber : null,
                    PackingListId = x.PackingListId,
                    TotalBox = x.Shipment != null ? x.Shipment.TotalBox : 0,
                    TotalPallet = x.Shipment != null ? x.Shipment.TotalPallet : 0,
                    GrossWeight = x.Shipment != null ? x.Shipment.GrossWeight : 0,
                    NetWeight = x.Shipment != null ? x.Shipment.NetWeight : 0,
                    Volume = x.Shipment != null ? x.Shipment.NetVolume : 0,
                    ShipmentDate = x.Shipment.ShipmentDate,
                    shipmentPackingSheets = x.PackingList.PackingListDetails.Where(b => b.ShipmentDetails.Count() > 0).Select(y => new shipmentPackingSheetDTO()
                    {
                        PackingListNumber = y.PackingList != null ? y.PackingList.PackingListNumber: null,
                        PalletId = y.SemiFinishGoodsPallet != null ? y.SemiFinishGoodsPallet.PalletId : null,
                        GrossWeight = y.ShipmentDetails != null ? y.ShipmentDetails.FirstOrDefault().GrossWeight : null,
                        NetWeight = y.ShipmentDetails != null ? y.ShipmentDetails.FirstOrDefault().NetWeight : null,
                         Volume = y.ShipmentDetails != null ? y.ShipmentDetails.FirstOrDefault().NetVolume : null,
                        PLTotalBox = y.SemiFinishGoodsPallet != null ? y.SemiFinishGoodsPallet.Quantity : 0
                    }).ToList()
                }).ToList();

                return selectdata;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<bool> UpsertTruckLoadingDetailOnCompletion(TruckLoadingSheetUpsertDTO model)
        {
            try
            {
                if(model.ShipmentId > 0)
                {
                    var shipment = await _shipmentRepository.GetSingle(model.ShipmentId);
                    var truckloadingData = await GetTruckLoadingBoxDetail(model.ShipmentId);
                    foreach(var data in truckloadingData)
                    {
                        var code = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.TRUCK_LOADING), GCode = "" };
                        string TLSheetCode =   _codeGenerationTemplateService.GenerateCode(code).Result.GCode;

                        //var qrcode = shipment.Code + "_" + data.shipmentPackingSheets.FirstOrDefault().PackingListNumber;
                        //var qrCodeUrl = _barcodeService.createQRCodeUrl(qrcode, "Content/QrCode/TruckLoadingSheet/", model.RootPath);

                        //var barcode = shipment.Code + "_" + data.shipmentPackingSheets.FirstOrDefault().PackingListNumber;
                        //var barCodeUrl = _barcodeService.createBarcodeUrl(barcode, "Content/Barcodes/TruckLoadingSheet", model.RootPath);

                        string BarcodeUrl = "";
                        string QrCodeUrl = "";
                        if (!string.IsNullOrEmpty(TLSheetCode))
                        {
                            QrCodeUrl = _barcodeService.createQRCodeUrl(TLSheetCode, "Content/QrCode/TruckLoadingSheet/", model.RootPath);
                            BarcodeUrl = _barcodeService.createBarcodeUrl(TLSheetCode, "Content/Barcodes/TruckLoadingSheet", model.RootPath);
                        }
                        TruckLoadingSheet truckLoading = new TruckLoadingSheet()
                        {
                            
                            Date = data.ShipmentDate,
                            ShipFrom = data.ShipFrom,
                            ShipTo = data.ShipTo,
                            TruckNumber = data.VehicleNumber,
                            TotalPallet = data.TotalPallet ?? 0,
                            BoxQty = data.TotalBox ?? 0,
                            GrossWeight = data.GrossWeight ?? 0,
                            NetWeight = data.NetWeight ?? 0,
                            Volume = data.Volume ?? 0,
                            ShipmentId = model.ShipmentId,
                            QrCode = TLSheetCode,
                            QrCodeUrl = QrCodeUrl,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = model.UserId,
                            CreatedOn = DateTime.Now,
                            BranchId = model.BranchId,
                            BarCode= TLSheetCode,
                            BarCodeUrl= BarcodeUrl
                        };

                        List<TruckLoadingSheetDetail> truckloadingDetailList = new List<TruckLoadingSheetDetail>();
                        foreach(var packingsheetitem in data.shipmentPackingSheets)
                        {
                            TruckLoadingSheetDetail truckLoadingSheetDetail = new TruckLoadingSheetDetail()
                            {
                                PalletId = packingsheetitem.PalletId,
                                GrossWeight = packingsheetitem.GrossWeight ?? 0,
                                NetWeight = packingsheetitem.NetWeight ?? 0,
                                Volume = packingsheetitem.Volume ?? 0,
                                PLTotalBox = packingsheetitem.PLTotalBox ?? 0,
                                PackingListNumber = packingsheetitem.PackingListNumber ?? "",
                                IsActive = true,
                                IsDeleted =false,
                                CreatedBy = model.UserId,
                                CreatedOn = DateTime.Now,
                                BranchId = model.BranchId
                            };

                            truckloadingDetailList.Add(truckLoadingSheetDetail);
                        }

                        truckLoading.TruckLoadingSheetDetails = truckloadingDetailList;
                        await _truckLoadingSheetRepository.Add(truckLoading);
                        await _unitOfWork.Commit();
                        
                    }
                    return true;
                }
                return false;
            }catch(Exception ex)
            {
                throw ex;
            }
        }


        public string GeneratePdfTemplateShipment(QueryResult<ShipmentDTO> result)
        {
            var sb = new StringBuilder();

            sb.Append(@"<html>
                            <head>
                                <h1>Shipment Plans</h1>
                            </head>
                            <body>
                                <table border='1' align='center' style='border-collapse:collapse;'>");
            sb.Append(@"<thead>
                            <tr>
                                        <th>Id</th>
                                        <th>Code</th>
                                        <th>Vehicle</th>
                                        <th>VehicleType</th>
                                        <th>Driver</th>
                                        <th>Driver Number</th>
                                        <th>Driver Licence Number</th>
                                        <th>Driver Address</th>
                                        <th>Transport Agency</th>
                                        <th>Transport Agency Address</th>
                                        <th>Transport Agency Person</th>
                                        <th>Transport Agency Person Number</th>
                                        <th>Ship Form</th>
                                        <th>Ship To</th>
                                        <th>Shipment Date</th>
                                        <th>Total Pallet</th>
                                        <th>Reservation Status</th>
                                        <th>Pon Status</th>
                                        <th>Completed</th>
                                        <th>isActive</th>
                                       
                                       
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
                                    <td>{16}</td>
                                    <td>{17}</td>
                                    <td>{18}</td>
                                    <td>{19}</td>
                                  
                                   
                     
                                   </tr>", item.Id,item.Code, item.VehicleName, item.MSTVehicleTypeName, item.DriverName, item.DriverPhoneNumber, item.DriverLicenseNumber, item.DriverAddress,item.TransportAgencyName, item.TransportAgencyAddress, item.TransportAgencyContactPersonName,item.TransportAgencyContactPersonPhoneNumber,
                                            item.ShipFrom,item.ShipTo,item.ShipmentDate,item.TotalPallet,item.MSTReservationStatusName,item.MSTPONStatusName,item.IsCompleted,item.IsActive);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }
        public bool CheckAvailablity(long ShipmentId)
        {

            try
            {
                var IsSufficientAvailble = false;
                if (ShipmentId > 0)
                {
                    var PlanDetaildata = _shipmentPlanPalletRepository.GetAll(x => x.IsActive == true && x.IsDeleted != true && x.ShipmentId == ShipmentId, a => a.PackingListDetail , a => a.PackingListDetail.MSTFinishGoods).ToList();
                    var FgDefaultLocation = _mst_WareHouseDefaultLocationRepository.GetAll(x => x.IsDeleted != true && x.Code == MainEnumHelper.GetDescription(Enums.DefaultLocationCode.FG_goods_Storage)).FirstOrDefault();
                    
                    IsSufficientAvailble = PlanDetaildata.Where(a => a.RemainingQuantity > 0).Count() == 0;
                    if (IsSufficientAvailble == false)
                    {
                        IsSufficientAvailble = true;
                        var selectedPallets = PlanDetaildata.Where(a => a.RemainingQuantity > 0).Select(a => new { SemiFinishGoodsPalletId = (a.PackingListDetail.SemiFinishGoodsPalletId ?? 0), ItemId = a.PackingListDetail.ItemId }).ToList();

                      //  var creditedItemsss = _itemTransactionDetailRepository.GetAll(a => a.WareHouseId == FgDefaultLocation.WareHouseId && a.SubInventoryId == FgDefaultLocation.SubInventoryId
                      // && a.ItemTransaction.TransactionType == "IN" && a.RemainingQuantity > 0
                      //&& (FgDefaultLocation.WareLocationId.HasValue == false || FgDefaultLocation.WareLocationId == 0 || a.WareHouseLocationId == FgDefaultLocation.WareLocationId)
                      //&& (selectedPallets.Any(b => b.SemiFinishGoodsPalletId == a.ItemTransaction.SemiFinishGoodsPalletId && a.ItemTransaction.ItemId == b.ItemId))
                      //, a => a.ItemTransaction).ToList();



                        var creditedItem = _itemTransactionDetailRepository.GetAll(a =>a.IsDeleted != true && a.IsActive != false &&  a.WareHouseId == FgDefaultLocation.WareHouseId && a.SubInventoryId == FgDefaultLocation.SubInventoryId
                           && a.ItemTransaction.TransactionType == "IN" && a.RemainingQuantity > 0
                          && (FgDefaultLocation.WareLocationId.HasValue == false || FgDefaultLocation.WareLocationId == 0 || a.WareHouseLocationId == FgDefaultLocation.WareLocationId)
                          , a => a.ItemTransaction).ToList();
                          

                        var creditedItems = creditedItem.Where(a => selectedPallets.Any(b => b.SemiFinishGoodsPalletId == a.ItemTransaction.SemiFinishGoodsPalletId && a.ItemTransaction.ItemId == b.ItemId)).ToList();




                        if (creditedItems.Count() != PlanDetaildata.Where(a => a.RemainingQuantity > 0).Count())
                        {
                            IsSufficientAvailble = false;
                        }
                    }
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

        public async Task<bool> ReserveFG(long id, bool isPerformedByUser = false)
        {

            try
            {
                if (id > 0)
                {

                    var FGDefaultLocation = _mst_WareHouseDefaultLocationRepository.GetAll(x => x.IsDeleted != true && x.Code == MainEnumHelper.GetDescription(Enums.DefaultLocationCode.FG_goods_Storage)).FirstOrDefault();

                    var shipmentplan = _shipmentRepository.GetById(x => x.Id == id);
                    var data = _shipmentPlanPalletRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.RemainingQuantity > 0 && x.ShipmentId == id,a=>a.PackingListDetail, a => a.PackingListDetail.MSTFinishGoods).ToList();
                    //var webdata = _webSettingRepository.GetAll().ToList().FirstOrDefault();
                    ItemReserveDTO itemReserveDTO = new ItemReserveDTO();
                    itemReserveDTO.ItemReserveDetailDTOs = new List<ItemReserveDetailDTO>();
                    itemReserveDTO.SourceWareHouseId = FGDefaultLocation.WareHouseId;
                    itemReserveDTO.SourceSubInventoryId = FGDefaultLocation.SubInventoryId;

                    if(data.Where(x => x.RemainingQuantity > 0).Count() == 0) return true;


                    foreach (var item in data)
                    {

                        if(item.RemainingQuantity > 0)
                        {
                            itemReserveDTO.ItemReserveDetailDTOs.Add(new ItemReserveDetailDTO()
                            {
                                ItemId =item.PackingListDetail.ItemId ?? 0,
                                ObjectId = item.Id,
                                ObjectName = "ShipmentPlanPallet",
                                Quantity = item.PackingListDetail.Quantity ?? 0,
                                SemiFinishGoodsPalletId = item.PackingListDetail.SemiFinishGoodsPalletId
                            });
                        }

                    }
                    bool res = false;
                    if(itemReserveDTO.ItemReserveDetailDTOs.Count() > 0) res = await _itemTransactionService.ReserveItemTransNItemTransDetail(itemReserveDTO);

                    if (res)
                    {

                        shipmentplan.MSTReservationStatusId = (int?)ReservationStatus.Reserved;
                        //salesorderplan.MSTOrderStatusId = (int)Enums.MST_SalesOrderStatus.PendingForProduction;


                        ActionLogDTO actionLog = new ActionLogDTO()
                        {
                            //Description = "Material Reserved for Sales Order Plan : " + salesorderplan.Id.ToString() +   ".",
                            Description = "FG Reserved For " + shipmentplan.Code + " .",
                            IsPerformedByUser = isPerformedByUser,
                            MST_LogStageId = (int)Enums.MST_LogStageEnum.SalesOrderPlan,
                            MST_ActionStageId = (int)Enums.MST_ActionStageEnum.Material_Reserved,
                            ObjectId = shipmentplan.Id
                        };
                        await _actionLogService.CreateActionLog(actionLog);
                        await _unitOfWork.Commit();
                    }
                    return res;
                }

                return false;

                //}
                //else
                //{
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<long> CreatePickingTask(long shipmentPlanId)
        {
            try
            {
                ShipmentTask sOtask = _shipmentTaskRepository.GetById(a => a.ShipmentId == shipmentPlanId && a.IsDeleted != true);
                if (sOtask != null)
                    return sOtask.ProcessTaskId ?? 0;
                var webdata = _webSettingRepository.GetAll().ToList().FirstOrDefault();
                var userId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId;
                Shipment shipment = _shipmentRepository.GetById(x => x.Id == shipmentPlanId);
                List<long> defaultIds = new List<long>();
                List<long> defaultRoleIds = new List<long>();
                if (!string.IsNullOrEmpty(webdata.DefaultProductionPlanPickingListAssignedToIds))
                    defaultIds = webdata.DefaultProductionPlanPickingListAssignedToIds.Split(",").Select(a => Convert.ToInt64(a)).ToList();
                if (!string.IsNullOrEmpty(webdata.DefaultProductionPlanPickingListAssignedToRoleIds))
                    defaultRoleIds = webdata.DefaultProductionPlanPickingListAssignedToRoleIds.Split(",").Select(a => Convert.ToInt64(a)).ToList();

                var code = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.Shipment_Plan_Process_Task), GCode = "" };

                ProcessTaskDTO processTask = new ProcessTaskDTO()
                {
                    TaskNumber = "TASK_" + shipment.Code,
                    Name = "Picking Task for ShipmentPlan: " + shipment.Code,
                    Code = _codeGenerationTemplateService.GenerateCode(code).Result.GCode,
                    Description = "Picking Task for ShipmentPlan : " + shipment.Code,
                    CreatedFromProcess = "ShipmentPlan",
                    AssignedBy = userId,
                    AssignedOn = DateTime.Now,
                    TaskDate = DateTime.Now,
                    SendEmailNotification = false,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId,
                    AssignedToRoleIds = defaultRoleIds,
                    AssignedToIds = defaultIds,
                    MSTProcessTaskStatusId = (int)Enums.MST_ProcessTaskStatus.Pending,
                    MSTProcessTaskPriorityId = (int)Enums.MSTProcessTaskPriorityStatus.MEDIUM,
                    MSTProcessTaskTypeId = (int)Enums.MSTProcessTaskType.Shipment_Plan,
                    TaskStartedOn = DateTime.Now
                };

                var processId = await _processTaskService.CreateProcessTask(processTask);

                ShipmentTask planTask = new ShipmentTask()
                {
                    ShipmentId = shipmentPlanId,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    ProcessTaskId = processId,

                };
                await _shipmentTaskRepository.Add(planTask);


                ActionLogDTO actionLogprocess = new ActionLogDTO()
                {
                    Description = "Process Task " + processTask.Code + " For Shipment Plan " + shipment.Code + " Created.",
                    IsPerformedByUser = false,
                    MST_LogStageId = (int)Enums.MST_LogStageEnum.ShipmentPlan,
                    MST_ActionStageId = (int)Enums.MST_ActionStageEnum.Picking_Task_Generated,
                    ObjectId = shipmentPlanId
                };
                ActionLogDTO actionLog = new ActionLogDTO()
                {
                    Description = "Shipment Plan Task For " + shipment.Code + " Created.",
                    IsPerformedByUser = false,
                    MST_LogStageId = (int)Enums.MST_LogStageEnum.ShipmentPlan,
                    MST_ActionStageId = (int)Enums.MST_ActionStageEnum.SHIPMENT_TASK,
                    ObjectId = shipmentPlanId
                };
                await _actionLogService.CreateActionLog(actionLogprocess);
                await _actionLogService.CreateActionLog(actionLog);
                await _unitOfWork.Commit();
                return processId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<long> GeneratePicking(long shipmentId, long processTaskId)
        {

            try
            {
                var pickinglst = _pickingListRepository.GetById(a => a.ProcessTaskId == processTaskId && a.IsDeleted != true);
                if (pickinglst != null)
                {
                    return pickinglst.Id;
                }
                //TO Do :: include kitting location 
                var FGDefaultLocation = _mst_WareHouseDefaultLocationRepository.GetAll(x => x.IsDeleted != true && x.Code == MainEnumHelper.GetDescription(Enums.DefaultLocationCode.FG_goods_Storage)).Include(x => x.WareLocation).FirstOrDefault();
                var DockDefaultLocation = _mst_WareHouseDefaultLocationRepository.GetAll(x => x.IsDeleted != true && x.Code == MainEnumHelper.GetDescription(Enums.DefaultLocationCode.DOCK)).Include(x => x.WareLocation).FirstOrDefault();
                if (DockDefaultLocation == null) return 0;

                //DefaultWareHouseLocation 
                var DockDefaultWareHouseLocation = _wareHouseLocationRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.IsDefault == true && x.WareHouseId == DockDefaultLocation.SubInventoryId, x => x.MSTWareHouseLocation).FirstOrDefault();
                var FGDefaultWareHouseLocation = _wareHouseLocationRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.IsDefault == true && x.WareHouseId == FGDefaultLocation.SubInventoryId).FirstOrDefault();

                var userId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId;
                var salesOrderPlan = _shipmentRepository.GetById(x => x.Id == shipmentId);
                var data = _shipmentPlanPalletRepository.GetAll(x => x.ShipmentId == shipmentId,a=>a.PackingListDetail, x => x.PackingListDetail.MSTFinishGoods).ToList();
                var code = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.SHIPMENT_PLIST), GCode = "" };
                PickingList plist = new PickingList()
                {
                    Code = _codeGenerationTemplateService.GenerateCode(code).Result.GCode,
                    SourceWareHouseId = FGDefaultLocation.WareHouseId,
                    SourceSubInventoryId = FGDefaultLocation.SubInventoryId,
                    SourceWareLocationId = FGDefaultWareHouseLocation != null ?  FGDefaultWareHouseLocation.MSTWareHouseLocationId : 0,
                    SourceWareHouseSubLocationId = FGDefaultLocation.WareHouseSubLocationId,
                    DeliverToWareHouseId = DockDefaultLocation.WareHouseId,
                    DeliverToSubInventoryId = DockDefaultLocation.SubInventoryId,
                    //DeliverToWareLocationId = DockDefaultLocation.WareLocation != null ? DockDefaultLocation.WareLocation.KittingLocationId : null,
                    DeliverToWareLocationId = DockDefaultWareHouseLocation != null && DockDefaultWareHouseLocation.MSTWareHouseLocation != null ? DockDefaultWareHouseLocation.MSTWareHouseLocation.KittingLocationId : null,
                    DeliverToWareHouseSubLocationId = DockDefaultLocation.WareHouseSubLocationId,
                    ProcessTaskId = processTaskId,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,

                };

                List<PickingListDetail> plistDetails = new List<PickingListDetail>();
                foreach (var item in data)
                {
                    List<WareHouseReservedItem> wareHouseReservedItems = _wareHouseReservedItemRepository.GetAll(a => a.IsDeleted != true && a.ObjectId == item.Id
                    && a.ObjectName == "ShipmentPlanPallet" && a.ItemId == item.PackingListDetail.ItemId).Include(x => x.ItemTransactionDetail).ToList();
                    foreach (var wareHouseReservedItem in wareHouseReservedItems)
                    {
                        PickingListDetail plistdetail = new PickingListDetail()
                        {
                            ItemId = item.PackingListDetail.ItemId,
                            PalletIDNumber = wareHouseReservedItem.PalletIDNumber,
                            CaseNumber = wareHouseReservedItem.CaseNumber,
                            CreatedBy = userId,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            BoxStatus = wareHouseReservedItem.BoxStatus,
                            ObjectName = wareHouseReservedItem.ObjectName,
                            ObjectId = wareHouseReservedItem.ObjectId,
                            RequiredQuantity = wareHouseReservedItem.Quantity,
                            WareHouseId = wareHouseReservedItem.ItemTransactionDetail != null ? wareHouseReservedItem.ItemTransactionDetail.WareHouseId : 0,
                            SubInventoryId = wareHouseReservedItem.ItemTransactionDetail != null ? wareHouseReservedItem.ItemTransactionDetail.SubInventoryId : 0,
                            WareHouseLocationId = wareHouseReservedItem.ItemTransactionDetail != null ? wareHouseReservedItem.ItemTransactionDetail.WareHouseLocationId : 0,
                            WareHouseSubLocationId = wareHouseReservedItem.ItemTransactionDetail != null ? wareHouseReservedItem.ItemTransactionDetail.WareHouseSubLocationId : 0,
                            TransactionDetailId = wareHouseReservedItem.ItemTransactionDetailId,
                            SemiFinishGoodsPalletId = wareHouseReservedItem.SemiFinishGoodsPalletId,
                            KittingTransactionDetailId = wareHouseReservedItem.ItemTransactionDetailId
                        };
                        plistDetails.Add(plistdetail);
                        wareHouseReservedItem.PickingCompletionDate = DateTime.Now;
                        wareHouseReservedItem.IsPickingCompleted = true;
                    }

                }

                plist.PickingListDetails = plistDetails;
                await _pickingListRepository.Add(plist);


                ActionLogDTO actionLog = new ActionLogDTO()
                {
                    Description = "Picking List Generated.",
                    IsPerformedByUser = true,
                    MST_LogStageId = (int)Enums.MST_LogStageEnum.ShipmentPlan,
                    MST_ActionStageId = (int)Enums.MST_ActionStageEnum.pickingList_Generated,
                    ObjectId = shipmentId
                };
                await _actionLogService.CreateActionLog(actionLog);
                await _unitOfWork.Commit();
                return plist.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<bool> UpdateOrderStatus(long shipmentId,string status)
        {
            try
            {
                Shipment shipment = await _shipmentRepository.GetSingle(shipmentId);
                if(shipment != null)
                {
                    if(shipment.MSTReservationStatusId == (int)Enums.ReservationStatus.Reserved) shipment.MSTPONStatusId = (int)Enums.PONStatus.Released;

                }

                var processTaskIds = _shipmentPlanPalletRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == shipmentId,x => x.PackingListDetail.PackingList).Select(x => x.PackingListDetail.PackingList.ProcessTaskId).Distinct().ToList();
                if(processTaskIds.Count() == 0) return false;
                foreach(var id in processTaskIds)
                {
                    var processTaskId = id;
                    var processTaskData = _processTaskRepository.GetAll(x => x.Id == id && x.IsActive != false && x.IsDeleted != true).FirstOrDefault();
                    if(processTaskData != null && processTaskData.ParentTaskId > 0)
                    {
                        processTaskId = processTaskData.ParentTaskId;
                    }

                    var salesOrderPlanTask = _salesOrderPlanTaskRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ProcessTaskId == processTaskId, x => x.SalesOrderPlan).FirstOrDefault();
                    if(salesOrderPlanTask != null && salesOrderPlanTask.SalesOrderPlan != null)
                    {

                        if (status == "PendingForGatePass" && salesOrderPlanTask.SalesOrderPlan.MSTOrderStatusId == (int)Enums.MST_SalesOrderStatus.PendingForGatePass )
                        {
                            salesOrderPlanTask.SalesOrderPlan.MSTOrderStatusId = (int)Enums.MST_SalesOrderStatus.Delivered;
                        }

                        else if (status == "PendingForDelivery" && salesOrderPlanTask.SalesOrderPlan.MSTOrderStatusId == (int)Enums.MST_SalesOrderStatus.PendingForDelivery)
                        {
                            salesOrderPlanTask.SalesOrderPlan.MSTOrderStatusId = (int)Enums.MST_SalesOrderStatus.PendingForGatePass;
                        }

                        else if (status == "PendingForShipment" &&  salesOrderPlanTask.SalesOrderPlan.MSTOrderStatusId == (int)Enums.MST_SalesOrderStatus.PendingForShipment)
                        {
                            salesOrderPlanTask.SalesOrderPlan.MSTOrderStatusId = (int)Enums.MST_SalesOrderStatus.PendingForDelivery;
                            //await _unitOfWork.Commit();
                        }
                      
                        
                        await _unitOfWork.Commit();
                        
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<salesReseveditemDTO>> GetWareHouseReservedItems(SalesQueryProc model)
        {
            try
            {

                if (model != null)
                {
                    List<salesReseveditemDTO> reseveditems = new List<salesReseveditemDTO>();
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
                return new List<salesReseveditemDTO>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private List<salesReseveditemDTO> getitemReserved(DataTable result)
        {
            var Item = result.AsEnumerable().Select(row =>
                         new salesReseveditemDTO
                         {
                             ID = (row.Field<long>("ID")),
                             ITEMCODE = (row.Field<string?>("ITEMCODE")),
                             RemainingQunatity = (row.Field<decimal?>("RemainingQantity")),
                             NAME = (row.Field<string?>("NAME")),
                             RequiredQuantity = (row.Field<decimal?>("RequiredQuantity")),
                             ActualReservedQunatity = (row.Field<decimal?>("ActualReservedQuantity")),
                             ReservedQunatity = (row.Field<decimal?>("ReservedQuantity")),
                         }).ToList();
            return Item;
        }


        //picked pallet for scanning
        public async Task<List<waitingScanPalletDTO>> getPickedPalletForScanning(long shipmentId)
        {

            try
            {
                if (shipmentId > 0)
                {
                    List<waitingScanPalletDTO> waitingScanPallet = new List<waitingScanPalletDTO>();
                    var shipmenttask = _shipmentTaskRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == shipmentId).Include(x => x.ProcessTasks).ThenInclude(x => x.PickingLists).ThenInclude(x => x.PickingListDetails).FirstOrDefault();
                    var packinglistdetail = _packingListDetailRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.PackingList.ShipmentPackingLists.Any(z => z.ShipmentId == shipmentId), x => x.SemiFinishGoodsPallet, x => x.PackingList);
                    foreach (var pickinglistitem in shipmenttask.ProcessTasks.PickingLists.FirstOrDefault().PickingListDetails)
                    {
                        if (pickinglistitem.IsPicked == true)
                        {
                            var packinglistdetaildata = packinglistdetail.Where(x => x.SemiFinishGoodsPallet.PalletId.Trim() == pickinglistitem.PalletIDNumber.Trim()).FirstOrDefault();
                            var waitingtoscanpallet = new waitingScanPalletDTO
                            {
                                PalletNumber = pickinglistitem.PalletIDNumber,
                                PickingListId = pickinglistitem.PickingListId,
                                PickingListDetailId = pickinglistitem.Id,
                                PackingListDetalId = packinglistdetaildata != null ? packinglistdetaildata.Id : 0,
                                PackingListId = packinglistdetaildata != null ? packinglistdetaildata.PackingListId : 0,
                                PackingListNumber = packinglistdetaildata != null && packinglistdetaildata.PackingList != null ? packinglistdetaildata.PackingList.PackingListNumber : "",
                                IsAddedToShipment = pickinglistitem.IsAddedToShipment ?? false,

                            };
                            waitingScanPallet.Add(waitingtoscanpallet);
                           
                        }
                    }
                    waitingScanPallet = waitingScanPallet.OrderBy(x => x.PalletNumber).ToList();
                    return waitingScanPallet;

                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<ShipmentResponseDTO> ScanPallet(ScanPalletShipmentInputDTO model)
        {
            try
            {
                model.PalletId = model.PalletId.Trim();
                //bool IsUpdate = false;
                var pldetail = _packingListDetailRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.SemiFinishGoodsPallet.PalletId == model.PalletId, x => x.SemiFinishGoodsPallet).FirstOrDefault();
                if (pldetail == null) return new ShipmentResponseDTO()
                {
                    ErrMssg = "Invalid Pallet",
                    IsError = true

                };

                //var packingListDetail = _packingListDetailRepository
                //    .GetAll(x => x.IsDeleted != true && x.IsActive != false && x.IsAddedToShipment == true && x.SemiFinishGoodsPallet.PalletId == model.PalletId && x.pi)
                //    .Include(x => x.SemiFinishGoodsPallet).ThenInclude(x => x.SemiFinishGoods)
                //    .FirstOrDefault();


                var shipmenttask = _shipmentTaskRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == model.ShipmentId).Include(x => x.ProcessTasks).
                    ThenInclude(x => x.PickingLists).ThenInclude(x => x.PickingListDetails).FirstOrDefault();

                var ispalletaddedtoshipment = shipmenttask.ProcessTasks.PickingLists.FirstOrDefault().PickingListDetails.Where(x => x.PalletIDNumber.Trim() == model.PalletId.Trim() && x.IsDeleted != true && x.IsActive != false).FirstOrDefault();

                if (ispalletaddedtoshipment == null) return new ShipmentResponseDTO()
                {
                    ErrMssg = "Invalid Pallet, Scan valid Pallet.",
                    IsError = true

                };



                if (ispalletaddedtoshipment != null)
                {
                    if(ispalletaddedtoshipment.IsAddedToShipment == true) return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Pallet Already Scanned, Cannot Scan Anymore.",
                        IsError = true

                    };

                    var totalScannedShipment = _shipmentDetailRepository.GetAll(x => x.IsActive != false && x.IsDeleted != true && x.ShipmentId == model.ShipmentId).ToList();
                    var TotalPickedPalletList = shipmenttask.ProcessTasks.PickingLists.FirstOrDefault().PickingListDetails.Where(x => x.IsDeleted != true && x.IsActive != false && x.IsAddedToShipment == true).ToList();


                    PackingList packingList = _packingListRepository.GetById(x => x.IsDeleted != true && x.IsActive != false && x.Id == pldetail.PackingListId, x => x.SalesOrder);
                    if (packingList != null && packingList.MST_ShipmentStatusId == (int)Enums.MST_ShipmentStatus.STARTED)
                    {
                        packingList.MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.PARTIALLY_COMPLETED;
                        PackingListLog packingListLog = new PackingListLog()
                        {
                            CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.PARTIALLY_COMPLETED,
                            MST_PackingListStageId = (int)Enums.MST_PackingListStage.SHIPMENT,
                            PackingListId = packingList.Id,
                            EventById = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            EventDate = DateTime.Now,
                        };
                        await _packingListLogRepository.Add(packingListLog);
                    }

                    if (packingList != null && (TotalPickedPalletList.Count() > 0 &&
                        (totalScannedShipment.Count() + 1) == TotalPickedPalletList.Count()))
                    {
                        packingList.MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.COMPLETED;
                        PackingListLog packingListLog = new PackingListLog()
                        {
                            CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            CreatedOn = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            MST_ShipmentStatusId = (int)Enums.MST_ShipmentStatus.COMPLETED,
                            MST_PackingListStageId = (int)Enums.MST_PackingListStage.SHIPMENT,
                            PackingListId = packingList.Id,
                            EventById = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            EventDate = DateTime.Now,
                        };
                        await _packingListLogRepository.Add(packingListLog);
                    }



                    ShipmentDetail shipmentDetail = new ShipmentDetail();
                    shipmentDetail.ShipmentId = model.ShipmentId;
                    shipmentDetail.NetWeight = pldetail.NetWeight;
                    shipmentDetail.GrossWeight = pldetail.GrossWeight ?? 0;
                    shipmentDetail.GrossVolume = pldetail.Volume;
                    shipmentDetail.NetVolume = pldetail.Volume;
                    shipmentDetail.BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId;
                    shipmentDetail.CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId;
                    shipmentDetail.CreatedOn = DateTime.Now;
                    shipmentDetail.IsActive = true;
                    shipmentDetail.IsDeleted = false;
                    shipmentDetail.PackingListDetailId = pldetail.Id;
                    await _shipmentDetailRepository.Add(shipmentDetail);

                    //update SHipment
                    Shipment shipment = _shipmentRepository.GetById(x => x.Id == model.ShipmentId);
                    shipment.TotalBox = shipment.TotalBox ?? 0 + (pldetail.SemiFinishGoodsPallet != null ? pldetail.SemiFinishGoodsPallet.Quantity : 0);
                    shipment.TotalPallet = shipment.TotalPallet ?? 0 + 1;
                    shipment.GrossWeight = shipment.GrossWeight ?? 0 + pldetail.GrossWeight;
                    shipment.NetWeight = shipment.NetWeight ?? 0 + pldetail.NetWeight;
                    shipment.NetVolume = shipment.NetVolume ?? 0 + pldetail.Volume;

                    //update pickinglistdetal
                    var pickinglistdetail = _pickingListDetailRepository.GetById(x => x.Id == ispalletaddedtoshipment.Id);
                    pickinglistdetail.IsAddedToShipment = true;

                    ActionLogDTO actionLogPOQC = new ActionLogDTO()
                    {
                        Description = "Shipment Pallet : " + model.PalletId + " Scanned.",
                        IsPerformedByUser = true,
                        MST_LogStageId = (int)Enums.MST_LogStageEnum.SalesOrderPlan,
                        MST_ActionStageId = (int)Enums.MST_ActionStageEnum.Shipment_Pallet_Scanned,
                        ObjectId = packingList.SalesOrder != null ? packingList.SalesOrder.SalesOrderPlanId : 0,
                    };
                    await _actionLogService.CreateActionLog(actionLogPOQC);
                    await _unitOfWork.Commit();

                    return new ShipmentResponseDTO()
                    {
                        ErrMssg = "Pallet Successfully Scanned.",
                        IsSuccess = true

                    };
                    //IsUpdate = true;

                }
                return new ShipmentResponseDTO()
                {
                    ErrMssg = "Something Went Wrong.",
                    IsError = true

                }; ;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<bool> ScanTruckLoadingSheetQrCode(ScanTruckLoadingSheetQrCodeDTO model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.QrCode))
                {
                    var truckloadingSheetData = _truckLoadingSheetRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.QrCode.Trim() == model.QrCode.Trim()).FirstOrDefault();
                    if(truckloadingSheetData != null)
                    {
                        truckloadingSheetData.GetPassedOn = DateTime.Now;
                        truckloadingSheetData.IsGatePassed = true;
                        await _unitOfWork.Commit();
                        var updateStatus = UpdateOrderStatus(truckloadingSheetData.ShipmentId, "PendingForGatePass");
                        return true;
                    }

                    return false;
                }
                return false;
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        public async Task<QueryResult<TruckLoadingSheetDTO>> SearchForTruckLoadingSheet(TruckLoadingQueryObject query)
        {
            try
            {
                if (string.IsNullOrEmpty(query.SortBy))
                {
                    query.SortBy = "Id";
                }

                var columnMap = new Dictionary<string, Expression<Func<TruckLoadingSheetDTO, object>>>()
                {
                    ["Id"] = p => p.Id,
                };

                var truckLoadingSheet = _truckLoadingSheetRepository
                                       .GetAll(x => x.IsDeleted != true).Include(x=>x.Shipment)
                                       .Include(x=>x.TruckLoadingSheetDetails).AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchString))
                {
                    truckLoadingSheet = truckLoadingSheet.Where(x => x.TruckNumber.Trim().ToLower().Contains(query.SearchString)|| x.ShipTo.Trim().ToLower().Contains(query.SearchString.Trim().ToLower())
                    );
                }
                if (query.IsPassed??false)// Is passed Is used For Waiting and All Distinguish
                {
                    truckLoadingSheet = truckLoadingSheet.Where(x => x.IsGatePassed != true); 
                }
                else
                {
                    truckLoadingSheet = truckLoadingSheet.OrderBy(x => x.IsGatePassed);
                }
                if (! string.IsNullOrEmpty(query.PassedOrFail))
                {
                    if(query.PassedOrFail=="Pass")
                          truckLoadingSheet = truckLoadingSheet.Where(x => x.IsGatePassed == true);
                    if(query.PassedOrFail == "Fail")
                        truckLoadingSheet = truckLoadingSheet.Where(x => x.IsGatePassed != true);
                }

                var itemList = truckLoadingSheet.Select(x => new TruckLoadingSheetDTO()
                {
                    Id = x.Id,
                    TruckNumber=x.TruckNumber,
                    ShipmentId=x.ShipmentId,
                    ShipFrom=x.ShipFrom,
                    ShipTo=x.ShipTo,
                    TotalPallet=x.TotalPallet,
                    BoxQty=x.BoxQty,
                    QrCode=x.QrCode,
                    QrCodeUrl=x.QrCodeUrl,
                    NetWeight=x.NetWeight,
                    Date=x.Date??DateTime.Now,
                    GrossWeight=x.GrossWeight,
                    Volume=x.Volume,
                    IsGatePassed=x.IsGatePassed,
                    GetPassedOn=x.GetPassedOn,
                    IsActive=x.IsActive,
                    BarCode=x.BarCode,
                    BarCodeUrl=x.BarCodeUrl,
                    ShipmentCode=x.Shipment!=null?x.Shipment.Code:"",
                    LoadingSheetDetailDTOs = _mapper.Map<List<TruckLoadingSheetDetailDTO>>(x.TruckLoadingSheetDetails.ToList())
                });

                var result = await itemList.ApplyOrdering(query, columnMap).ToListAsync();
                var filterdatacount = result.Count();
                var pagination = _mapper.Map<List<TruckLoadingSheetDTO>>(result);

                var queryResult = new QueryResult<TruckLoadingSheetDTO>
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

        public string GeneratePdfTemplateTruckLoadingSheet(QueryResult<TruckLoadingSheetDTO> result)
        {
            var sb = new StringBuilder();

            sb.Append(@"<html>
                            <head>
                                <h1>Loading Sheet</h1>
                            </head>
                            <body>
                               <table border='1' align='center' style='border-collapse:collapse;'>");
            sb.Append(@"<thead>
                            <tr>
                                        <th>Id</th>
                                        <th>Truck Number</th>
                                        <th>Code</th>
                                        <th>Barcode</th>
                                        <th>Ship Form</th>
                                        <th>Ship To</th>
                                        <th>Total Pallet</th>
                                        <th>Gross Weight</th>
                                        <th>Net Weight</th>
                                        <th>Box QTY</th>
                                        <th>Volume</th>
                                        <th>Is Passed</th>
                                       
                                       
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
                                  
                                   
                     
                                   </tr>", item.Id, item.TruckNumber, item.QrCode,item.BarCode, item.ShipFrom, item.ShipTo, item.TotalPallet, item.GrossWeight, item.NetWeight, item.BoxQty, item.Volume,
                                            item.IsGatePassed);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }

        public string GeneratePdfTemplateTruckAllSheet(QueryResult<TruckLoadingSheetDTO> result)
        {
            var sb = new StringBuilder();

            sb.Append(@"<html>
                            <head>
                                <h1>Loading Sheet</h1>
                            </head>
                            <body>
                               <table border='1' align='center' style='border-collapse:collapse;'>");
            sb.Append(@"<thead>
                            <tr>
                                        <th>Id</th>
                                        <th>Truck Number</th>
                                        <th>Code</th>
                                        <th>Barcode</th>
                                        <th>Ship Form</th>
                                        <th>Ship To</th>
                                        <th>Total Pallet</th>
                                        <th>Gross Weight</th>
                                        <th>Net Weight</th>
                                        <th>Box QTY</th>
                                        <th>Volume</th>
                                        <th>Is Passed</th>
                                        <th>Passed On</th>
                                       
                                       
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
                                  
                                   
                     
                                   </tr>", item.Id, item.TruckNumber, item.QrCode, item.BarCode, item.ShipFrom, item.ShipTo, item.TotalPallet, item.GrossWeight, item.NetWeight, item.BoxQty, item.Volume,
                                            item.IsGatePassed,item.Date);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }

        public async Task<shipmentApprovalResponseDTO> CreateshipmentApprovalLog(ShipmentApprovalLogDTO model)
        {
            try
            {
                ShipmentApprovalLog shipmentApproval = _mapper.Map<ShipmentApprovalLog>(model);
                if (model.Id == 0)
                {
                    Shipment shipment = await _shipmentRepository.GetSingle(model.ShipmentId ?? 0);
                    CurrentUserDetailsDTO cuserDetail = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor);

                    shipment.CreatedOn = DateTime.Now;



                    model.AssignedToIds.Remove(0);
                    model.AssignedToRoleIds.Remove(0);
                    if (model.AssignedToIds.Count() > 0)
                    {
                        var shipmentApprovallog = _approvalLogRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == model.ShipmentId).ToList();
                        if (shipmentApprovallog.Count() > 0)
                        {
                            foreach (var item in shipmentApprovallog)
                            {
                                var assignmentdetail = _approvalAssignmentDetailRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ObjectName == "Shipment Approval Log" && x.ObjectId == item.Id).FirstOrDefault();
                                if (assignmentdetail != null && model.AssignedToIds.Contains(assignmentdetail.AssignedToId) && assignmentdetail.StatusId == (int)Enums.MST_ApprovalAssignmentStatus.Accepted) return new shipmentApprovalResponseDTO() { ShipmentId = shipment.Id, IsError = true, Msg = "Already Approved by This User" };

                                if (assignmentdetail != null && model.AssignedToIds.Contains(assignmentdetail.AssignedToId) && assignmentdetail.StatusId == (int)Enums.MST_ApprovalAssignmentStatus.Draft) return new shipmentApprovalResponseDTO() { ShipmentId = shipment.Id, IsError = true, Msg = "Already In Approval Pending For This User" };
                            }
                        }
                        await _approvalLogRepository.Add(shipmentApproval);
                        await _unitOfWork.Commit();
                        foreach (long id in model.AssignedToIds)
                            await _approvalAssignmentDetailRepository.Add(new ApprovalAssignmentDetail()
                            {
                                AssignedBy = model.CreatedBy ?? 0,
                                AssignedToId = id,
                                AssignedOn = DateTime.Now,
                                IsActive = true,
                                ObjectId = shipment.Id,
                                ObjectName = "ShipmentApprovalLog",
                                IsDeleted = false,
                                //BranchId = cuserDetail.BranchId,
                                CreatedBy = model.CreatedBy,
                                CreatedOn = DateTime.Now,
                                StatusId = (long)Enums.MST_ApprovalAssignmentStatus.Draft
                            });

                        //shipment.MSTStockAdjustmentStatusId = (int)Enums.MST_StockAdjustmentStatus.WaitingForApproval;
                        await _unitOfWork.Commit();
                    }
                    if (model.AssignedToRoleIds.Count > 0)
                    {

                        var shipmentApprovalLogs = _approvalLogRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ShipmentId == model.ShipmentId).ToList();
                        if (shipmentApprovalLogs.Count() > 0)
                        {
                            foreach (var item in shipmentApprovalLogs)
                            {
                                var assignmentdetail = _approvalAssignmentDetailRepository.GetAll(x => x.IsDeleted != true && x.IsActive != false && x.ObjectName == "ShipmentApprovalLog" && x.ObjectId == item.Id).FirstOrDefault();
                                if (assignmentdetail != null && model.AssignedToRoleIds.Contains(assignmentdetail.AssingedToRoleId) && assignmentdetail.StatusId == (int)Enums.MST_ApprovalAssignmentStatus.Draft) return new shipmentApprovalResponseDTO() { ShipmentId = shipment.Id, IsError = true, Msg = "Already In Approval Pending For this Role" }; ;
                            }
                        }

                        foreach (long id in model.AssignedToRoleIds)
                            await _approvalAssignmentDetailRepository.Add(new ApprovalAssignmentDetail()
                            {
                                AssignedBy = model.CreatedBy ?? 0,
                                AssingedToRoleId = id,
                                AssignedOn = DateTime.Now,
                                IsActive = true,
                                ObjectId = shipment.Id,
                                ObjectName = "StockAdjustmentLog",
                                IsDeleted = false,
                                //BranchId = cuserDetail.BranchId,
                                CreatedBy = model.CreatedBy,
                                CreatedOn = DateTime.Now,
                                StatusId = (long)Enums.MST_ApprovalAssignmentStatus.Draft
                            });
                    //    stock.MSTStockAdjustmentStatusId = (int)Enums.MST_StockAdjustmentStatus.WaitingForApproval;
                        await _unitOfWork.Commit();
                    }

                    PushQueueMessageDTO messageJson = new PushQueueMessageDTO()
                    {
                        sound = "bingbong.aiff",
                        title = "",
                        body = "A Approval Request has Arrived."
                    };

                    PushQueueDataDTO dataJson = new PushQueueDataDTO()
                    {
                        targetModule = (int)Enums.TargetModule.BOM,
                        entityID = model.ShipmentId
                    };

                    PushQueue pushqueue = new PushQueue()
                    {
                        UserDeviceId = null,
                        UserdetailsId = model.AssignedToIds.Count() > 0 ? model.AssignedToIds[0] : 0,
                        RoleId = model.AssignedToRoleIds.Count() > 0 ? model.AssignedToRoleIds[0] : 0,
                        CreatedOn = DateTime.Now,
                        ScheduledOn = DateTime.Now,
                        PlainMessage = "A Approval Request has Arrived.",
                        PlainTitle = "Approval Request",
                        EntityId = model.ShipmentId,
                        //MessageJson = "{\"sound\":\"bingbong.aiff\",\"title\":\"\",\"body\":\"" + message + "\"}"
                        MessageJson = JsonConvert.SerializeObject(messageJson),
                        DataJson = JsonConvert.SerializeObject(dataJson)
                    };

                    await _pushQueueRepository.Add(pushqueue);
                    await _unitOfWork.Commit();
                }
                return new shipmentApprovalResponseDTO() { ShipmentId = shipmentApproval.Id, IsError = false, Msg = "Sent for Approval Successfully." }; ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<QueryResult<ShipmentApprovalLogDTO>> ShipmentApprovallogItem(ShipmentApprovalLogQueryObject query, long userId)
        {
            try
            {
                if (string.IsNullOrEmpty(query.SortBy))
                {
                    query.SortBy = "Id";
                }

                var columnMap = new Dictionary<string, Expression<Func<ShipmentApprovalLogDTO, object>>>()
                {
                    ["Id"] = p => p.Id,
                    ["FileName"] = p => p.FileName,
                    ["RelatedFileUrl"] = p => p.RelatedFileUrl,
                    ["EventDate"] = p => p.EventDate,
                    ["FileType"] = p => p.FileType,
                    ["Remarks"] = p => p.Remarks,
                    ["AssignedToId"] = p => p.AssignedToIds,
                    ["AssignedRoleId"] = p => p.AssignedToRoleIds
                };



                var isApprover = false;
                List<ApprovalAssignmentDetail> asd = _approvalAssignmentDetailRepository.GetAll(x => x.IsDeleted != true).ToList();
                //List<MST_BOMStatus> bomstatus = _mST_BOMStatusRepository.GetAll(x => x.IsDeleted != true).ToList();
               List<Entities.MST_ApprovalAssignmentStatus> approvalAssignmentstatus = _mst_ApprovalAssignmentStatusRepository.GetAll(x => x.IsDeleted != true).ToList();
                List<UserDetails> userdetail = _userDetailsRepository.GetAll().ToList();
                List<Entities.Role> role = _roleRepository.GetAll(x => x.IsDeleted != true).ToList();

                var stocklog = _approvalLogRepository.GetAll(x => x.IsDeleted != true && x.ShipmentId == query.ShipmentId);
                var isdata = stocklog.Where(x => x.EventById == userId);
                if (isdata.Count() > 0)
                {
                    stocklog = stocklog.Where(x => x.EventById == userId);
                }
                //bomapprovalloglist = bomapprovalloglist.Where(x => x.BOMId ==query.BOMId);
                var stockitem = stocklog.Select(x => new ShipmentApprovalLogDTO()
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    FileType = x.FileType,
                    RelatedFileUrl = x.RelatedFileUrl,
                    Remarks = x.Remarks,
                    EventDate = x.EventDate,
                });
                var result = await stockitem.ApplyOrdering(query, columnMap).ToListAsync();
                var filterdatacount = stocklog.Count();


                var pagination = _mapper.Map<List<ShipmentApprovalLogDTO>>(result);
                foreach (var data in pagination)
                {
                    var asdd = asd.Where(x => x.ObjectName == "StockAdjustmentLog" && x.ObjectId == data.Id).FirstOrDefault();

                    if (asdd != null)
                    {
                        data.ShipmentId = (long)asdd.Id;
                        data.ApprovalAssignmentId = asdd.Id;
                        data.ReasonForRejection = asdd.ReasonForRejection;
                        data.AssignedToIds = new List<long> { asdd.AssignedToId };
                        data.AssignedToRoleIds = new List<long> { asdd.AssingedToRoleId };
                        //data.MST_BOMStatusName = asdd.StatusId != null && asdd.StatusId > 0 ? bomstatus.Where(x => x.Id == asdd.StatusId).FirstOrDefault().Name : null;
                        data.MST_ApprovalAssignmentStatusName = asdd.StatusId != null && asdd.StatusId > 0 ? approvalAssignmentstatus.Where(x => x.Id == asdd.StatusId).FirstOrDefault().Name : null;
                        data.AssignedRoleName = asdd.AssingedToRoleId != null && asdd.AssingedToRoleId > 0 ? role.Where(x => x.Id == asdd.AssingedToRoleId).FirstOrDefault().Name : null;
                        data.AssignedUserName = asdd.AssignedToId != null && asdd.AssignedToId > 0 ? userdetail.Where(x => x.Id == asdd.AssignedToId).FirstOrDefault().FirstName + " " + userdetail.Where(x => x.Id == asdd.AssignedToId).FirstOrDefault().LastName : null;

                        if (asdd.AssignedToId != null && asdd.AssignedToId > 0) data.IsApprovers = asdd.AssignedToId == userId;
                        if (asdd.AssingedToRoleId != null && asdd.AssingedToRoleId > 0) data.IsApprovers = asdd.AssingedToRoleId == _userRoleRepository.GetAll(x => x.UserId == userId).FirstOrDefault().RoleId;
                    }


                }
                var queryResult = new QueryResult<ShipmentApprovalLogDTO>
                {
                    TotalItems = stocklog.Count(),
                    Items = pagination,
                    IsBomCreator = isdata.Count() > 0
                };
                return queryResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> AcceptShipmentApproval(ShipmentApprovalDTO model)
        {
            try
            {
                if (model.ShipmentId > 0 && model.ShipmentApprovalId > 0)
                {
                    int NOfApproval = 3;
                    List<ShipmentApprovalLog> shipmentApprovals = _approvalLogRepository.GetAll(x => x.IsDeleted != true && x.ShipmentId == model.ShipmentId).ToList();
                    WebSetting webSetting = _webSettingRepository.GetAll().FirstOrDefault();
                    if (webSetting != null)
                    {
                        if (webSetting.NoOfApproval > 0) NOfApproval = webSetting.NoOfApproval ?? 3;
                    }
                    if (shipmentApprovals.Count() > 0)
                    {
                        var isAcceptedAll = false;
                        var count = 0;
                        List<ApprovalAssignmentDetail> asdetail = _approvalAssignmentDetailRepository.GetAll(x => x.IsDeleted != true).ToList();
                        var salog = shipmentApprovals.Where(x => x.Id != model.ShipmentApprovalId);
                        foreach (var stockadj in salog)
                        {
                            if (count == (NOfApproval - 1)) break;
                            var asnd = asdetail.FirstOrDefault(x => x.ObjectName == "ShipmentApprovalLog" && x.ObjectId == stockadj.Id);
                            if (asnd != null && asnd.StatusId == (int)Enums.MST_ApprovalAssignmentStatus.Accepted)
                            {
                                count++;
                            }
                        }

                        if (count == (NOfApproval - 1))
                        {
                            ApprovalAssignmentDetail approvalAssignmentDetail = _approvalAssignmentDetailRepository.GetById(model.ShipmentApprovalId);
                            if (approvalAssignmentDetail != null)
                            {
                                approvalAssignmentDetail.StatusId = (int)Enums.MST_ApprovalAssignmentStatus.Accepted;
                                approvalAssignmentDetail.UpdatedBy = model.userId;
                                approvalAssignmentDetail.UpdatedOn = DateTime.Now;

                                Shipment shipment= _shipmentRepository.GetAll(x => x.Id == model.ShipmentId, x => x.ShipmentDetails).ToList().FirstOrDefault();
                               // shipment. = (int)Enums.MST_StockAdjustmentStatus.Completed;


                                ShipmentApprovalLog shipmentApprovalLog = new ShipmentApprovalLog()
                                {

                                    Remarks = "Shipment Approval Log",
                                    ShipmentId = model.ShipmentId,
                                    EventDate = DateTime.Now,
                                };
                                await _approvalLogRepository.Add(shipmentApprovalLog);

                                if (shipment.ShipmentDetails.Count() > 0)
                                {
                                    shipment.ShipmentDetails.Where(x => x.IsDeleted != true).ToList();
                                    if (shipment.ShipmentDetails.Count() > 0)
                                    {
                                        //foreach (var item in shipment.ShipmentDetails)
                                        //{
                                        //    if (item.Quantity > 0 && item.ItemId > 0)
                                        //    {
                                        //        SemiFinishGoodsPalletDTO data = new SemiFinishGoodsPalletDTO();
                                        //        var itemdata = await _itemRepository.GetSingle(item.ItemId);
                                        //        if (item != null) data.ItemCode = itemdata.ItemCode;
                                        //        data.Description = itemdata.Description;
                                        //        data.code = stock.Code;
                                        //        data.UserId = model.userId;
                                        //        data.BranchId = model.BranchId;
                                        //        data.Quantity = item.Quantity;
                                        //        data.CurrentWareHouseId = stock.WareHouseId;
                                        //        data.CurrentSubInventoryId = stock.SubInventoryId;
                                        //        data.CurrentWareHouseLocationId = stock.WareHouseLocationId;
                                        //        data.CurrentWareHouseSubLocationId = stock.WareHouseSubLocationId;
                                        //        data.MSTPalletTypeId = (int)Enums.MstPalletType.StockAdjustment_Pallet;
                                        //        var palletId = await _semiFinishGoodsPalletService.CreatePalletForStockAdjustment(data, model.rootpath);
                                        //        item.PalletId = palletId;

                                        //        ItemTransPalletDataForStockAdjustmentDTO creditModel = new ItemTransPalletDataForStockAdjustmentDTO()
                                        //        {
                                        //            IsDebit = false,
                                        //            UserId = model.userId,
                                        //            BranchId = model.BranchId,
                                        //            StockAdjustmentDetailId = item.Id,
                                        //            PalletId = palletId
                                        //        };

                                        //        var itemTransResult = await ItemTransactionPalletForStockAdjustment(creditModel);


                                        //    }
                                        //    else
                                        //    {

                                        //        ItemTransactionNTransDetailDTO itemtrans = new ItemTransactionNTransDetailDTO()
                                        //        {
                                        //            ItemId = item.ItemId,
                                        //            Quantity = (-1 * item.Quantity),
                                        //            MSTItemTransactionStageId = (int)Enums.ItemTransactionStage.StockAdjustmentDetail,
                                        //            ObjectName = "StockAdjustmentDetail",
                                        //            ObjectId = item.Id,
                                        //            PalletIDNumber = "",
                                        //            CaseNumber = "",
                                        //            SourceWareHouseId = stock.WareHouseId,
                                        //            SourceSubInventoryId = stock.SubInventoryId,
                                        //            SourceWareHouseLocationId = stock.WareHouseLocationId,
                                        //            SourceWareHouseSubLocationId = stock.WareHouseSubLocationId,
                                        //            BranchId = model.userId,
                                        //            UpdateTransactionRemainingItem = true

                                        //        };

                                        //        await _itemTransactionService.createItemTransNItemTransDetail(itemtrans);
                                        //    }
                                        //}
                                    }
                                }

                                //var fgId = bomm.MSTFinishGoodsId;
                                await _unitOfWork.Commit();

                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            ApprovalAssignmentDetail approvalAssignmentDetail = _approvalAssignmentDetailRepository.GetById(model.ShipmentApprovalId);
                            if (approvalAssignmentDetail != null)
                            {
                                approvalAssignmentDetail.StatusId = (int)Enums.MST_ApprovalAssignmentStatus.Accepted;
                                approvalAssignmentDetail.UpdatedBy = model.userId;
                                approvalAssignmentDetail.UpdatedOn = DateTime.Now;
                                await _unitOfWork.Commit();
                                return true;
                            }
                            return false;
                        }
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

        public async Task<bool> RejectShipmentApproval(long shipmentAprrovalId, long userId)
        {
            try
            {
                if (shipmentAprrovalId > 0)
                {

                    ApprovalAssignmentDetail approvalAssignmentDetail = _approvalAssignmentDetailRepository.GetById(shipmentAprrovalId);
                    if (approvalAssignmentDetail != null)
                    {
                        approvalAssignmentDetail.StatusId = (int)Enums.MST_ApprovalAssignmentStatus.Rejected;
                        approvalAssignmentDetail.UpdatedBy = userId;
                        approvalAssignmentDetail.UpdatedOn = DateTime.Now;
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

        public List<ShipmentStatusForDDL> GetForDropDown()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RejectShipmentApprovalWithComment(ShipmentRejectDTO model)
        {

            try
            {
                if (model.ShipmentApprovalId > 0 && !string.IsNullOrEmpty(model.Remarks))
                {

                    ApprovalAssignmentDetail approvalAssignmentDetail = _approvalAssignmentDetailRepository.GetById(model.ShipmentApprovalId);
                    if (approvalAssignmentDetail != null)
                    {
                        approvalAssignmentDetail.StatusId = (int)Enums.MST_ApprovalAssignmentStatus.Rejected;
                        approvalAssignmentDetail.UpdatedBy = model.UserId;
                        approvalAssignmentDetail.UpdatedOn = DateTime.Now;
                        approvalAssignmentDetail.ReasonForRejection = model.Remarks;
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


        // Get Data For Invoice Create
        public async Task<List<Acc_InvoiceDTO>> GetDataForInvoiceCreate(long ShipmentId)
        {
            try
            {
                if (ShipmentId == 0) return new List<Acc_InvoiceDTO>();
                var shipmentData = _shipmentRepository.GetById(ShipmentId);
                List<long?> salesOrderPlanId = await GetSalesOrderplanIdByShipment(ShipmentId);

                List<Acc_InvoiceDTO> ACCinvoiceList = new  List<Acc_InvoiceDTO>();
                foreach (var SOPid in salesOrderPlanId.Distinct())
                {
                    var InvoiceCode = new CodeGenerationDTO { Code = MainEnumHelper.GetDescription(CodeGenerationEnum.INVOICE), GCode = "" };
                    Acc_InvoiceDTO invoice = new Acc_InvoiceDTO();
                    List<Acc_InvoiceDetailsDTO> invoiceDetailList = new List<Acc_InvoiceDetailsDTO>();
                    SalesOrderPlan salesOrderplanData = _salesOrderPlanRepository
                                                    .GetAll(x => x.Id == SOPid)
                                                    .Include(x => x.EstimateSalesOrderplans)
                                                    .ThenInclude(x => x.Acc_Estimate)
                                                    .ThenInclude(x => x.EstimateDetails)
                                                    .ThenInclude(x => x.Tax)
                                                    .Include(x => x.CustomerPurchaseOrder)
                                                    .Include(x => x.Customer)
                                                    .ThenInclude(x => x.MST_Currency)
                                                    .Include(x => x.SalesOrderPlanDetails).FirstOrDefault();
                    if (salesOrderplanData == null) return new List<Acc_InvoiceDTO>();
                    var currencyId = salesOrderplanData.EstimateSalesOrderplans != null && salesOrderplanData.EstimateSalesOrderplans.Acc_Estimate != null ? salesOrderplanData.EstimateSalesOrderplans.Acc_Estimate.CurrencyId : 0;
                    var companyId = salesOrderplanData.EstimateSalesOrderplans != null && salesOrderplanData.EstimateSalesOrderplans.Acc_Estimate != null ? salesOrderplanData.EstimateSalesOrderplans.Acc_Estimate.CompanyId : 0;
                    var tax = salesOrderplanData.EstimateSalesOrderplans != null && salesOrderplanData.EstimateSalesOrderplans.Acc_Estimate != null && salesOrderplanData.EstimateSalesOrderplans.Acc_Estimate.EstimateDetails != null ? salesOrderplanData.EstimateSalesOrderplans.Acc_Estimate.EstimateDetails.FirstOrDefault().Tax : null;

                    invoice.CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId;
                    invoice.CreatedOn = DateTime.Now;
                    invoice.InvoiceNumber = _codeGenerationTemplateService.GenerateCode(InvoiceCode).Result.GCode;
                    invoice.IsDeleted = false;
                    invoice.IsActive = true;
                    invoice.SalesOrderPlanId = SOPid;
                    invoice.BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId;
                    invoice.CustomerId = salesOrderplanData.CustomerId;
                    invoice.CompanyId = companyId;
                    invoice.InvoiceDate = DateTime.Now;
                    invoice.IsTaxExclusive = false;
                    invoice.Adjustment = 0;
                    invoice.AdjustmentLabel = "Adjustment";
                    invoice.ShippingCharge = 0;
                    invoice.Terms = 0;
                    invoice.ShipmentId = ShipmentId;
                    invoice.CurrencySymbol = salesOrderplanData.Customer != null && salesOrderplanData.Customer.MST_Currency != null ? salesOrderplanData.Customer.MST_Currency.Symbol : null;
                    invoice.SalesPerson = salesOrderplanData.CustomerPurchaseOrder != null && salesOrderplanData.CustomerPurchaseOrder.Customer != null ? salesOrderplanData.CustomerPurchaseOrder.Customer.SalesPersonId : 0;
                    //invoice.c
                    var packinglistdetailItemIdsList = _shipmentPlanPalletRepository.GetAll(x => x.IsDeleted != true && x.ShipmentId == ShipmentId && x.PackingListDetail.MSTFinishGoodsId > 0)
                        .Select(x => x.PackingListDetail.ItemId).ToList();

                    //var salesOrderPlanDetailsIdList = _packingListDetailRepository.GetAll(x => x.IsDeleted != true && packinglistdetailIdsList.Any(a => a == x.Id)).Select(x => x.SalesOrderPallet.SalesOrder.salesOrderDetails).ToList();
                    var salesOrderplandetails = salesOrderplanData.SalesOrderPlanDetails.Where(z => packinglistdetailItemIdsList.Any(a => a == z.ItemId)).ToList();

                    foreach (var salesorderplanItem in salesOrderplandetails)
                    {

                        decimal TotalAmount = salesorderplanItem.Quantity * salesorderplanItem.Price ?? 0;
                        Acc_InvoiceDetailsDTO detail = new Acc_InvoiceDetailsDTO()
                        {
                            ItemId = salesorderplanItem.ItemId,
                            Quantity = (int)salesorderplanItem.Quantity,
                            Rate = salesorderplanItem.Price,
                            TaxId = tax != null ? tax.Id : 0,
                            TaxRate = tax != null ? tax.Rate : 0,
                            TaxAmount = tax != null ? (tax.Rate / 100) * TotalAmount : 0,
                            Amount = TotalAmount + (tax.Rate / 100) * TotalAmount,
                            DiscountAmount = 0,
                            Description = salesorderplanItem.Item != null ? salesorderplanItem.Item.Description : "",
                            BranchId = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).BranchId,
                            CreatedBy = SessionHelper.GetUserDetailId(_userDetailsRepository, _httpContextAccessor).UserId,
                            CreatedOn = DateTime.Now,
                            IsDeleted = false,
                            IsActive = true
                        };
                        invoiceDetailList.Add(detail);
                    }
                    invoice.Total = invoiceDetailList.Sum(x => x.Amount);
                    invoice.SubTotal = invoiceDetailList.Sum(x => x.Amount);
                    invoice.InvoiceDetails = invoiceDetailList;

                    ACCinvoiceList.Add(invoice);
                }
                return ACCinvoiceList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}

