using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DinkToPdf.Contracts;
using LogicLync.Api.ActionFilter;
using LogicLync.Api.Helpers;
using LogicLync.Api.Infrastructure;
using LogicLync.DTO;
using LogicLync.Entities;
using LogicLync.Service;
using LogicLync.Service.HelperClasses;
using LogicLync.Service.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace LogicLync.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController : BaseApiController
    {
        private IPaymentMethodService _paymentMethodService;
        private readonly IConverter _converter;
        IWebHostEnvironment _environment;
        public PaymentMethodController(IPaymentMethodService paymentMethodService,IConverter converter, IWebHostEnvironment environment)
        {
            this._paymentMethodService = paymentMethodService;
            _converter = converter;
             _environment=environment;
        }

        [HttpGet]
        [Route("GetAllPaymentMethods")]
        public  IActionResult GetAllPaymentMethods()
        {
            try
            {
                var result =  _paymentMethodService.GetAll();
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        //[SystemPermissionAttribute(new Enums.SystemPermissions[] { Enums.SystemPermissions.PaymentMethods })]
        //[DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.PaymentMethods_ViewAll })]
        //[ServiceFilter(typeof(SystemPermissionActionFilter))]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var result = _paymentMethodService.GetAll();
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        //[DataPermissionAttribute(new Enums.DataPermissions[] {Enums.DataPermissions.PaymentMethods_Edit })]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _paymentMethodService.GetPaymentById(id);
                return Ok(result);

            }
            catch(Exception e)
            {
                return BadRequest(e);
            }
        }


        [HttpPost]
        [Route("UploadImages")]
        public async Task<IActionResult> UploadImages(int id)
        {
            try
            {
                var files = Request.Form.Files;
              

                if (files == null || !files.Any()) return BadRequest("No file found");
                //foreach (var item in files)
                //{
                //    if (item.Length > photoSettings.MaxBytes) return BadRequest(" Max file size exceed.");
                //    if (!photoSettings.AcceptedFileTypes.Any(x => x.ToLower() == Path.GetExtension(item.FileName.ToLower())))
                //    {
                //        return BadRequest("Invalid file type.");
                //    }
                //}
                string uploadFolderPath = Path.Combine(_environment.ContentRootPath, "Uploads/PaymentMethods");
                string uploadImagePath = Path.Combine(_environment.ContentRootPath, "Uploads/PaymentMethods");
                if (!Directory.Exists(uploadFolderPath))
                {
                    Directory.CreateDirectory(uploadFolderPath);
                }

                uploadImagePath = Path.Combine(_environment.ContentRootPath, "Uploads/PaymentMethods");
                if (!Directory.Exists(uploadImagePath))
                {
                    Directory.CreateDirectory(uploadImagePath);
                }
                var listfilename = new List<string>();
                var databasefilepath = string.Empty;
                foreach (var item in files)
                {
                    var filename = string.Empty;
                    if (item != null)
                    {
                        filename = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);
                        var  filepath = Path.Combine(uploadImagePath, filename);
                        using (var stream = new FileStream(filepath, FileMode.Create))
                        {
                            await item.CopyToAsync(stream);
                            listfilename.Add(filename);
                        }
                        databasefilepath = "Uploads/PaymentMethods/"+filename;
                    }
                }


                var mydata = new PaymentMethodImageDTO()
                {
                    Id = 1,
                    LogoURL = databasefilepath
                };
                

                return Ok(mydata);
            }
            catch (Exception ex)
            {

                return BadRequest("Something went wrong");
            }

        }

        [HttpPost]
        [Route("DeleteImage/{Id}")]
        public async Task<IActionResult> DeleteImage(string ImageId)
        {
            try
            {

                string uploadImagePath = Path.Combine(_environment.ContentRootPath, "Uploads/PaymentMethods");
                var fileInfo = (uploadImagePath + "//" + ImageId);
                /*Delete from the Disk: */
                System.IO.File.Delete(fileInfo);
                /*Now return remaining images: */
                
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest("Failed to Delete the Image.");
            }
        }

        [HttpPost]
        [Route("CreatePaymentMethod")]
        public async Task<IActionResult> CreatePaymentMethod(PaymentMethodDTO masterlist)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int result=0;
                    if (masterlist.Id == 0)
                    {
                        result = (int) await _paymentMethodService.Create(masterlist);
                    }
                   
                    return Ok(result);
                }
                else
                {
                    return BadRequest("ModelState is not valid.");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e); ;
            }
        }
        [HttpPost]
        //[DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.PaymentMethods_Add })]
        [Route("UpsertPaymentMethod")]
        public async Task<IActionResult> UpsertPaymentMethod(PaymentMethodDTO masterlist)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int result = 0;
                    
                        result = (int)await _paymentMethodService.Update(masterlist);
                
                    return Ok(result);
    }
                else
                {
                    return BadRequest("ModelState is not valid.");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e); ;
            }
        }
        [Route("PaymentMethodItem")]
        [HttpPost]
        //[SystemPermissionAttribute(new Enums.SystemPermissions[] { Enums.SystemPermissions.PaymentMethods})]
        //[DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.PaymentMethods_ViewAll })]
        public async Task<IActionResult> PaymentMethodItem(SearchQueryObject query)
        {
            try
            {
                var result = await _paymentMethodService.PaymentMethod(query);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex); ;
            }
           
        }


        [Route("ActivePaymentMethodList")]
        [HttpPost]
        //[SystemPermissionAttribute(new Enums.SystemPermissions[] { Enums.SystemPermissions.PaymentMethods})]
        //[DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.PaymentMethods_ViewAll })]
        public async Task<IActionResult> ActivePaymentMethodList()
        {
            try
            {
                var result = await _paymentMethodService.GetAllPaymentMethod();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex); ;
            }

        }

        [Route("PaymentMethodStatus")]
        [HttpGet]
        public IActionResult PaymentMethodStatus()
        {
         return Ok(LogicLync.DTO.PaymentMethodStatusDTO.PaymentMethodStatusList);
                      
        }



        [Route("ExportToExcel")]
       // [DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.PaymentMethods_Export_Excel })]
        [HttpPost]
        public async Task<FileContentResult> ExportToExcel(SearchQueryObject query)
        {
            try
            {

                if (query.printall)
                {
                    query.Page = 1;
                    query.PageSize = int.MaxValue;
                }
                QueryResult<PaymentMethodDTO> result = await _paymentMethodService.PaymentMethod(query);

                using (var workBook = new XLWorkbook())
                {
                    var workSheet = workBook.Worksheets.Add("PaymentMethodList");
                    var currentRow = 1;

                    workSheet.Cell(currentRow, 1).SetValue("ID");
                    workSheet.Cell(currentRow, 2).SetValue("ShortName");
                    workSheet.Cell(currentRow, 3).SetValue("Description");
                    workSheet.Cell(currentRow, 4).SetValue("PageId");
                    workSheet.Cell(currentRow, 5).SetValue("Status");


                    foreach (var item in result.Items)
                    {

                        currentRow++;
                        workSheet.Cell(currentRow, 1).SetValue(item.Id);
                        workSheet.Cell(currentRow, 2).SetValue(item.ShortName);
                        workSheet.Cell(currentRow, 3).SetValue(item.Description);
                        workSheet.Cell(currentRow, 4).SetValue(item.PageId);
                        workSheet.Cell(currentRow, 5).SetValue(item.Status);
                    }

                    using (var stream = new MemoryStream())
                    {
                        workBook.SaveAs(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        var content = stream.ToArray();
                        return File(
                            content,
                             "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "PaymentMethodList.xlsx"
                         );

                    }
                }

            }
            catch (Exception e)
            {
                throw;
            }

        }
        [Route("ExportToPdf")]
        [HttpPost]
       // [DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.PaymentMethods_Export_PDF })]
        public async Task<FileContentResult> ExportToPdf(SearchQueryObject query)
        {
            try
            {

                if (query.printall)
                {
                    query.Page = 1;
                    query.PageSize = int.MaxValue;
                }
                QueryResult<PaymentMethodDTO> result = await _paymentMethodService.PaymentMethod(query);

                return File(_converter.Convert(PrintPdfHelper.CreateTablePDF(_paymentMethodService.GeneratePdfTemplateString(result))), "application/pdf");
            }
            catch (Exception e)
            {
                throw;
            }

        }

        [HttpPost]
        [Route("DeletePaymentMethod/{id}")]
        //[DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.PaymentMethods_Delete })]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            try
            {
                if (id != 0)
                {
                    await _paymentMethodService.Delete(id);
                    return Ok();
                }
                else
                {
                    return BadRequest("Not Found.");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}