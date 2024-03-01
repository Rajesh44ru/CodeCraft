[HttpGet]
[Route("ReleaseShipmentPlan/{Id}")]
public async Task<IActionResult> ReleaseShipmentPlan(long Id)
{
    try
    {
        if (Id > 0)
        {
            //checking if item exists
            bool IsAvailable = _shipmentService.CheckAvailablity(Id);
            if (IsAvailable)
            {
                //reserving items
                bool isReversed = await _shipmentService.ReserveFG(Id, false);
                if (isReversed)
                {
                    //generation invoice
                  //  var invoiceRes = await _invoiceService.CreateInvoiceByShipmentID(Id);

                    var InvoiceListData = await _shipmentService.GetDataForInvoiceCreate(Id);

                    if (InvoiceListData.Count() > 0)
                    {

                        var accesssToken = HttpContext.Request.Headers.Where(x => x.Key == "Authorization").FirstOrDefault().Value.FirstOrDefault();
                        var token = accesssToken.Split(" ")[1];
                        string web = Configuration["AdminLinkDetails:APIWebHost"];
                        string ApiUrl = "api/Acc_Invoice/CreateList";
                        HttpClientDTO httpClientDTO = new HttpClientDTO()
                        {
                            Token = token,
                            ApiWebHost = web,
                            APIUrl = ApiUrl,
                            Model = InvoiceListData
                        };
                        var response = await _httpClientService.CallPostMethodAPI(httpClientDTO);
                        if(response==false) return Ok(new Response
                        {
                            Id = Id,
                            Status = Ok().StatusCode,
                            Msg = "Invoice Not Created"
                        });
                        ActionLogDTO actionLog = new ActionLogDTO()
                        {
                            Description = "Invoice Created For Shipment",
                            IsPerformedByUser = true,
                            MST_LogStageId = (int)Enums.MST_LogStageEnum.ShipmentPlan,
                            MST_ActionStageId = (int)Enums.MST_ActionStageEnum.Created,
                            ObjectId = Id
                        };
                        await _actionLogService.CreateActionLog(actionLog);
                    }
                    //generating task
                    long pickingTaskID = await _shipmentService.CreatePickingTask(Id);
                    if(pickingTaskID == 0) return Ok(new Response
                    {
                        Id = Id,
                        Status = Ok().StatusCode,
                        Msg = "Error while creating Picking Task."
                    });

                    //Generating pickinglist
                    long pickingListId = await _shipmentService.GeneratePicking(Id, pickingTaskID);
                    if (pickingListId == 0) return Ok(new Response
                    {
                        Id = Id,
                        Status = Ok().StatusCode,
                        Msg = "Error while creating Picking List."
                    });
                    //updating shipment
                    //Shipment shipment =  await _shipmentRepository.GetSingle(Id);
                    //if(shipment != null)
                    //{
                    //    if (shipment.MSTReservationStatusId == (int)Enums.ReservationStatus.Reserved) shipment.MSTPONStatusId = (int)Enums.PONStatus.Released;
                    //    await _unitOfWork.Commit();
                    //}

                    //updating shipment and SalesOrderStatus

                    var result = await _shipmentService.UpdateOrderStatus(Id, "PendingForShipment");

                    return Ok(new Response
                    {
                        Id = pickingListId,
                        Status = Ok().StatusCode,
                        Msg = pickingListId == 0 ? "No PickingList Created" : RespMsg.MSGOKADD,
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Id = 0,
                        Status = Ok().StatusCode,
                        Msg = RespMsg.MSGOK,
                        Data = isReversed
                    });
                }
            }
            else
            {
                return Ok(new
                {
                    Id = 0,
                    Status = Ok().StatusCode,
                    Msg = RespMsg.MSGOK,
                    Data = IsAvailable
                });
            }
        }
        

        return NotFound(new Response
        { Id = 0, Status = NotFound().StatusCode, Msg = RespMsg.MSGNOTFOUND, });
    }
    catch (Exception e)
    {
        return BadRequest(new Response
        { Id = 0, Status = BadRequest().StatusCode, Msg = RespMsg.MSGBADREQUEST, });
    }
}

[HttpPost]
[Route("ExportToExcelWaitingSheet")]
[DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.WAREHOUSE_TRUCK_LOADING_SHEET_WAITING_SHEET_EXPORT_TO_EXCEL })]
// [DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.FinishGoods_Export_Excel })]
public async Task<FileContentResult> ExportToExcelWaitingSheet(TruckLoadingQueryObject query)
{
    try
    {

        if (query.printall)
        {
            query.Page = 1;
            query.PageSize = int.MaxValue;
        }
        QueryResult<TruckLoadingSheetDTO> result = await _shipmentService.SearchForTruckLoadingSheet(query);

        using (var workBook = new XLWorkbook())
        {
            var workSheet = workBook.Worksheets.Add("Loading Sheet");
            var currentRow = 1;

            workSheet.Cell(currentRow, 1).SetValue("Truck Number");
            workSheet.Cell(currentRow, 2).SetValue("Code");
           // workSheet.Cell(currentRow, 3).SetValue("QR Code");
            workSheet.Cell(currentRow, 3).SetValue("Barcode");
            workSheet.Cell(currentRow, 4).SetValue("Ship Form");
            workSheet.Cell(currentRow, 5).SetValue("Ship To");
            workSheet.Cell(currentRow, 6).SetValue("Total Pallet");
            workSheet.Cell(currentRow, 7).SetValue("Gross Weight");
            workSheet.Cell(currentRow, 8).SetValue("Net Weight");
            workSheet.Cell(currentRow, 9).SetValue("Box QTY");
            workSheet.Cell(currentRow, 10).SetValue("Volume");
            workSheet.Cell(currentRow, 11).SetValue("Is Passed");

            foreach (var item in result.Items)
            {

                currentRow++;
                workSheet.Cell(currentRow, 1).SetValue(item.TruckNumber);
                workSheet.Cell(currentRow, 2).SetValue(item.QrCode);
                //workSheet.Cell(currentRow, 3).SetValue(item.QrCodeUrl);
                workSheet.Cell(currentRow, 3).SetValue(item.BarCode);
                workSheet.Cell(currentRow, 4).SetValue(item.ShipFrom);
                workSheet.Cell(currentRow, 5).SetValue(item.ShipTo);
                workSheet.Cell(currentRow, 6).SetValue(item.TotalPallet);
                workSheet.Cell(currentRow, 7).SetValue(item.GrossWeight);
                workSheet.Cell(currentRow, 8).SetValue(item.NetWeight);
                workSheet.Cell(currentRow, 9).SetValue(item.BoxQty);
                workSheet.Cell(currentRow, 10).SetValue(item.Volume);
                workSheet.Cell(currentRow, 11).SetValue(item.IsGatePassed);

            }

            using (var stream = new MemoryStream())
            {
                workBook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var content = stream.ToArray();
                return File(
                    content,
                     "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Loading Sheet.xlsx"
                 );

            }
        }

    }
    catch (Exception e)
    {
        throw;
    }

}

