using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LogicLync.Api.Infrastructure;
using LogicLync.DTO;
using LogicLync.Entities;
using LogicLync.Service.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using LogicLync.Api.ActionFilter;
using LogicLync.Service;
using LogicLync.Service.HelperClasses;
using static LogicLync.DTO.LogicLyncEnum;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace LogicLync.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseApiController
    {
        //ControllerBase
        private HttpClient client = new HttpClient();
        readonly KhaltiPaymentSetting khaltiPaymentSetting;
        readonly ESewaPaymentSetting eSewaPaymentSetting;
        readonly IMEPaySetting imePaySetting;
        readonly ConnectIPSSetting connectIPSSetting;
        readonly FonePaySetting fonePaySetting;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentDetailService _paymentDetailService;
        private readonly IOrderService _orderService;
        private readonly IQRCodeService _qrcodeservice;

        IWebHostEnvironment host;
        public readonly IRemoteAPIService _remoteAPIService;
        private readonly IStoreService _storeService;
        private readonly IUserDetailService _userDetailService;
        public PaymentController(IWebHostEnvironment environment, IQRCodeService qrCodeService, IPaymentService paymentService, IOrderService orderService, 
            IOptionsSnapshot<KhaltiPaymentSetting> optionsKhalti, 
            IOptionsSnapshot<ESewaPaymentSetting> optionsEsewa, 
            IOptionsSnapshot<IMEPaySetting> optionsIMEPay,
            IOptionsSnapshot<ConnectIPSSetting> optionsConnectIPS,
            IOptionsSnapshot<FonePaySetting> optionsFonePay,
            IPaymentDetailService paymentDetailService,
            IRemoteAPIService remoteAPIService,
            IStoreService storeService,
            IUserDetailService userDetailService)
        {
            host = environment;
            _paymentService = paymentService;
            _paymentDetailService = paymentDetailService;
            _orderService = orderService;
            khaltiPaymentSetting = optionsKhalti.Value;
            eSewaPaymentSetting = optionsEsewa.Value;
            imePaySetting = optionsIMEPay.Value;
            connectIPSSetting = optionsConnectIPS.Value;
            fonePaySetting = optionsFonePay.Value;
            _remoteAPIService = remoteAPIService;
            _userDetailService = userDetailService;
            _qrcodeservice = qrCodeService;
        }

        [HttpPost]
        [Route("VerifyKhaltiPayment")]
        [Authorize]
        public async Task<IActionResult> VerifyKhaltiPaymentAsync(OrderPaymentDTO order)
        {
            try
            {
                // here check amount in server side too for correct verification of amount
                decimal mainPriceInPaisa = await _orderService.GetOrderTotal(order.OrderId);
                if (mainPriceInPaisa > 0)
                {
                    if (!(mainPriceInPaisa == order.Amount))
                    {
                        //return Ok("Amount not vaild. Please contact with administrator.");
                        return Ok(0);
                    }
                }
                //// check already paid or not for particular order.
                bool isPaymentDone = _paymentService.IsPaymentDone(order.OrderId);
                if (isPaymentDone == true)
                {
                    //return Ok("You have already paid for this order.");
                    return Ok(1);
                }

                if (mainPriceInPaisa == order.Amount && isPaymentDone == false)
                {
                    client = new HttpClient();
                    string values = "{\"token\": \"" + order.Token + "\", \"amount\": " + order.Amount * 100 + "}";
                    var content = new System.Net.Http.StringContent(values, Encoding.UTF8, "application/json");
                    IEnumerable<string> tokenValue;
                    if (client.DefaultRequestHeaders.TryGetValues("Authorization", out tokenValue) == false)
                    {
                        client.DefaultRequestHeaders.Add("Authorization", khaltiPaymentSetting.KhaltiSecretKey);
                    }
                    var response = await client.PostAsync(khaltiPaymentSetting.KhaltiVerifyURL, content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    try
                    {
                        KhaltiResponseDTO responseObj = JsonConvert.DeserializeObject<KhaltiResponseDTO>(responseString);
                      
                        //added to update paymet status
                        PaymentDTO payment =  await _paymentService.GetPaymentByOrderId(order.OrderId);
                        payment.Amount = order.Amount;
                        payment.PaymentProcessorId = (int)Enums.PaymentProcessors.KHALTI;
                        payment.Status = string.IsNullOrEmpty(responseObj.error_key) == true ? (int)Enums.PaymentStatus.Paid : (int)Enums.PaymentStatus.Failed;
                        var paymentId = await _paymentService.Update(payment);
                        //PaymentDTO payment = new PaymentDTO()
                        //{
                        //    OrderId = order.OrderId,
                        //    Amount = order.Amount,
                        //    PaymentProcessorId = (int)Enums.PaymentProcessors.KHALTI,
                        //    Status = string.IsNullOrEmpty(responseObj.error_key) == true ? (int)Enums.PaymentStatus.Paid : (int)Enums.PaymentStatus.Failed
                        //};
                        //var paymentId = await _paymentService.Create(payment);

                        PaymentDetailDTO paymentDetail = new PaymentDetailDTO()
                        {
                            PaymentId = paymentId,
                            Token = order.Token,
                            TransactionId = responseObj != null ? responseObj.idx : "",
                            FeeAmount = responseObj != null ? responseObj.fee_amount : 0,
                            State = responseObj != null && responseObj.state != null && responseObj.state.name != null ? responseObj.state.name.ToLower() : "",
                            Refunded = responseObj != null ? responseObj.refunded : false,
                            Response = responseString
                        };
                        var paymentDetailId = await _paymentDetailService.Create(paymentDetail);
                        //return Ok("Payment Successful.Please wait........");

                        if(payment.Status == (int)Enums.PaymentStatus.Paid)
                        {
                            //var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);

                            var userId = await _userDetailService.GetUserDetailId(GetUserId());
                            //var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);
                            var paidcomment = "Updated By System for Payment method = " + Enums.PaymentProcessors.KHALTI.ToString();
                            await _orderService.UpdateStatus(order.OrderId, paidcomment, OrderStatusEnum.C009.ToString(), userId);
                            var invoicecomment = "Updated By System for  Invoice Payment method = " + Enums.PaymentProcessors.KHALTI.ToString();
                            await _orderService.UpdateStatus(order.OrderId, invoicecomment, OrderStatusEnum.C010.ToString(), userId);
                            var completecomment = "Updated By System for  Complete Payment method = " + Enums.PaymentProcessors.KHALTI.ToString();
                            await _orderService.UpdateStatus(order.OrderId, completecomment, OrderStatusEnum.C011.ToString(), userId);

                        }
                        else
                        {
                            return Ok("payment cannot success");
                        }

                            return Ok(2);
                        //}
                        //else
                        //return Ok(string.IsNullOrEmpty(responseObj.detail) ? responseObj.detail : responseObj.token);
                    }
                    catch (Exception ex)
                    {
                        //return Ok("Try again later.");
                        return Ok(ex);
                    }
                }
                else
                {
                    //return Ok("Amount not vaild. Please contact with administrator.");
                    return Ok(4);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        [Route("VerifyApplePayment")]
        [Authorize]
        public async Task<IActionResult> VerifyApplePayment(OderPaymentAppleDTO order)
        {
            try
            {
                if (order.transactionStatus == "FAIL")
                {
                    PaymentDTO payment = await _paymentService.GetPaymentByOrderId(order.OrderId);
                    payment.Amount = order.Amount;
                    payment.PaymentProcessorId = (int)Enums.PaymentProcessors.APPLE;
                    payment.Status = (int)Enums.PaymentStatus.Failed;
                    var paymentId = await _paymentService.Update(payment);

                    PaymentDetailDTO paymentDetail = new PaymentDetailDTO()
                    {
                        PaymentId = paymentId,
                        Token = order.Token,
                        TransactionId = order.transactionId,
                        FeeAmount = 0,
                        State = order.transactionStateIOS,
                        Refunded =  false,
                        Response = JsonConvert.SerializeObject(order)
                    };
                    var paymentDetailId = await _paymentDetailService.Create(paymentDetail);
                    return Ok(0);
                }
                else
                {
                    // here check amount in server side too for correct verification of amount
                    decimal mainPriceInPaisa = await _orderService.GetOrderTotal(order.OrderId);
                    /*
                    if (mainPriceInPaisa > 0)
                    {
                        if (!(mainPriceInPaisa == order.Amount))
                        {
                            //return Ok("Amount not vaild. Please contact with administrator.");
                            return Ok(0);
                        }
                    }
                    */
                    //// check already paid or not for particular order.
                    bool isPaymentDone = _paymentService.IsPaymentDone(order.OrderId);
                    if (isPaymentDone == true)
                    {
                        //return Ok("You have already paid for this order.");
                        return Ok(1);
                    }

                    if (isPaymentDone == false && order.transactionStateIOS == "purchased")
                    {
                        PaymentDTO payment = await _paymentService.GetPaymentByOrderId(order.OrderId);
                        payment.Amount = order.Amount;
                        payment.PaymentProcessorId = (int)Enums.PaymentProcessors.APPLE;
                        payment.Status = (int)Enums.PaymentStatus.Paid;
                        var paymentId = await _paymentService.Update(payment);

                        PaymentDetailDTO paymentDetail = new PaymentDetailDTO()
                        {
                            PaymentId = paymentId,
                            Token = order.Token,
                            TransactionId = order.transactionId,
                            FeeAmount = 0,
                            State = order.transactionStateIOS,
                            Refunded = false,
                            Response = JsonConvert.SerializeObject(order)
                        };
                        var paymentDetailId = await _paymentDetailService.Create(paymentDetail);
                        //return Ok("Payment Successful.Please wait........");

                        if (payment.Status == (int)Enums.PaymentStatus.Paid)
                        {
                            //var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);

                            var userId = await _userDetailService.GetUserDetailId(GetUserId());
                            //var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);
                            var paidcomment = "Updated By System for Payment method = " + Enums.PaymentProcessors.APPLE.ToString();
                            await _orderService.UpdateStatus(order.OrderId, paidcomment, OrderStatusEnum.C009.ToString(), userId);
                            var invoicecomment = "Updated By System for  Invoice Payment method = " + Enums.PaymentProcessors.APPLE.ToString();
                            await _orderService.UpdateStatus(order.OrderId, invoicecomment, OrderStatusEnum.C010.ToString(), userId);
                            var completecomment = "Updated By System for  Complete Payment method = " + Enums.PaymentProcessors.APPLE.ToString();
                            await _orderService.UpdateStatus(order.OrderId, completecomment, OrderStatusEnum.C011.ToString(), userId);

                        }
                        else
                        {
                            return Ok("payment cannot success");
                        }

                        return Ok(2);
                        //}
                        //else
                        //return Ok(string.IsNullOrEmpty(responseObj.detail) ? responseObj.detail : responseObj.token);

                    }
                    else
                    {
                        //return Ok("Amount not vaild. Please contact with administrator.");
                        return Ok(4);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        [Route("VerifyESewaPayment")]
        [Authorize]
        public async Task<ActionResult> VerifyESewaPayment(OrderPaymentDTO order)
        {
            try
            {
                // here check amount in server side too for correct verification of amount
                decimal mainPriceInPaisa = await _orderService.GetOrderTotal(order.OrderId);
                if (mainPriceInPaisa > 0)
                {
                    if (!(mainPriceInPaisa == order.Amount))
                    {
                        //return Ok("Amount not vaild. Please contact with administrator.");
                        return Ok(0);
                    }
                }
                //// check already paid or not for particular order.
                bool isPaymentDone = _paymentService.IsPaymentDone(order.OrderId);
                if (isPaymentDone == true)
                {
                    //return Ok("You have already paid for this order.");

                    var userId = await _userDetailService.GetUserDetailId(GetUserId());
                    //var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);
                    var paidcomment = "Updated By System for Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                    await _orderService.UpdateStatus(order.OrderId, paidcomment, OrderStatusEnum.C009.ToString(), userId);
                    var invoicecomment = "Updated By System for  invoice Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                    await _orderService.UpdateStatus(order.OrderId, invoicecomment, OrderStatusEnum.C010.ToString(), userId);
                    var completecomment = "Updated By System for  Complete Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                    await _orderService.UpdateStatus(order.OrderId, completecomment, OrderStatusEnum.C011.ToString(), userId);
                    return Ok(1);
                }

                if (mainPriceInPaisa == order.Amount && isPaymentDone == false)
                {
                    //for mobile 
                    if (order.IsMobile)
                    {
                        HttpClient clientMb = new HttpClient();
                        string valueURL = "?txnRefId=" + order.Token;
                        clientMb.DefaultRequestHeaders.Add("merchantId", eSewaPaymentSetting.ESewa_CLIENT_ID);
                        clientMb.DefaultRequestHeaders.Add("merchantSecret", eSewaPaymentSetting.ESewa_SECRET_KEY);
                        clientMb.DefaultRequestHeaders.Add("ContentType", "application/json");
                        var response = await clientMb.GetAsync(eSewaPaymentSetting.ESewaVerifyURLMobile + valueURL);
                        var responseString = await response.Content.ReadAsStringAsync();

                        if (responseString.StartsWith("{")) // unsucess result  for mobile
                        {
                            var obj1 = "[" + responseString + "]";
                            List<EsewaMessageErrorDTO> responseObjDTo = JsonConvert.DeserializeObject<List<EsewaMessageErrorDTO>>(obj1);
                            return Ok(responseObjDTo[0].Message.ErrorMessage);
                        }
                        else
                        {
                            List<EsewaResponseDTO> responseObj = JsonConvert.DeserializeObject<List<EsewaResponseDTO>>(responseString);
                            if (responseObj[0].TransactionDetails.Status == "COMPLETE")
                            {
                                 PaymentDTO payment = await _paymentService.GetPaymentByOrderId(order.OrderId);
                                if (payment == null) return Ok("payment cannot success");
                                else
                                {
                                    payment.Amount = order.Amount;
                                    payment.PaymentProcessorId = (int)Enums.PaymentProcessors.ESEWA;
                                    payment.Status = responseObj[0].TransactionDetails.Status == "COMPLETE" ? (int)Enums.PaymentStatus.Paid : (int)Enums.PaymentStatus.Failed;
                                    var paymentId = await _paymentService.Update(payment);
                                    PaymentDetailDTO paymentDetail = new PaymentDetailDTO()
                                    {
                                        PaymentId = paymentId,
                                        Token = order.Token,
                                        //TransactionId = responseStringValue != null ? responseStringValue.idx : "",
                                        //FeeAmount = responseStringValue != null ? responseStringValue.fee_amount : 0,
                                        //State = responseStringValue != null && responseStringValue.state != null && responseStringValue.state.name != null ? responseObj.state.name.ToLower() : "",
                                        Refunded = false,
                                        Response = responseString
                                    };
                                    var paymentDetailId = await _paymentDetailService.Create(paymentDetail);
                                }
                                    
                                    //return Ok("Payment Successful.Please wait........");
                                    if (payment.Status == (int)Enums.PaymentStatus.Paid)
                                    {
                                        var userId = await _userDetailService.GetUserDetailId(GetUserId());
                                        //var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);
                                        var paidcomment = "Updated By System for Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                                        await _orderService.UpdateStatus(order.OrderId, paidcomment, OrderStatusEnum.C009.ToString(), userId);
                                        var invoicecomment = "Updated By System for  invoice Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                                        await _orderService.UpdateStatus(order.OrderId, invoicecomment, OrderStatusEnum.C010.ToString(), userId);
                                        var completecomment = "Updated By System for  Complete Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                                        await _orderService.UpdateStatus(order.OrderId, completecomment, OrderStatusEnum.C011.ToString(), userId);
                                    }
                                    else
                                        return Ok("payment cannot success");
                                    
                                 return Ok(2);
                            }
                            else
                               return Ok("payment cannot success");

                            
                        }
                    }
                    else
                    {
                        string valueURL = "?amt=" + order.Amount + "&rid=" + order.Token + "&pid=" + order.OrderCode + "&scd=" + eSewaPaymentSetting.ESewaMerchantCode;
                        var response = await client.GetAsync(eSewaPaymentSetting.ESewaVerifyURL + valueURL);
                        var responseString = await response.Content.ReadAsStringAsync();
                        try
                        {
                            LogicLync.DTO.response eSewaResponse = null;
                            XmlSerializer serializer = new XmlSerializer(typeof(LogicLync.DTO.response));
                            MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes(responseString));
                            eSewaResponse = (LogicLync.DTO.response)serializer.Deserialize(mStrm);
                            string responseStringValue = eSewaResponse.response_code.Trim();

                            //update payment status
                            PaymentDTO payment = await _paymentService.GetPaymentByOrderId(order.OrderId);
                            if (payment == null) return Ok("payment cannnot sucess");
                            else
                            {
                                payment.Amount = order.Amount;
                                payment.PaymentProcessorId = (int)Enums.PaymentProcessors.ESEWA;
                                payment.Status = string.IsNullOrEmpty(responseStringValue) == false && responseStringValue.ToLower() == Enums.ESewaResponse.success.ToString().ToLower() ? (int)Enums.PaymentStatus.Paid : (int)Enums.PaymentStatus.Failed;
                                var paymentId = await _paymentService.Update(payment);
                                PaymentDetailDTO paymentDetail = new PaymentDetailDTO()
                                {
                                    PaymentId = paymentId,
                                    Token = order.Token,
                                    //TransactionId = responseStringValue != null ? responseStringValue.idx : "",
                                    //FeeAmount = responseStringValue != null ? responseStringValue.fee_amount : 0,
                                    //State = responseStringValue != null && responseStringValue.state != null && responseStringValue.state.name != null ? responseObj.state.name.ToLower() : "",
                                    Refunded = false,
                                    Response = responseString
                                };
                                var paymentDetailId = await _paymentDetailService.Create(paymentDetail);
                            }
                            //return Ok("Payment Successful.Please wait........");
                            if (payment.Status == (int)Enums.PaymentStatus.Paid)
                            {
                                var userId = await _userDetailService.GetUserDetailId(GetUserId());
                                //var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);
                                var paidcomment = "Updated By System for Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                                await _orderService.UpdateStatus(order.OrderId, paidcomment, OrderStatusEnum.C009.ToString(), userId);
                                var invoicecomment = "Updated By System for  invoice Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                                await _orderService.UpdateStatus(order.OrderId, invoicecomment, OrderStatusEnum.C010.ToString(), userId);
                                var completecomment = "Updated By System for  Complete Payment Method = " + Enums.PaymentProcessors.ESEWA.ToString();
                                await _orderService.UpdateStatus(order.OrderId, completecomment, OrderStatusEnum.C011.ToString(), userId);

                            }
                            else
                            {
                                return Ok("payment cannot success");
                            }
                            return Ok(2);
                            //}
                            //else
                            //return Ok(string.IsNullOrEmpty(responseObj.detail) ? responseObj.detail : responseObj.token);*/
                        }
                        catch (Exception ex)
                        {
                            //return Ok("Try again later.");
                            return Ok(3);
                        }
                    }
                }
                else
                {
                    //return Ok("Amount not vaild. Please contact with administrator.");
                    return Ok(4);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("GetIMEPayToken")]
        [Authorize]
        public async Task<ActionResult> GetIMEPayToken(OrderPaymentDTO order)
        {
            try
            {
                string imeCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(imePaySetting.IMEPayAPIUser + ":" + imePaySetting.IMEPayAPIPassword));
                string imeModule = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(imePaySetting.IMEPayAPIModule));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imePaySetting.IMEPayTokenAPI);
                request.Method = "POST";
                request.Headers.Add("Authorization", "Basic " + imeCredentials);
                request.Headers.Add("Module", imeModule);
                string refId = "Ref-" + order.OrderId;
                string values = "{\"MerchantCode\":\"" + imePaySetting.IMEPayAPIMerchantCode + "\", \"Amount\":\"" + order.Amount + "\", \"RefId\":\"" + refId + "\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(values);
                request.ContentType = "text/plain";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                string responseFromServer = "";
                using (dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
                response.Close();
                IMEPayResponseDTO responseObj = JsonConvert.DeserializeObject<IMEPayResponseDTO>(responseFromServer);

                PaymentDTO payment = new PaymentDTO()
                {
                    OrderId = order.OrderId,
                    Amount = order.Amount,
                    PaymentProcessorId = (int)Enums.PaymentProcessors.IMEPAY,
                    Status = (int)Enums.PaymentStatus.Pending
                };
                var paymentId = await _paymentService.Create(payment);

                PaymentDetailDTO paymentDetail = new PaymentDetailDTO()
                {
                    PaymentId = paymentId,
                    Token = responseObj.TokenId,
                    TransactionId = "",
                    FeeAmount = 0,
                    State = "",
                    Refunded = false,
                    Response = responseFromServer,
                    MerchantCode = imePaySetting.IMEPayAPIMerchantCode,
                    RefId = refId,
                    RequestDate = DateTime.Now,
                    ResponseDate = DateTime.Now
                };
                var paymentDetailId = await _paymentDetailService.Create(paymentDetail);

                string requestString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(responseObj.TokenId + "|" + imePaySetting.IMEPayAPIMerchantCode + "|" + refId + "|" + order.Amount + "|GET|" + imePaySetting.IMEPayResponseURL + "|" + imePaySetting.IMEPayFailedURL));

                string checkoutRequestURL = imePaySetting.IMEPayCheckoutAPI + "?data=" + requestString;
                return Ok(new
                {
                    url = checkoutRequestURL
                });
            } catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("VerifyIMEPayToken")]
        [Authorize]
        public async Task<ActionResult> VerifyIMEPayToken(OrderPaymentDTO order)
        {
            try
            {
                if (string.IsNullOrEmpty(order.Token.Trim()) == true)
                    throw new Exception("Invalid transaction.");

                byte[] responseByte = Convert.FromBase64String(order.Token);
                if (responseByte == null)
                    throw new Exception("Invalid transaction.");

                string responseString = ASCIIEncoding.ASCII.GetString(responseByte);
                if (string.IsNullOrEmpty(responseString) == false)
                {
                    string[] splitResponseString = responseString.Split("|");

                    PaymentDetailDTO paymentDtl = _paymentDetailService.GetPaymentDetailByRefId(splitResponseString[4]);

                    paymentDtl.TransactionId = splitResponseString[3];
                    paymentDtl.Msisdn = splitResponseString[2];
                    paymentDtl.ImeTxnStatus = Convert.ToInt32(splitResponseString[0]);

                    await _paymentDetailService.Update(paymentDtl);

                    string imeCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(imePaySetting.IMEPayAPIUser + ":" + imePaySetting.IMEPayAPIPassword));
                    string imeModule = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(imePaySetting.IMEPayAPIModule));

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imePaySetting.IMEPayConfirmAPI);
                    request.Method = "POST";
                    request.Headers.Add("Authorization", "Basic " + imeCredentials);
                    request.Headers.Add("Module", imeModule);

                    string values = "{\"MerchantCode\":\"" + imePaySetting.IMEPayAPIMerchantCode + "\",\"RefId\":\"" + paymentDtl.RefId + "\",\"TokenId\":\"" + paymentDtl.Token + "\",\"TransactionId\":\"" + paymentDtl.TransactionId + "\",\"Msisdn\":\"" + paymentDtl.Msisdn + "\"}";
                    byte[] byteArray = Encoding.UTF8.GetBytes(values);
                    request.ContentType = "text/plain";
                    request.ContentLength = byteArray.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse response = request.GetResponse();
                    string responseFromServer = "";
                    using (dataStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        responseFromServer = reader.ReadToEnd();
                    }
                    response.Close();
                    IMEPayConfirmResponseDTO responseObj = JsonConvert.DeserializeObject<IMEPayConfirmResponseDTO>(responseFromServer);

                    if (responseObj.ResponseCode == "0")
                    {
                        return Ok(1);
                    } else
                    {
                        return Ok(0);
                    }
                }
                return Ok(1);
            } catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("RecheckIMEPayTransaction")]
        [Authorize]
        public async Task<ActionResult> RecheckIMEPayTransaction(Int64 paymentId)
        {
            try
            {
                PaymentDTO payment = await _paymentService.GetPaymentById(paymentId, x => x.Id == paymentId, x => x.PaymentDetail);

                if (payment == null)
                    throw new Exception("Invalid payment id");

                string imeCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(imePaySetting.IMEPayAPIUser + ":" + imePaySetting.IMEPayAPIPassword));
                string imeModule = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(imePaySetting.IMEPayAPIModule));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imePaySetting.IMEPayReCheckAPI);
                request.Method = "POST";
                request.Headers.Add("Authorization", "Basic " + imeCredentials);
                request.Headers.Add("Module", imeModule);

                string values = "{\"MerchantCode\":\"" + imePaySetting.IMEPayAPIMerchantCode + "\", \"RefId\":\"" + payment.PaymentDetail.RefId + "\", \"TokenId\": \"" + payment.PaymentDetail.Token + "\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(values);
                request.ContentType = "text/plain";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                string responseFromServer = "";
                using (dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
                response.Close();
                IMEPayConfirmResponseDTO responseObj = JsonConvert.DeserializeObject<IMEPayConfirmResponseDTO>(responseFromServer);

                PaymentDetailDTO updateDetail = payment.PaymentDetail;

                updateDetail.ImeTxnStatus = Convert.ToInt32(responseObj.ResponseCode);
                updateDetail.Msisdn = responseObj.Msisdn;
                updateDetail.TransactionId = responseObj.TransactionId;
                updateDetail.Response = responseObj.ResponseDescription;
                updateDetail.RefId = responseObj.RefId;
                updateDetail.Token = responseObj.TokenId;

                await _paymentDetailService.Update(updateDetail);

                if (updateDetail.ImeTxnStatus == 0)
                {
                    return Ok(1);
                }
                return Ok(0);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("GenerateConnectIPSToken")]
        [Authorize]
        public async Task<ActionResult> GenerateConnectIPSToken(OrderPaymentDTO order)
        {
            try {
                string todayDate = DateTime.Now.ToString("dd-MM-yyyy");
                String stringToHash = "MERCHANTID=" + connectIPSSetting.Connect_IPS_MerchantID + 
                    ",APPID=" + connectIPSSetting.Connect_IPS_AppID + 
                    ",APPNAME=" + connectIPSSetting.Connect_IPS_AppName + 
                    ",TXNID=" + order.OrderId + 
                    ",TXNDATE=" + todayDate + 
                    ",TXNCRNCY=" + connectIPSSetting.Connect_IPS_TxnCrncy + 
                    ",TXNAMT=" + order.Amount + 
                    ",REFERENCEID=" + order.OrderCode + 
                    ",REMARKS=" + order.OrderCode + 
                    ",PARTICULARS=" + order.OrderCode + 
                    ",TOKEN=TOKEN";

                string signedHashMessage = this.generateConnectIPSToken(stringToHash, connectIPSSetting.Connect_IPS_PFX_Password);

                return Ok(new
                {
                    MERCHANTID = connectIPSSetting.Connect_IPS_MerchantID,
                    APPID = connectIPSSetting.Connect_IPS_AppID,
                    APPNAME = connectIPSSetting.Connect_IPS_AppName,
                    TXNID = order.OrderId,
                    TXNDATE = todayDate,
                    TXNCRNCY = connectIPSSetting.Connect_IPS_TxnCrncy,
                    TXNAMT = order.Amount,
                    REFERENCEID = order.OrderCode,
                    REMARKS = order.OrderCode,
                    PARTICULARS = order.OrderCode,
                    TOKEN = signedHashMessage
                });
            }catch(Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("VerifyConnectIPSPayment")]
        [Authorize]
        public async Task<ActionResult> VerifyConnectIPSPayment(OrderPaymentDTO order)
        {
            if (order.OrderId <= 0)
                return Ok(0);

            decimal orderAmount = await _orderService.GetOrderTotal(order.OrderId) * 100;

            if (orderAmount <= 0)
                return Ok(0);

            String stringToHash = "MERCHANTID=" + connectIPSSetting.Connect_IPS_MerchantID + 
                    ",APPID=" + connectIPSSetting.Connect_IPS_AppID + 
                    ",REFERENCEID=" + order.OrderId + 
                    ",TXNAMT=" + orderAmount;

            string orderCode = "kkinni-100-100-" + order.OrderId.ToString();

            string signedHashMessage = this.generateConnectIPSToken(stringToHash, connectIPSSetting.Connect_IPS_PFX_Password);

            try
            {
                string connectIPSCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(connectIPSSetting.Connect_IPS_Transaction_Validation_UserName + ":" + connectIPSSetting.Connect_IPS_Transaction_Validation_Password));
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(connectIPSSetting.Connect_IPS_Transaction_Validation_URL);
                request.Method = "POST";
                request.Headers.Add("Authorization", "Basic " + connectIPSCredentials);
                
                string values = "{\"merchantId\":\"" + connectIPSSetting.Connect_IPS_MerchantID + "\", \"appId\":\"" + connectIPSSetting.Connect_IPS_AppID + "\", \"referenceId\": \"" + order.OrderId + "\", \"txnAmt\": \"" + orderAmount + "\", \"token\":\"" + signedHashMessage + "\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(values);
                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                string responseFromServer = "";
                using (dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
                response.Close();
                ConnectIPSResponseDTO responseObj = JsonConvert.DeserializeObject<ConnectIPSResponseDTO>(responseFromServer);

                ConnectIPSTransactionDetailResponseDTO responseDetailObj = new ConnectIPSTransactionDetailResponseDTO();
                string responseDetailFromServer = "";
                
                if (responseObj != null && responseObj.status.ToLower() == "success")
                {
                    string connectIPSCredentialsDetail = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(connectIPSSetting.Connect_IPS_Transaction_Validation_UserName + ":" + connectIPSSetting.Connect_IPS_Transaction_Validation_Password));

                    HttpWebRequest requestDetail = (HttpWebRequest)WebRequest.Create(connectIPSSetting.Connect_IPS_Transaction_Detail_URL);
                    requestDetail.Method = "POST";
                    requestDetail.Headers.Add("Authorization", "Basic " + connectIPSCredentialsDetail);

                    string valuesDetail = "{\"merchantId\":\"" + connectIPSSetting.Connect_IPS_MerchantID + "\", \"appId\":\"" + connectIPSSetting.Connect_IPS_AppID + "\", \"referenceId\": \"" + order.OrderId + "\", \"txnAmt\": \"" + orderAmount + "\", \"token\":\"" + signedHashMessage + "\"}";
                    byte[] byteArrayDetail = Encoding.UTF8.GetBytes(valuesDetail);
                    requestDetail.ContentType = "application/json";
                    requestDetail.ContentLength = byteArrayDetail.Length;
                    Stream dataStreamDetail = requestDetail.GetRequestStream();
                    dataStreamDetail.Write(byteArrayDetail, 0, byteArrayDetail.Length);
                    dataStreamDetail.Close();
                    WebResponse responseDetail = requestDetail.GetResponse();
                    using (dataStreamDetail = responseDetail.GetResponseStream())
                    {
                        StreamReader readerDetail = new StreamReader(dataStreamDetail);
                        responseDetailFromServer = readerDetail.ReadToEnd();
                    }
                    responseDetail.Close();
                    responseDetailObj = JsonConvert.DeserializeObject<ConnectIPSTransactionDetailResponseDTO>(responseDetailFromServer);
                }

                PaymentDTO payment = new PaymentDTO()
                {
                    OrderId = order.OrderId,
                    Amount = order.Amount/100,
                    PaymentProcessorId = (int)Enums.PaymentProcessors.CONNECTIPS,
                    Status = responseObj != null && responseObj.status.ToLower() == "success" ? (int)Enums.PaymentStatus.Paid : (int)Enums.PaymentStatus.Failed
                };
                var paymentId = await _paymentService.Create(payment);

                PaymentDetailDTO paymentDetail = new PaymentDetailDTO()
                {
                    PaymentId = paymentId,
                    Token = signedHashMessage,
                    TransactionId = responseObj.referenceId,
                    FeeAmount = responseDetailObj != null ? responseDetailObj.chargeAmt : 0,
                    State = responseObj.statusDesc,
                    Refunded = false,
                    Response = string.IsNullOrEmpty(responseDetailFromServer) ? responseFromServer : responseDetailFromServer,
                    MerchantCode = connectIPSSetting.Connect_IPS_MerchantID,
                    RefId = responseDetailObj != null ? responseDetailObj.refId : responseObj.referenceId,
                    RequestDate = DateTime.Now,
                    ResponseDate = DateTime.Now,
                };

                var paymentDetailId = await _paymentDetailService.Create(paymentDetail);
                if (payment.Status != (int)Enums.PaymentStatus.Paid)
                {
                    var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);

                }
                else
                {
                    return Ok("payment cannot success");
                }

                if (payment.Status == (int)Enums.PaymentStatus.Paid)
                {
                    return Ok(1);
                }
                return Ok(0);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("GeneratePhonePayToken")]
        [Authorize]
        public async Task<ActionResult> GeneratePhonePayToken(OrderPaymentDTO order)
        {
            try
            {
                string date = DateTime.Now.ToString("MM/dd/yyyy");
                string remark1 = "payment";
                string remark2 = "for subject";
                //string message = order.Amount + "," + order.OrderCode + "," + fonePaySetting.FonePay_MerchantCode + "," + remark1 + "," + remark2;
                string message = fonePaySetting.FonePayQR_MerchantCode + ",P," + order.OrderCode + "," + order.Amount + ",NPR," + date + "," + remark1 + "," + remark2 + "," + fonePaySetting.FonePay_Merchant_Return_URL;
                string secret = fonePaySetting.FonePayQR_Secret;
                //string hash = generateHash(secret, message);
               string hash = generateHash(secret,message);

                //if(string.IsNullOrEmpty(hash) == false)
                //{
                //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fonePaySetting.FonePay_QRCode_Request_URL);
                //    request.Method = "POST";
                //    request.Headers.Add("Content-Type", "application/json");

                //    string values = "{" +
                //                    "\"amount\":\"" + order.Amount + "\", " +
                //                    "\"remarks1\":\"" + remark1 + "\", " +
                //                    "\"remarks2\": \"" + remark2 + "\", " +
                //                    "\"prn\": \"" + order.OrderCode + "\", " +
                //                    "\"merchantCode\":\"" + fonePaySetting.FonePay_MerchantCode + "\", " +
                //                    "\"dataValidation\":\"" + hash + "\", " +
                //                    "\"username\":\"" + fonePaySetting.FonePay_Merchant_UserName + "\", " +
                //                    "\"password\":\"" + fonePaySetting.FonePay_Merchant_Password + "\"" +
                //                    "}";
                //    byte[] byteArray = Encoding.UTF8.GetBytes(values);
                //    request.ContentType = "application/json";
                //    request.ContentLength = byteArray.Length;
                //    Stream dataStream = request.GetRequestStream();
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    dataStream.Close();
                //    WebResponse response = request.GetResponse();
                //    string responseFromServer = "";
                //    using (dataStream = response.GetResponseStream())
                //    {
                //        StreamReader reader = new StreamReader(dataStream);
                //        responseFromServer = reader.ReadToEnd();
                //    }
                //    response.Close();
                //    FonePayResponseDTO responseObj = JsonConvert.DeserializeObject<FonePayResponseDTO>(responseFromServer);
                //    return Ok(responseObj);
                //}
                //return Ok(new FonePayResponseDTO());
               
                var data = new { 
                    URL = fonePaySetting.FonePay_Merchant_Request_URL,
                    //PID = fonePaySetting.FonePay_MerchantCode,
                    PID = fonePaySetting.FonePayQR_MerchantCode,
                    MD = "P",
                    AMT = order.Amount,
                    CRN = "NPR",
                    DT = date,
                    R1 = remark1,
                    R2 = remark2,
                    DV = hash.ToLower(),
                    RU = fonePaySetting.FonePay_Merchant_Return_URL,
                    PRN = order.OrderCode
                };
                PaymentDTO payment = await _paymentService.GetPaymentByOrderId(order.OrderId);
                payment.PaymentProcessorId = (int)Enums.PaymentProcessors.FONEPAY;
                payment.PaymentFinal = JsonConvert.SerializeObject(data);
                await _paymentService.Update(payment);
                return Ok(data);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("VerifyPhonePayPayment")]
        [Authorize]
        public async Task<ActionResult> VerifyPhonePayPayment(FonePayPaymentVerificationDTO verification)
        {
            try
            {
                Int64 orderId = Convert.ToInt64(verification.PRN.Split("-")[3]);
                string secret = fonePaySetting.FonePayQR_Secret;
                //string message = verification.PID + "," + verification.P_AMT + "," + verification.PRN + "," + verification.BID + "," + verification.UID;
                string message = verification.PRN + "," + verification.PID  + "," + verification.PS + "," + verification.RC + "," + verification.UID + "," + verification.BC + "," + verification.INI + "," + verification.P_AMT + "," + verification.R_AMT;
                string hash = _qrcodeservice.generateHash(secret, message);
                
                //string hashh = generateHash(secret, message);


                string jsonResult = "{" +
                                    "\"prn\":\"" + verification.PRN + "\", " +
                                    "\"merchantCode\":\"" + verification.PID + "\", " +
                                    "\"amount\": \"" + verification.P_AMT + "\"" +
                                    "}";

                //string authHeaderString = fonePaySetting.FonePayQR_Merchant_UserName + "," + fonePaySetting.FonePayQR_Merchant_Password + "," + "POST" + "," + "application/json" + "," + "/merchant/merchantDetailsForThirdParty/txnVerification" + "," + jsonResult;
                //string hasAuthHeader = generateHash(secret, authHeaderString);

                if (string.IsNullOrEmpty(hash) == false && verification.DV.ToLower() == hash.ToLower())
                {
                    //string fonePayCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(fonePaySetting.FonePayQR_Merchant_UserName + ":" + fonePaySetting.FonePayQR_Merchant_Password));
                    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fonePaySetting.FonePay_Merchant_Payment_Vericication_URL);
                    //request.Method = "POST";
                    //request.Headers.Add("Authorization", "Basic " + fonePayCredentials);
                    //request.Headers.Add("auth", hasAuthHeader);
                    //request.Headers.Add("Content-Type", "application/json");

                    //byte[] byteArray = Encoding.UTF8.GetBytes(jsonResult);
                    //request.ContentType = "application/json";
                    //request.ContentLength = byteArray.Length;
                    //Stream dataStream = request.GetRequestStream();
                    //dataStream.Write(byteArray, 0, byteArray.Length);
                    //dataStream.Close();
                    //WebResponse response = request.GetResponse();
                    //string responseFromServer = "";
                    //using (dataStream = response.GetResponseStream())
                    //{
                    //    StreamReader reader = new StreamReader(dataStream);
                    //    responseFromServer = reader.ReadToEnd();
                    //}
                    //response.Close();
                    //FonePayVerificationResponseDTO responseObj = JsonConvert.DeserializeObject<FonePayVerificationResponseDTO>(responseFromServer);

                    //update payment status
                    PaymentDTO payment = await _paymentService.GetPaymentByOrderId(orderId);
                    payment.Amount = Convert.ToDecimal(verification.P_AMT);
                    payment.PaymentProcessorId = (int)Enums.PaymentProcessors.FONEPAY;
                    payment.Status = verification.RC.ToLower() == "successful" ? (int)Enums.PaymentStatus.Paid : (int)Enums.PaymentStatus.Failed;
                    var paymentId = await _paymentService.Update(payment);

                    //PaymentDTO payment = new PaymentDTO()
                    //{
                    //    OrderId = orderId,
                    //    Amount = Convert.ToDecimal(verification.P_AMT),
                    //    PaymentProcessorId = (int)Enums.PaymentProcessors.FONEPAY,
                    //    //Status = responseObj != null && responseObj.paymentStatus.ToLower() == "success" ? (int)Enums.PaymentStatus.Paid : (int)Enums.PaymentStatus.Failed
                    //    Status = verification.RC.ToLower() == "successful" ? (int)Enums.PaymentStatus.Paid : (int)Enums.PaymentStatus.Failed
                    //};
                    //var paymentId = await _paymentService.Create(payment);

                    PaymentDetailDTO paymentDetail = new PaymentDetailDTO()
                    {
                        PaymentId = paymentId,
                        Token = verification.DV,
                        //TransactionId = responseObj.fonepayTraceId,
                        TransactionId = verification.UID,
                        FeeAmount = (double)(Convert.ToDecimal(verification.R_AMT) - Convert.ToDecimal(verification.P_AMT)),
                        //State = responseObj.paymentStatuspaymentStatus,
                        State = verification.RC,
                        Refunded = false,
                        Response = jsonResult,
                        MerchantCode = fonePaySetting.FonePayQR_MerchantCode,
                        //RefId = responseObj.fonepayTraceId,
                        RefId = verification.UID,
                        RequestDate = DateTime.Now,
                        ResponseDate = DateTime.Now,
                    };

                    var paymentDetailId = await _paymentDetailService.Create(paymentDetail);


                    if(payment.Status == (int)Enums.PaymentStatus.Paid)
                    {

                        var userId = await _userDetailService.GetUserDetailId(GetUserId());
                        //var orderstatus = await _orderService.updateIfPaymentSuccess(order.OrderId);
                        var paidcomment = "Updated By System for Payment Method = " + Enums.PaymentProcessors.FONEPAY.ToString();
                        await _orderService.UpdateStatus(orderId, paidcomment, OrderStatusEnum.C009.ToString(), userId);
                        var invoicecomment = "Updated By System for  Invoice Payment Method = " + Enums.PaymentProcessors.FONEPAY.ToString();
                        await _orderService.UpdateStatus(orderId, invoicecomment, OrderStatusEnum.C010.ToString(), userId);
                        var completecomment = "Updated By System for  Complete Payment Method = " + Enums.PaymentProcessors.FONEPAY.ToString();
                        await _orderService.UpdateStatus(orderId, completecomment, OrderStatusEnum.C011.ToString(), userId);


                        return Ok(1);
                    } else
                    {
                        return Ok(0);
                    }
                }
                return Ok(0);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("PayWithCashOnDelivery")]
        [Authorize]
        public async Task<ActionResult> PayWithCashOnDelivery(PaymentDTO paymentDetail)
        {
            try
            {
                var result = await _paymentService.UpsertCashOnDeliveryPayment(paymentDetail);
                return Ok(result);
            } catch(Exception ex)
            {
                throw;
            }
        }

        private String generateHash(String secretKey, String message)
        {
            try
            {
                byte[] b = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)).ComputeHash(Encoding.UTF8.GetBytes(message));
                string byteTOHex = bytesToHex(b);
                return byteTOHex;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private static String bytesToHex(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private string generateConnectIPSToken(string stringToHash, string pfxPassword)
        {
            string signedHashMessage = "";

            try
            {
                string Filename = Path.Combine(host.ContentRootPath, "DigitalCertificates//CREDITOR.pfx");
                using (var crypt = new SHA256Managed())
                using (var cert = new X509Certificate2(Filename, pfxPassword, X509KeyStorageFlags.Exportable))
                {
                    byte[] data = Encoding.UTF8.GetBytes(stringToHash);

                    RSA csp = null;
                    if (cert != null)
                    {
                        csp = cert.PrivateKey as RSA;
                    }

                    if (csp == null)
                    {
                        throw new Exception("No valid cert was found");
                    }

                    csp.ImportParameters(csp.ExportParameters(true));
                    byte[] signatureByte = csp.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    signedHashMessage = Convert.ToBase64String(signatureByte);
                }
            }
            catch (Exception ex)
            {
                signedHashMessage = "";
            }
            return signedHashMessage;
        }



        [HttpPost]
        [Route("GetAllTransaction")]
        //[DataPermissionAttribute(new Enums.DataPermissions[] { Enums.DataPermissions.Orders_ViewAll })]
        public async Task<IActionResult> GetAllTransaction(TransactionQueryObject query)
        {
            try
            {
                int store = await GetStoreId(_storeService);
         
                var result = await _paymentService.GetAllTransaction(query,store);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong while trying to get Orders.");
            }
        }


        [HttpGet]
        [Route("PaymentDetailFromPaymentId/{id}")]
        public async Task<IActionResult> PaymentDetailFromPaymentId(long id)
        {
            try
            {
                int store = await GetStoreId(_storeService);
                //Int64 userId = await _userDetailService.GetUserDetailId(GetUserId());
                var result = await _paymentService.PaymentDetailFromPaymentId(id);
                return Ok(result);
            }
            catch (Exception ex)
            {

                throw;
            }

        }




    }
}