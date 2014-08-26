using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TagDistributor.DAL;
using TagDistributor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TagDistributor.Utils;

namespace TagDistributor.Controllers
{
    public class TagDistributsController : Controller
    {
        private TDContext db = new TDContext();

        public ActionResult Query()
        {
            return Json(db.TagDistributs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Distribut()
        {
            Stream inputStream = Request.InputStream;
            StreamReader reader = new StreamReader(inputStream);
            string bodyText = reader.ReadToEnd();

            JObject o = new JObject();

            if (bodyText.StartsWith("{") && (bodyText.EndsWith("}")))
            {
                JObject jObj = JObject.Parse(bodyText);
                string name = "";
                long beginID = -1;
                long endID = -1;
                int flag = 0;
                long num = 0;

                var num_t = jObj.GetValue("num");
                if (num_t != null)
                {
                    //The request must be included "num"
                    num = num_t.Value<long>();

                    var name_t = jObj.GetValue("name");
                    if (name_t != null)
                    {
                        name = name_t.Value<string>();
                    }

                    var beginID_t = jObj.GetValue("bid");
                    if (beginID_t != null)
                    {
                        beginID = beginID_t.Value<long>();
                    }

                    var endID_t = jObj.GetValue("eid");
                    if (endID_t != null)
                    {
                        endID = endID_t.Value<long>();
                    }

                    var flag_t = jObj.GetValue("flag");
                    if (flag_t != null)
                    {
                        flag = flag_t.Value<int>();
                    }

                    JObject result = TagSplit.selectData(name, beginID, endID, num, flag);
                    if (result != null)
                    {
                        long beginID_result = (long)result["bId"];
                        long endID_result = (long)result["eId"];
                        string msg_result = (string)result["msg"];

                        o.Add(new JProperty("bid", beginID_result));//success
                        o.Add(new JProperty("eId", endID_result));
                        o.Add(new JProperty("msg", msg_result));
                        o.Add(new JProperty("success", 1));//success
                    }

                    return Json(o.ToString());
                }

                System.Diagnostics.Debug.Write("name:", name);
            }
            o.Add(new JProperty("success", 0));

            return Json(o.ToString());
        }

        public ActionResult ReturnTag()
        {
            Stream inputStream = Request.InputStream;
            StreamReader reader = new StreamReader(inputStream);
            string bodyText = reader.ReadToEnd();

            JObject o = new JObject();

            if (bodyText.StartsWith("{") && (bodyText.EndsWith("}")))
            {
                JObject jObj = JObject.Parse(bodyText);
                string name = "";
                long beginID = 0;
                long num = 0;

                //The request must be included "num"

                var beginID_t = jObj.GetValue("bid");
                if (beginID_t != null)
                {
                    beginID = beginID_t.Value<long>();

                    var num_t = jObj.GetValue("num");
                    if (num_t != null)
                    {
                        num = num_t.Value<long>();
                    }

                    var name_t = jObj.GetValue("name");
                    if (name_t != null)
                    {
                        name = name_t.Value<string>();
                    }
                    JObject result = TagSplit.rollback(name, beginID, num);
                    if (result != null)
                    {
                        string msg_result = (string)result["msg"];
                        o.Add(new JProperty("msg", msg_result));
                        o.Add(new JProperty("success", 1));//success
                    }

                    return Json(o.ToString());
                }
            }

            o.Add(new JProperty("success", 0));//fail

            return Json(o.ToString());
        }

        // GET: TagDistributs
        //public ActionResult Index()
        //{
        //    return View(db.TagDistributs.ToList());
        //}

        public ActionResult Index(string searchString, string userName)
        {

            var UserList = new List<String>();

            var UserQry = from d in this.db.TagDistributs
                          orderby d.Username
                          select d.Username;
            UserList.AddRange(UserQry.ToList().Distinct());
            this.ViewBag.UserName = new SelectList(UserList);

            var tagDistributs = from m in this.db.TagDistributs
                                select m;
            if (!string.IsNullOrEmpty(searchString))
            {
                tagDistributs = tagDistributs.Where(h => h.BeginID.ToString().Contains(searchString));
            }
            if (!string.IsNullOrEmpty(userName))
            {
                tagDistributs = tagDistributs.Where(x => x.Username == userName);
            }

            return View(tagDistributs);
        }

        // GET: TagDistributs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TagDistribut tagDistribut = db.TagDistributs.Find(id);
            if (tagDistribut == null)
            {
                return HttpNotFound();
            }
            return View(tagDistribut);
        }

        // GET: TagDistributs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TagDistributs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,BeginID,EndID,Username,DistributDate")] TagDistribut tagDistribut)
        {
            if (ModelState.IsValid)
            {
                db.TagDistributs.Add(tagDistribut);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tagDistribut);
        }

        // GET: TagDistributs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TagDistribut tagDistribut = db.TagDistributs.Find(id);
            if (tagDistribut == null)
            {
                return HttpNotFound();
            }
            return View(tagDistribut);
        }

        // POST: TagDistributs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,BeginID,EndID,Username,DistributDate")] TagDistribut tagDistribut)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tagDistribut).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tagDistribut);
        }

        // GET: TagDistributs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TagDistribut tagDistribut = db.TagDistributs.Find(id);
            if (tagDistribut == null)
            {
                return HttpNotFound();
            }
            return View(tagDistribut);
        }

        // POST: TagDistributs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TagDistribut tagDistribut = db.TagDistributs.Find(id);
            db.TagDistributs.Remove(tagDistribut);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
