using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Xml.Linq;

namespace NodeStructureApp.Controllers
{
    public class NodesController : Controller
    {
        private ChallengCodingDBEntities db = new ChallengCodingDBEntities();
        private readonly string connectionString = "data source=DESKTOP-3HEB05O\\SQLEXPRESS;initial catalog=ChallengCodingDB;integrated security=True;encrypt=False;MultipleActiveResultSets=True;App=EntityFramework\" providerName=\"System.Data.EntityClient";
        private SelectList GetNodesSelectList(int? selectedNodeId = null)
        {
            var nodes = db.NodeTbls.Select(n => new { n.NodeId, n.NodeName }).ToList();
            return new SelectList(nodes, "NodeId", "NodeName", selectedNodeId);
        }
        public ActionResult Index()
        {
            return View(db.NodeTbls.ToList());
        }
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ParentNodeId = GetNodesSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NodeId,NodeName,ParentNodeId,IsActive")] NodeTbl node)
        {
            if (ModelState.IsValid)
            {
                db.NodeTbls.Add(node);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentNodeId = GetNodesSelectList(node.ParentNodeId);
            return View(node);
        }
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                NodeTbl node = db.NodeTbls.Find(id);
                if (node == null)
                {
                    return HttpNotFound();
                }
                ViewBag.ParentNodeId = GetNodesSelectList(node.ParentNodeId);
                return View(node);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "NodeId,NodeName,ParentNodeId,IsActive")] NodeTbl node)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(node).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(node);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult Delete(int id)
        {
            NodeTbl node = db.NodeTbls.Find(id);
            db.NodeTbls.Remove(node);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetNodes()
        {
            List<NodeTbl> nodes = new List<NodeTbl>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetActiveNodes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            nodes.Add(new NodeTbl
                            {
                                NodeId = (int)reader["NodeId"],
                                NodeName = (string)reader["NodeName"],
                                ParentNodeId = reader["ParentNodeId"] != DBNull.Value ? (int?)reader["ParentNodeId"] : null,
                                IsActive = (bool)reader["IsActive"],
                                //StartDate = (DateTime)reader["StartDate"]
                            });
                        }
                    }
                }
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
        }
    }
}