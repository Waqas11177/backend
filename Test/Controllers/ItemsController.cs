using DAL.Model;
using DAL.Repositeries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test.ViewModel;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ILogger<ItemsController> _logger;
        private readonly RepositeryContext _dbContext;

        public ItemsController(ILogger<ItemsController> logger, RepositeryContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        [HttpPost, Route("ItemRecord")]
        public IActionResult ItemRecord(ItemViewModel model)
        {

            try
            {
                if (model.master.id == 0)
                {
                    MasterItem master = new MasterItem();
                    List<Detailitem> detailItems = new List<Detailitem>();

                    master.ItemCode = Helper.GetMaxCode(_dbContext);
                    master.ItemName = model.master.ItemName;
                    _dbContext.masterItems.Add(master);
                    _dbContext.SaveChanges();

                    foreach (var item in model.detail)
                    {
                        Detailitem detailitem = new Detailitem();
                        detailitem.MasterItemId = master.Id;
                        detailitem.Name = item.Name;
                        detailitem.Description = item.Description;
                        detailitem.Qty = item.Qty;

                        detailItems.Add(detailitem);
                    }

                    _dbContext.detailitems.AddRange(detailItems);

                    _dbContext.SaveChanges();

                    return Ok(new { master, detailItems });
                }
                else
                {
                    MasterItem master = _dbContext.masterItems.Where(x => x.Id == model.master.id).FirstOrDefault();
                    if (master == null)
                    {
                        return NotFound(new { message = "Master item not found." });
                    }

                    master.ItemName = model.master.ItemName;
                    _dbContext.masterItems.Update(master);
                    _dbContext.SaveChanges();

                    List<Detailitem> detailItems = _dbContext.detailitems.Where(x => x.MasterItemId == master.Id).ToList();

                    foreach (var item in model.detail)
                    {
                        if (item.id!=0)
                        {
                            var existingDetail = detailItems.FirstOrDefault(x => x.ID == item.id && x.MasterItemId == master.Id);
                            if (existingDetail != null)
                            {
                                existingDetail.Name = item.Name;
                                existingDetail.Description = item.Description;
                                existingDetail.Qty = item.Qty;
                            }
                           
                        }
                        else
                        {
                            Detailitem newDetail = new Detailitem
                            {
                                MasterItemId = master.Id,
                                Name = item.Name,
                                Description = item.Description,
                                Qty = item.Qty
                            };
                            detailItems.Add(newDetail);
                        }


                    }

                    var itemNamesToKeep = model.detail.Select(x => x.id).ToList();

                    var detailsToRemove = detailItems
                        .Where(x => !itemNamesToKeep.Contains(x.ID) && x.MasterItemId == master.Id)
                        .ToList();

                    if (detailsToRemove.Any())
                    {
                        _dbContext.detailitems.RemoveRange(detailsToRemove);
                        _dbContext.SaveChanges();
                    }

                    _dbContext.detailitems.UpdateRange(detailItems.Where(x => itemNamesToKeep.Contains(x.ID)));
                    _dbContext.SaveChanges();


                    //_dbContext.detailitems.UpdateRange(detailItems);
                    //_dbContext.SaveChanges();

                    return Ok(new { master, detailItems });
                }
            }
            catch (Exception e)
            {
                return StatusCode(400, new { message = "An error occurred while processing your request.", details = e.Message });
            }
        }


        [HttpGet, Route("MasterItemRecord")]
        public IActionResult MasterItemRecord([FromQuery] int id)
        {
            try
            {
                var masterdata = _dbContext.masterItems.Where(x => x.Id == id).Select(x => x.ItemName).FirstOrDefault();

                if (masterdata != null)
                {
                    return Ok(new { ItemName = masterdata });
                }
                else
                {
                    return BadRequest(new { message = "Given Id does not exist in this system" });
                }
            }
            catch (Exception e)
            {
                return StatusCode(400, new { message = "An error occurred while processing your request.", details = e.Message });
            }
        }


        [HttpGet, Route("DetailItemRecord")]
        public IActionResult DetailItemRecord(int id)
        {
            try
            {
                var check = _dbContext.detailitems.Where(x => x.MasterItemId == id).FirstOrDefault();
                if (check!=null)
                {
                    var detaildata = (from a in _dbContext.detailitems
                                      where a.MasterItemId == id
                                      select new DetailViewModel
                                      {
                                          id=a.ID,
                                          Name = a.Name,
                                          Description = a.Description,
                                          Qty = a.Qty
                                      }).ToList();

                    return Ok(detaildata);
                }
                else
                {
                    return BadRequest("Given Id does not exist in this system");
                }
                
            }
            catch (Exception e)
            {
                return StatusCode(400, new { message = "An error occurred while processing your request.", details = e.Message });
            }

        }
    }
}
