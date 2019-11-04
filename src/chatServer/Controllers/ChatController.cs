using chatBLL;
using chatDAL;
using chatModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatserver.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
            })
            .ToArray();
        }

        /// <summary>
        /// 获取指定用户所有讨论组
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("roombyuid")]
        public ActionResult GetRoomByUid(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
                return null;// Json(null, JsonRequestBehavior.AllowGet);
            return new JsonResult(ChatroomDAL.Instance.GetChatRoomByUid(uid));
        }

        /// <summary>
        /// 获取所有记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("allrec")]
        public ActionResult GetAllRec(User user)
        {
            try
            {
                //byte[] arr = new byte[HttpContext.Request.Body.Length];
                //HttpContext.Request.Body.Read(arr, 0, arr.Length);
                //Dictionary<string, string> keyValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.Text.Encoding.UTF8.GetString(arr));
                string system = user.System;// keyValues["system"];
                string maincode = user.MainCode;//keyValues["maincode"].ToString();
                string mainid = user.MainID;//keyValues["mainid"].ToString();
                if (!string.IsNullOrWhiteSpace(system) && !string.IsNullOrWhiteSpace(maincode) && !string.IsNullOrWhiteSpace(mainid))
                {
                    RecordBLL recordbll = new RecordBLL();
                    IList<Record> records = recordbll.GetAll(system, maincode, mainid);
                    if (records != null && records.Count > 0)
                    {
                        return new JsonResult(records);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
